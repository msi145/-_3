using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MeApi.Models
{
    [Table("raw_material_batches")]
    public class RawMaterialBatch
    {
        [Key] public int Id { get; set; }
        [Required] public int MaterialId { get; set; }
        public int? SupplierId { get; set; }
        public string BatchNumber { get; set; }
        [Required] public DateTime ReceiptDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        [Required] public decimal QuantityKg { get; set; }
        [Required] public decimal RemainingQty { get; set; }
        public string Status { get; set; }
        public string CertificateRef { get; set; }

        public int? RawMaterialsId { get; set; }
        public int? SuppliersId { get; set; }
    }
}