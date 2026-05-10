using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MeApi.Models
{

    [Table("step_executions")]
    public class StepExecution
    {
        [Key] public int Id { get; set; }
        public int BatchId { get; set; }
        public int TechStepId { get; set; }
        public string Status { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public int? StartedBy { get; set; }
        public int? FinishedBy { get; set; }
        public decimal? ActualParamValue { get; set; }
        public int? ActualDurationMin { get; set; }
        public string Comment { get; set; }
        public int? EquipmentId { get; set; }

        public int? EquipmentId1 { get; set; }
        public int? TechStepsId { get; set; }
        public int? ProductionBatchesId { get; set; }
        public int? UsersId { get; set; }
        public int? Users1Id { get; set; }
        public int? UsersId1 { get; set; }
        public int? UsersId2 { get; set; }
    }
}