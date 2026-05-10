using System.Collections.Generic;
using System.Data.Entity;  // ← EF6 вместо EF Core
using System.Linq;
using System.Net;
using System.Web.Http;
using MeApi.Data;
using MeApi.Models;

namespace MeApi.Controllers
{
    public class ProductsController : ApiController
    {
        private readonly AppDbContext _db = new AppDbContext();

        // GET: api/products
        public IHttpActionResult GetProducts()
        {
            var products = _db.Products.ToList();
            return Ok(products);
        }

        // GET: api/products/5
        public IHttpActionResult GetProduct(int id)
        {
            var product = _db.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        // POST: api/products
        public IHttpActionResult PostProduct(Product product)
        {
            _db.Products.Add(product);
            _db.SaveChanges();
            return CreatedAtRoute("DefaultApi", new { id = product.Id }, product);
        }

        // PUT: api/products/5
        public IHttpActionResult PutProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest("ID не совпадает");
            }
            _db.Entry(product).State = EntityState.Modified;
            _db.SaveChanges();

            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }

        // DELETE: api/products/5
        public IHttpActionResult DeleteProduct(int id)
        {
            var product = _db.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            _db.Products.Remove(product);
            _db.SaveChanges();

            return Ok();
        }
    }
}