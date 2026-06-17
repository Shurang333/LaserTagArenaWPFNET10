namespace LaserTagArenaWPFNET10.Models
{
    public static class NotificationTypes
    {
        public const string OrderCreate = "OrderCreate";
        public const string OrderStatus = "OrderStatus";
        public const string OrderDelete = "OrderDelete";
        public const string CartAdd = "CartAdd";
        public const string CartRemove = "CartRemove";
        public const string EquipmentAdd = "EquipmentAdd";
        public const string EquipmentUpdate = "EquipmentUpdate";
        public const string EquipmentDelete = "EquipmentDelete";
        public const string PriceChange = "PriceChange";
        public const string StockChange = "StockChange";
        public const string ShipmentCreate = "ShipmentCreate";
        public const string ShipmentComplete = "ShipmentComplete";
        public const string CategoryAdd = "CategoryAdd";
        public const string CategoryDelete = "CategoryDelete";
        public const string ManufacturerAdd = "ManufacturerAdd";
        public const string ManufacturerDelete = "ManufacturerDelete";
        public const string UserRegister = "UserRegister";
        public const string UserBlock = "UserBlock";
        public const string UserActivate = "UserActivate";
        public const string UserRoleChange = "UserRoleChange";
        public const string System = "System";

        public static string GetLabel(string? type)
        {
            return type switch
            {
                OrderCreate => "Заказ",
                OrderStatus => "Статус заказа",
                OrderDelete => "Удаление заказа",
                CartAdd => "Корзина",
                CartRemove => "Корзина",
                EquipmentAdd => "Новый товар",
                EquipmentUpdate => "Товар изменён",
                EquipmentDelete => "Товар удалён",
                PriceChange => "Цена",
                StockChange => "Склад",
                ShipmentCreate => "Поставка",
                ShipmentComplete => "Поставка завершена",
                CategoryAdd => "Категория",
                CategoryDelete => "Категория",
                ManufacturerAdd => "Производитель",
                ManufacturerDelete => "Производитель",
                UserRegister => "Регистрация",
                UserBlock => "Блокировка",
                UserActivate => "Активация",
                UserRoleChange => "Роль",
                _ => "Система"
            };
        }
    }
}