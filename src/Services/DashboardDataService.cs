using PROJECT_SBV1.Models;

namespace PROJECT_SBV1.Services
{
    public class DashboardDataService
    {
        public List<CustomerOrder> GetRecentOrders()
        {
            return new List<CustomerOrder>
            {
                new("Press", "London", "22.08.2022", "Delivered", "₹920"),
                new("Marina", "Man city", "24.08.2022", "Processed", "₹452"),
                new("Alex", "Unknown", "15.08.2022", "Cancelled", "₹1,200"),
                new("Robert", "New York", "03.08.2022", "Delivered", "₹1,235"),
                new("Sophia", "Berlin", "28.08.2022", "Delivered", "₹850"),
                new("Liam", "Toronto", "30.08.2022", "Processed", "₹640")
            };
        }

        public (string Month, int Value)[] GetSalesDynamics()
        {
            return new (string Month, int Value)[]
            {
                ("JAN", 120),
                ("FEB", 190),
                ("MAR", 140),
                ("APR", 260),
                ("MAY", 210),
                ("JUN", 340),
                ("JUL", 290),
                ("AUG", 380),
                ("SEP", 240),
                ("OCT", 310),
                ("NOV", 390),
                ("DEC", 270)
            };
        }

        public float[] GetActivityCurve()
        {
            // Monthly activity points (normalized 0.0 - 1.0)
            return new float[] { 0.2f, 0.35f, 0.28f, 0.5f, 0.45f, 0.72f, 0.65f, 0.85f, 0.75f, 0.9f, 0.82f, 0.95f };
        }
    }
}
