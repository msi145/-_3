using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MeApi.Models
{
    [Table("products")]
    public class Product
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        [Column("product_type")]  // ← Добавить это!
        public string ProductType { get; set; }

        [Column("release_form")]  // ← Добавить это!
        public string ReleaseForm { get; set; }

        public string Status { get; set; }
    }
}