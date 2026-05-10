using System.Data.Entity; // Важно для EF6
using MeApi.Models;

namespace MeApi.Data
{
    public class AppDbContext : DbContext
    {
        // Конструктор использует строку "DefaultConnection" из Web.config
        public AppDbContext() : base("name=DefaultConnection")
        {
        }

        // Ваши DbSet
        public DbSet<Product> Products { get; set; }
        public DbSet<RawMaterial> RawMaterials { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<RawMaterialBatch> RawMaterialBatches { get; set; }
        public DbSet<Equipment> Equipments { get; set; }

        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<RecipeComponent> RecipeComponents { get; set; }
        public DbSet<TechMap> TechMaps { get; set; }
        public DbSet<TechStep> TechSteps { get; set; }
        public DbSet<ProductionOrder> ProductionOrders { get; set; }
        public DbSet<ProductionBatch> ProductionBatches { get; set; }
        public DbSet<BatchRawMaterial> BatchRawMaterials { get; set; }
        public DbSet<StepExecution> StepExecutions { get; set; }
        public DbSet<EquipmentTelemetry> EquipmentTelemetry { get; set; }

        public DbSet<Deviation> Deviations { get; set; }
        public DbSet<LabTest> LabTests { get; set; }
        public DbSet<LabTestResult> LabTestResults { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
    }
}