using System;
using System.Windows.Forms;
using PROJECT_SBV1.Forms;
using PROJECT_SBV1.Models;

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
            using (var splash = new SplashScreen())
            {
                if (splash.ShowDialog() != DialogResult.OK)
                {
                    return; // User cancelled / closed splash
                }
            }

            // Step 2: Run Application Session Context managing Login <-> Dashboard transitions
            Application.Run(new AppSessionContext());
        }
    }

    /// <summary>
    /// Application context managing seamless transitions between LoginForm and DashboardForm upon Sign Out.
    /// </summary>
    internal class AppSessionContext : ApplicationContext
    {
        public AppSessionContext()
        {
            ShowLogin();
        }

        public void ShowLogin(string? prefillUsername = null)
        {
            var login = new LoginForm();
            if (!string.IsNullOrEmpty(prefillUsername))
            {
                login.SetUsername(prefillUsername);
            }

            login.StartPosition = FormStartPosition.CenterScreen;

            login.FormClosed += (s, e) =>
            {
                if (login.DialogResult == DialogResult.OK && login.AuthenticatedUser != null)
                {
                    ShowDashboard(login.AuthenticatedUser);
                }
                else
                {
                    ExitThread();
                }
            };

            login.Show();
        }

        public void ShowDashboard(User user)
        {
            var dashboard = new DashboardForm(user);

            dashboard.FormClosed += (s, e) =>
            {
                if (dashboard.IsSignedOut)
                {
                    ShowLogin(user.Username);
                }
                else
                {
                    ExitThread();
                }
            };

            dashboard.WindowState = FormWindowState.Maximized;
            dashboard.Show();
            dashboard.Activate();
            dashboard.BringToFront();
        }
    }
}