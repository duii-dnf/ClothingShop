using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ClothingShop.Models;
using ClothingShop.Services;
using ClothingShop.Web.Services;
using ClothingShop.Web.ViewModels;

namespace ClothingShop.Controllers
{
    public class CartController : Controller
    {
        private CartService _cartService = new CartService();
        private ShopDbContext db = new ShopDbContext();

        public ActionResult Index()
        {
            var cart = _cartService.GetCart();
            Session["CartCount"] = cart.Sum(x => x.Quantity);
            ViewBag.Total = _cartService.GetCartTotal();
            return View(cart);
        }

        [HttpPost]
        public ActionResult Add(int productId, int? variantId = null, string size = null, string color = null, int quantity = 1, decimal? price = null, string productName = null, string imageUrl = null)
        {
            if (quantity <= 0) quantity = 1;

            var product = db.Products.Find(productId);
            if (product == null || !product.IsActive)
            {
                if (Request.IsAjaxRequest())
                    return Json(new { success = false, message = "Sản phẩm không tồn tại hoặc đã ngừng kinh doanh." });
                TempData["Error"] = "Sản phẩm không tồn tại hoặc đã ngừng kinh doanh.";
                return RedirectToAction("Index");
            }

            ProductVariant variant = null;
            if (variantId.HasValue && variantId.Value > 0)
            {
                variant = db.ProductVariants.FirstOrDefault(v => v.VariantId == variantId.Value && v.ProductId == productId && v.IsActive);
            }

            // If no variantId provided, try to find a matching variant by size and color, or take first active variant
            if (variant == null)
            {
                if (!string.IsNullOrEmpty(size) && !string.IsNullOrEmpty(color))
                {
                    variant = db.ProductVariants.FirstOrDefault(v => v.ProductId == productId && v.IsActive &&
                                ((v.Size != null && v.Size.Code == size) || (v.Size != null && v.Size.Name == size)) &&
                                ((v.Color != null && v.Color.Name == color) || (v.Color != null && v.Color.Code == color)));
                }

                if (variant == null)
                {
                    variant = db.ProductVariants.FirstOrDefault(v => v.ProductId == productId && v.IsActive && v.Stock > 0)
                           ?? db.ProductVariants.FirstOrDefault(v => v.ProductId == productId && v.IsActive);
                }
            }

            int availableStock = variant != null ? variant.Stock : product.Stock;
            if (availableStock <= 0)
            {
                if (Request.IsAjaxRequest())
                    return Json(new { success = false, message = "Biến thể hoặc sản phẩm đã hết hàng!" });
                TempData["Error"] = "Biến thể hoặc sản phẩm đã hết hàng!";
                return RedirectToAction("Index");
            }

            // Check current in-cart quantity
            string selectedSize = variant?.Size?.Name ?? variant?.Size?.Code ?? size ?? "Freesize";
            string selectedColor = variant?.Color?.Name ?? color ?? "Đen";
            string colorHex = variant?.Color?.HexCode ?? "#1a1a1a";
            int? selectedVariantId = variant?.VariantId;
            string sku = variant?.SKU ?? $"SP{product.ProductId:03d}";

            decimal unitPrice = variant?.Price ?? product.DiscountPrice ?? product.Price;
            if (price.HasValue && price.Value > 0 && !variant?.Price.HasValue == true)
            {
                unitPrice = price.Value;
            }

            var item = new CartItemVM
            {
                ProductId = productId,
                ProductVariantId = selectedVariantId,
                SKU = sku,
                Size = selectedSize,
                Color = selectedColor,
                ColorHex = colorHex,
                ProductName = product.Name,
                ImageUrl = product.ImageUrl,
                UnitPrice = unitPrice,
                Quantity = quantity,
                MaxStock = availableStock
            };

            var existingInCart = _cartService.GetCart().FirstOrDefault(x => x.ItemKey == item.ItemKey);
            int currentQty = existingInCart != null ? existingInCart.Quantity : 0;
            if (currentQty + quantity > availableStock)
            {
                string msg = $"Tồn kho chỉ còn {availableStock} sản phẩm (bạn đã có {currentQty} trong giỏ).";
                if (Request.IsAjaxRequest())
                    return Json(new { success = false, message = msg });
                TempData["Error"] = msg;
                return RedirectToAction("Index");
            }

            _cartService.AddItem(item);
            int newCartCount = _cartService.GetCartCount();
            Session["CartCount"] = newCartCount;

            if (Request.IsAjaxRequest())
            {
                return Json(new
                {
                    success = true,
                    message = $"Đã thêm {product.Name} ({selectedSize} - {selectedColor}) vào giỏ hàng!",
                    cartCount = newCartCount
                });
            }

            TempData["Success"] = $"Đã thêm {product.Name} vào giỏ hàng!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Update(string itemKey, int quantity, int? productId = null)
        {
            if (string.IsNullOrEmpty(itemKey) && productId.HasValue)
            {
                _cartService.UpdateQuantity(productId.Value, quantity);
            }
            else
            {
                _cartService.UpdateQuantity(itemKey, quantity);
            }

            int newCartCount = _cartService.GetCartCount();
            Session["CartCount"] = newCartCount;

            if (Request.IsAjaxRequest())
            {
                var cart = _cartService.GetCart();
                var item = cart.FirstOrDefault(x => x.ItemKey == itemKey || (productId.HasValue && x.ProductId == productId.Value));
                return Json(new
                {
                    success = true,
                    itemTotal = item != null ? item.TotalPrice.ToString("N0") + " VNĐ" : "0 VNĐ",
                    cartTotal = _cartService.GetCartTotal().ToString("N0") + " VNĐ",
                    cartCount = newCartCount
                });
            }

            return RedirectToAction("Index");
        }

        public ActionResult Remove(string itemKey, int? productId = null)
        {
            if (!string.IsNullOrEmpty(itemKey))
            {
                _cartService.RemoveItem(itemKey);
            }
            else if (productId.HasValue)
            {
                _cartService.RemoveItem(productId.Value);
            }

            Session["CartCount"] = _cartService.GetCartCount();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
