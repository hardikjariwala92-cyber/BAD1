using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using PROJECT_SBV1.Controls;
using PROJECT_SBV1.Models;
using PROJECT_SBV1.Theme;

namespace PROJECT_SBV1.Forms
{
    /// <summary>
    /// Premium, modern success notification dialog displayed after an order is saved.
    /// Replaces generic system MessageBox with responsive SaaS aesthetics and full theme support.
    /// </summary>
    public class OrderSuccessDialog : Form
    {
        private readonly CustomerOrder _order;

        public OrderSuccessDialog(CustomerOrder order)
        {
            _order = order;

            ClientSize = new Size(460, 420);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Order Saved Successfully";
            DoubleBuffered = true;
            ShowIcon = false;
            ShowInTaskbar = false;
            BackColor = ThemeManager.Background;

            BuildUI();
        }

        private void BuildUI()
        {
            bool isDark = ThemeManager.CurrentTheme == AppTheme.Dark;

            // 1. Main Container Card
            var pnlContainer = new RoundedPanel
            {
                Location = new Point(18, 16),
                Size = new Size(ClientSize.Width - 36, ClientSize.Height - 32),
                BorderRadius = 14,
                BorderWidth = 1,
                CustomBackgroundColor = ThemeManager.CardBg,
                CustomBorderColor = ThemeManager.CardBorder
            };
            Controls.Add(pnlContainer);

            // 2. Animated / Gradient Success Circle Badge
            var pnlCheckBadge = new Panel
            {
                Location = new Point((pnlContainer.Width - 56) / 2, 20),
                Size = new Size(56, 56),
                BackColor = Color.Transparent
            };
            pnlCheckBadge.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                var rect = new Rectangle(0, 0, 55, 55);
                using (var brush = new LinearGradientBrush(rect, Color.FromArgb(16, 185, 129), Color.FromArgb(5, 150, 105), LinearGradientMode.ForwardDiagonal))
                {
                    g.FillEllipse(brush, rect);
                }

                using (var pen = new Pen(Color.FromArgb(80, 255, 255, 255), 2f))
                {
                    g.DrawEllipse(pen, 2, 2, 51, 51);
                }

                // White Checkmark Glyph
                using var font = new Font("Segoe UI", 20f, FontStyle.Bold);
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("✓", font, Brushes.White, rect, sf);
            };
            pnlContainer.Controls.Add(pnlCheckBadge);

            // 3. Title & Subtitle
            var lblTitle = new Label
            {
                Text = "Order Saved Successfully!",
                Font = new Font("Segoe UI", 13.5f, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(16, 84),
                Size = new Size(pnlContainer.Width - 32, 26)
            };
            pnlContainer.Controls.Add(lblTitle);

            var lblSubtitle = new Label
            {
                Text = "Order details have been verified and added to live analytics.",
                Font = new Font("Segoe UI", 9f),
                ForeColor = ThemeManager.TextSecondary,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(16, 112),
                Size = new Size(pnlContainer.Width - 32, 20)
            };
            pnlContainer.Controls.Add(lblSubtitle);

            // 4. Receipt Details Card
            var pnlReceipt = new RoundedPanel
            {
                Location = new Point(20, 140),
                Size = new Size(pnlContainer.Width - 40, 180),
                BorderRadius = 10,
                BorderWidth = 1,
                CustomBackgroundColor = isDark ? Color.FromArgb(24, 26, 38) : Color.FromArgb(248, 250, 252),
                CustomBorderColor = ThemeManager.CardBorder
            };
            pnlContainer.Controls.Add(pnlReceipt);

            int startY = 14;
            int rowHeight = 32;

            AddReceiptRow(pnlReceipt, "Customer", _order.ProfileName, ThemeManager.TextPrimary, startY, true);
            AddReceiptRow(pnlReceipt, "Delivery City", _order.Address, ThemeManager.TextSecondary, startY + rowHeight, false);
            AddReceiptRow(pnlReceipt, "Order Amount", _order.Price, Color.FromArgb(16, 185, 129), startY + rowHeight * 2, true);
            AddReceiptRow(pnlReceipt, "Order Status", _order.Status, GetStatusColor(_order.Status), startY + rowHeight * 3, true);
            AddReceiptRow(pnlReceipt, "Order Date", _order.OrderDate, ThemeManager.TextSecondary, startY + rowHeight * 4, false);

            // 5. Action Button
            var btnDone = new ModernButton
            {
                Text = "✓  View in Dashboard",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Location = new Point(20, 332),
                Size = new Size(pnlContainer.Width - 40, 40),
                NormalColor = ThemeManager.AccentBlue,
                HoverColor = Color.FromArgb(98, 133, 255),
                PressedColor = Color.FromArgb(60, 95, 220),
                ForeColor = Color.White,
                BorderRadius = 8
            };
            btnDone.Click += (s, e) =>
            {
                DialogResult = DialogResult.OK;
                Close();
            };
            pnlContainer.Controls.Add(btnDone);

            AcceptButton = btnDone;
        }

        private void AddReceiptRow(Panel container, string label, string value, Color valueColor, int y, bool isBold)
        {
            var lblName = new Label
            {
                Text = label,
                Font = new Font("Segoe UI", 8.8f),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(14, y),
                Size = new Size(130, 24),
                TextAlign = ContentAlignment.MiddleLeft
            };
            container.Controls.Add(lblName);

            var lblVal = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 9.5f, isBold ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = valueColor,
                Location = new Point(150, y),
                Size = new Size(container.Width - 164, 24),
                TextAlign = ContentAlignment.MiddleRight,
                AutoEllipsis = true
            };
            container.Controls.Add(lblVal);
        }

        private static Color GetStatusColor(string status)
        {
            return status?.Trim().ToLowerInvariant() switch
            {
                "delivered" => Color.FromArgb(16, 185, 129),
                "processed" => Color.FromArgb(245, 158, 11),
                "cancelled" => Color.FromArgb(239, 68, 68),
                _ => ThemeManager.AccentBlue
            };
        }
    }
}
