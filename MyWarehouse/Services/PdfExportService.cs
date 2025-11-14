using iTextSharp.text;
using iTextSharp.text.pdf;
using MyWarehouse.Models.Entities;
using System.IO;
using Font = iTextSharp.text.Font;

namespace MyWarehouse.Services
{
    public interface IPdfExportService
    {
        void ExportProductCurrentState(Product product, string filePath);
        void ExportProductFullHistory(Product product, ICollection<DeliveryTask> history, string filePath);
    }

    public class PdfExportService : IPdfExportService
    {
        private readonly Font _titleFont;
        private readonly Font _headerFont;
        private readonly Font _normalFont;
        private readonly Font _smallFont;

        public PdfExportService()
        {
            // Устанавливаем кириллические шрифты
            var baseFont = BaseFont.CreateFont(@"c:\windows\fonts\arial.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            _titleFont = new Font(baseFont, 16, Font.BOLD);
            _headerFont = new Font(baseFont, 12, Font.BOLD);
            _normalFont = new Font(baseFont, 10, Font.NORMAL);
            _smallFont = new Font(baseFont, 8, Font.NORMAL);
        }

        public void ExportProductCurrentState(Product product, string filePath)
        {
            using var fileStream = new FileStream(filePath, FileMode.Create);
            using var document = new Document(PageSize.A4, 50, 50, 50, 50);
            var writer = PdfWriter.GetInstance(document, fileStream);

            document.Open();

            // Заголовок документа
            AddHeader(document, $"ТЕКУЩЕЕ СОСТОЯНИЕ ТОВАРА - {product.Name}");

            // Информация о товаре
            AddProductInfo(document, product);

            // Текущие запасы
            AddCurrentStocks(document, product);

            // Подпись
            AddFooter(document);

            document.Close();
        }

        public void ExportProductFullHistory(Product product, ICollection<DeliveryTask> history, string filePath)
        {
            using var fileStream = new FileStream(filePath, FileMode.Create);
            using var document = new Document(PageSize.A4, 50, 50, 50, 50);
            var writer = PdfWriter.GetInstance(document, fileStream);

            document.Open();

            // Заголовок документа
            AddHeader(document, $"ПОЛНАЯ ИСТОРИЯ ТОВАРА - {product.Name}");

            // Информация о товаре
            AddProductInfo(document, product);

            // История перемещений
            AddMovementHistory(document, history);

            // Текущие запасы
            AddCurrentStocks(document, product);

            // Подпись
            AddFooter(document);

            document.Close();
        }

        private void AddHeader(Document document, string title)
        {
            // Лого и заголовок
            var table = new PdfPTable(2) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 1, 3 });

            // Левый столбец - логотип (заглушка)
            var logoCell = new PdfPCell(new Phrase("СКЛАД", _headerFont))
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_LEFT,
                VerticalAlignment = Element.ALIGN_MIDDLE
            };
            table.AddCell(logoCell);

            // Правый столбец - заголовок и дата
            var headerCell = new PdfPCell
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT
            };

            var headerContent = new Paragraph
            {
                new Phrase(title, _titleFont),
                new Phrase("\nДата формирования: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm"), _smallFont)
            };
            headerCell.AddElement(headerContent);
            table.AddCell(headerCell);

            document.Add(table);
            document.Add(new Paragraph(" ")); // Отступ
        }

        private void AddProductInfo(Document document, Product product)
        {
            var section = new Paragraph("ОСНОВНАЯ ИНФОРМАЦИЯ О ТОВАРЕ", _headerFont)
            {
                SpacingAfter = 10f
            };
            document.Add(section);

            var table = new PdfPTable(2) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 1, 2 });

            AddTableRow(table, "Наименование:", product.Name);
            AddTableRow(table, "Артикул (SKU):", product.SKU);
            AddTableRow(table, "Категория:", product.Category?.Name ?? "Не указана");
            AddTableRow(table, "Вес единицы:", $"{product.Weight:N2} кг");
            AddTableRow(table, "Габариты:", $"{product.Length:N2} x {product.Width:N2} x {product.Height:N2} см");

            var features = new List<string>();
            if (product.IsFragile) features.Add("Хрупкий");
            if (product.IsWaterSensitive) features.Add("Боится влаги");
            AddTableRow(table, "Особенности:", features.Any() ? string.Join(", ", features) : "Нет");

            AddTableRow(table, "Описание:", product.Description ?? "Не указано");
            AddTableRow(table, "Срок годности:", product.ExpirationDate?.ToString("dd.MM.yyyy") ?? "Не ограничен");

            document.Add(table);
            document.Add(new Paragraph(" "));
        }

        private void AddCurrentStocks(Document document, Product product)
        {
            var section = new Paragraph("ТЕКУЩИЕ ЗАПАСЫ НА СКЛАДЕ", _headerFont);
            section.SpacingAfter = 10f;
            document.Add(section);

            if (!product.Stocks.Any())
            {
                document.Add(new Paragraph("Товар отсутствует на складе", _normalFont));
                return;
            }

            var table = new PdfPTable(4) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 3, 2, 2, 3 });

            // Заголовки таблицы
            AddTableHeaderCell(table, "Местоположение");
            AddTableHeaderCell(table, "Количество");
            AddTableHeaderCell(table, "Максимум");
            AddTableHeaderCell(table, "Ограничения");

            foreach (var stock in product.Stocks)
            {
                table.AddCell(new PdfPCell(new Phrase(stock.Location?.DisplayName ?? "Не указано", _normalFont)));
                table.AddCell(new PdfPCell(new Phrase(stock.ProductQuantity.ToString(), _normalFont)));
                table.AddCell(new PdfPCell(new Phrase(stock.MaxProductQuantity.ToString(), _normalFont)));

                var constraints = stock.Location != null
                    ? $"{stock.Location.MaxLength:N2}/{stock.Location.MaxWidth:N2}/{stock.Location.MaxHeight:N2}/{stock.Location.MaxWeight:N2}"
                    : "Не указаны";
                table.AddCell(new PdfPCell(new Phrase(constraints, _normalFont)));
            }

            document.Add(table);
            document.Add(new Paragraph(" "));
        }

        private void AddMovementHistory(Document document, ICollection<DeliveryTask> history)
        {
            var section = new Paragraph("ИСТОРИЯ ПЕРЕМЕЩЕНИЙ", _headerFont);
            section.SpacingAfter = 10f;
            document.Add(section);

            if (history.Count == 0)
            {
                document.Add(new Paragraph("История перемещений отсутствует", _normalFont));
                document.Add(new Paragraph(" "));
                return;
            }

            var table = new PdfPTable(5) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 2, 2, 2, 2, 2 });

            // Заголовки таблицы
            AddTableHeaderCell(table, "Дата");
            AddTableHeaderCell(table, "Тип операции");
            AddTableHeaderCell(table, "Количество");
            AddTableHeaderCell(table, "Откуда");
            AddTableHeaderCell(table, "Куда");

            foreach (var task in history.OrderByDescending(t => t.CreatedAt))
            {
                table.AddCell(new PdfPCell(new Phrase(task.CreatedAt.ToString("dd.MM.yyyy HH:mm"), _normalFont)));
                table.AddCell(new PdfPCell(new Phrase(task.DeliveryType.Name ?? "Не указан", _normalFont)));
                table.AddCell(new PdfPCell(new Phrase(task.ProductQuantity.ToString(), _normalFont)));
                table.AddCell(new PdfPCell(new Phrase(task.FromLocation?.DisplayName ?? "-", _normalFont)));
                table.AddCell(new PdfPCell(new Phrase(task.ToLocation?.DisplayName ?? "-", _normalFont)));
            }

            document.Add(table);
            document.Add(new Paragraph(" "));
        }

        private void AddFooter(Document document)
        {
            document.Add(new Paragraph(" "));
            var footer = new Paragraph($"Документ сформирован автоматически системой управления складом\n{DateTime.Now:dd.MM.yyyy HH:mm}", _smallFont)
            {
                Alignment = Element.ALIGN_CENTER
            };
            document.Add(footer);
        }

        private void AddTableRow(PdfPTable table, string label, string value)
        {
            table.AddCell(new PdfPCell(new Phrase(label, _headerFont)) { Border = Rectangle.NO_BORDER });
            table.AddCell(new PdfPCell(new Phrase(value, _normalFont)) { Border = Rectangle.NO_BORDER });
        }

        private void AddTableHeaderCell(PdfPTable table, string text)
        {
            var cell = new PdfPCell(new Phrase(text, _headerFont))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                BackgroundColor = BaseColor.LIGHT_GRAY
            };
            table.AddCell(cell);
        }
    }
}