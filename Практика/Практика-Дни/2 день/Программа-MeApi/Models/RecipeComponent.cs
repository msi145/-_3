using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MeApi.Models
{

    [Table("recipe_components")]
    public class RecipeComponent
    {
        [Key] public int Id { get; set; }
        public int RecipeId { get; set; }
        public int MaterialId { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public decimal? TolerancePct { get; set; }
        public int? LoadOrder { get; set; }

        public int? RawMaterialsId { get; set; }
        public int? RecipesId { get; set; }
    }
}