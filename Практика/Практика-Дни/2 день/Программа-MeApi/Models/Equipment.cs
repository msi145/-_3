using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MeApi.Models
{
    [Table("equipments")]
    public class Equipment
    {
        [Key] public int Id { get; set; }
        public string Name { get; set; }
        public string EquipmentType { get; set; }
        public string Location { get; set; }
        public decimal? MaxCapacity { get; set; }
        public string CapacityUnit { get; set; }
        public string Status { get; set; }
        public DateTime? LastMaintenance { get; set; }
    }
}