using System.Linq;
using System.Web.Mvc;
using ClothingShop.Models;
using ClothingShop.Web.Services;
using ClothingShop.Web.ViewModels;

namespace ClothingShop.Controllers
{
    public class CartController : Controller
    {
        private CartService _cartService = new CartService();
        private ClothingShopDBEntities db = new ClothingShopDBEntities();

        public ActionResult Index()
        {
            var cart = _cartService.GetCart();
            Session["CartCount"] = cart.Sum(x => x.Quantity);
            ViewBag.Total = _cartService.GetCartTotal();
            return View(cart);
        }

        // Thêm vào giỏ bằng Form POST thuần (KHÔNG dùng AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(int productId, int? variantId = null, int quantity = 1, string returnUrl = null)
        {
            if (quantity <= 0) quantity = 1;

            var product = db.Products.Find(productId);
            if (product == null || !product.IsActive)
            {
                TempData["Error"] = "Sản phẩm không tồn tại hoặc đã ngừng kinh doanh.";
                return RedirectToAction("Index", "Product");
            }

            ProductVariant variant = null;
            if (variantId.HasValue && variantId.Value > 0)
            {
                variant = db.ProductVariants
                    .Include("ProductSize")
                    .Include("ProductColor")
                    .FirstOrDefault(v => v.VariantId == variantId.Value && v.ProductId == productId && v.IsActive);
            }

            if (variant == null)
            {
                variant = db.ProductVariants
                    .Include("ProductSize")
                    .Include("ProductColor")
                    .FirstOrDefault(v => v.ProductId == productId && v.IsActive && v.Stock > 0)
                    ?? db.ProductVariants
                    .Include("ProductSize")
                    .Include("ProductColor")
                    .FirstOrDefault(v => v.ProductId == productId && v.IsActive);
            }

            int availableStock = variant != null ? variant.Stock : product.Stock;
            if (availableStock <= 0)
            {
                TempData["Error"] = "Sản phẩm / biến thể đã hết hàng.";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            string selectedSize = variant?.ProductSize?.Name ?? variant?.ProductSize?.Code ?? "Freesize";
            string selectedColor = variant?.ProductColor?.Name ?? "Mặc định";
            string colorHex = variant?.ProductColor?.HexCode ?? "#1a1a1a";
            decimal unitPrice = variant?.Price ?? product.DiscountPrice ?? product.Price;

            var item = new CartItemVM
            {
                ProductId = productId,
                ProductVariantId = variant?.VariantId,
                SKU = variant?.SKU ?? ("SP" + product.ProductId.ToString("000")),
                Size = selectedSize,
                Color = selectedColor,
                ColorHex = colorHex,
                ProductName = product.Name,
                ImageUrl = product.ImageUrl,
                UnitPrice = unitPrice,
                Quantity = quantity,
                MaxStock = availableStock
            };

            var existing = _cartService.GetCart().FirstOrDefault(x => x.ItemKey == item.ItemKey);
            int currentQty = existing != null ? existing.Quantity : 0;
            if (currentQty + quantity > availableStock)
            {
                TempData["Error"] = $"Tồn kho chỉ còn {availableStock} sản phẩm (bạn đã có {currentQty} trong giỏ).";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            _cartService.AddItem(item);
            Session["CartCount"] = _cartService.GetCartCount();
            TempData["Success"] = $"Đã thêm \"{product.Name}\" ({selectedSize} - {selectedColor}) vào giỏ hàng!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            if (Request.UrlReferrer != null && Url.IsLocalUrl(Request.UrlReferrer.PathAndQuery))
                return Redirect(Request.UrlReferrer.PathAndQuery);

            return RedirectToAction("Index");
        }

        // Cập nhật số lượng bằng Form POST thuần (KHÔNG dùng AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(string itemKey, int quantity)
        {
            _cartService.UpdateQuantity(itemKey, quantity);
            Session["CartCount"] = _cartService.GetCartCount();
            TempData["Success"] = "Đã cập nhật giỏ hàng.";
            return RedirectToAction("Index");
        }

        public ActionResult Remove(string itemKey)
        {
            _cartService.RemoveItem(itemKey);
            Session["CartCount"] = _cartService.GetCartCount();
            TempData["Success"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
