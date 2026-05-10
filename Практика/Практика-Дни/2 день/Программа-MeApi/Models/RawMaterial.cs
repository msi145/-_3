using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MeApi.Models
{

    [Table("raw_materials")]
    public class RawMaterial
    {
        [Key] public int Id { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public string MaterialType { get; set; }
        public string CasNumber { get; set; }
        public string Specification { get; set; }

    }
}