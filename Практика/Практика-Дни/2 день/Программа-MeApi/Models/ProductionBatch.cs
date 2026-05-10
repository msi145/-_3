using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MeApi.Models
{
    [Table("production_batches")]
    public class ProductionBatch
    {
        [Key] public int Id { get; set; }
        public int? OrderId { get; set; }
        public string BatchNumber { get; set; }
        public string Status { get; set; }
        public decimal? ActualQty { get; set; }
        public string QtyUnit { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public int? ResponsibleUser { get; set; }
        public string Notes { get; set; }

        public int? ProductionOrdersId { get; set; }
        public int? UsersId { get; set; }
    }
}