using System;
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
            var session = HttpContext.Current?.Session;
            if (session == null) return new List<CartItemVM>();

            var cart = session[CartSessionKey] as List<CartItemVM>;
            if (cart == null)
            {
                cart = new List<CartItemVM>();
                session[CartSessionKey] = cart;
            }
            return cart;
        }

        public void AddItem(CartItemVM item)
        {
            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(x => x.ItemKey == item.ItemKey);

            if (existingItem != null)
            {
                int newQty = existingItem.Quantity + item.Quantity;
                if (item.MaxStock > 0 && newQty > item.MaxStock)
                {
                    newQty = item.MaxStock;
                }
                existingItem.Quantity = newQty;
            }
            else
            {
                if (item.MaxStock > 0 && item.Quantity > item.MaxStock)
                {
                    item.Quantity = item.MaxStock;
                }
                cart.Add(item);
            }
            SaveCart(cart);
        }

        public void UpdateQuantity(string itemKey, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ItemKey == itemKey);
            if (item != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    if (item.MaxStock > 0 && quantity > item.MaxStock)
                    {
                        quantity = item.MaxStock;
                    }
                    item.Quantity = quantity;
                }
            }
            SaveCart(cart);
        }

        // Backward compatibility
        public void UpdateQuantity(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                UpdateQuantity(item.ItemKey, quantity);
            }
        }

        public void RemoveItem(string itemKey)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ItemKey == itemKey);
            if (item != null)
            {
                cart.Remove(item);
            }
            SaveCart(cart);
        }

        // Backward compatibility
        public void RemoveItem(int productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                cart.Remove(item);
            }
            SaveCart(cart);
        }

        public void ClearCart()
        {
            var session = HttpContext.Current?.Session;
            if (session != null)
            {
                session[CartSessionKey] = null;
                session["CartCount"] = 0;
            }
        }

        public decimal GetCartTotal()
        {
            return GetCart().Sum(x => x.TotalPrice);
        }

        public int GetCartCount()
        {
            return GetCart().Sum(x => x.Quantity);
        }

        private void SaveCart(List<CartItemVM> cart)
        {
            var session = HttpContext.Current?.Session;
            if (session != null)
            {
                session[CartSessionKey] = cart;
                session["CartCount"] = cart.Sum(x => x.Quantity);
            }
        }
    }
}
