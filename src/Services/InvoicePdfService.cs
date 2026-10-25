using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PROJECT_SBV1.Models;

namespace PROJECT_SBV1.Services
{
    public class InvoicePdfService
    {
        private static InvoicePdfService? _instance;
        public static InvoicePdfService Instance => _instance ??= new InvoicePdfService();

        public string GenerateInvoicePdf(Invoice invoice, BusinessProfile profile, string? targetPath = null)
        {
            if (string.IsNullOrWhiteSpace(targetPath))
            {
                string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "STOMS_Invoices");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                string safeInvNo = string.Join("_", invoice.InvoiceNumber.Split(Path.GetInvalidFileNameChars()));
                targetPath = Path.Combine(folder, $"{safeInvNo}_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }

            var doc = new PdfDocument();
            doc.Info.Title = $"Invoice {invoice.InvoiceNumber}";
            doc.Info.Author = profile.BusinessName;
            doc.Info.Subject = $"Billing for {invoice.CustomerName}";

            var page = doc.AddPage();
            page.Size = PdfSharp.PageSize.A4;
            page.Orientation = PdfSharp.PageOrientation.Portrait;

            using (var gfx = XGraphics.FromPdfPage(page))
            {
                DrawInvoicePage(gfx, invoice, profile, page.Width.Point, page.Height.Point);
            }

            doc.Save(targetPath);
            return targetPath;
        }

        public void OpenPdf(string filePath)
        {
            if (File.Exists(filePath))
            {
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
            }
        }

        public void PrintPdf(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    var psi = new ProcessStartInfo(filePath)
                    {
                        Verb = "print",
                        UseShellExecute = true,
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };
                    Process.Start(psi);
                }
                catch
                {
                    // Fallback to opening default PDF viewer for user to print
                    OpenPdf(filePath);
                }
            }
        }

        private void DrawInvoicePage(XGraphics gfx, Invoice invoice, BusinessProfile profile, double pageWidth, double pageHeight)
        {
            double margin = 36;
            double contentWidth = pageWidth - (margin * 2);
            double y = margin;

            // Palette
            var primaryColor = XColor.FromArgb(30, 41, 59);    // Slate 800
            XColor accentColor;
            try
            {
                var ac = ColorTranslator.FromHtml(profile.AccentColorHex);
                accentColor = XColor.FromArgb(ac.R, ac.G, ac.B);
            }
            catch
            {
                accentColor = XColor.FromArgb(59, 130, 246);   // Blue 500 default
            }

            var textDark = XColor.FromArgb(15, 23, 42);       // Slate 900
            var textMuted = XColor.FromArgb(100, 116, 139);   // Slate 500
            var borderLight = XColor.FromArgb(226, 232, 240); // Slate 200
            var headerBg = XColor.FromArgb(248, 250, 252);    // Slate 50

            var fontTitle = new XFont("Arial", 20, XFontStyleEx.Bold);
            var fontSub = new XFont("Arial", 8.5, XFontStyleEx.Regular);
            var fontBold = new XFont("Arial", 9.5, XFontStyleEx.Bold);
            var fontRegular = new XFont("Arial", 9, XFontStyleEx.Regular);
            var fontSmall = new XFont("Arial", 8, XFontStyleEx.Regular);
            var fontSmallBold = new XFont("Arial", 8, XFontStyleEx.Bold);

            // 1. TOP HEADER: Business Logo (if available) & Business Name on Left, "INVOICE" on Right
            double headerLeftX = margin;
            if (!string.IsNullOrWhiteSpace(profile.LogoPath) && File.Exists(profile.LogoPath))
            {
                try
                {
                    using var xImg = XImage.FromFile(profile.LogoPath);
                    gfx.DrawImage(xImg, margin, y + 2, 40, 40);
                    headerLeftX += 48;
                }
                catch { }
            }

            gfx.DrawString(profile.BusinessName, fontTitle, new XSolidBrush(primaryColor), new XPoint(headerLeftX, y + 18));
            gfx.DrawString(profile.Tagline, fontSub, new XSolidBrush(accentColor), new XPoint(headerLeftX, y + 33));

            // Right Header: INVOICE label
            var invTitleFont = new XFont("Arial", 24, XFontStyleEx.Bold);
            string invTitle = "TAX INVOICE";
            double titleWidth = gfx.MeasureString(invTitle, invTitleFont).Width;
            gfx.DrawString(invTitle, invTitleFont, new XSolidBrush(accentColor), new XPoint(pageWidth - margin - titleWidth, y + 18));

            // Status Badge
            string status = invoice.Status.ToUpperInvariant();
            XColor badgeBg = status switch
            {
                "PAID" => XColor.FromArgb(220, 252, 231),
                "PARTIALLY PAID" => XColor.FromArgb(254, 243, 199),
                "OVERDUE" => XColor.FromArgb(254, 226, 226),
                "CANCELLED" => XColor.FromArgb(241, 245, 249),
                _ => XColor.FromArgb(254, 240, 220)
            };
            XColor badgeText = status switch
            {
                "PAID" => XColor.FromArgb(22, 101, 52),
                "PARTIALLY PAID" => XColor.FromArgb(146, 64, 14),
                "OVERDUE" => XColor.FromArgb(153, 27, 27),
                "CANCELLED" => XColor.FromArgb(100, 116, 139),
                _ => XColor.FromArgb(154, 52, 18)
            };

            double badgeW = 85;
            double badgeH = 18;
            double badgeX = pageWidth - margin - badgeW;
            double badgeY = y + 26;
            gfx.DrawRoundedRectangle(new XPen(badgeText, 0.5), new XSolidBrush(badgeBg), badgeX, badgeY, badgeW, badgeH, 4, 4);
            var sfCenter = new XStringFormat { Alignment = XStringAlignment.Center, LineAlignment = XLineAlignment.Center };
            gfx.DrawString(status, fontSmallBold, new XSolidBrush(badgeText), new XRect(badgeX, badgeY, badgeW, badgeH), sfCenter);

            y += 48;

            // Divider
            gfx.DrawLine(new XPen(borderLight, 1), margin, y, pageWidth - margin, y);
            y += 12;

            // 2. METADATA: Seller Details (Left) vs Invoice Details (Right)
            double metaSectionY = y;
            double colW = contentWidth / 2 - 10;

            // Left: Seller Info
            gfx.DrawString("BILLED FROM:", fontSmallBold, new XSolidBrush(textMuted), new XPoint(margin, metaSectionY + 4));
            double sellerY = metaSectionY + 16;
            gfx.DrawString(profile.BusinessName, fontBold, new XSolidBrush(textDark), new XPoint(margin, sellerY)); sellerY += 12;

            string addr1 = string.IsNullOrWhiteSpace(profile.Address) ? "Business Address Not Set" : profile.Address;
            if (addr1.Length > 36) addr1 = addr1[..34] + "…";
            gfx.DrawString(addr1, fontRegular, new XSolidBrush(textMuted), new XPoint(margin, sellerY)); sellerY += 12;

            string addr2 = $"{profile.City}{(string.IsNullOrWhiteSpace(profile.Country) ? "" : ", " + profile.Country)}".Trim(',', ' ');
            if (!string.IsNullOrWhiteSpace(addr2))
            {
                if (addr2.Length > 36) addr2 = addr2[..34] + "…";
                gfx.DrawString(addr2, fontRegular, new XSolidBrush(textMuted), new XPoint(margin, sellerY)); sellerY += 12;
            }

            string contact = $"Phone: {profile.Phone} | Email: {profile.Email}";
            if (contact.Length > 36) contact = contact[..34] + "…";
            gfx.DrawString(contact, fontRegular, new XSolidBrush(textMuted), new XPoint(margin, sellerY)); sellerY += 12;

            if (!string.IsNullOrWhiteSpace(profile.GSTIN))
            {
                gfx.DrawString($"GSTIN: {profile.GSTIN}", fontBold, new XSolidBrush(primaryColor), new XPoint(margin, sellerY));
                sellerY += 12;
            }

            // Right: Invoice Info Box
            double rightColX = pageWidth - margin - colW;
            gfx.DrawRoundedRectangle(new XPen(borderLight, 1), new XSolidBrush(headerBg), rightColX, metaSectionY, colW, 68, 6, 6);

            gfx.DrawString($"Invoice No: {invoice.InvoiceNumber}", fontBold, new XSolidBrush(primaryColor), new XPoint(rightColX + 10, metaSectionY + 16));
            gfx.DrawString($"Invoice Date: {invoice.InvoiceDate:dd MMM yyyy}", fontRegular, new XSolidBrush(textDark), new XPoint(rightColX + 10, metaSectionY + 34));
            gfx.DrawString($"Due Date: {invoice.DueDate:dd MMM yyyy} ({invoice.PaymentTerms})", fontRegular, new XSolidBrush(textDark), new XPoint(rightColX + 10, metaSectionY + 50));

            y = Math.Max(sellerY, metaSectionY + 70) + 8;

            // 3. BILL TO (Customer Details)
            gfx.DrawRoundedRectangle(new XPen(borderLight, 1), new XSolidBrush(headerBg), margin, y, contentWidth, 48, 6, 6);
            gfx.DrawString("BILL TO / CLIENT DETAILS:", fontSmallBold, new XSolidBrush(textMuted), new XPoint(margin + 10, y + 13));

            string clientLine1 = string.IsNullOrWhiteSpace(invoice.CompanyName) 
                ? invoice.CustomerName 
                : $"{invoice.CustomerName} ({invoice.CompanyName})";
            if (clientLine1.Length > 40) clientLine1 = clientLine1[..38] + "…";
            gfx.DrawString(clientLine1, fontBold, new XSolidBrush(textDark), new XPoint(margin + 10, y + 26));

            string clientLine2 = $"{invoice.BillingAddress}  |  Phone: {invoice.CustomerPhone}  |  Email: {invoice.CustomerEmail}";
            if (clientLine2.Length > 75) clientLine2 = clientLine2[..73] + "…";
            gfx.DrawString(clientLine2, fontRegular, new XSolidBrush(textMuted), new XPoint(margin + 10, y + 39));

            if (!string.IsNullOrWhiteSpace(invoice.CustomerGSTIN))
            {
                gfx.DrawString($"GSTIN: {invoice.CustomerGSTIN}", fontSmallBold, new XSolidBrush(primaryColor), new XPoint(pageWidth - margin - 145, y + 26));
            }

            y += 56;

            // 4. ITEMS TABLE
            double tableY = y;
            double colItem = 145;
            double colDesc = 130;
            double colQty = 35;
            double colRate = 65;
            double colDisc = 38;
            double colTax = 38;
            double colTotal = 67;

            // Dynamic row height to guarantee 1-page fit
            double rowHeight = invoice.Items.Count > 10 ? 16 : (invoice.Items.Count > 6 ? 18 : 20);

            // Table Header Bar
            gfx.DrawRoundedRectangle(new XSolidBrush(primaryColor), margin, tableY, contentWidth, 22, 4, 4);

            double curX = margin + 8;
            gfx.DrawString("Service / Product", fontSmallBold, XBrushes.White, new XPoint(curX, tableY + 15)); curX += colItem;
            gfx.DrawString("Description", fontSmallBold, XBrushes.White, new XPoint(curX, tableY + 15)); curX += colDesc;

            string hQty = "Qty";
            gfx.DrawString(hQty, fontSmallBold, XBrushes.White, new XPoint(curX + colQty - 6 - gfx.MeasureString(hQty, fontSmallBold).Width, tableY + 15)); curX += colQty;

            string hRate = "Rate";
            gfx.DrawString(hRate, fontSmallBold, XBrushes.White, new XPoint(curX + colRate - 6 - gfx.MeasureString(hRate, fontSmallBold).Width, tableY + 15)); curX += colRate;

            string hDisc = "Disc%";
            gfx.DrawString(hDisc, fontSmallBold, XBrushes.White, new XPoint(curX + colDisc - 6 - gfx.MeasureString(hDisc, fontSmallBold).Width, tableY + 15)); curX += colDisc;

            string hTax = "Tax%";
            gfx.DrawString(hTax, fontSmallBold, XBrushes.White, new XPoint(curX + colTax - 6 - gfx.MeasureString(hTax, fontSmallBold).Width, tableY + 15)); curX += colTax;

            string hTotal = "Total";
            gfx.DrawString(hTotal, fontSmallBold, XBrushes.White, new XPoint(curX + colTotal - 8 - gfx.MeasureString(hTotal, fontSmallBold).Width, tableY + 15));

            tableY += 24;

            // Table Rows
            int index = 0;
            foreach (var item in invoice.Items)
            {
                XColor rowBg = (index % 2 == 0) ? XColor.FromArgb(255, 255, 255) : XColor.FromArgb(248, 250, 252);
                gfx.DrawRectangle(new XSolidBrush(rowBg), margin, tableY, contentWidth, rowHeight);
                gfx.DrawLine(new XPen(borderLight, 0.5), margin, tableY + rowHeight, pageWidth - margin, tableY + rowHeight);

                double textY = tableY + (rowHeight * 0.68);
                curX = margin + 8;
                string sName = item.ServiceName.Length > 20 ? item.ServiceName[..19] + "…" : item.ServiceName;
                gfx.DrawString(sName, fontBold, new XSolidBrush(textDark), new XPoint(curX, textY)); curX += colItem;

                string desc = item.Description.Length > 22 ? item.Description[..21] + "…" : item.Description;
                gfx.DrawString(desc, fontRegular, new XSolidBrush(textMuted), new XPoint(curX, textY)); curX += colDesc;

                string qtyStr = item.Quantity.ToString("0.##");
                gfx.DrawString(qtyStr, fontRegular, new XSolidBrush(textDark), new XPoint(curX + colQty - 6 - gfx.MeasureString(qtyStr, fontRegular).Width, textY)); curX += colQty;

                string rateStr = "₹" + item.UnitPrice.ToString("N2");
                gfx.DrawString(rateStr, fontRegular, new XSolidBrush(textDark), new XPoint(curX + colRate - 6 - gfx.MeasureString(rateStr, fontRegular).Width, textY)); curX += colRate;

                string discStr = item.DiscountPercent > 0 ? $"{item.DiscountPercent}%" : "-";
                gfx.DrawString(discStr, fontRegular, new XSolidBrush(textDark), new XPoint(curX + colDisc - 6 - gfx.MeasureString(discStr, fontRegular).Width, textY)); curX += colDisc;

                string taxStr = $"{item.TaxPercent}%";
                gfx.DrawString(taxStr, fontRegular, new XSolidBrush(textDark), new XPoint(curX + colTax - 6 - gfx.MeasureString(taxStr, fontRegular).Width, textY)); curX += colTax;

                string lineTotalStr = "₹" + item.LineTotal.ToString("N2");
                gfx.DrawString(lineTotalStr, fontBold, new XSolidBrush(primaryColor), new XPoint(curX + colTotal - 8 - gfx.MeasureString(lineTotalStr, fontBold).Width, textY));

                tableY += rowHeight;
                index++;
            }

            y = tableY + 10;

            // 5. SUMMARY & TOTALS BLOCK
            double summaryW = 215;
            double summaryX = pageWidth - margin - summaryW;

            // Draw Bank details on Left
            double bankW = contentWidth - summaryW - 16;
            double bankBoxHeight = 100;
            gfx.DrawRoundedRectangle(new XPen(borderLight, 1), new XSolidBrush(headerBg), margin, y, bankW, bankBoxHeight, 6, 6);
            gfx.DrawString("PAYMENT INSTRUCTIONS & BANK DETAILS", fontSmallBold, new XSolidBrush(primaryColor), new XPoint(margin + 10, y + 15));
            string branchStr = string.IsNullOrWhiteSpace(profile.BranchName) ? "" : $"  |  Branch: {profile.BranchName}";
            gfx.DrawString($"Bank: {profile.BankName}{branchStr}", fontRegular, new XSolidBrush(textDark), new XPoint(margin + 10, y + 30));
            gfx.DrawString($"A/C No: {profile.AccountNumber}  |  IFSC: {profile.IFSC}", fontRegular, new XSolidBrush(textDark), new XPoint(margin + 10, y + 44));
            gfx.DrawString($"UPI ID: {profile.UPIId}", fontBold, new XSolidBrush(accentColor), new XPoint(margin + 10, y + 59));
            string firstTerm = profile.DefaultTerms.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
            gfx.DrawString(firstTerm, fontSmall, new XSolidBrush(textMuted), new XPoint(margin + 10, y + 74));
            gfx.DrawString("Authorized Signatory — S TOM'S Digital Platform", fontSmallBold, new XSolidBrush(textDark), new XPoint(margin + 10, y + 90));

            // Summary Rows on Right
            double sumY = y;
            DrawSummaryLine(gfx, "Subtotal:", "₹" + invoice.SubTotal.ToString("N2"), fontRegular, summaryX, ref sumY, summaryW, textDark);
            if (invoice.ItemDiscountTotal + invoice.OverallDiscount > 0)
            {
                DrawSummaryLine(gfx, "Discount Total:", "-₹" + (invoice.ItemDiscountTotal + invoice.OverallDiscount).ToString("N2"), fontRegular, summaryX, ref sumY, summaryW, XColor.FromArgb(22, 101, 52));
            }
            DrawSummaryLine(gfx, "Taxable Value:", "₹" + invoice.TaxableAmount.ToString("N2"), fontRegular, summaryX, ref sumY, summaryW, textDark);

            if (invoice.IsInterState)
            {
                DrawSummaryLine(gfx, $"IGST ({invoice.TaxRate}%):", "₹" + invoice.IgstAmount.ToString("N2"), fontRegular, summaryX, ref sumY, summaryW, textDark);
            }
            else
            {
                DrawSummaryLine(gfx, $"CGST ({invoice.TaxRate / 2}%):", "₹" + invoice.CgstAmount.ToString("N2"), fontRegular, summaryX, ref sumY, summaryW, textDark);
                DrawSummaryLine(gfx, $"SGST ({invoice.TaxRate / 2}%):", "₹" + invoice.SgstAmount.ToString("N2"), fontRegular, summaryX, ref sumY, summaryW, textDark);
            }

            // Grand Total Pill
            sumY += 3;
            gfx.DrawRoundedRectangle(new XSolidBrush(primaryColor), summaryX, sumY, summaryW, 24, 4, 4);
            gfx.DrawString("GRAND TOTAL:", fontBold, XBrushes.White, new XPoint(summaryX + 10, sumY + 16));
            string totalStr = "₹" + invoice.GrandTotal.ToString("N2");
            double tW = gfx.MeasureString(totalStr, fontBold).Width;
            gfx.DrawString(totalStr, fontBold, XBrushes.White, new XPoint(summaryX + summaryW - tW - 10, sumY + 16));
            sumY += 28;

            DrawSummaryLine(gfx, "Amount Paid:", "₹" + invoice.AmountPaid.ToString("N2"), fontBold, summaryX, ref sumY, summaryW, XColor.FromArgb(22, 101, 52));
            DrawSummaryLine(gfx, "Balance Due:", "₹" + invoice.BalanceDue.ToString("N2"), fontBold, summaryX, ref sumY, summaryW, invoice.BalanceDue > 0 ? XColor.FromArgb(185, 28, 28) : textDark);

            // 6. FOOTER NOTE: Safely positioned within single-page bounds
            double contentBottom = Math.Max(y + bankBoxHeight, sumY);
            double footerY = Math.Min(Math.Max(contentBottom + 18, 680), 715);

            gfx.DrawLine(new XPen(borderLight, 0.5), margin, footerY, pageWidth - margin, footerY);
            string footerNote = string.IsNullOrWhiteSpace(profile.FooterNote)
                ? $"Thank you for your business! For billing inquiries, contact {profile.Email}"
                : profile.FooterNote;
            gfx.DrawString(footerNote, fontSmall, new XSolidBrush(textMuted), new XPoint(margin, footerY + 13));
            gfx.DrawString("Page 1 of 1", fontSmall, new XSolidBrush(textMuted), new XPoint(pageWidth - margin - 45, footerY + 13));
        }

        private void DrawSummaryLine(XGraphics gfx, string label, string value, XFont font, double x, ref double y, double width, XColor color)
        {
            gfx.DrawString(label, font, new XSolidBrush(color), new XPoint(x, y + 12));
            double vW = gfx.MeasureString(value, font).Width;
            gfx.DrawString(value, font, new XSolidBrush(color), new XPoint(x + width - vW, y + 12));
            y += 18;
        }

        // =========================================================================================
        // GDI+ LIVE BITMAP RENDERER (100% Matching Layout for Real-Time On-Screen Invoice Preview)
        // =========================================================================================
        public Bitmap RenderInvoiceBitmap(Invoice invoice, BusinessProfile profile, float scale = 1.0f)
        {
            if (scale <= 0.1f) scale = 1.0f;
            float baseW = 595.28f;
            float baseH = 841.89f;
            int bmpW = Math.Max(100, (int)Math.Round(baseW * scale));
            int bmpH = Math.Max(100, (int)Math.Round(baseH * scale));

            var bmp = new Bitmap(bmpW, bmpH);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            g.ScaleTransform(scale, scale);
            DrawInvoiceGdi(g, invoice, profile, baseW, baseH);
            return bmp;
        }

        private void DrawInvoiceGdi(Graphics g, Invoice invoice, BusinessProfile profile, float pageWidth, float pageHeight)
        {
            float margin = 36f;
            float contentWidth = pageWidth - (margin * 2f);
            float y = margin;

            Color primaryColor = Color.FromArgb(30, 41, 59);    // Slate 800
            Color accentColor;
            try
            {
                accentColor = ColorTranslator.FromHtml(profile.AccentColorHex);
            }
            catch
            {
                accentColor = Color.FromArgb(59, 130, 246);   // Blue 500
            }

            Color textDark = Color.FromArgb(15, 23, 42);       // Slate 900
            Color textMuted = Color.FromArgb(100, 116, 139);   // Slate 500
            Color borderLight = Color.FromArgb(226, 232, 240); // Slate 200
            Color headerBg = Color.FromArgb(248, 250, 252);    // Slate 50

            using var fontTitle = new Font("Arial", 20f, FontStyle.Bold);
            using var fontSub = new Font("Arial", 8.5f, FontStyle.Regular);
            using var fontBold = new Font("Arial", 9.5f, FontStyle.Bold);
            using var fontRegular = new Font("Arial", 9f, FontStyle.Regular);
            using var fontSmall = new Font("Arial", 8f, FontStyle.Regular);
            using var fontSmallBold = new Font("Arial", 8f, FontStyle.Bold);
            using var invTitleFont = new Font("Arial", 24f, FontStyle.Bold);

            // 1. TOP HEADER: Business Logo (if available) & Business Name on Left, "INVOICE" on Right
            float headerLeftX = margin;
            if (!string.IsNullOrWhiteSpace(profile.LogoPath) && File.Exists(profile.LogoPath))
            {
                try
                {
                    using var img = Image.FromFile(profile.LogoPath);
                    g.DrawImage(img, margin, y + 2, 40, 40);
                    headerLeftX += 48;
                }
                catch { }
            }

            using (var bPrimary = new SolidBrush(primaryColor))
            using (var bAccent = new SolidBrush(accentColor))
            {
                g.DrawString(profile.BusinessName, fontTitle, bPrimary, new PointF(headerLeftX, y + 2));
                var nameSize = g.MeasureString(profile.BusinessName, fontTitle);
                float taglineY = y + 2 + Math.Max(nameSize.Height - 6, 22f);
                g.DrawString(profile.Tagline, fontSub, bAccent, new PointF(headerLeftX, taglineY));

                // Right Header: TAX INVOICE
                string invTitle = "TAX INVOICE";
                var titleSize = g.MeasureString(invTitle, invTitleFont);
                g.DrawString(invTitle, invTitleFont, bAccent, new PointF(pageWidth - margin - titleSize.Width, y + 2));
            }

            // Status Badge
            string status = invoice.Status.ToUpperInvariant();
            (Color badgeBg, Color badgeText) = status switch
            {
                "PAID" => (Color.FromArgb(220, 252, 231), Color.FromArgb(22, 101, 52)),
                "PARTIALLY PAID" => (Color.FromArgb(254, 243, 199), Color.FromArgb(146, 64, 14)),
                "OVERDUE" => (Color.FromArgb(254, 226, 226), Color.FromArgb(153, 27, 27)),
                "CANCELLED" => (Color.FromArgb(241, 245, 249), Color.FromArgb(100, 116, 139)),
                _ => (Color.FromArgb(254, 240, 220), Color.FromArgb(154, 52, 18))
            };

            float badgeW = 85f;
            float badgeH = 18f;
            float badgeX = pageWidth - margin - badgeW;
            float badgeY = y + 28f;
            var badgeRect = new RectangleF(badgeX, badgeY, badgeW, badgeH);
            using (var pBadgePath = CreateGdiRoundedPath(badgeRect, 4f))
            using (var bBadgeBg = new SolidBrush(badgeBg))
            using (var penBadge = new Pen(badgeText, 0.5f))
            using (var bBadgeText = new SolidBrush(badgeText))
            using (var sfCenter = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.FillPath(bBadgeBg, pBadgePath);
                g.DrawPath(penBadge, pBadgePath);
                g.DrawString(status, fontSmallBold, bBadgeText, badgeRect, sfCenter);
            }

            y += 48f;

            // Divider Line
            using (var penDiv = new Pen(borderLight, 1f))
            {
                g.DrawLine(penDiv, margin, y, pageWidth - margin, y);
            }
            y += 12f;

            // 2. METADATA: Seller Details (Left) vs Invoice Details (Right)
            float metaSectionY = y;
            float colW = contentWidth / 2f - 10f;

            using var sfLeftTrim = new StringFormat
            {
                Alignment = StringAlignment.Near,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            };
            using var sfRightTrim = new StringFormat
            {
                Alignment = StringAlignment.Far,
                LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter,
                FormatFlags = StringFormatFlags.NoWrap
            };

            using (var bMuted = new SolidBrush(textMuted))
            using (var bDark = new SolidBrush(textDark))
            using (var bPrimary = new SolidBrush(primaryColor))
            {
                g.DrawString("BILLED FROM:", fontSmallBold, bMuted, new PointF(margin, metaSectionY + 2));
                float sellerY = metaSectionY + 16f;
                g.DrawString(profile.BusinessName, fontBold, bDark, new RectangleF(margin, sellerY, colW - 8, 14), sfLeftTrim); sellerY += 14f;

                string addr1 = string.IsNullOrWhiteSpace(profile.Address) ? "Business Address Not Set" : profile.Address;
                g.DrawString(addr1, fontRegular, bMuted, new RectangleF(margin, sellerY, colW - 8, 14), sfLeftTrim); sellerY += 13f;

                string addr2 = $"{profile.City}{(string.IsNullOrWhiteSpace(profile.Country) ? "" : ", " + profile.Country)}".Trim(',', ' ');
                if (!string.IsNullOrWhiteSpace(addr2))
                {
                    g.DrawString(addr2, fontRegular, bMuted, new RectangleF(margin, sellerY, colW - 8, 14), sfLeftTrim); sellerY += 13f;
                }

                string contact = $"Phone: {profile.Phone}  |  Email: {profile.Email}";
                g.DrawString(contact, fontRegular, bMuted, new RectangleF(margin, sellerY, colW - 8, 14), sfLeftTrim); sellerY += 13f;

                if (!string.IsNullOrWhiteSpace(profile.GSTIN))
                {
                    g.DrawString($"GSTIN: {profile.GSTIN}", fontBold, bPrimary, new RectangleF(margin, sellerY, colW - 8, 14), sfLeftTrim);
                    sellerY += 13f;
                }

                // Right: Invoice Info Box
                float rightColX = pageWidth - margin - colW;
                var invBoxRect = new RectangleF(rightColX, metaSectionY, colW, 68f);
                using (var pBoxPath = CreateGdiRoundedPath(invBoxRect, 6f))
                using (var bBoxBg = new SolidBrush(headerBg))
                using (var penBorder = new Pen(borderLight, 1f))
                {
                    g.FillPath(bBoxBg, pBoxPath);
                    g.DrawPath(penBorder, pBoxPath);
                }

                g.DrawString($"Invoice No: {invoice.InvoiceNumber}", fontBold, bPrimary, new PointF(rightColX + 10, metaSectionY + 10));
                g.DrawString($"Invoice Date: {invoice.InvoiceDate:dd MMM yyyy}", fontRegular, bDark, new PointF(rightColX + 10, metaSectionY + 28));
                g.DrawString($"Due Date: {invoice.DueDate:dd MMM yyyy} ({invoice.PaymentTerms})", fontRegular, bDark, new PointF(rightColX + 10, metaSectionY + 46));

                y = Math.Max(sellerY, metaSectionY + 70f) + 8f;

                // 3. BILL TO (Customer Details)
                var billToRect = new RectangleF(margin, y, contentWidth, 48f);
                using (var pBillTo = CreateGdiRoundedPath(billToRect, 6f))
                using (var bBillBg = new SolidBrush(headerBg))
                using (var penBorder = new Pen(borderLight, 1f))
                {
                    g.FillPath(bBillBg, pBillTo);
                    g.DrawPath(penBorder, pBillTo);
                }

                g.DrawString("BILL TO / CLIENT DETAILS:", fontSmallBold, bMuted, new PointF(margin + 10, y + 6));
                string clientLine1 = string.IsNullOrWhiteSpace(invoice.CompanyName) 
                    ? invoice.CustomerName 
                    : $"{invoice.CustomerName} ({invoice.CompanyName})";
                g.DrawString(clientLine1, fontBold, bDark, new RectangleF(margin + 10, y + 18, contentWidth - 170, 14), sfLeftTrim);
                string clientLine2 = $"{invoice.BillingAddress}  |  Phone: {invoice.CustomerPhone}  |  Email: {invoice.CustomerEmail}";
                g.DrawString(clientLine2, fontRegular, bMuted, new RectangleF(margin + 10, y + 32, contentWidth - 20, 14), sfLeftTrim);

                if (!string.IsNullOrWhiteSpace(invoice.CustomerGSTIN))
                {
                    g.DrawString($"GSTIN: {invoice.CustomerGSTIN}", fontSmallBold, bPrimary, new RectangleF(pageWidth - margin - 150, y + 18, 140, 14), sfRightTrim);
                }
            }

            y += 56f;

            // 4. ITEMS TABLE
            float tableY = y;
            float colItem = 145f;
            float colDesc = 130f;
            float colQty = 35f;
            float colRate = 65f;
            float colDisc = 38f;
            float colTax = 38f;
            float colTotal = 67f;

            float rowHeight = invoice.Items.Count > 10 ? 16f : (invoice.Items.Count > 6 ? 18f : 20f);

            // Table Header Bar
            var tableHeaderRect = new RectangleF(margin, tableY, contentWidth, 22f);
            using (var pHeaderPath = CreateGdiRoundedPath(tableHeaderRect, 4f))
            using (var bPrimary = new SolidBrush(primaryColor))
            {
                g.FillPath(bPrimary, pHeaderPath);
            }

            using (var bWhite = new SolidBrush(Color.White))
            {
                float curX = margin + 8f;
                g.DrawString("Service / Product", fontSmallBold, bWhite, new RectangleF(curX, tableY + 4, colItem - 6, 14), sfLeftTrim); curX += colItem;
                g.DrawString("Description", fontSmallBold, bWhite, new RectangleF(curX, tableY + 4, colDesc - 6, 14), sfLeftTrim); curX += colDesc;
                g.DrawString("Qty", fontSmallBold, bWhite, new RectangleF(curX, tableY + 4, colQty - 6, 14), sfRightTrim); curX += colQty;
                g.DrawString("Rate", fontSmallBold, bWhite, new RectangleF(curX, tableY + 4, colRate - 6, 14), sfRightTrim); curX += colRate;
                g.DrawString("Disc%", fontSmallBold, bWhite, new RectangleF(curX, tableY + 4, colDisc - 6, 14), sfRightTrim); curX += colDisc;
                g.DrawString("Tax%", fontSmallBold, bWhite, new RectangleF(curX, tableY + 4, colTax - 6, 14), sfRightTrim); curX += colTax;
                g.DrawString("Total", fontSmallBold, bWhite, new RectangleF(curX, tableY + 4, colTotal - 8, 14), sfRightTrim);
            }

            tableY += 24f;

            // Table Rows
            int index = 0;
            using (var bDark = new SolidBrush(textDark))
            using (var bMuted = new SolidBrush(textMuted))
            using (var bPrimary = new SolidBrush(primaryColor))
            using (var penBorder = new Pen(borderLight, 0.5f))
            {
                foreach (var item in invoice.Items)
                {
                    Color rowBg = (index % 2 == 0) ? Color.White : Color.FromArgb(248, 250, 252);
                    using (var bRowBg = new SolidBrush(rowBg))
                    {
                        g.FillRectangle(bRowBg, margin, tableY, contentWidth, rowHeight);
                    }
                    g.DrawLine(penBorder, margin, tableY + rowHeight, pageWidth - margin, tableY + rowHeight);

                    float textY = tableY + ((rowHeight - 12f) / 2f);
                    float curX = margin + 8f;
                    g.DrawString(item.ServiceName, fontBold, bDark, new RectangleF(curX, textY, colItem - 6, 14), sfLeftTrim); curX += colItem;
                    g.DrawString(item.Description, fontRegular, bMuted, new RectangleF(curX, textY, colDesc - 6, 14), sfLeftTrim); curX += colDesc;

                    g.DrawString(item.Quantity.ToString("0.##"), fontRegular, bDark, new RectangleF(curX, textY, colQty - 6, 14), sfRightTrim); curX += colQty;
                    g.DrawString("₹" + item.UnitPrice.ToString("N2"), fontRegular, bDark, new RectangleF(curX, textY, colRate - 6, 14), sfRightTrim); curX += colRate;
                    g.DrawString(item.DiscountPercent > 0 ? $"{item.DiscountPercent}%" : "-", fontRegular, bDark, new RectangleF(curX, textY, colDisc - 6, 14), sfRightTrim); curX += colDisc;
                    g.DrawString($"{item.TaxPercent}%", fontRegular, bDark, new RectangleF(curX, textY, colTax - 6, 14), sfRightTrim); curX += colTax;
                    g.DrawString("₹" + item.LineTotal.ToString("N2"), fontBold, bPrimary, new RectangleF(curX, textY, colTotal - 8, 14), sfRightTrim);

                    tableY += rowHeight;
                    index++;
                }
            }

            y = tableY + 10f;

            // 5. SUMMARY & TOTALS BLOCK
            float summaryW = 215f;
            float summaryX = pageWidth - margin - summaryW;

            // Bank details on Left
            float bankW = contentWidth - summaryW - 16f;
            float bankBoxHeight = 100f;
            var bankRect = new RectangleF(margin, y, bankW, bankBoxHeight);
            using (var pBank = CreateGdiRoundedPath(bankRect, 6f))
            using (var bBankBg = new SolidBrush(headerBg))
            using (var penBorder = new Pen(borderLight, 1f))
            {
                g.FillPath(bBankBg, pBank);
                g.DrawPath(penBorder, pBank);
            }

            using (var bPrimary = new SolidBrush(primaryColor))
            using (var bDark = new SolidBrush(textDark))
            using (var bAccent = new SolidBrush(accentColor))
            using (var bMuted = new SolidBrush(textMuted))
            {
                g.DrawString("PAYMENT INSTRUCTIONS & BANK DETAILS", fontSmallBold, bPrimary, new PointF(margin + 10, y + 10));
                string branchStr = string.IsNullOrWhiteSpace(profile.BranchName) ? "" : $"  |  Branch: {profile.BranchName}";
                g.DrawString($"Bank: {profile.BankName}{branchStr}", fontRegular, bDark, new PointF(margin + 10, y + 26));
                g.DrawString($"A/C No: {profile.AccountNumber}  |  IFSC: {profile.IFSC}", fontRegular, bDark, new PointF(margin + 10, y + 40));
                g.DrawString($"UPI ID: {profile.UPIId}", fontBold, bAccent, new PointF(margin + 10, y + 55));
                string firstTerm = profile.DefaultTerms.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
                g.DrawString(firstTerm, fontSmall, bMuted, new PointF(margin + 10, y + 71));
                g.DrawString("Authorized Signatory — S TOM'S Digital Platform", fontSmallBold, bDark, new PointF(margin + 10, y + 86));

                // Summary Rows on Right
                float sumY = y;
                DrawGdiSummaryLine(g, "Subtotal:", "₹" + invoice.SubTotal.ToString("N2"), fontRegular, summaryX, ref sumY, summaryW, textDark);
                if (invoice.ItemDiscountTotal + invoice.OverallDiscount > 0)
                {
                    DrawGdiSummaryLine(g, "Discount Total:", "-₹" + (invoice.ItemDiscountTotal + invoice.OverallDiscount).ToString("N2"), fontRegular, summaryX, ref sumY, summaryW, Color.FromArgb(22, 101, 52));
                }
                DrawGdiSummaryLine(g, "Taxable Value:", "₹" + invoice.TaxableAmount.ToString("N2"), fontRegular, summaryX, ref sumY, summaryW, textDark);

                if (invoice.IsInterState)
                {
                    DrawGdiSummaryLine(g, $"IGST ({invoice.TaxRate}%):", "₹" + invoice.IgstAmount.ToString("N2"), fontRegular, summaryX, ref sumY, summaryW, textDark);
                }
                else
                {
                    DrawGdiSummaryLine(g, $"CGST ({invoice.TaxRate / 2}%):", "₹" + invoice.CgstAmount.ToString("N2"), fontRegular, summaryX, ref sumY, summaryW, textDark);
                    DrawGdiSummaryLine(g, $"SGST ({invoice.TaxRate / 2}%):", "₹" + invoice.SgstAmount.ToString("N2"), fontRegular, summaryX, ref sumY, summaryW, textDark);
                }

                // Grand Total Pill
                sumY += 3f;
                var totalPillRect = new RectangleF(summaryX, sumY, summaryW, 24f);
                using (var pPill = CreateGdiRoundedPath(totalPillRect, 4f))
                using (var bWhite = new SolidBrush(Color.White))
                {
                    g.FillPath(bPrimary, pPill);
                    g.DrawString("GRAND TOTAL:", fontBold, bWhite, new PointF(summaryX + 10, sumY + 5));
                    string totalStr = "₹" + invoice.GrandTotal.ToString("N2");
                    var tSize = g.MeasureString(totalStr, fontBold);
                    g.DrawString(totalStr, fontBold, bWhite, new PointF(summaryX + summaryW - tSize.Width - 10, sumY + 5));
                }
                sumY += 28f;

                DrawGdiSummaryLine(g, "Amount Paid:", "₹" + invoice.AmountPaid.ToString("N2"), fontBold, summaryX, ref sumY, summaryW, Color.FromArgb(22, 101, 52));
                DrawGdiSummaryLine(g, "Balance Due:", "₹" + invoice.BalanceDue.ToString("N2"), fontBold, summaryX, ref sumY, summaryW, invoice.BalanceDue > 0 ? Color.FromArgb(185, 28, 28) : textDark);

                // 6. FOOTER NOTE
                float contentBottom = Math.Max(y + bankBoxHeight, sumY);
                float footerY = Math.Min(Math.Max(contentBottom + 18f, 680f), 715f);

                using (var penBorder = new Pen(borderLight, 0.5f))
                {
                    g.DrawLine(penBorder, margin, footerY, pageWidth - margin, footerY);
                }
                string footerNote = string.IsNullOrWhiteSpace(profile.FooterNote)
                    ? $"Thank you for your business! For billing inquiries, contact {profile.Email}"
                    : profile.FooterNote;
                g.DrawString(footerNote, fontSmall, bMuted, new PointF(margin, footerY + 6));
                g.DrawString("Page 1 of 1", fontSmall, bMuted, new PointF(pageWidth - margin - 50, footerY + 6));
            }
        }

        private void DrawGdiSummaryLine(Graphics g, string label, string value, Font font, float x, ref float y, float width, Color color)
        {
            using var b = new SolidBrush(color);
            g.DrawString(label, font, b, new PointF(x, y));
            var vSize = g.MeasureString(value, font);
            g.DrawString(value, font, b, new PointF(x + width - vSize.Width, y));
            y += 18f;
        }

        private static GraphicsPath CreateGdiRoundedPath(RectangleF rect, float radius)
        {
            var path = new GraphicsPath();
            float d = radius * 2f;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
