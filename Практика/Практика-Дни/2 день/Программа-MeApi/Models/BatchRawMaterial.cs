using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MeApi.Models
{
    [Table("batch_raw_materials")]
    public class BatchRawMaterial
    {
        [Key] public int Id { get; set; }
        public int BatchId { get; set; }
        public int RmBatchId { get; set; }
        public decimal? PlannedQty { get; set; }
        public decimal? ActualQty { get; set; }
        public string Unit { get; set; }
        public DateTime? AddedAt { get; set; }
        public int? AddedBy { get; set; }

        public int? ProductionBatchesId { get; set; }
        public int? RawMaterialBatchesId { get; set; }
        public int? UsersId { get; set; }
    }
}