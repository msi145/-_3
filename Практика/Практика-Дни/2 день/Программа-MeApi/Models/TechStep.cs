using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MeApi.Models
{
    [Table("tech_steps")]
    public class TechStep
    {
        [Key] public int Id { get; set; }
        public int TechMapId { get; set; }
        public int StepNumber { get; set; }
        public string StepType { get; set; }
        public string Name { get; set; }
        public string Instructions { get; set; }
        public string ParamName { get; set; }
        public decimal? ParamTarget { get; set; }
        public decimal? ParamMin { get; set; }
        public decimal? ParamMax { get; set; }
        public string ParamUnit { get; set; }
        public int? DurationMin { get; set; }
        public bool? IsMandatory { get; set; }
        public int? EquipmentId { get; set; }

        public int? EquipmentId1 { get; set; }
        public int? TechMapsId { get; set; }
    }
}