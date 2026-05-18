// Models/QualityControlObject.cs
namespace WpfApp1.Models
{
    public class QualityControlObject
    {
        public string ObjectType { get; set; }      // raw_material, product
        public int ObjectId { get; set; }
        public string ObjectName { get; set; }
        public string BatchNumber { get; set; }
        public string BatchStatus { get; set; }
        public string TestStatus { get; set; }
        public int? TestId { get; set; }
        public string LastDecision { get; set; }

        // Отображение в UI
        public string DisplayName => $"{ObjectName} - {BatchNumber}";
        public string StatusIcon => GetStatusIcon();
        public string StatusColor => GetStatusColor();

        private string GetStatusIcon()
        {
            if (TestStatus == "completed")
                return LastDecision == "passed" ? "✅" : "❌";
            else if (TestStatus == "in_progress")
                return "🔄";
            else if (TestStatus == "pending")
                return "⏳";
            else if (BatchStatus == "approved")
                return "✅";
            else if (BatchStatus == "rejected")
                return "❌";
            return "⚪";
        }

        private string GetStatusColor()
        {
            if (TestStatus == "completed")
                return LastDecision == "passed" ? "#4CAF50" : "#F44336";
            else if (TestStatus == "in_progress")
                return "#FF9800";
            else if (TestStatus == "pending")
                return "#2196F3";
            else if (BatchStatus == "approved")
                return "#4CAF50";
            else if (BatchStatus == "rejected")
                return "#F44336";
            return "#9E9E9E";
        }
    }
}