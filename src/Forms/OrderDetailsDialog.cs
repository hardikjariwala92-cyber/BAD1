using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PROJECT_SBV1.Controls;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Services;
using PROJECT_SBV1.Theme;

namespace PROJECT_SBV1.Forms
{
    /// <summary>
    /// Interactive Order Details dialog launched when double-clicking any customer order row.
    /// Allows real-time inspection, status updating, and invoice sync.
    /// </summary>
    public class OrderDetailsDialog : Form
    {
        private readonly CustomerOrder _order;
        private readonly Action<CustomerOrder>? _onOrderUpdated;
        private readonly DatabaseService _dbService = DatabaseService.Instance;

        private RoundedComboBox _cmbStatus = null!;
        private Label _lblStatusPill = null!;
        private Color _lblStatusPillTagBg = Color.FromArgb(20, 54, 38);
        private Color _lblStatusPillTagFg = Color.FromArgb(52, 211, 153);

        public OrderDetailsDialog(CustomerOrder order, Action<CustomerOrder>? onOrderUpdated = null)
        {
            _order = order;
            _onOrderUpdated = onOrderUpdated;

            Text = $"Order Details — {order.ProfileName}";
            Size = new Size(680, 530);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = ThemeManager.Background;
            DoubleBuffered = true;

            BuildUI();
        }

        private void BuildUI()
        {
            bool isDark = ThemeManager.CurrentTheme == AppTheme.Dark;

            // 1. Top Customer Header Card
            var pnlHeader = new RoundedPanel
            {
                Location = new Point(20, 16),
                Size = new Size(ClientSize.Width - 40, 105),
                BorderRadius = 14,
                BorderWidth = 1,
                CustomBackgroundColor = ThemeManager.CardBg,
                CustomBorderColor = ThemeManager.CardBorder
            };
            Controls.Add(pnlHeader);

            // Avatar Box
            var pnlAvatar = new Panel
            {
                Location = new Point(18, 18),
                Size = new Size(54, 54),
                BackColor = Color.Transparent
            };
            pnlAvatar.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var rect = new Rectangle(0, 0, 54, 54);
                Color c1 = GetAvatarStartColor(_order.ProfileName);
                Color c2 = GetAvatarEndColor(_order.ProfileName);
                using (var brush = new LinearGradientBrush(rect, c1, c2, LinearGradientMode.ForwardDiagonal))
                {
                    e.Graphics.FillEllipse(brush, rect);
                }
                using (var ringPen = new Pen(Color.FromArgb(60, 255, 255, 255), 1.5f))
                {
                    e.Graphics.DrawEllipse(ringPen, 1, 1, 52, 52);
                }
                string initial = _order.ProfileName.Length > 0 ? _order.ProfileName[0].ToString().ToUpperInvariant() : "?";
                using var font = new Font("Segoe UI", 16f, FontStyle.Bold);
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                e.Graphics.DrawString(initial, font, Brushes.White, rect, sf);
            };
            pnlHeader.Controls.Add(pnlAvatar);

            // Name & Info
            var lblName = new Label
            {
                Text = _order.ProfileName,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(84, 18),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            pnlHeader.Controls.Add(lblName);

            var lblLocation = new Label
            {
                Text = $"📍 {_order.Address}    •    📅 Order Date: {_order.OrderDate}",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(84, 50),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            pnlHeader.Controls.Add(lblLocation);

            // Live Status Pill in Header (Rounded Badge)
            _lblStatusPill = new Label
            {
                Text = _order.Status,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Location = new Point(pnlHeader.Width - 140, 22),
                Size = new Size(115, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            _lblStatusPill.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                var rect = new Rectangle(0, 0, _lblStatusPill.Width - 1, _lblStatusPill.Height - 1);
                using var path = CreateRoundedPath(rect, 12);
                using (var brush = new SolidBrush(_lblStatusPillTagBg))
                {
                    e.Graphics.FillPath(brush, path);
                }
                using (var borderPen = new Pen(Color.FromArgb(50, _lblStatusPillTagFg), 1f))
                {
                    e.Graphics.DrawPath(borderPen, path);
                }
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                using var textBrush = new SolidBrush(_lblStatusPillTagFg);
                e.Graphics.DrawString(_lblStatusPill.Text, _lblStatusPill.Font, textBrush, rect, sf);
            };
            UpdateStatusPillStyle(_lblStatusPill, _order.Status);
            pnlHeader.Controls.Add(_lblStatusPill);

            // 2. Order Breakdown Card
            var pnlBody = new RoundedPanel
            {
                Location = new Point(20, 135),
                Size = new Size(ClientSize.Width - 40, 300),
                BorderRadius = 14,
                BorderWidth = 1,
                CustomBackgroundColor = ThemeManager.CardBg,
                CustomBorderColor = ThemeManager.CardBorder
            };
            Controls.Add(pnlBody);

            var lblSectionTitle = new Label
            {
                Text = "Order Summary & Financials",
                Font = new Font("Segoe UI", 11.5f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(18, 16),
                AutoSize = true,
                BackColor = Color.Transparent,
                UseMnemonic = false
            };
            pnlBody.Controls.Add(lblSectionTitle);

            // Metric Summary Boxes inside Body
            int boxW = (pnlBody.Width - 36 - 20) / 3;
            pnlBody.Controls.Add(CreateSummaryBox("TOTAL AMOUNT", _order.Price, ThemeManager.AccentBlue, 18, 50, boxW, 72));
            pnlBody.Controls.Add(CreateSummaryBox("PAYMENT STATUS", "Settled & Verified", Color.FromArgb(16, 185, 129), 18 + boxW + 10, 50, boxW, 72));
            pnlBody.Controls.Add(CreateSummaryBox("DELIVERY CHANNEL", "Express Air Logistics", Color.FromArgb(245, 158, 11), 18 + (boxW + 10) * 2, 50, boxW, 72));

            // Status Management Editor
            var lblChangeStatus = new Label
            {
                Text = "Update Fulfillment Status:",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                Location = new Point(18, 145),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            pnlBody.Controls.Add(lblChangeStatus);

            _cmbStatus = new RoundedComboBox
            {
                BorderRadius = 12,
                Size = new Size(220, 36),
                Location = new Point(18, 175),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            _cmbStatus.Items.AddRange(new string[] { "Delivered", "Processed", "Cancelled" });
            _cmbStatus.SelectedItem = _order.Status;
            _cmbStatus.ApplyTheme(ThemeManager.CurrentTheme);
            _cmbStatus.SelectedIndexChanged += (s, e) =>
            {
                string sel = _cmbStatus.SelectedItem?.ToString() ?? "Delivered";
                UpdateStatusPillStyle(_lblStatusPill, sel);
            };
            pnlBody.Controls.Add(_cmbStatus);

            var lblStatusHelp = new Label
            {
                Text = "Select a new status and click 'Save Status' to immediately synchronize with SQLite.",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(18, 225),
                AutoSize = true,
                BackColor = Color.Transparent
            };
            pnlBody.Controls.Add(lblStatusHelp);

            // 3. Bottom Action Buttons
            var btnSave = new ModernButton
            {
                Text = "💾 Save Status",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Size = new Size(130, 38),
                Location = new Point(ClientSize.Width - 20 - 130, ClientSize.Height - 52),
                NormalColor = ThemeManager.AccentBlue,
                HoverColor = Color.FromArgb(90, 130, 255),
                BorderRadius = 10,
                UseGradient = true,
                GradientColorStart = Color.FromArgb(79, 70, 229),
                GradientColorEnd = Color.FromArgb(99, 102, 241),
                HoverGradientColorStart = Color.FromArgb(99, 102, 241),
                HoverGradientColorEnd = Color.FromArgb(129, 140, 248),
                HasGlowOnHover = true,
                GlowColor = Color.FromArgb(99, 102, 241)
            };
            btnSave.Click += async (s, e) =>
            {
                string newStatus = _cmbStatus.SelectedItem?.ToString() ?? _order.Status;
                _order.Status = newStatus;
                await _dbService.UpdateOrderStatusAsync(_order.ProfileName, newStatus);
                _onOrderUpdated?.Invoke(_order);
                MessageBox.Show(this, $"Order status for {_order.ProfileName} updated to '{newStatus}' successfully!", "Status Synchronized", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Close();
            };
            Controls.Add(btnSave);

            var btnClose = new ModernButton
            {
                Text = "Close",
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Size = new Size(90, 38),
                Location = new Point(ClientSize.Width - 20 - 130 - 100, ClientSize.Height - 52),
                BorderRadius = 10,
                IsOutlined = !isDark,
                BorderOutlineColor = isDark ? Color.FromArgb(50, 54, 75) : Color.FromArgb(203, 213, 225),
                NormalColor = isDark ? Color.FromArgb(35, 38, 54) : Color.FromArgb(241, 245, 249),
                HoverColor = isDark ? Color.FromArgb(50, 54, 75) : Color.FromArgb(226, 232, 240),
                PressedColor = isDark ? Color.FromArgb(60, 65, 90) : Color.FromArgb(203, 213, 225),
                ForeColor = isDark ? Color.White : Color.FromArgb(51, 65, 85),
                TextHoverColor = isDark ? Color.White : Color.FromArgb(15, 23, 42)
            };
            btnClose.Click += (s, e) => Close();
            Controls.Add(btnClose);
        }

        private Control CreateSummaryBox(string label, string value, Color accent, int x, int y, int w, int h)
        {
            bool isDark = ThemeManager.CurrentTheme == AppTheme.Dark;
            Color boxBg = isDark ? Color.FromArgb(24, 26, 38) : Color.FromArgb(248, 250, 252);
            Color boxBorder = isDark ? Color.FromArgb(40, 44, 65) : Color.FromArgb(226, 232, 240);
            Color labelColor = isDark ? ThemeManager.TextSecondary : Color.FromArgb(100, 116, 139);

            var pnl = new RoundedPanel
            {
                Location = new Point(x, y),
                Size = new Size(w, h),
                BorderRadius = 10,
                BorderWidth = 1,
                CustomBackgroundColor = boxBg,
                CustomBorderColor = boxBorder
            };

            var lblT = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = labelColor,
                Location = new Point(12, 10),
                AutoSize = true,
                BackColor = Color.Transparent,
                UseMnemonic = false
            };
            pnl.Controls.Add(lblT);

            // Dynamic font size: scale appropriately so long text like "Express Air Logistics" never clips
            float fontSize = value.Length > 16 ? 10f : 11f;
            var lblV = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", fontSize, FontStyle.Bold),
                ForeColor = accent,
                Location = new Point(12, 32),
                AutoSize = true,
                BackColor = Color.Transparent,
                UseMnemonic = false
            };
            pnl.Controls.Add(lblV);

            return pnl;
        }

        private void UpdateStatusPillStyle(Label pill, string status)
        {
            pill.Text = status;
            bool isDark = ThemeManager.CurrentTheme == AppTheme.Dark;
            if (status == "Delivered")
            {
                _lblStatusPillTagBg = isDark ? Color.FromArgb(20, 54, 38) : Color.FromArgb(209, 250, 229);
                _lblStatusPillTagFg = isDark ? Color.FromArgb(52, 211, 153) : Color.FromArgb(5, 150, 105);
            }
            else if (status == "Processed")
            {
                _lblStatusPillTagBg = isDark ? Color.FromArgb(54, 42, 16) : Color.FromArgb(254, 243, 199);
                _lblStatusPillTagFg = isDark ? Color.FromArgb(251, 191, 36) : Color.FromArgb(217, 119, 6);
            }
            else if (status == "Cancelled")
            {
                _lblStatusPillTagBg = isDark ? Color.FromArgb(54, 22, 26) : Color.FromArgb(254, 226, 226);
                _lblStatusPillTagFg = isDark ? Color.FromArgb(248, 113, 113) : Color.FromArgb(220, 38, 38);
            }
            else
            {
                _lblStatusPillTagBg = isDark ? Color.FromArgb(35, 38, 54) : Color.FromArgb(241, 245, 249);
                _lblStatusPillTagFg = isDark ? Color.White : Color.FromArgb(71, 85, 105);
            }
            pill.Invalidate();
        }

        private static GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int diameter = radius * 2;
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        private static Color GetAvatarStartColor(string name)
        {
            int hash = Math.Abs(name.GetHashCode());
            Color[] colors = {
                Color.FromArgb(236, 72, 153), // Pink
                Color.FromArgb(14, 165, 233), // Sky
                Color.FromArgb(16, 185, 129), // Emerald
                Color.FromArgb(168, 85, 247), // Purple
                Color.FromArgb(249, 115, 22), // Orange
                Color.FromArgb(99, 102, 241)  // Indigo
            };
            return colors[hash % colors.Length];
        }

        private static Color GetAvatarEndColor(string name)
        {
            int hash = Math.Abs(name.GetHashCode());
            Color[] colors = {
                Color.FromArgb(219, 39, 119), // Rose Pink
                Color.FromArgb(2, 132, 199),  // Deep Sky
                Color.FromArgb(5, 150, 105),  // Deep Emerald
                Color.FromArgb(147, 51, 234), // Deep Purple
                Color.FromArgb(234, 88, 12),  // Deep Orange
                Color.FromArgb(79, 70, 229)   // Deep Indigo
            };
            return colors[hash % colors.Length];
        }
    }
}
