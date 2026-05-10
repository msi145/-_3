using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using MeApi.Data;
using MeApi.Models;
using System.Data.Entity;
namespace MeApi.Controllers
{
    [RoutePrefix("api/RawMaterials")]
    public class RawMaterialsController : ApiController
    {
        private readonly AppDbContext _db = new AppDbContext();

        // GET: api/RawMaterials
        [HttpGet]
        public IHttpActionResult GetRawMaterials()
        {
            var materials = _db.RawMaterials.ToList();
            return Ok(materials);
        }

        // GET: api/RawMaterials/5
        [HttpGet]
        [Route("{id}")]
        public IHttpActionResult GetRawMaterial(int id)
        {
            var material = _db.RawMaterials.Find(id);
            if (material == null) return NotFound();
            return Ok(material);
        }

        // POST: api/RawMaterials
        [HttpPost]
        public IHttpActionResult PostRawMaterial(RawMaterial material)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            _db.RawMaterials.Add(material);
            _db.SaveChanges();
            // Возвращаем созданный объект с ID
            return CreatedAtRoute("GetRawMaterial", new { id = material.Id }, material);
        }

        // PUT: api/RawMaterials/5
        [HttpPut]
        [Route("{id}")]
        public IHttpActionResult PutRawMaterial(int id, RawMaterial material)
        {
            if (id != material.Id) return BadRequest("ID не совпадает");

            _db.Entry(material).State = (_db.Entry(material).State = EntityState.Modified);
            try
            {
                _db.SaveChanges();
            }
            catch
            {
                return BadRequest("Ошибка обновления");
            }
            return StatusCode(System.Net.HttpStatusCode.NoContent);
        }

        // DELETE: api/RawMaterials/5
        [HttpDelete]
        [Route("{id}")]
        public IHttpActionResult DeleteRawMaterial(int id)
        {
            var material = _db.RawMaterials.Find(id);
            if (material == null) return NotFound();

            _db.RawMaterials.Remove(material);
            _db.SaveChanges();
            return Ok();
        }
    }
}