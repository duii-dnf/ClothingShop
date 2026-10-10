using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClothingShop.Models;

namespace ClothingShop.Services
{
    // Sắp xếp sản phẩm dùng chung cho trang chủ và trang danh sách 
    public static class ProductSorting
    {
        public const string Newest = "newest";
        public const string Featured = "featured";
        public const string PriceAsc = "price_asc";
        public const string PriceDesc = "price_desc";
        public const string NameAsc = "name_asc";
        public const string NameDesc = "name_desc";

        // Thứ tự hiển thị trong dropdown
        public static readonly KeyValuePair<string, string>[] Options =
        {
            new KeyValuePair<string, string>(Newest, "Mới nhất"),
            new KeyValuePair<string, string>(Featured, "Nổi bật"),
            new KeyValuePair<string, string>(PriceAsc, "Giá: thấp đến cao"),
            new KeyValuePair<string, string>(PriceDesc, "Giá: cao đến thấp"),
            new KeyValuePair<string, string>(NameAsc, "Tên: A - Z"),
            new KeyValuePair<string, string>(NameDesc, "Tên: Z - A")
        };

        // Giá trị lạ hoặc rỗng thì về "Mới nhất"; không bao giờ trả về null
        public static string Normalize(string sort)
        {
            if (string.IsNullOrWhiteSpace(sort)) return Newest;
            sort = sort.Trim().ToLowerInvariant();
            return Options.Any(o => o.Key == sort) ? sort : Newest;
        }

        // Sắp xếp ngay trong SQL; gọi TRƯỚC Skip/Take khi phân trang
        public static IQueryable<Product> Apply(IQueryable<Product> query, string sort)
        {
            switch (Normalize(sort))
            {
                case PriceAsc:
                    // Dùng đúng giá đang hiển thị (giá khuyến mãi nếu có và thấp hơn giá gốc)
                    return query.OrderBy(p => p.DiscountPrice.HasValue && p.DiscountPrice.Value < p.Price
                                                ? p.DiscountPrice.Value : p.Price)
                                .ThenByDescending(p => p.ProductId);
                case PriceDesc:
                    return query.OrderByDescending(p => p.DiscountPrice.HasValue && p.DiscountPrice.Value < p.Price
                                                ? p.DiscountPrice.Value : p.Price)
                                .ThenByDescending(p => p.ProductId);
                case NameAsc:
                    return query.OrderBy(p => p.Name).ThenByDescending(p => p.ProductId);
                case NameDesc:
                    return query.OrderByDescending(p => p.Name).ThenByDescending(p => p.ProductId);
                case Featured:
                    return query.OrderByDescending(p => p.IsFeatured)
                                .ThenByDescending(p => p.CreatedAt)
                                .ThenByDescending(p => p.ProductId);
                default: // Newest
                    return query.OrderByDescending(p => p.CreatedAt)
                                .ThenByDescending(p => p.ProductId);
            }
        }
    }
}