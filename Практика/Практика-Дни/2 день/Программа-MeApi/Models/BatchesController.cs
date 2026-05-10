using System;
using System.Web.Http;
using MeApi.Data;
using MeApi.Models;

namespace MeApi.Controllers
{
    [RoutePrefix("api/Batches")]
    public class BatchesController : ApiController
    {
        private readonly AppDbContext _db = new AppDbContext();
        [HttpPost]
        [Route("CreateFromOrder")]
        public IHttpActionResult CreateBatchFromOrder([FromBody] int orderId)
        {
            var order = _db.ProductionOrders.Find(orderId);
            if (order == null) return NotFound(); // ← Исправлено!

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    var batch = new ProductionBatch
                    {
                        OrderId = order.Id,
                        BatchNumber = "BATCH-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                        Status = "Planned",
                        StartedAt = DateTime.Now
                    };

                    _db.ProductionBatches.Add(batch);
                    _db.SaveChanges();

                    // Логика списания сырья...

                    transaction.Commit();
                    return Ok(batch);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return InternalServerError(ex);
                }
            }

        }

    } 
}