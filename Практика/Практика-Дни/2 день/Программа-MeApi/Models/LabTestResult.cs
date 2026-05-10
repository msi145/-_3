using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MeApi.Models
{

    [Table("lab_test_results")]
    public class LabTestResult
    {
        [Key] public int Id { get; set; }
        public int TestId { get; set; }
        public string ParameterName { get; set; }
        public decimal? MeasuredValue { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string Unit { get; set; }
        public bool? IsWithinRange { get; set; }
        public DateTime? MeasuredAt { get; set; }

        public int? LabTestsId { get; set; }
    }
}