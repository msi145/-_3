using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MeApi.Models
{
    [Table("suppliers")]
    public class Supplier
    {
        [Key] public int Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string ContactInfo { get; set; }
        public bool? IsActive { get; set; }
    }
}