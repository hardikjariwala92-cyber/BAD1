using System.Drawing;
using PROJECT_SBV1.Controls;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;
using Xunit;

namespace PROJECT_SBV1.Tests
{
    public class DynamicDashboardHoverTests
    {
        [Fact]
        public void TestMetricDetailInfo_ModelProperties()
        {
            var info = new MetricDetailInfo
            {
                Title = "Customer Orders Performance",
                Icon = "📦",
                CurrentValue = "201 Orders",
                PreviousValue = "185 Orders",
                GrowthText = "↑ 8.2% since last month",
                IsPositiveGrowth = true,
                Category = "Sales & Fulfillment",
                Stats = new List<(string Label, string Value)>
                {
                    ("Delivered Orders", "164"),
                    ("Processed in Transit", "28")
                }
            };

            Assert.Equal("Customer Orders Performance", info.Title);
            Assert.Equal("📦", info.Icon);
            Assert.Equal("201 Orders", info.CurrentValue);
            Assert.Equal("185 Orders", info.PreviousValue);
            Assert.True(info.IsPositiveGrowth);
            Assert.Equal(2, info.Stats.Count);
        }

        [Fact]
        public void TestBarChartControl_SetDataAndProperties()
        {
            var chart = new BarChartControl();
            var data = new (string Label, int Value)[]
            {
                ("JAN", 120), ("FEB", 190), ("MAR", 140), ("APR", 260)
            };

            chart.SetData(data, 500);

            Assert.Equal(4, chart.DataPoints.Length);
            Assert.Equal(500, chart.MaxValue);
            Assert.Equal("JAN", chart.DataPoints[0].Label);
            Assert.Equal(120, chart.DataPoints[0].Value);
        }

        [Fact]
        public void TestSplineChartControl_SetDataAndPoints()
        {
            var chart = new SplineChartControl();
            float[] curve = { 0.2f, 0.4f, 0.6f, 0.8f, 1.0f };

            chart.SetData(curve, 500);

            Assert.Equal(5, chart.NormalizedPoints.Length);
            Assert.Equal(500, chart.MaxValue);
            Assert.Equal(0.2f, chart.NormalizedPoints[0]);
            Assert.Equal(1.0f, chart.NormalizedPoints[4]);
        }

        [Fact]
        public void TestDonutChartControl_SlicesAndSetSlices()
        {
            var donut = new DonutChartControl();
            var slices = new List<DonutSlice>
            {
                new("Paid", 70f, Color.Blue),
                new("Trial", 30f, Color.Cyan)
            };

            donut.SetSlices(slices);

            Assert.Equal(2, donut.Slices.Count);
            Assert.Equal("Paid", donut.Slices[0].Label);
            Assert.Equal(70f, donut.Slices[0].Percentage);
            Assert.Equal("Trial", donut.Slices[1].Label);
            Assert.Equal(30f, donut.Slices[1].Percentage);
        }

        [Fact]
        public void TestRoundedPanel_HoverAndMetricInfoSupport()
        {
            var panel = new RoundedPanel
            {
                EnableHoverEffect = true,
                MetricInfo = new MetricDetailInfo
                {
                    Title = "Revenue Card",
                    CurrentValue = "$1,352"
                }
            };

            Assert.True(panel.EnableHoverEffect);
            Assert.NotNull(panel.MetricInfo);
            Assert.Equal("Revenue Card", panel.MetricInfo.Title);
        }

        [Fact]
        public void TestDatabaseService_DynamicSalesDynamics_MultiYear()
        {
            var db = DatabaseService.Instance;
            var points2021 = db.GetSalesDynamics(2021);
            var points2022 = db.GetSalesDynamics(2022);

            Assert.NotNull(points2021);
            Assert.Equal(12, points2021.Length);
            Assert.NotNull(points2022);
            Assert.Equal(12, points2022.Length);

            // 2022 values differ from 2021
            Assert.NotEqual(points2021[0].Value, points2022[0].Value);
        }

        [Fact]
        public void TestDatabaseService_DynamicActivityCurve_MultiYear()
        {
            var db = DatabaseService.Instance;
            var curve2021 = db.GetActivityCurve(2021);
            var curve2022 = db.GetActivityCurve(2022);

            Assert.NotNull(curve2021);
            Assert.Equal(12, curve2021.Length);
            Assert.NotNull(curve2022);
            Assert.Equal(12, curve2022.Length);
        }
    }
}
