import base64
import os
import subprocess
import sys

BASE_DIR = r"D:\sofwere"
REPORTS_DIR = os.path.join(BASE_DIR, "reports")
IMG_DIR = os.path.join(REPORTS_DIR, "img")
HTML_PATH = os.path.join(REPORTS_DIR, "STOMS_Sprint_Showcase.html")
PDF_PATH = os.path.join(REPORTS_DIR, "STOMS_Sprint_Showcase.pdf")
EDGE_PATH = r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe"

def get_base64_image(filename):
    path = os.path.join(IMG_DIR, filename)
    if not os.path.exists(path):
        print(f"Warning: {path} not found")
        return ""
    with open(path, "rb") as f:
        data = f.read()
    return f"data:image/png;base64,{base64.b64encode(data).decode('utf-8')}"

img_customer = get_base64_image("customer_table_redesign.png")
img_settings_btn = get_base64_image("settings_company_details_button.png")
img_wireframe = get_base64_image("invoice_designer_wireframe.png")
img_designer_live = get_base64_image("invoice_designer_live.png")
img_designer_full = get_base64_image("invoice_designer_full_preview.png")
img_light_mode = get_base64_image("settings_light_mode_contrast.png")

html_content = f"""<!DOCTYPE html>
<html lang="en">
<head>
<meta charset="utf-8">
<title>S TOM'S Platform — Engineering Sprint Showcase</title>
<style>
  @page {{
    size: A4 landscape;
    margin: 0;
  }}
  * {{
    box-sizing: border-box;
    margin: 0;
    padding: 0;
  }}
  html, body {{
    width: 297mm;
    height: 210mm;
    background-color: #0b0f19;
    color: #f1f5f9;
    font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
    -webkit-print-color-adjust: exact;
    print-color-adjust: exact;
  }}
  .slide {{
    width: 297mm;
    height: 210mm;
    max-height: 210mm;
    padding: 14mm 18mm 12mm 18mm;
    page-break-after: always;
    break-after: page;
    position: relative;
    overflow: hidden;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    background: #0b0f19;
  }}
  /* Header styling */
  .slide-header {{
    display: flex;
    justify-content: space-between;
    align-items: flex-start;
    margin-bottom: 5mm;
  }}
  .slide-tag {{
    display: inline-flex;
    align-items: center;
    gap: 6px;
    padding: 4px 10px;
    border-radius: 999px;
    font-size: 11px;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.06em;
    background: rgba(99, 102, 241, 0.15);
    color: #818cf8;
    border: 1px solid rgba(99, 102, 241, 0.3);
    margin-bottom: 6px;
  }}
  .tag-dot {{
    width: 6px;
    height: 6px;
    border-radius: 50%;
    background: #818cf8;
  }}
  .slide-tag.green {{
    background: rgba(16, 185, 129, 0.15);
    color: #34d399;
    border-color: rgba(16, 185, 129, 0.3);
  }}
  .slide-tag.green .tag-dot {{ background: #34d399; }}
  .slide-tag.amber {{
    background: rgba(245, 158, 11, 0.15);
    color: #fbbf24;
    border-color: rgba(245, 158, 11, 0.3);
  }}
  .slide-tag.amber .tag-dot {{ background: #fbbf24; }}

  .slide-title {{
    font-size: 24px;
    font-weight: 800;
    color: #ffffff;
    line-height: 1.2;
    letter-spacing: -0.02em;
  }}
  .slide-subtitle {{
    font-size: 13px;
    color: #94a3b8;
    margin-top: 4px;
    line-height: 1.4;
  }}
  .header-brand {{
    text-align: right;
    font-size: 11px;
    color: #64748b;
    font-weight: 600;
  }}
  .header-brand span {{
    color: #818cf8;
    font-weight: 700;
  }}

  /* Content area */
  .slide-body {{
    flex: 1;
    display: flex;
    gap: 16px;
    min-height: 0;
  }}
  .col-text {{
    flex: 1.1;
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    gap: 10px;
  }}
  .col-visual {{
    flex: 1.2;
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: center;
  }}

  /* Cards & blocks */
  .info-card {{
    background: #131926;
    border: 1px solid #1e293b;
    border-radius: 10px;
    padding: 12px 14px;
  }}
  .info-card-title {{
    font-size: 12px;
    font-weight: 700;
    color: #38bdf8;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    margin-bottom: 6px;
    display: flex;
    align-items: center;
    gap: 6px;
  }}
  .info-card p, .info-card li {{
    font-size: 12.5px;
    color: #cbd5e1;
    line-height: 1.45;
  }}
  .info-card ul {{
    list-style: none;
    padding-left: 0;
  }}
  .info-card li {{
    position: relative;
    padding-left: 14px;
    margin-bottom: 5px;
  }}
  .info-card li::before {{
    content: "•";
    color: #818cf8;
    position: absolute;
    left: 0;
    font-weight: bold;
  }}

  /* Metric pill bar */
  .metric-row {{
    display: grid;
    grid-template-columns: repeat(4, 1fr);
    gap: 10px;
  }}
  .metric-box {{
    background: #131926;
    border: 1px solid #1e293b;
    border-radius: 8px;
    padding: 10px 12px;
    text-align: center;
  }}
  .metric-val {{
    font-size: 20px;
    font-weight: 800;
    color: #ffffff;
    font-family: monospace;
  }}
  .metric-val.green {{ color: #34d399; }}
  .metric-val.indigo {{ color: #818cf8; }}
  .metric-val.cyan {{ color: #38bdf8; }}
  .metric-val.amber {{ color: #fbbf24; }}
  .metric-label {{
    font-size: 10px;
    font-weight: 600;
    color: #94a3b8;
    text-transform: uppercase;
    letter-spacing: 0.04em;
    margin-top: 3px;
  }}

  /* Mock window for screenshots */
  .mock-window {{
    width: 100%;
    background: #161e2e;
    border: 1px solid #2d3748;
    border-radius: 10px;
    overflow: hidden;
    box-shadow: 0 16px 32px rgba(0,0,0,0.4);
    display: flex;
    flex-direction: column;
  }}
  .window-bar {{
    background: #0f1523;
    padding: 6px 12px;
    display: flex;
    align-items: center;
    gap: 8px;
    border-bottom: 1px solid #1f293d;
  }}
  .window-dots {{
    display: flex;
    gap: 5px;
  }}
  .dot {{
    width: 8px;
    height: 8px;
    border-radius: 50%;
  }}
  .dot.red {{ background: #ef4444; }}
  .dot.yellow {{ background: #f59e0b; }}
  .dot.green {{ background: #10b981; }}
  .window-title {{
    font-size: 10.5px;
    color: #94a3b8;
    font-family: monospace;
    flex: 1;
    text-align: center;
    margin-right: 35px;
  }}
  .window-img {{
    width: 100%;
    height: auto;
    display: block;
    object-fit: contain;
    max-height: 120mm;
    background: #000;
  }}

  /* Footer styling */
  .slide-footer {{
    display: flex;
    justify-content: space-between;
    align-items: center;
    border-top: 1px solid #1e293b;
    padding-top: 3mm;
    margin-top: 4mm;
    font-size: 11px;
    color: #64748b;
  }}
  .footer-left {{
    display: flex;
    align-items: center;
    gap: 12px;
  }}
  .footer-dot {{
    width: 4px;
    height: 4px;
    border-radius: 50%;
    background: #475569;
  }}
  .footer-author {{
    color: #cbd5e1;
    font-weight: 600;
  }}
  .footer-page {{
    color: #818cf8;
    font-weight: 700;
  }}

  /* COVER SLIDE SPECIAL */
  .cover-slide {{
    display: flex;
    flex-direction: column;
    justify-content: space-between;
    padding: 20mm 24mm 16mm 24mm;
    background: radial-gradient(circle at 80% 20%, rgba(99, 102, 241, 0.12) 0%, transparent 60%),
                radial-gradient(circle at 10% 90%, rgba(56, 189, 248, 0.08) 0%, transparent 50%),
                #0b0f19;
  }}
  .cover-badge {{
    display: inline-flex;
    align-items: center;
    gap: 8px;
    padding: 6px 14px;
    border-radius: 999px;
    background: rgba(99, 102, 241, 0.15);
    border: 1px solid rgba(99, 102, 241, 0.4);
    color: #a5b4fc;
    font-size: 12px;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.08em;
  }}
  .cover-hero {{
    margin: auto 0;
    max-width: 90%;
  }}
  .cover-title {{
    font-size: 42px;
    font-weight: 900;
    line-height: 1.15;
    letter-spacing: -0.03em;
    color: #ffffff;
    margin-bottom: 14px;
  }}
  .cover-title span {{
    background: linear-gradient(135deg, #60a5fa 0%, #a855f7 100%);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
  }}
  .cover-desc {{
    font-size: 16px;
    color: #94a3b8;
    line-height: 1.55;
    max-width: 800px;
    margin-bottom: 24px;
  }}
  .cover-meta {{
    display: flex;
    gap: 24px;
    align-items: center;
    border-top: 1px solid #1e293b;
    padding-top: 16px;
  }}
  .cover-author-card {{
    display: flex;
    align-items: center;
    gap: 12px;
  }}
  .author-avatar {{
    width: 44px;
    height: 44px;
    border-radius: 50%;
    background: linear-gradient(135deg, #4f46e5, #06b6d4);
    display: flex;
    align-items: center;
    justify-content: center;
    font-weight: 800;
    font-size: 16px;
    color: #fff;
  }}
  .author-details h4 {{
    font-size: 15px;
    font-weight: 700;
    color: #f8fafc;
  }}
  .author-details p {{
    font-size: 12px;
    color: #94a3b8;
  }}
  .tech-pills {{
    display: flex;
    gap: 8px;
    margin-left: auto;
  }}
  .pill {{
    padding: 5px 12px;
    border-radius: 6px;
    font-size: 11px;
    font-weight: 600;
    background: #161e2e;
    color: #cbd5e1;
    border: 1px solid #283548;
  }}

  /* Pros / Cons grid */
  .tradeoffs-grid {{
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 16px;
    width: 100%;
    height: 100%;
  }}
  .tradeoff-card {{
    background: #131926;
    border-radius: 10px;
    padding: 16px;
    display: flex;
    flex-direction: column;
    gap: 10px;
  }}
  .tradeoff-card.pros {{
    border: 1px solid rgba(16, 185, 129, 0.35);
  }}
  .tradeoff-card.cons {{
    border: 1px solid rgba(245, 158, 11, 0.35);
  }}
  .tradeoff-header {{
    font-size: 14px;
    font-weight: 800;
    display: flex;
    align-items: center;
    gap: 8px;
    text-transform: uppercase;
    letter-spacing: 0.05em;
  }}
  .tradeoff-card.pros .tradeoff-header {{ color: #34d399; }}
  .tradeoff-card.cons .tradeoff-header {{ color: #fbbf24; }}
  .tradeoff-item {{
    background: #0f1420;
    border: 1px solid #1c2638;
    border-radius: 8px;
    padding: 10px 12px;
  }}
  .tradeoff-item h5 {{
    font-size: 12.5px;
    font-weight: 700;
    color: #f1f5f9;
    margin-bottom: 3px;
  }}
  .tradeoff-item p {{
    font-size: 11.5px;
    color: #94a3b8;
    line-height: 1.4;
  }}

  /* Roadmap list */
  .roadmap-grid {{
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 14px;
    width: 100%;
  }}
  .roadmap-card {{
    background: #131926;
    border: 1px solid #1e293b;
    border-radius: 10px;
    padding: 14px 16px;
  }}
  .roadmap-card.done {{
    border-left: 4px solid #10b981;
  }}
  .roadmap-card.upcoming {{
    border-left: 4px solid #818cf8;
  }}
  .roadmap-status {{
    font-size: 10px;
    font-weight: 700;
    text-transform: uppercase;
    letter-spacing: 0.06em;
    margin-bottom: 4px;
  }}
  .roadmap-card.done .roadmap-status {{ color: #34d399; }}
  .roadmap-card.upcoming .roadmap-status {{ color: #818cf8; }}
  .roadmap-title {{
    font-size: 13.5px;
    font-weight: 700;
    color: #ffffff;
    margin-bottom: 4px;
  }}
  .roadmap-desc {{
    font-size: 12px;
    color: #94a3b8;
    line-height: 1.4;
  }}
</style>
</head>
<body>

<!-- SLIDE 1: COVER -->
<div class="slide cover-slide">
  <div>
    <div class="cover-badge">
      <span>●</span> S TOM'S Platform — Engineering Sprint Showcase
    </div>
  </div>

  <div class="cover-hero">
    <h1 class="cover-title">
      Building a Real-Time Live Vector PDF Designer & Modernizing Desktop UX
    </h1>
    <p class="cover-desc">
      How we replaced rigid modal popups with a 250ms debounced live GDI+ vector preview engine, revamped data tables with modern rounded bootstrap aesthetics, and fixed theme contrast regressions in C# .NET 8.
    </p>
  </div>

  <div>
    <div class="cover-meta">
      <div class="cover-author-card">
        <div class="author-avatar">HJ</div>
        <div class="author-details">
          <h4>Hardik Jariwala</h4>
          <p>Full-Stack Developer (UI/UX & Engine) • S TOM'S Platform</p>
        </div>
      </div>
      <div class="tech-pills">
        <div class="pill">C# .NET 8</div>
        <div class="pill">WinForms Native</div>
        <div class="pill">GDI+ Vector Engine</div>
        <div class="pill">PDFsharp</div>
        <div class="pill">SQLite</div>
      </div>
    </div>
  </div>
</div>

<!-- SLIDE 2: THE PROBLEM & TODAY'S GOALS -->
<div class="slide">
  <div class="slide-header">
    <div>
      <div class="slide-tag">Sprint Context & Overview</div>
      <h2 class="slide-title">The Challenge: Eliminating Modal Friction & Visual Clutter</h2>
      <p class="slide-subtitle">Solving real daily pain points for service business operators</p>
    </div>
    <div class="header-brand">S TOM'S Platform<br><span>Sprint 2026-10-25</span></div>
  </div>

  <div class="slide-body">
    <div class="col-text">
      <div class="info-card">
        <div class="info-card-title">🚨 1. The Popup Dialog Pain Point</div>
        <p>Previously, updating invoice branding required opening a secondary modal dialog. Admins had to type blindly, click save, exit, export a sample PDF, notice a typo or misaligned logo, and repeat the entire loop. It broke user flow completely.</p>
      </div>

      <div class="info-card">
        <div class="info-card-title">📋 2. Legacy Data Table Appearance</div>
        <p>The Customers register felt rigid and dated. Sharp square corners, lack of inline edit/delete actions, and default Windows scrollbars made a modern desktop feel like an old legacy utility.</p>
      </div>

      <div class="info-card">
        <div class="info-card-title">☀️ 3. Light Mode Contrast Regression</div>
        <p>Switching to Light Mode caused company preview details to disappear completely because text colors were hardcoded with Dark Mode white tints over a white card background.</p>
      </div>
    </div>

    <div class="col-visual" style="flex: 1.1; justify-content: space-between;">
      <div class="info-card" style="width: 100%; border-color: rgba(99, 102, 241, 0.4);">
        <div class="info-card-title" style="color: #818cf8;">🎯 Today's Engineering Mandate</div>
        <ul>
          <li><strong>Single-Screen Live Studio:</strong> Replace the modal popup with a side-by-side editing & live preview canvas.</li>
          <li><strong>Zero-Lag Vector Canvas:</strong> Render real-time invoice previews in RAM without blocking UI threads.</li>
          <li><strong>Table UX Polish:</strong> Add soft rounded borders, inline action buttons, and custom smooth scrolling.</li>
          <li><strong>Universal Theme Harmony:</strong> Guarantee 100% text readability across both Light and Dark themes.</li>
        </ul>
      </div>

      <div class="metric-row" style="width: 100%; margin-top: 10px;">
        <div class="metric-box">
          <div class="metric-val green">101/101</div>
          <div class="metric-label">Tests Passed</div>
        </div>
        <div class="metric-box">
          <div class="metric-val indigo">250ms</div>
          <div class="metric-label">Debounced Lag</div>
        </div>
        <div class="metric-box">
          <div class="metric-val cyan">60 FPS</div>
          <div class="metric-label">UI Fluidity</div>
        </div>
        <div class="metric-box">
          <div class="metric-val amber">0</div>
          <div class="metric-label">Build Warnings</div>
        </div>
      </div>
    </div>
  </div>

  <div class="slide-footer">
    <div class="footer-left">
      <span class="footer-author">Hardik Jariwala</span>
      <span class="footer-dot"></span>
      <span>S TOM'S Platform — Engineering Sprint</span>
    </div>
    <div class="footer-page">Slide 2 of 9</div>
  </div>
</div>

<!-- SLIDE 3: CUSTOMER REGISTER TABLE POLISH -->
<div class="slide">
  <div class="slide-header">
    <div>
      <div class="slide-tag green">Feature 01: Presentation Polish</div>
      <h2 class="slide-title">Modernizing the Customer Register Table</h2>
      <p class="slide-subtitle">Bringing modern web SaaS table aesthetics to native Windows Forms</p>
    </div>
    <div class="header-brand">S TOM'S Platform<br><span>UI/UX Architecture</span></div>
  </div>

  <div class="slide-body">
    <div class="col-text">
      <div class="info-card">
        <div class="info-card-title">✨ Soft Rounded Container</div>
        <p>Encapsulated the customer data grid inside a custom <code>RoundedPanel</code> with a 12px border radius, subtle Slate borders, and padding that eliminates harsh rectangular edges.</p>
      </div>

      <div class="info-card">
        <div class="info-card-title">⚡ Inline Quick Actions (Edit & Delete)</div>
        <p>Added dedicated row-level action buttons (<code>✏️ Edit</code> and <code>🗑️ Delete</code>) directly into each table row. Operators no longer need to hunt through contextual right-click menus.</p>
      </div>

      <div class="info-card">
        <div class="info-card-title">📜 Smooth Custom Scrollbar</div>
        <p>Eliminated the harsh default Windows 3D scrollbar with a sleek, minimalist custom scrollbar control that synchronizes perfectly with mousewheel and arrow key navigation.</p>
      </div>

      <div class="info-card">
        <div class="info-card-title">🟢 Live Status Pill Badges</div>
        <p>Customer status is rendered as rounded pill badges with rich color accents (Emerald for Active, Amber for Inactive/Pending) for instant scannability.</p>
      </div>
    </div>

    <div class="col-visual">
      <div class="mock-window">
        <div class="window-bar">
          <div class="window-dots"><div class="dot red"></div><div class="dot yellow"></div><div class="dot green"></div></div>
          <div class="window-title">Customers Register — Modern Rounded Card & Actions</div>
        </div>
        <img class="window-img" src="{img_customer}" alt="Customer Table Polish">
      </div>
    </div>
  </div>

  <div class="slide-footer">
    <div class="footer-left">
      <span class="footer-author">Hardik Jariwala</span>
      <span class="footer-dot"></span>
      <span>S TOM'S Platform — Customers Module</span>
    </div>
    <div class="footer-page">Slide 3 of 9</div>
  </div>
</div>

<!-- SLIDE 4: SINGLE SCREEN INVOICE DESIGNER -->
<div class="slide">
  <div class="slide-header">
    <div>
      <div class="slide-tag">Feature 02: Core Workflow</div>
      <h2 class="slide-title">The Single-Screen Live PDF Designer</h2>
      <p class="slide-subtitle">Eliminating modal popups with a high-efficiency 50/50 split-view studio</p>
    </div>
    <div class="header-brand">S TOM'S Platform<br><span>Live PDF Designer</span></div>
  </div>

  <div class="slide-body">
    <div class="col-text">
      <div class="info-card">
        <div class="info-card-title">📐 Split-Screen Studio Architecture</div>
        <p>Instead of hiding inputs in a popup, <code>InvoiceDesignerForm</code> splits the screen evenly: editing controls stay anchored on the left while a live vector preview canvas occupies the right.</p>
      </div>

      <div class="info-card">
        <div class="info-card-title">🗂️ 3 Structured Grouping Cards</div>
        <ul>
          <li><strong>Business Information:</strong> Company Name, Tagline, GSTIN, Address, City, Country, Phone, Email.</li>
          <li><strong>Bank & Payment Details:</strong> Bank Name, Account Number, IFSC Code, UPI ID for instant QR payments.</li>
          <li><strong>Branding & Identity:</strong> Logo upload with browse/clear actions + Custom Accent Color palette selector.</li>
        </ul>
      </div>

      <div class="info-card">
        <div class="info-card-title">🔍 Interactive Canvas Controls</div>
        <p>Integrated real-time canvas zoom (<code>Fit Page</code>, <code>Zoom In (+)</code>, <code>Zoom Out (-)</code>, percentage display) and an active invoice selector to preview real client invoices instantly.</p>
      </div>
    </div>

    <div class="col-visual">
      <div class="mock-window">
        <div class="window-bar">
          <div class="window-dots"><div class="dot red"></div><div class="dot yellow"></div><div class="dot green"></div></div>
          <div class="window-title">InvoiceDesignerForm.cs — Split-Pane Live Studio</div>
        </div>
        <img class="window-img" src="{img_designer_live}" alt="Invoice Designer Live Studio">
      </div>
    </div>
  </div>

  <div class="slide-footer">
    <div class="footer-left">
      <span class="footer-author">Hardik Jariwala</span>
      <span class="footer-dot"></span>
      <span>S TOM'S Platform — Live PDF Designer</span>
    </div>
    <div class="footer-page">Slide 4 of 9</div>
  </div>
</div>

<!-- SLIDE 5: ENGINEERING DEEP DIVE (GDI+ & DEBOUNCE) -->
<div class="slide">
  <div class="slide-header">
    <div>
      <div class="slide-tag cyan">Architecture Deep Dive</div>
      <h2 class="slide-title">How the Live Vector Preview Engine Works</h2>
      <p class="slide-subtitle">High-fidelity vector graphics without disk thrashing or UI stutter</p>
    </div>
    <div class="header-brand">S TOM'S Platform<br><span>GDI+ & In-Memory Rendering</span></div>
  </div>

  <div class="slide-body">
    <div class="col-text">
      <div class="info-card">
        <div class="info-card-title">⚡ The Problem with Direct PDF Generation</div>
        <p>Writing a physical PDF via PDFsharp on every keystroke takes 120-250ms of disk I/O, creates file lock contention, and freezes the WinForms UI thread during typing.</p>
      </div>

      <div class="info-card">
        <div class="info-card-title">🧠 Dual-Engine In-Memory Model</div>
        <p>We created <code>InvoicePdfService.RenderInvoiceBitmap()</code>, a pure GDI+ vector renderer that reproduces the exact coordinates, font metrics, accent bars, and tables of PDFsharp directly into an in-memory Bitmap buffer. Zero disk I/O.</p>
      </div>

      <div class="info-card">
        <div class="info-card-title">⏱️ 250ms Keystroke Debouncing</div>
        <p>Every keystroke in the company name, address, or bank inputs resets a 250ms debounce timer. The vector canvas only redraws once the operator pauses, guaranteeing silky smooth 60 FPS typing.</p>
      </div>

      <div class="info-card">
        <div class="info-card-title">💾 Seamless SQLite Persistence</div>
        <p>Clicking <code>Save Changes</code> atomically commits the brand settings to local SQLite storage and propagates updates across the entire application.</p>
      </div>
    </div>

    <div class="col-visual">
      <div class="mock-window">
        <div class="window-bar">
          <div class="window-dots"><div class="dot red"></div><div class="dot yellow"></div><div class="dot green"></div></div>
          <div class="window-title">GDI+ Vector Canvas — Pixel-Perfect Invoice Mirror</div>
        </div>
        <img class="window-img" src="{img_designer_full}" alt="GDI+ Vector Preview Canvas">
      </div>
    </div>
  </div>

  <div class="slide-footer">
    <div class="footer-left">
      <span class="footer-author">Hardik Jariwala</span>
      <span class="footer-dot"></span>
      <span>S TOM'S Platform — Graphics Engine</span>
    </div>
    <div class="footer-page">Slide 5 of 9</div>
  </div>
</div>

<!-- SLIDE 6: THEME ENGINE & LIGHT MODE CONTRAST FIX -->
<div class="slide">
  <div class="slide-header">
    <div>
      <div class="slide-tag amber">Bug Fix & Theme Polish</div>
      <h2 class="slide-title">Light Mode Text Readability & Live Sync</h2>
      <p class="slide-subtitle">Eliminating invisible white-on-white text regressions across theme toggles</p>
    </div>
    <div class="header-brand">S TOM'S Platform<br><span>ThemeManager System</span></div>
  </div>

  <div class="slide-body">
    <div class="col-text">
      <div class="info-card">
        <div class="info-card-title">🐛 What Caused the Bug?</div>
        <p>The company preview card in Settings was initially written with hardcoded Dark Mode tints (<code>#F1F5F9</code> white and <code>#94A3B8</code> light slate). When users switched to Light Mode, the card turned white but the text remained white, becoming completely invisible.</p>
      </div>

      <div class="info-card">
        <div class="info-card-title">🎨 High-Contrast Slate Hierarchy</div>
        <ul>
          <li><strong>Business Title:</strong> Slate 900 (<code>#0F172A</code>) — Bold, high-contrast dark text.</li>
          <li><strong>Contact & Bank Values:</strong> Slate 800 (<code>#1E293B</code>) — Crisp dark body text.</li>
          <li><strong>Section Headers:</strong> Slate 600 (<code>#475569</code>) — Clear uppercase labels.</li>
          <li><strong>Address:</strong> Slate 500 (<code>#64748B</code>) — Subtle secondary text.</li>
          <li><strong>Tagline & UPI:</strong> Indigo 600 (<code>#4F46E5</code>) | <strong>GSTIN:</strong> Emerald 600 (<code>#059669</code>).</li>
        </ul>
      </div>

      <div class="info-card">
        <div class="info-card-title">🔄 Dynamic Theme Synchronization</div>
        <p>Wired <code>_settingsCompanyPreviewPanel</code> and <code>_lblSettingsCurrentTheme</code> directly into <code>ApplyThemeColors()</code> so toggling the theme switch immediately updates the preview without restarting the app.</p>
      </div>
    </div>

    <div class="col-visual">
      <div class="mock-window">
        <div class="window-bar">
          <div class="window-dots"><div class="dot red"></div><div class="dot yellow"></div><div class="dot green"></div></div>
          <div class="window-title">Settings Screen — Light Mode Contrast Resolution</div>
        </div>
        <img class="window-img" src="{img_light_mode}" alt="Light Mode Contrast Resolution">
      </div>
    </div>
  </div>

  <div class="slide-footer">
    <div class="footer-left">
      <span class="footer-author">Hardik Jariwala</span>
      <span class="footer-dot"></span>
      <span>S TOM'S Platform — Settings Module</span>
    </div>
    <div class="footer-page">Slide 6 of 9</div>
  </div>
</div>

<!-- SLIDE 7: ENGINEERING TRADEOFFS (PROS & CONS) -->
<div class="slide">
  <div class="slide-header">
    <div>
      <div class="slide-tag">Engineering Trade-offs</div>
      <h2 class="slide-title">Honest Architecture Review: Pros & Cons</h2>
      <p class="slide-subtitle">Evaluating our engineering decisions with transparent, practical trade-offs</p>
    </div>
    <div class="header-brand">S TOM'S Platform<br><span>Tech Assessment</span></div>
  </div>

  <div class="slide-body">
    <div class="tradeoffs-grid">
      <!-- PROS -->
      <div class="tradeoff-card pros">
        <div class="tradeoff-header">
          <span>✓</span> Key Engineering Advantages (Pros)
        </div>

        <div class="tradeoff-item">
          <h5>⚡ Blazing Native Performance</h5>
          <p>Zero Electron bloat. The application launches in under 400ms, consumes less than 65MB RAM, and handles vector graphics with 60 FPS hardware acceleration.</p>
        </div>

        <div class="tradeoff-item">
          <h5>🔒 100% Offline-First Privacy</h5>
          <p>All invoice and customer data resides in local SQLite (<code>sales_billing.db</code>). Perfect for businesses requiring full compliance with zero cloud leaks or downtime.</p>
        </div>

        <div class="tradeoff-item">
          <h5>🎯 WYSIWYG Precision</h5>
          <p>The GDI+ in-memory vector canvas mirrors PDFsharp output pixel-for-pixel, preventing formatting surprises when exporting client tax invoices.</p>
        </div>

        <div class="tradeoff-item">
          <h5>🧪 101 Automated Unit Tests</h5>
          <p>Automated test suite covers financial math, tax tiers, payment transitions, SQLite persistence, and vector scaling with a 100% pass rate.</p>
        </div>
      </div>

      <!-- CONS -->
      <div class="tradeoff-card cons">
        <div class="tradeoff-header">
          <span>⚠️</span> Technical Constraints & Trade-Offs (Cons)
        </div>

        <div class="tradeoff-item">
          <h5>⚠️ Dual Rendering Maintenance</h5>
          <p>Maintaining both GDI+ (screen) and PDFsharp (file) layout code means layout coordinate changes must be mirrored across both rendering pipelines.</p>
        </div>

        <div class="tradeoff-item">
          <h5>⚠️ Windows OS Exclusive</h5>
          <p>Built with Windows Forms on .NET 8, meaning macOS and Linux users cannot run the desktop client natively without virtualization.</p>
        </div>

        <div class="tradeoff-item">
          <h5>⚠️ Single-Terminal Concurrency</h5>
          <p>Local SQLite is ideal for single-operator machines, but large multi-counter retail setups will require a client-server database (e.g. PostgreSQL or SQL Server).</p>
        </div>

        <div class="tradeoff-item">
          <h5>⚠️ Manual Print Scaling on Rare DPIs</h5>
          <p>High-DPI monitors require careful GDI+ GraphicsContainer scaling to prevent font blurriness on custom 175% Windows display scaling.</p>
        </div>
      </div>
    </div>
  </div>

  <div class="slide-footer">
    <div class="footer-left">
      <span class="footer-author">Hardik Jariwala</span>
      <span class="footer-dot"></span>
      <span>S TOM'S Platform — Engineering Review</span>
    </div>
    <div class="footer-page">Slide 7 of 9</div>
  </div>
</div>

<!-- SLIDE 8: SPRINT STATUS & UPCOMING ROADMAP -->
<div class="slide">
  <div class="slide-header">
    <div>
      <div class="slide-tag green">Sprint Status & Roadmap</div>
      <h2 class="slide-title">Where We Stand & What's Left to Build</h2>
      <p class="slide-subtitle">Clear visibility into current deliverables and upcoming sprint milestones</p>
    </div>
    <div class="header-brand">S TOM'S Platform<br><span>Milestone Roadmap</span></div>
  </div>

  <div class="slide-body">
    <div class="roadmap-grid">
      <!-- COMPLETED 1 -->
      <div class="roadmap-card done">
        <div class="roadmap-status">✓ Completed & Verified</div>
        <div class="roadmap-title">Single-Screen Live Invoice Designer</div>
        <div class="roadmap-desc">Split-screen branding studio, 250ms debounced GDI+ vector preview, live zoom controls, and persistent SQLite profile storage.</div>
      </div>

      <!-- COMPLETED 2 -->
      <div class="roadmap-card done">
        <div class="roadmap-status">✓ Completed & Verified</div>
        <div class="roadmap-title">Customer Register UX Modernization</div>
        <div class="roadmap-desc">12px rounded cards, inline row actions (Edit/Delete with safety confirmations), smooth custom vertical scrollbar, and status badges.</div>
      </div>

      <!-- COMPLETED 3 -->
      <div class="roadmap-card done">
        <div class="roadmap-status">✓ Completed & Verified</div>
        <div class="roadmap-title">High-Contrast Light/Dark Theme Engine</div>
        <div class="roadmap-desc">Slate color hierarchy fixing white-on-white text in Light Mode, plus dynamic real-time theme synchronization without form reloads.</div>
      </div>

      <!-- COMPLETED 4 -->
      <div class="roadmap-card done">
        <div class="roadmap-status">✓ Completed & Verified</div>
        <div class="roadmap-title">101/101 Automated Unit Tests</div>
        <div class="roadmap-desc">Zero warnings, zero build errors. Complete automated verification across taxes, discounts, SQLite CRUD, and graphics scaling.</div>
      </div>

      <!-- UPCOMING 1 -->
      <div class="roadmap-card upcoming">
        <div class="roadmap-status">⏳ Next Sprint Priority</div>
        <div class="roadmap-title">Direct SMTP Email Invoice Delivery</div>
        <div class="roadmap-desc">One-click "Send to Customer" button that generates the PDF and sends it directly via configured SMTP with custom email templates.</div>
      </div>

      <!-- UPCOMING 2 -->
      <div class="roadmap-card upcoming">
        <div class="roadmap-status">⏳ Next Sprint Priority</div>
        <div class="roadmap-title">GSTR-1 Tax JSON Portal Export</div>
        <div class="roadmap-desc">Automated monthly sales tax aggregation exported directly in Indian GST government portal-compliant JSON format.</div>
      </div>

      <!-- UPCOMING 3 -->
      <div class="roadmap-card upcoming">
        <div class="roadmap-status">⏳ Upcoming Milestone</div>
        <div class="roadmap-title">Multi-Template Preset Gallery</div>
        <div class="roadmap-desc">Allow operators to toggle between Minimalist Modern, Corporate Formal, and Compact 80mm Thermal Receipt layouts with 1 click.</div>
      </div>

      <!-- UPCOMING 4 -->
      <div class="roadmap-card upcoming">
        <div class="roadmap-status">⏳ Future Roadmap</div>
        <div class="roadmap-title">Centralized Multi-Terminal Sync</div>
        <div class="roadmap-desc">Cloud/LAN sync engine allowing multiple checkout billing counters to share real-time customer and inventory databases.</div>
      </div>
    </div>
  </div>

  <div class="slide-footer">
    <div class="footer-left">
      <span class="footer-author">Hardik Jariwala</span>
      <span class="footer-dot"></span>
      <span>S TOM'S Platform — Sprint Roadmap</span>
    </div>
    <div class="footer-page">Slide 8 of 9</div>
  </div>
</div>

<!-- SLIDE 9: CLOSING & SUMMARY -->
<div class="slide cover-slide" style="justify-content: space-between;">
  <div>
    <div class="cover-badge">
      <span>●</span> Engineering Showcase Summary
    </div>
  </div>

  <div class="cover-hero">
    <h2 class="cover-title" style="font-size: 36px;">
      Desktop Software Doesn't Have to Feel Like 2005.
    </h2>
    <p class="cover-desc" style="font-size: 15px; max-width: 850px;">
      With modern C# .NET 8, clean architecture layer separation, and obsessive attention to UI ergonomics, desktop applications can be just as sleek, responsive, and delightful as the best modern web SaaS — while remaining 10x faster and 100% offline-ready.
    </p>
    <div style="margin-top: 18px; display: flex; gap: 12px; flex-wrap: wrap;">
      <span class="pill" style="background: rgba(99, 102, 241, 0.2); color: #a5b4fc; border-color: rgba(99, 102, 241, 0.4);">#DotNet8</span>
      <span class="pill" style="background: rgba(16, 185, 129, 0.2); color: #6ee7b7; border-color: rgba(16, 185, 129, 0.4);">#WinForms</span>
      <span class="pill" style="background: rgba(56, 189, 248, 0.2); color: #7dd3fc; border-color: rgba(56, 189, 248, 0.4);">#SoftwareEngineering</span>
      <span class="pill" style="background: rgba(245, 158, 11, 0.2); color: #fde68a; border-color: rgba(245, 158, 11, 0.4);">#BuildingInPublic</span>
      <span class="pill">#DesktopApp</span>
      <span class="pill">#CleanArchitecture</span>
      <span class="pill">#UIUXDesign</span>
    </div>
  </div>

  <div>
    <div class="cover-meta">
      <div class="cover-author-card">
        <div class="author-avatar">HJ</div>
        <div class="author-details">
          <h4>Hardik Jariwala</h4>
          <p>Full-Stack Developer (UI/UX & Engine) • S TOM'S Platform Team</p>
        </div>
      </div>
      <div style="margin-left: auto; text-align: right;">
        <p style="font-size: 12px; color: #94a3b8;">Repository: <strong style="color: #cbd5e1;">Saumya25-hub/TEAM-SOFT-SB1</strong></p>
        <p style="font-size: 11px; color: #64748b;">Ready for LinkedIn Carousel & PDF Document Sharing</p>
      </div>
    </div>
  </div>
</div>

</body>
</html>
"""

with open(HTML_PATH, "w", encoding="utf-8") as f:
    f.write(html_content)

print(f"Generated HTML at {HTML_PATH}")

cmd = [
    EDGE_PATH,
    "--headless",
    "--disable-gpu",
    "--run-all-compositor-stages-before-draw",
    f"--print-to-pdf={PDF_PATH}",
    HTML_PATH
]

print("Converting to PDF via Microsoft Edge...")
result = subprocess.run(cmd, capture_output=True, text=True)
if os.path.exists(PDF_PATH):
    size_kb = os.path.getsize(PDF_PATH) / 1024
    print(f"SUCCESS: PDF created at {PDF_PATH} ({size_kb:.1f} KB)")
else:
    print("FAILED: PDF not found. Error output:", result.stderr)
