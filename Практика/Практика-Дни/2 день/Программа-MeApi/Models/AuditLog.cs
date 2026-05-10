using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MeApi.Models
{
    [Table("audit_log")]
    public class AuditLog
    {
        [Key] public long Id { get; set; } // bigint в БД
        public int? UserId { get; set; }
        public string Action { get; set; }
        public string EntityType { get; set; }
        public int? EntityId { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        public string IpAddress { get; set; }
        public DateTime? OccurredAt { get; set; }

        public int? UsersId { get; set; }
    }
}