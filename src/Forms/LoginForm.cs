using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;

namespace PROJECT_SBV1.Forms
{
    public partial class LoginForm : Form
    {
        private readonly AuthService _authService = new();
        public User? AuthenticatedUser { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
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

            // Draw Brand Logo (Stylized 4-petal flower icon)
            int logoX = 35;
            int logoY = 30;
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
