using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MeApi.Models
{
    [Table("lab_tests")]
    public class LabTest
    {
        [Key] public int Id { get; set; }
        public string ObjectType { get; set; }
        public int ObjectId { get; set; }
        public string Status { get; set; }
        public int? LabUserId { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Decision { get; set; }
        public string    Conclusion { get; set; }

        public int? UsersId { get; set; }
        public int? Users1Id { get; set; }
        public int? UsersId1 { get; set; }
        public int? UsersId2 { get; set; }
    }
}