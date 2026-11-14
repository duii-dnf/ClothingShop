using System.Collections.Generic;
using System.Linq;
using System.Web;
using ClothingShop.Web.ViewModels;

namespace ClothingShop.Web.Services
{
    public class CartService
    {
        private const string CartSessionKey = "ShoppingCart";

        public List<CartItemVM> GetCart()
        {
            var cart = HttpContext.Current.Session[CartSessionKey] as List<CartItemVM>;
            if (cart == null)
            {
                cart = new List<CartItemVM>();
                HttpContext.Current.Session[CartSessionKey] = cart;
            }
            return cart;
        }

        public void AddItem(CartItemVM item)
        {
            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(x => x.ProductId == item.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity; // Cộng dồn nếu đã có
            }
            else
            {
                cart.Add(item);
            }
            HttpContext.Current.Session[CartSessionKey] = cart;
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                if (quantity <= 0)
                    cart.Remove(item); // Xóa nếu số lượng = 0
                else
                    item.Quantity = quantity;
            }
            HttpContext.Current.Session[CartSessionKey] = cart;
        }

        public void RemoveItem(int productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                cart.Remove(item);
            }
            HttpContext.Current.Session[CartSessionKey] = cart;
        }

        public void ClearCart()
        {
            HttpContext.Current.Session[CartSessionKey] = null;
        }

        public decimal GetCartTotal()
        {
            return GetCart().Sum(x => x.TotalPrice);
        }
    }
}