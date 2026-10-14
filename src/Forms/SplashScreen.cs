using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PROJECT_SBV1.Controls;

namespace PROJECT_SBV1.Forms
{
    public class SplashScreen : Form
    {
        private readonly System.Windows.Forms.Timer _progressTimer = new();
        private int _progress = 0;
        private readonly Label _statusLabel = new();
        private readonly Label _percentLabel = new();

        public SplashScreen()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(620, 360);
            BackColor = Color.FromArgb(19, 20, 27);
            DoubleBuffered = true;
            ShowInTaskbar = false;

            BuildUI();

            _progressTimer.Interval = 35;
            _progressTimer.Tick += ProgressTimer_Tick;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            _progressTimer.Start();
        }

        private void BuildUI()
        {
            // App Title
            var titleLabel = new Label
            {
                Text = "Business Analytics",
                Font = new Font("Segoe UI", 22f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(140, 95)
            };
            Controls.Add(titleLabel);

            // Subtitle
            var subLabel = new Label
            {
                Text = "S TOM'S Enterprise Management System • .NET 8.0",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(142, 146, 188),
                AutoSize = true,
                Location = new Point(142, 142)
            };
            Controls.Add(subLabel);

            // Status label
            _statusLabel.Text = "Initializing application core...";
            _statusLabel.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
            _statusLabel.ForeColor = Color.FromArgb(160, 165, 205);
            _statusLabel.AutoSize = true;
            _statusLabel.Location = new Point(50, 260);
            Controls.Add(_statusLabel);

            // Percent label
            _percentLabel.Text = "0%";
            _percentLabel.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            _percentLabel.ForeColor = Color.FromArgb(76, 111, 255);
            _percentLabel.AutoSize = true;
            _percentLabel.Location = new Point(540, 260);
            Controls.Add(_percentLabel);
        }

        private void ProgressTimer_Tick(object? sender, EventArgs e)
        {
            _progress += 2;
            if (_progress > 100) _progress = 100;

            _percentLabel.Text = $"{_progress}%";

            if (_progress < 25)
                _statusLabel.Text = "Loading application core & theme engine...";
            else if (_progress < 50)
                _statusLabel.Text = "Loading security services & authentication...";
            else if (_progress < 75)
                _statusLabel.Text = "Preparing Business Analytics dashboard...";
            else if (_progress < 95)
                _statusLabel.Text = "Setting up interactive charts & data views...";
            else
                _statusLabel.Text = "Ready! Launching system...";

            Invalidate();

            if (_progress >= 100)
            {
                _progressTimer.Stop();
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Form border
            using (var borderPen = new Pen(Color.FromArgb(45, 48, 68), 1.5f))
            {
                g.DrawRectangle(borderPen, 0, 0, Width - 1, Height - 1);
            }

            // Draw glowing app logo (Stylized 4-petal cross)
            int logoX = 60;
            int logoY = 95;
            using (var brush1 = new SolidBrush(Color.FromArgb(140, 82, 255))) // Purple
            {
                g.FillEllipse(brush1, logoX, logoY, 24, 24);
                g.FillEllipse(brush1, logoX + 26, logoY + 26, 24, 24);
            }
            using (var brush2 = new SolidBrush(Color.FromArgb(236, 72, 153))) // Pink
            {
                g.FillEllipse(brush2, logoX + 26, logoY, 24, 24);
                g.FillEllipse(brush2, logoX, logoY + 26, 24, 24);
            }

            // Progress Bar Track
            int barX = 50;
            int barY = 285;
            int barWidth = Width - 100;
            int barHeight = 8;

            using (var trackBrush = new SolidBrush(Color.FromArgb(32, 34, 48)))
            {
                g.FillRectangle(trackBrush, barX, barY, barWidth, barHeight);
            }

            // Progress Fill
            if (_progress > 0)
            {
                int fillWidth = (barWidth * _progress) / 100;
                using var fillBrush = new LinearGradientBrush(
                    new Point(barX, barY),
                    new Point(barX + barWidth, barY),
                    Color.FromArgb(76, 111, 255),
                    Color.FromArgb(140, 82, 255)
                );
                g.FillRectangle(fillBrush, barX, barY, fillWidth, barHeight);
            }
        }
    }
}
