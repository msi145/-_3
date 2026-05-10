using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MeApi.Models
{
    [Table("users")]
    public class User
    {
        [Key] public int Id { get; set; }
        public string Username { get; set; }
        public string  PasswordHash { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public int? RoleId { get; set; }
        public int? DepartmentId { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime? CreatedAt { get; set; }

        // Служебные FK (артефакты EF)
        public int? DepartmentsId { get; set; }
        public int? RolesId { get; set; }
    }
}