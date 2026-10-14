using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PROJECT_SBV1.Controls;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;

namespace PROJECT_SBV1.Forms
{
    public class LoginForm : Form
    {
        private readonly AuthService _authService = new();
        public User? AuthenticatedUser { get; private set; }

        private TextBox _txtUsername = null!;
        private TextBox _txtPassword = null!;
        private Label _lblError = null!;
        private CheckBox _chkShowPassword = null!;

        public LoginForm()
        {
            Text = "Business Analytics — Sign In";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(880, 530);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.FromArgb(19, 20, 27);
            DoubleBuffered = true;

            BuildUI();
        }

        private void BuildUI()
        {
            // Split Container Layout: Left Banner (340px), Right Form (Remaining)
            var leftPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = 350,
                BackColor = Color.FromArgb(24, 26, 36)
            };
            leftPanel.Paint += LeftPanel_Paint;
            Controls.Add(leftPanel);

            // Left Banner Content
            var brandTitle = new Label
            {
                Text = "Business Analytics",
                Font = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(45, 130)
            };
            leftPanel.Controls.Add(brandTitle);

            var brandSub = new Label
            {
                Text = "Comprehensive analytics platform for monitoring orders, revenues, subscriptions and real-time activity.",
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(150, 155, 195),
                Size = new Size(260, 80),
                Location = new Point(45, 175)
            };
            leftPanel.Controls.Add(brandSub);

            // Left features
            string[] features = { "✔ Live Light & Dark Theme", "✔ Real-time Metrics & Donut Charts", "✔ Sales Dynamics & User Activity", "✔ Visual Studio .NET 8.0 Windows Forms" };
            int featY = 270;
            foreach (var f in features)
            {
                var lblFeat = new Label
                {
                    Text = f,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                    ForeColor = Color.FromArgb(170, 180, 220),
                    AutoSize = true,
                    Location = new Point(45, featY)
                };
                leftPanel.Controls.Add(lblFeat);
                featY += 28;
            }

            // Right Panel (Login form)
            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(19, 20, 27),
                Padding = new Padding(60, 40, 60, 40)
            };
            Controls.Add(rightPanel);

            var heading = new Label
            {
                Text = "Sign In",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(60, 45)
            };
            rightPanel.Controls.Add(heading);

            var subHeading = new Label
            {
                Text = "Enter your username and password to access the dashboard",
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = Color.FromArgb(142, 146, 188),
                AutoSize = true,
                Location = new Point(62, 85)
            };
            rightPanel.Controls.Add(subHeading);

            // Username Label & TextBox
            var lblUser = new Label
            {
                Text = "Username",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(200, 205, 230),
                AutoSize = true,
                Location = new Point(62, 130)
            };
            rightPanel.Controls.Add(lblUser);

            var userContainer = new RoundedPanel
            {
                BorderRadius = 8,
                BorderWidth = 1,
                CustomBackgroundColor = Color.FromArgb(29, 31, 42),
                CustomBorderColor = Color.FromArgb(45, 48, 68),
                Size = new Size(380, 42),
                Location = new Point(62, 155)
            };
            _txtUsername = new TextBox
            {
                Text = "admin",
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(29, 31, 42),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10.5f),
                Size = new Size(350, 24),
                Location = new Point(15, 9)
            };
            userContainer.Controls.Add(_txtUsername);
            rightPanel.Controls.Add(userContainer);

            // Password Label & TextBox
            var lblPass = new Label
            {
                Text = "Password",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(200, 205, 230),
                AutoSize = true,
                Location = new Point(62, 215)
            };
            rightPanel.Controls.Add(lblPass);

            var passContainer = new RoundedPanel
            {
                BorderRadius = 8,
                BorderWidth = 1,
                CustomBackgroundColor = Color.FromArgb(29, 31, 42),
                CustomBorderColor = Color.FromArgb(45, 48, 68),
                Size = new Size(380, 42),
                Location = new Point(62, 240)
            };
            _txtPassword = new TextBox
            {
                Text = "admin123",
                PasswordChar = '•',
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(29, 31, 42),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10.5f),
                Size = new Size(350, 24),
                Location = new Point(15, 9)
            };
            passContainer.Controls.Add(_txtPassword);
            rightPanel.Controls.Add(passContainer);

            // Show password checkbox
            _chkShowPassword = new CheckBox
            {
                Text = "Show Password",
                ForeColor = Color.FromArgb(142, 146, 188),
                Font = new Font("Segoe UI", 8.5f),
                AutoSize = true,
                Location = new Point(64, 290)
            };
            _chkShowPassword.CheckedChanged += (s, e) =>
            {
                _txtPassword.PasswordChar = _chkShowPassword.Checked ? '\0' : '•';
            };
            rightPanel.Controls.Add(_chkShowPassword);

            // Error Label
            _lblError = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(239, 68, 68),
                AutoSize = true,
                Location = new Point(64, 315),
                Visible = false
            };
            rightPanel.Controls.Add(_lblError);

            // Sign In Button
            var btnSignIn = new ModernButton
            {
                Text = "Sign In to Dashboard",
                BorderRadius = 10,
                NormalColor = Color.FromArgb(76, 111, 255),
                HoverColor = Color.FromArgb(100, 130, 255),
                Size = new Size(380, 44),
                Location = new Point(62, 340)
            };
            btnSignIn.Click += BtnSignIn_Click;
            rightPanel.Controls.Add(btnSignIn);

            // Demo Credential Pill
            var demoPill = new RoundedPanel
            {
                BorderRadius = 8,
                BorderWidth = 1,
                CustomBackgroundColor = Color.FromArgb(26, 30, 46),
                CustomBorderColor = Color.FromArgb(40, 50, 80),
                Size = new Size(380, 36),
                Location = new Point(62, 400)
            };
            var lblHint = new Label
            {
                Text = "💡 Demo credentials: admin / admin123 (or Hardik / 1234)",
                Font = new Font("Segoe UI", 8f, FontStyle.Regular),
                ForeColor = Color.FromArgb(140, 170, 245),
                AutoSize = true,
                Location = new Point(15, 9)
            };
            demoPill.Controls.Add(lblHint);
            rightPanel.Controls.Add(demoPill);

            // Allow enter key submission
            _txtUsername.KeyDown += OnKeyDownSubmit;
            _txtPassword.KeyDown += OnKeyDownSubmit;
        }

        private void OnKeyDownSubmit(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                BtnSignIn_Click(this, EventArgs.Empty);
            }
        }

        private void BtnSignIn_Click(object? sender, EventArgs e)
        {
            if (_authService.ValidateCredentials(_txtUsername.Text, _txtPassword.Text, out var user, out string error))
            {
                AuthenticatedUser = user;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                _lblError.Text = error;
                _lblError.Visible = true;
            }
        }

        private void LeftPanel_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw Brand Logo (flower icon)
            int logoX = 45;
            int logoY = 70;
            using (var brush1 = new SolidBrush(Color.FromArgb(140, 82, 255)))
            {
                g.FillEllipse(brush1, logoX, logoY, 18, 18);
                g.FillEllipse(brush1, logoX + 20, logoY + 20, 18, 18);
            }
            using (var brush2 = new SolidBrush(Color.FromArgb(236, 72, 153)))
            {
                g.FillEllipse(brush2, logoX + 20, logoY, 18, 18);
                g.FillEllipse(brush2, logoX, logoY + 20, 18, 18);
            }
        }
    }
}
