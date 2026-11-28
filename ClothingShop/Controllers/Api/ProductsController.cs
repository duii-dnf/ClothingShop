using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using ClothingShop.Models;

namespace ClothingShop.Controllers.Api
{
    public class ProductsController : ApiController
    {
        private ClothingShopDBEntities db = new ClothingShopDBEntities();

        public ProductsController()
        {
            db.Configuration.ProxyCreationEnabled = false;
        }

        // GET: api/products
        public IHttpActionResult GetAll()
        {
            var list = db.Products.OrderByDescending(p => p.ProductId).ToList();
            return Ok(list);
        }

        // GET: api/products/5
        public IHttpActionResult GetById(int id)
        {
            var product = db.Products.Find(id);
            if (product == null) return NotFound();
            return Ok(product);
        }

        // POST: api/products
        public IHttpActionResult Post([FromBody] Product product)
        {
            if (product == null || string.IsNullOrWhiteSpace(product.Name))
                return BadRequest("Dữ liệu sản phẩm không hợp lệ.");

            if (string.IsNullOrWhiteSpace(product.ImageUrl))
                product.ImageUrl = "default.jpg";

            product.CreatedAt = DateTime.Now;
            db.Products.Add(product);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = product.ProductId }, product);
        }

        // PUT: api/products/5
        public IHttpActionResult Put(int id, [FromBody] Product product)
        {
            if (product == null || string.IsNullOrWhiteSpace(product.Name))
                return BadRequest("Dữ liệu sản phẩm không hợp lệ.");

            var existing = db.Products.Find(id);
            if (existing == null) return NotFound();

            existing.Name = product.Name;
            existing.Description = product.Description;
            existing.Price = product.Price;
            existing.DiscountPrice = product.DiscountPrice;
            existing.Gender = product.Gender;
            existing.IsNew = product.IsNew;
            existing.IsFeatured = product.IsFeatured;
            existing.Sizes = product.Sizes;
            existing.Colors = product.Colors;
            existing.Stock = product.Stock;
            existing.IsActive = product.IsActive;
            existing.CategoryId = product.CategoryId;
            if (!string.IsNullOrWhiteSpace(product.ImageUrl))
                existing.ImageUrl = product.ImageUrl;

            db.SaveChanges();
            return Ok(existing);
        }

        // DELETE: api/products/5
        public IHttpActionResult Delete(int id)
        {
            var product = db.Products
                .Include(p => p.ProductStyles)
                .Include(p => p.ProductVariants)
                .Include(p => p.InventoryLogs)
                .Include(p => p.Favorites)
                .FirstOrDefault(p => p.ProductId == id);
            if (product == null) return NotFound();

            if (db.OrderDetails.Any(d => d.ProductId == id))
                return Content(HttpStatusCode.Conflict, "Sản phẩm đã có trong đơn hàng, không thể xóa.");

            product.ProductStyles.Clear();
            db.Favorites.RemoveRange(product.Favorites.ToList());
            db.InventoryLogs.RemoveRange(product.InventoryLogs.ToList());
            db.ProductVariants.RemoveRange(product.ProductVariants.ToList());
            db.Products.Remove(product);
            db.SaveChanges();
            return Ok(new { message = "Đã xóa sản phẩm", id });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
