using Microsoft.EntityFrameworkCore;
using MyWarehouse.Models;
using MyWarehouse.Models.Entities;

namespace MyWarehouse.Services
{
    public interface ITaskProcessingService
    {
        Task<bool> CompleteTaskAsync(int taskId);
        Task<bool> CancelTaskAsync(int taskId);
    }

    public class TaskProcessingService(AppDbContext db) : ITaskProcessingService
    {
        private readonly AppDbContext _db = db;

        public async Task<bool> CompleteTaskAsync(int taskId)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var task = await _db.CURS_DeliveryTasks
                    .Include(t => t.Product)
                    .Include(t => t.DeliveryType)
                    .FirstOrDefaultAsync(t => t.IdDeliveryTask == taskId);

                if (task == null || task.TaskStatusId != (int)DeliveryTaskStatus.InProgress)
                    return false;

                // Выполняем операцию в зависимости от типа
                var success = task.DeliveryTypeId switch
                {
                    (int)DeliveryOperationType.Moving => await ProcessMovingOperationAsync(task),
                    (int)DeliveryOperationType.Incoming => await ProcessIncomingOperationAsync(task),
                    (int)DeliveryOperationType.Outgoing => await ProcessOutgoingOperationAsync(task),
                    _ => false
                };

                if (success)
                {
                    task.TaskStatusId = (int)DeliveryTaskStatus.Completed;
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return true;
                }

                await transaction.RollbackAsync();
                return false;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> CancelTaskAsync(int taskId)
        {
            var task = await _db.CURS_DeliveryTasks.FindAsync(taskId);
            if (task == null) return false;

            task.TaskStatusId = (int)DeliveryTaskStatus.Rejected;
            await _db.SaveChangesAsync();
            return true;
        }

        private async Task<bool> ProcessIncomingOperationAsync(DeliveryTask task)
        {
            // Приемка товара - увеличиваем количество на складе
            if (task.ToLocationId == null)
                throw new InvalidOperationException("Для операции приемки должна быть указана целевая локация");

            var stock = await _db.CURS_Stocks
                .FirstOrDefaultAsync(s => s.ProductId == task.ProductId && s.LocationId == task.ToLocationId.Value);

            if (stock == null)
            {
                // Создаем новую запись на складе
                stock = new Stock
                {
                    ProductId = task.ProductId,
                    LocationId = task.ToLocationId.Value,
                    ProductQuantity = task.ProductQuantity,
                    MaxProductQuantity = await CalculateMaxQuantityAsync(task.ProductId, task.ToLocationId.Value)
                };
                _db.CURS_Stocks.Add(stock);
            }
            else
            {
                // Проверяем вместимость
                if (stock.ProductQuantity + task.ProductQuantity > stock.MaxProductQuantity)
                    throw new InvalidOperationException($"Превышена максимальная вместимость локации. Доступно: {stock.MaxProductQuantity - stock.ProductQuantity}");

                stock.ProductQuantity += task.ProductQuantity;
            }

            return true;
        }

        private async Task<bool> ProcessOutgoingOperationAsync(DeliveryTask task)
        {
            // Отгрузка товара - уменьшаем количество на складе
            if (task.FromLocationId == null)
                throw new InvalidOperationException("Для операции отгрузки должна быть указана исходная локация");

            var stock = await _db.CURS_Stocks
                .FirstOrDefaultAsync(s => s.ProductId == task.ProductId && s.LocationId == task.FromLocationId.Value);

            if (stock == null || stock.ProductQuantity < task.ProductQuantity)
                throw new InvalidOperationException($"Недостаточно товара на складе. Доступно: {stock?.ProductQuantity ?? 0}, требуется: {task.ProductQuantity}");

            stock.ProductQuantity -= task.ProductQuantity;

            // Если количество стало 0, удаляем запись (опционально)
            if (stock.ProductQuantity == 0)
                _db.CURS_Stocks.Remove(stock);

            return true;
        }

        private async Task<bool> ProcessMovingOperationAsync(DeliveryTask task)
        {
            // Перемещение товара - уменьшаем в одной локации, увеличиваем в другой
            if (task.FromLocationId == null || task.ToLocationId == null)
                throw new InvalidOperationException("Для операции перемещения должны быть указаны обе локации");

            // Сначала проверяем наличие товара в исходной локации
            var fromStock = await _db.CURS_Stocks
                .FirstOrDefaultAsync(s => s.ProductId == task.ProductId && s.LocationId == task.FromLocationId.Value);

            if (fromStock == null || fromStock.ProductQuantity < task.ProductQuantity)
                throw new InvalidOperationException($"Недостаточно товара для перемещения. Доступно: {fromStock?.ProductQuantity ?? 0}, требуется: {task.ProductQuantity}");

            // Проверяем целевую локацию
            var toStock = await _db.CURS_Stocks
                .FirstOrDefaultAsync(s => s.ProductId == task.ProductId && s.LocationId == task.ToLocationId.Value);

            if (toStock == null)
            {
                toStock = new Stock
                {
                    ProductId = task.ProductId,
                    LocationId = task.ToLocationId.Value,
                    ProductQuantity = 0,
                    MaxProductQuantity = await CalculateMaxQuantityAsync(task.ProductId, task.ToLocationId.Value)
                };
                _db.CURS_Stocks.Add(toStock);
            }

            // Проверяем вместимость целевой локации
            if (toStock.ProductQuantity + task.ProductQuantity > toStock.MaxProductQuantity)
                throw new InvalidOperationException($"Превышена максимальная вместимость целевой локации. Доступно: {toStock.MaxProductQuantity - toStock.ProductQuantity}");

            // Выполняем перемещение
            fromStock.ProductQuantity -= task.ProductQuantity;
            toStock.ProductQuantity += task.ProductQuantity;

            // Удаляем пустую запись если нужно
            if (fromStock.ProductQuantity == 0)
                _db.CURS_Stocks.Remove(fromStock);

            return true;
        }

        private async Task<int> CalculateMaxQuantityAsync(int productId, int locationId)
        {
            var product = await _db.CURS_Products
                .Include(p => p.Unit)
                .FirstOrDefaultAsync(p => p.IdProduct == productId);

            var location = await _db.CURS_Locations
                .FirstOrDefaultAsync(l => l.IdLocation == locationId);

            if (product == null || location == null)
                return 1000; // Значение по умолчанию

            // Рассчитываем максимальное количество на основе объема и веса
            var productVolume = product.Length * product.Width * product.Height;
            var locationVolume = location.MaxLength * location.MaxWidth * location.MaxHeight;

            var volumeCapacity = (int)(locationVolume / productVolume);
            var weightCapacity = (int)(location.MaxWeight / product.Weight);

            return Math.Min(volumeCapacity, weightCapacity);
        }
    }
}