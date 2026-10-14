using PROJECT_SBV1.Forms;

namespace PROJECT_SBV1
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Step 1: Launch modern Splash Screen
            using var splash = new SplashScreen();
            if (splash.ShowDialog() != DialogResult.OK)
            {
                return; // User cancelled / closed splash
            }

            // Step 2: Launch Login Screen
            using var login = new LoginForm();
            if (login.ShowDialog() != DialogResult.OK)
            {
                return; // User cancelled / closed login
            }

            // Step 3: Run Main Dashboard with authenticated user session
            Application.Run(new DashboardForm(login.AuthenticatedUser));
        }
    }
}