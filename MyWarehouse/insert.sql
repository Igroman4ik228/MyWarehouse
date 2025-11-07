-- ===========================================
-- 1. Таблица: CURS_Roles
-- ===========================================
INSERT INTO CURS_Roles (Name, Description)
VALUES
('Менеджер склада', 'Управление запасами и задачами'),
('Курьер', 'Выполнение задач по доставке');

-- ===========================================
-- 2. Таблица: CURS_Users
-- ===========================================
INSERT INTO CURS_Users (FirstName, LastName, Login, Password, RoleId, Patronymic)
VALUES
('Иван', 'Петров', 'ivan', '12345', 1, 'Сергеевич'),
('Мария', 'Сидорова', 'maria', 'qwerty', 2, NULL),
('Алексей', 'Кузнецов', 'alex', 'password', 3, 'Игоревич');

-- ===========================================
-- 3. Таблица: CURS_Categories
-- ===========================================
INSERT INTO CURS_Categories (Name, Description)
VALUES
('Продукты питания', 'Скоропортящиеся продукты'),
('Бытовая химия', 'Средства для уборки и гигиены'),
('Электроника', 'Бытовая и офисная техника');

-- ===========================================
-- 4. Таблица: CURS_Units
-- ===========================================
INSERT INTO CURS_Units (Name, Description)
VALUES
('шт', 'Штуки'),
('кг', 'Килограммы'),
('л', 'Литры');

-- ===========================================
-- 5. Таблица: CURS_Products
-- ===========================================
INSERT INTO CURS_Products
(Name, Description, CategoryId, UnitId, ManufacturerId, Weight, SKU, ExpirationDate,
 IsFragile, IsWaterSensitive, Length, Width, Height)
VALUES
('Молоко', 'Пастеризованное молоко 3.2%', 1, 3, 1, 1.05, 'SKU001', '2025-12-01', 0, 1, 10, 10, 25),
('Стиральный порошок', 'Порошок для белых тканей', 2, 1, 2, 1.2, 'SKU002', NULL, 0, 0, 25, 15, 10),
('Смартфон', 'Модель X1000', 3, 1, 3, 0.25, 'SKU003', NULL, 1, 0, 15, 7, 1);

-- ===========================================
-- 6. Таблица: CURS_Locations
-- ===========================================
INSERT INTO CURS_Locations (Shelf, Cell, Zone, MaxLength, MaxWidth, MaxHeight, MaxWeight)
VALUES
('A', '01', 'Z1', 100, 100, 100, 1000),
('B', '02', 'Z2', 150, 100, 120, 2000),
('C', '03', 'Z3', 200, 150, 150, 3000);

-- ===========================================
-- 7. Таблица: CURS_Stocks
-- ===========================================
INSERT INTO CURS_Stocks (ProductId, ProductQuantity, LocationId, MaxProductQuantity)
VALUES
(1, 50, 1, 200),
(2, 30, 2, 100),
(3, 10, 3, 50);

-- ===========================================
-- 8. Таблица: CURS_Clients
-- ===========================================
INSERT INTO CURS_Clients (Name, Description, Email, Phone)
VALUES
('ООО "Ромашка"', 'Клиент из Ярославля', 'info@romashka.ru', '+79001234567'),
('ЗАО "ТехноДом"', 'Регулярный клиент', 'sales@technodom.ru', '+79009876543'),
('ИП Соколова', 'Магазин электроники', 'ip.sokolova@mail.ru', '+79001112233');

-- ===========================================
-- 9. Таблица: CURS_TaskStatuses
-- ===========================================
INSERT INTO CURS_TaskStatuses (Name, Description)
VALUES
('Создана', 'Задача ожидает выполнения'),
('В процессе', 'Задача выполняется'),
('Завершена', 'Задача успешно выполнена');

-- ===========================================
-- 10. Таблица: CURS_DeliveryTypes
-- ===========================================
INSERT INTO CURS_DeliveryTypes (Name, Description)
VALUES
('Внутренняя', 'Перемещение между складами'),
('Внешняя', 'Доставка клиенту'),
('Возврат', 'Возврат товара на склад');

-- ===========================================
-- 11. Таблица: CURS_DeliveryTasks
-- ===========================================
INSERT INTO CURS_DeliveryTasks
(ProductId, ClientId, ProductQuantity, CreatedUserId, TaskStatusId, DeliveryTypeId,
 ExecutorUserId, FromLocationId, ToLocationId, CreatedAt)
VALUES
(1, 1, 10, 1, 1, 2, 3, 1, 2, GETUTCDATE()),
(2, 2, 5, 2, 2, 1, 3, 2, 3, GETUTCDATE()),
(3, 3, 2, 1, 3, 3, NULL, 3, NULL, GETUTCDATE());
