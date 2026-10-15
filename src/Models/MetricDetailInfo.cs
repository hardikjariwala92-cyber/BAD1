namespace PROJECT_SBV1.Models
{
    public class MetricDetailInfo
    {
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string CurrentValue { get; set; } = string.Empty;
        public string PreviousValue { get; set; } = string.Empty;
        public string GrowthText { get; set; } = string.Empty;
        public bool IsPositiveGrowth { get; set; } = true;
        public string Category { get; set; } = "Metric Overview";
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public List<(string Label, string Value)> Stats { get; set; } = new();
        public string FooterTip { get; set; } = "Hover for quick summary • Click for full details";
    }
}
