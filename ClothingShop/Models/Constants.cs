using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ClothingShop.Models
{
    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Customer = "Customer";
    }

    public static class OrderStatus
    {
        public const string Pending = "Pending";       // Chờ xác nhận
        public const string Confirmed = "Confirmed";   // Đã xác nhận
        public const string Shipping = "Shipping";     // Đang giao
        public const string Completed = "Completed";   // Hoàn thành (tính doanh thu)
        public const string Cancelled = "Cancelled";   // Đã hủy
    }

    public static class PaymentMethods
    {
        public const string COD = "COD";
        public const string BankTransfer = "Chuyển khoản";
    }
}