using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MeApi.Models
{
    [Table("deviations")]
    public class Deviation
    {
        [Key] public int Id { get; set; }
        public int BatchId { get; set; }
        public int? StepExecutionId { get; set; }
        public string Severity { get; set; }
        public string DeviationType { get; set; }
        public string Description { get; set; }
        public int? RegisteredBy { get; set; }
        public DateTime? RegisteredAt { get; set; }
        public string ResolutionStatus { get; set; }
        public int? ResolvedBy { get; set; }
        public DateTime? ResolvedAt { get; set; }

        public int? ProductionBatchesId { get; set; }
        public int? StepExecutionsId { get; set; }
        public int? UsersId { get; set; }
        public int? Users1Id { get; set; }
        public int? UsersId1 { get; set; }
        public int? UsersId2 { get; set; }
    }
}