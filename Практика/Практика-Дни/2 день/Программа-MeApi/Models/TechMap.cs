using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MeApi.Models
{

    [Table("tech_maps")]
    public class TechMap
    {
        [Key] public int Id { get; set; }
        public int RecipeId { get; set; }
        public string Name { get; set; }
        public int Version { get; set; }
        public string Status { get; set; }
        public int? EquipmentId { get; set; }
        public int? CreatedBy { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public int? EquipmentId1 { get; set; }
        public int? RecipesId { get; set; }
        public int? UsersId { get; set; }
        public int? Users1Id { get; set; }
        public int? UsersId1 { get; set; }
        public int? UsersId2 { get; set; }
    }
}