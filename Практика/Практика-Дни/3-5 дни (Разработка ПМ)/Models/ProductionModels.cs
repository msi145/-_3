using System;
using System.Collections.Generic;

namespace WpfApp1.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ProductType { get; set; }
        public string ReleaseForm { get; set; }
        public string Status { get; set; }
    }

    public class Recipe
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Version { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public int? CreatedBy { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public virtual Product Product { get; set; }
        public virtual List<RecipeComponent> Components { get; set; }
    }

    public class RecipeComponent
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public int MaterialId { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public decimal? TolerancePct { get; set; }
        public int? LoadOrder { get; set; }

        // Navigation properties
        public virtual RawMaterial RawMaterial { get; set; }
        public virtual Recipe Recipe { get; set; }

        // Добавьте это свойство
        public string MaterialName { get; set; }
    }

    public class TechMap
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public string Name { get; set; }
        public int Version { get; set; }
        public string Status { get; set; }
        public int? EquipmentId { get; set; }
        public int? CreatedBy { get; set; }
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public virtual Recipe Recipe { get; set; }
        public virtual Equipment Equipment { get; set; }
        public virtual List<TechStep> Steps { get; set; }
    }

    public class TechStep
    {
        public int Id { get; set; }
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

        public virtual TechMap TechMap { get; set; }
        public virtual Equipment Equipment { get; set; }
    }

    public class Equipment
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string EquipmentType { get; set; }
        public string Location { get; set; }
        public decimal? MaxCapacity { get; set; }
        public string CapacityUnit { get; set; }
        public string Status { get; set; }
        public DateTime? LastMaintenance { get; set; }
    }

    public class RawMaterial
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public string MaterialType { get; set; }
        public string CasNumber { get; set; }
        public string Specification { get; set; }
        public bool? IsHazardous { get; set; }
    }

    public class RawMaterialBatch
    {
        public int Id { get; set; }
        public int MaterialId { get; set; }
        public int? SupplierId { get; set; }
        public string BatchNumber { get; set; }
        public DateTime ReceiptDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public decimal QuantityKg { get; set; }
        public decimal RemainingQty { get; set; }
        public string Status { get; set; }
        public string CertificateRef { get; set; }

        public virtual RawMaterial RawMaterial { get; set; }
        public virtual Supplier Supplier { get; set; }
    }

    public class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string ContactInfo { get; set; }
        public bool? IsActive { get; set; }
    }

    public class ProductionOrder
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public int? ProductId { get; set; }
        public int? RecipeId { get; set; }
        public int? TechMapId { get; set; }
        public decimal? PlannedQty { get; set; }
        public string QtyUnit { get; set; }
        public DateTime PlannedDate { get; set; }
        public string Status { get; set; }
        public int? Priority { get; set; }
        public int? CreatedBy { get; set; }

        public virtual Product Product { get; set; }
        public virtual Recipe Recipe { get; set; }
        public virtual TechMap TechMap { get; set; }
        public virtual User CreatedByUser { get; set; }
        public virtual List<ProductionBatch> Batches { get; set; }
    }

    public class ProductionBatch
    {
        public int Id { get; set; }
        public int? OrderId { get; set; }
        public string BatchNumber { get; set; }
        public string Status { get; set; }
        public decimal? ActualQty { get; set; }
        public string QtyUnit { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public int? ResponsibleUser { get; set; }
        public string Notes { get; set; }

        public virtual ProductionOrder Order { get; set; }
        public virtual User Responsible { get; set; }
        public virtual List<StepExecution> StepExecutions { get; set; }
        public virtual List<BatchRawMaterial> BatchRawMaterials { get; set; }
        public virtual List<Deviation> Deviations { get; set; }
        public string? ResponsibleName { get; internal set; }
    }

    public class BatchRawMaterial
    {
        public int Id { get; set; }
        public int BatchId { get; set; }
        public int RmBatchId { get; set; }
        public decimal? PlannedQty { get; set; }
        public decimal? ActualQty { get; set; }
        public string Unit { get; set; }
        public DateTime? AddedAt { get; set; }
        public int? AddedBy { get; set; }

        public virtual ProductionBatch Batch { get; set; }
        public virtual RawMaterialBatch RawMaterialBatch { get; set; }
        public virtual User AddedByUser { get; set; }
    }

    public class StepExecution
    {
        public int Id { get; set; }
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

        public virtual ProductionBatch Batch { get; set; }
        public virtual TechStep TechStep { get; set; }
        public virtual Equipment Equipment { get; set; }
        public virtual User StartedByUser { get; set; }
        public virtual User FinishedByUser { get; set; }
        public virtual List<EquipmentTelemetry> Telemetry { get; set; }
        public string? StepName { get; internal set; }
    }

    public class EquipmentTelemetry
    {
        public long Id { get; set; }
        public int EquipmentId { get; set; }
        public int? StepExecId { get; set; }
        public string ParameterName { get; set; }
        public decimal? Value { get; set; }
        public string Unit { get; set; }
        public DateTime? RecordedAt { get; set; }

        public virtual Equipment Equipment { get; set; }
        public virtual StepExecution StepExecution { get; set; }
    }

    public class Deviation
    {
        public int Id { get; set; }
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

        public virtual ProductionBatch Batch { get; set; }
        public virtual StepExecution StepExecution { get; set; }
        public virtual User RegisteredByUser { get; set; }
        public virtual User ResolvedByUser { get; set; }
    }

    public class LabTest
    {
        public int Id { get; set; }
        public string ObjectType { get; set; }
        public int ObjectId { get; set; }
        public string Status { get; set; }
        public int? LabUserId { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Decision { get; set; }
        public string Conclusion { get; set; }

        public virtual User LabUser { get; set; }
        public virtual User CreatedByUser { get; set; }
        public virtual List<LabTestResult> Results { get; set; }
    }

    public class LabTestResult
    {
        public int Id { get; set; }
        public int TestId { get; set; }
        public string ParameterName { get; set; }
        public decimal? MeasuredValue { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string Unit { get; set; }
        public bool? IsWithinRange { get; set; }
        public DateTime? MeasuredAt { get; set; }

        public virtual LabTest LabTest { get; set; }
    }

}