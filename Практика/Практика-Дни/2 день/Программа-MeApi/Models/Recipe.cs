using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MeApi.Models
{
    [Table("recipes")]
    public class Recipe
    {
        [Key] public int Id { get; set; }
        public int ProductId { get; set; }
        public int Version { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public int? CreatedBy { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }

        // Служебные FK
        public int? ProductsId { get; set; }
        public int? UsersId { get; set; }
        public int? Users1Id { get; set; }
        public int? UsersId1 { get; set; }
        public int? UsersId2 { get; set; }
    }
}