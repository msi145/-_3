using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MeApi.Models
{
    [Table("equipment_telemetry")]
    public class EquipmentTelemetry
    {
        [Key] public long Id { get; set; } // bigint в БД
        public int EquipmentId { get; set; }
        public int? StepExecId { get; set; }
        public string ParameterName { get; set; }
        public decimal? Value { get; set; }
        public string Unit { get; set; }
        public DateTime? RecordedAt { get; set; }

        public int? EquipmentId1 { get; set; }
        public int? StepExecutionsId { get; set; }
    }
}