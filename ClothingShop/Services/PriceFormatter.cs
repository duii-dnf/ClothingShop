using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;

namespace ClothingShop.Services
{
    // Định dạng tiền Việt: 1.250.000₫ (không phụ thuộc culture của máy)
    public static class PriceFormatter
    {
        private static readonly CultureInfo Vi = new CultureInfo("vi-VN");

        public static string Vnd(decimal value)
        {
            return value.ToString("#,##0", Vi) + "\u20AB"; // ký tự ₫
        }

        public static string Vnd(decimal? value)
        {
            return value.HasValue ? Vnd(value.Value) : string.Empty;
        }
    }
}