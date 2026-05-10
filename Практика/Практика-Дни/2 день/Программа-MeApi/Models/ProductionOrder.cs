using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MeApi.Models
{
    [Table("production_orders")]
    public class ProductionOrder
    {
        [Key] public int Id { get; set; }
        public string OrderNumber { get; set; }
        public int? ProductId { get; set; }
        public int? RecipeId { get; set; }
        public int? TechMapId { get; set; }
        public decimal? PlannedQty { get; set; }
        public string QtyUnit { get; set; }
        [Required] public DateTime PlannedDate { get; set; } // NOT NULL в БД
        public string Status { get; set; }
        public int? Priority { get; set; }
        public int? CreatedBy { get; set; }

        public int? ProductsId { get; set; }
        public int? RecipesId { get; set; }
        public int? TechMapsId { get; set; }
        public int? UsersId { get; set; }
    }
}