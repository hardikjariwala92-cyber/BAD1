using System.Diagnostics;
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

        private void DrawInvoicePage(XGraphics gfx, Invoice invoice, BusinessProfile profile, double pageWidth, double pageHeight)
        {
            double margin = 40;
            double contentWidth = pageWidth - (margin * 2);
            double y = margin;

            // Palette
            var primaryColor = XColor.FromArgb(30, 41, 59);    // Slate 800
            var accentColor = XColor.FromArgb(59, 130, 246);   // Blue 500
            var textDark = XColor.FromArgb(15, 23, 42);       // Slate 900
            var textMuted = XColor.FromArgb(100, 116, 139);   // Slate 500
            var borderLight = XColor.FromArgb(226, 232, 240); // Slate 200
            var headerBg = XColor.FromArgb(248, 250, 252);    // Slate 50

            var fontTitle = new XFont("Arial", 22, XFontStyleEx.Bold);
            var fontSub = new XFont("Arial", 9, XFontStyleEx.Regular);
            var fontBold = new XFont("Arial", 10, XFontStyleEx.Bold);
            var fontRegular = new XFont("Arial", 9.5, XFontStyleEx.Regular);
            var fontSmall = new XFont("Arial", 8, XFontStyleEx.Regular);
            var fontSmallBold = new XFont("Arial", 8, XFontStyleEx.Bold);

            // 1. TOP HEADER: Business Name & Logo on Left, "INVOICE" on Right
            gfx.DrawString(profile.BusinessName, fontTitle, new XSolidBrush(primaryColor), new XPoint(margin, y + 20));
            gfx.DrawString(profile.Tagline, fontSub, new XSolidBrush(accentColor), new XPoint(margin, y + 36));

            // Right Header: INVOICE label
            var invTitleFont = new XFont("Arial", 26, XFontStyleEx.Bold);
            string invTitle = "TAX INVOICE";
            double titleWidth = gfx.MeasureString(invTitle, invTitleFont).Width;
            gfx.DrawString(invTitle, invTitleFont, new XSolidBrush(accentColor), new XPoint(pageWidth - margin - titleWidth, y + 20));

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

            double badgeW = 90;
            double badgeH = 20;
            double badgeX = pageWidth - margin - badgeW;
            double badgeY = y + 28;
            gfx.DrawRoundedRectangle(new XPen(badgeText, 0.5), new XSolidBrush(badgeBg), badgeX, badgeY, badgeW, badgeH, 4, 4);
            var sfCenter = new XStringFormat { Alignment = XStringAlignment.Center, LineAlignment = XLineAlignment.Center };
            gfx.DrawString(status, fontSmallBold, new XSolidBrush(badgeText), new XRect(badgeX, badgeY, badgeW, badgeH), sfCenter);

            y += 55;

            // Divider
            gfx.DrawLine(new XPen(borderLight, 1), margin, y, pageWidth - margin, y);
            y += 15;

            // 2. METADATA: Seller Details (Left) vs Invoice Details (Right)
            double colW = contentWidth / 2 - 10;

            // Left: Seller Info
            gfx.DrawString("BILLED FROM:", fontSmallBold, new XSolidBrush(textMuted), new XPoint(margin, y));
            y += 14;
            gfx.DrawString(profile.BusinessName, fontBold, new XSolidBrush(textDark), new XPoint(margin, y)); y += 13;
            gfx.DrawString(profile.Address + ", " + profile.City, fontRegular, new XSolidBrush(textMuted), new XPoint(margin, y)); y += 13;
            gfx.DrawString($"Phone: {profile.Phone} | Email: {profile.Email}", fontRegular, new XSolidBrush(textMuted), new XPoint(margin, y)); y += 13;
            if (!string.IsNullOrWhiteSpace(profile.GSTIN))
            {
                gfx.DrawString($"GSTIN: {profile.GSTIN}", fontBold, new XSolidBrush(primaryColor), new XPoint(margin, y));
                y += 13;
            }

            // Right: Invoice Info Box
            double rightColX = pageWidth - margin - colW;
            double metaY = y - 52;
            gfx.DrawRoundedRectangle(new XPen(borderLight, 1), new XSolidBrush(headerBg), rightColX, metaY, colW, 68, 6, 6);

            gfx.DrawString($"Invoice No: {invoice.InvoiceNumber}", fontBold, new XSolidBrush(primaryColor), new XPoint(rightColX + 12, metaY + 18));
            gfx.DrawString($"Invoice Date: {invoice.InvoiceDate:dd MMM yyyy}", fontRegular, new XSolidBrush(textDark), new XPoint(rightColX + 12, metaY + 34));
            gfx.DrawString($"Due Date: {invoice.DueDate:dd MMM yyyy} ({invoice.PaymentTerms})", fontRegular, new XSolidBrush(textDark), new XPoint(rightColX + 12, metaY + 50));

            y += 10;

            // 3. BILL TO (Customer Details)
            gfx.DrawRoundedRectangle(new XPen(borderLight, 1), new XSolidBrush(headerBg), margin, y, contentWidth, 54, 6, 6);
            gfx.DrawString("BILL TO / CLIENT DETAILS:", fontSmallBold, new XSolidBrush(textMuted), new XPoint(margin + 12, y + 14));

            string clientLine1 = string.IsNullOrWhiteSpace(invoice.CompanyName) 
                ? invoice.CustomerName 
                : $"{invoice.CustomerName} ({invoice.CompanyName})";
            gfx.DrawString(clientLine1, fontBold, new XSolidBrush(textDark), new XPoint(margin + 12, y + 28));

            string clientLine2 = $"{invoice.BillingAddress}  |  Phone: {invoice.CustomerPhone}  |  Email: {invoice.CustomerEmail}";
            gfx.DrawString(clientLine2, fontRegular, new XSolidBrush(textMuted), new XPoint(margin + 12, y + 42));

            if (!string.IsNullOrWhiteSpace(invoice.CustomerGSTIN))
            {
                gfx.DrawString($"GSTIN: {invoice.CustomerGSTIN}", fontSmallBold, new XSolidBrush(primaryColor), new XPoint(pageWidth - margin - 150, y + 28));
            }

            y += 66;

            // 4. ITEMS TABLE
            double tableY = y;
            double colItem = 170;
            double colDesc = 120;
            double colQty = 40;
            double colRate = 60;
            double colDisc = 40;
            double colTax = 40;

            // Table Header Bar
            gfx.DrawRoundedRectangle(new XSolidBrush(primaryColor), margin, tableY, contentWidth, 24, 4, 4);

            double curX = margin + 8;
            gfx.DrawString("Service / Product", fontSmallBold, XBrushes.White, new XPoint(curX, tableY + 16)); curX += colItem;
            gfx.DrawString("Description", fontSmallBold, XBrushes.White, new XPoint(curX, tableY + 16)); curX += colDesc;
            gfx.DrawString("Qty", fontSmallBold, XBrushes.White, new XPoint(curX, tableY + 16)); curX += colQty;
            gfx.DrawString("Rate", fontSmallBold, XBrushes.White, new XPoint(curX, tableY + 16)); curX += colRate;
            gfx.DrawString("Disc%", fontSmallBold, XBrushes.White, new XPoint(curX, tableY + 16)); curX += colDisc;
            gfx.DrawString("Tax%", fontSmallBold, XBrushes.White, new XPoint(curX, tableY + 16)); curX += colTax;
            gfx.DrawString("Total", fontSmallBold, XBrushes.White, new XPoint(curX, tableY + 16));

            tableY += 26;

            // Table Rows
            int index = 0;
            foreach (var item in invoice.Items)
            {
                XColor rowBg = (index % 2 == 0) ? XColor.FromArgb(255, 255, 255) : XColor.FromArgb(248, 250, 252);
                gfx.DrawRectangle(new XSolidBrush(rowBg), margin, tableY, contentWidth, 22);
                gfx.DrawLine(new XPen(borderLight, 0.5), margin, tableY + 22, pageWidth - margin, tableY + 22);

                curX = margin + 8;
                gfx.DrawString(item.ServiceName, fontBold, new XSolidBrush(textDark), new XPoint(curX, tableY + 15)); curX += colItem;

                string desc = item.Description.Length > 24 ? item.Description[..24] + "..." : item.Description;
                gfx.DrawString(desc, fontRegular, new XSolidBrush(textMuted), new XPoint(curX, tableY + 15)); curX += colDesc;

                gfx.DrawString(item.Quantity.ToString("0.##"), fontRegular, new XSolidBrush(textDark), new XPoint(curX, tableY + 15)); curX += colQty;
                gfx.DrawString("₹" + item.UnitPrice.ToString("N2"), fontRegular, new XSolidBrush(textDark), new XPoint(curX, tableY + 15)); curX += colRate;
                gfx.DrawString(item.DiscountPercent > 0 ? $"{item.DiscountPercent}%" : "-", fontRegular, new XSolidBrush(textDark), new XPoint(curX, tableY + 15)); curX += colDisc;
                gfx.DrawString($"{item.TaxPercent}%", fontRegular, new XSolidBrush(textDark), new XPoint(curX, tableY + 15)); curX += colTax;
                gfx.DrawString("₹" + item.LineTotal.ToString("N2"), fontBold, new XSolidBrush(primaryColor), new XPoint(curX, tableY + 15));

                tableY += 22;
                index++;
            }

            y = tableY + 12;

            // 5. SUMMARY & TOTALS BLOCK
            double summaryW = 220;
            double summaryX = pageWidth - margin - summaryW;

            // Draw Bank details on Left
            double bankW = contentWidth - summaryW - 20;
            gfx.DrawRoundedRectangle(new XPen(borderLight, 1), new XSolidBrush(headerBg), margin, y, bankW, 115, 6, 6);
            gfx.DrawString("PAYMENT INSTRUCTIONS & BANK DETAILS", fontSmallBold, new XSolidBrush(primaryColor), new XPoint(margin + 12, y + 16));
            gfx.DrawString($"Bank: {profile.BankName}", fontRegular, new XSolidBrush(textDark), new XPoint(margin + 12, y + 32));
            gfx.DrawString($"A/C No: {profile.AccountNumber}  |  IFSC: {profile.IFSC}", fontRegular, new XSolidBrush(textDark), new XPoint(margin + 12, y + 48));
            gfx.DrawString($"UPI ID: {profile.UPIId}", fontBold, new XSolidBrush(accentColor), new XPoint(margin + 12, y + 64));
            gfx.DrawString(profile.DefaultTerms.Split('\n')[0], fontSmall, new XSolidBrush(textMuted), new XPoint(margin + 12, y + 80));
            gfx.DrawString("Authorized Signatory — S TOM'S Digital Platform", fontSmallBold, new XSolidBrush(textDark), new XPoint(margin + 12, y + 102));

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
            sumY += 4;
            gfx.DrawRoundedRectangle(new XSolidBrush(primaryColor), summaryX, sumY, summaryW, 26, 4, 4);
            gfx.DrawString("GRAND TOTAL:", fontBold, XBrushes.White, new XPoint(summaryX + 10, sumY + 17));
            string totalStr = "₹" + invoice.GrandTotal.ToString("N2");
            double tW = gfx.MeasureString(totalStr, fontBold).Width;
            gfx.DrawString(totalStr, fontBold, XBrushes.White, new XPoint(summaryX + summaryW - tW - 10, sumY + 17));
            sumY += 32;

            DrawSummaryLine(gfx, "Amount Paid:", "₹" + invoice.AmountPaid.ToString("N2"), fontBold, summaryX, ref sumY, summaryW, XColor.FromArgb(22, 101, 52));
            DrawSummaryLine(gfx, "Balance Due:", "₹" + invoice.BalanceDue.ToString("N2"), fontBold, summaryX, ref sumY, summaryW, invoice.BalanceDue > 0 ? XColor.FromArgb(185, 28, 28) : textDark);

            // Footer Note
            y = pageHeight - margin - 20;
            gfx.DrawLine(new XPen(borderLight, 0.5), margin, y, pageWidth - margin, y);
            gfx.DrawString("Thank you for your business! For billing inquiries, contact billing@stoms.io", fontSmall, new XSolidBrush(textMuted), new XPoint(margin, y + 14));
            gfx.DrawString("Page 1 of 1", fontSmall, new XSolidBrush(textMuted), new XPoint(pageWidth - margin - 45, y + 14));
        }

        private void DrawSummaryLine(XGraphics gfx, string label, string value, XFont font, double x, ref double y, double width, XColor color)
        {
            gfx.DrawString(label, font, new XSolidBrush(color), new XPoint(x, y + 12));
            double vW = gfx.MeasureString(value, font).Width;
            gfx.DrawString(value, font, new XSolidBrush(color), new XPoint(x + width - vW, y + 12));
            y += 18;
        }
    }
}
