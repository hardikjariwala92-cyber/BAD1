using System.Drawing;
using System.Windows.Forms;
using PROJECT_SBV1.Controls;

namespace PROJECT_SBV1.Forms
{
    partial class AddCustomerOrder
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                PROJECT_SBV1.Theme.ThemeManager.OnThemeChanged -= HandleThemeChanged;
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            _pnlHeader = new RoundedPanel();
            _pnlIconBadge = new Panel();
            _lblHeaderTitle = new Label();
            _lblHeaderSubtitle = new Label();
            _pnlOrderDetails = new RoundedPanel();
            _lblSectionTag = new Label();
            _lblCustomerName = new Label();
            _lblCustomerHint = new Label();
            _cmbCustomer = new ComboBox();
            _lblAddress = new Label();
            _lblAddressHint = new Label();
            _txtAddress = new TextBox();
            _lblPrice = new Label();
            _lblPriceHint = new Label();
            _txtPrice = new TextBox();
            _lblStatus = new Label();
            _lblStatusHint = new Label();
            _cmbStatus = new ComboBox();
            _lblOrderDate = new Label();
            _lblDateHint = new Label();
            _dtpOrderDate = new DateTimePicker();
            _lblValidation = new Label();
            _btnClear = new ModernButton();
            _btnCancel = new ModernButton();
            _btnSave = new ModernButton();

            _pnlHeader.SuspendLayout();
            _pnlOrderDetails.SuspendLayout();
            SuspendLayout();

            // 
            // _pnlHeader (Top Brand / Title Banner)
            // 
            _pnlHeader.Location = new Point(18, 14);
            _pnlHeader.Size = new Size(504, 66);
            _pnlHeader.BorderRadius = 12;
            _pnlHeader.BorderWidth = 1;
            _pnlHeader.Controls.Add(_pnlIconBadge);
            _pnlHeader.Controls.Add(_lblHeaderTitle);
            _pnlHeader.Controls.Add(_lblHeaderSubtitle);

            // 
            // _pnlIconBadge
            // 
            _pnlIconBadge.Location = new Point(14, 11);
            _pnlIconBadge.Size = new Size(44, 44);
            _pnlIconBadge.BackColor = Color.Transparent;

            // 
            // _lblHeaderTitle
            // 
            _lblHeaderTitle.Location = new Point(68, 11);
            _lblHeaderTitle.Size = new Size(420, 24);
            _lblHeaderTitle.Text = "Add New Customer Order";
            _lblHeaderTitle.Font = new Font("Segoe UI", 13f, FontStyle.Bold);

            // 
            // _lblHeaderSubtitle
            // 
            _lblHeaderSubtitle.Location = new Point(69, 36);
            _lblHeaderSubtitle.Size = new Size(420, 18);
            _lblHeaderSubtitle.Text = "Register customer orders to update live sales analytics.";
            _lblHeaderSubtitle.Font = new Font("Segoe UI", 8.5f);

            // 
            // _pnlOrderDetails (Main Form Card)
            // 
            _pnlOrderDetails.Location = new Point(18, 88);
            _pnlOrderDetails.Size = new Size(504, 356);
            _pnlOrderDetails.BorderRadius = 12;
            _pnlOrderDetails.BorderWidth = 1;
            _pnlOrderDetails.Controls.Add(_lblSectionTag);
            _pnlOrderDetails.Controls.Add(_lblCustomerName);
            _pnlOrderDetails.Controls.Add(_lblCustomerHint);
            _pnlOrderDetails.Controls.Add(_cmbCustomer);
            _pnlOrderDetails.Controls.Add(_lblAddress);
            _pnlOrderDetails.Controls.Add(_lblAddressHint);
            _pnlOrderDetails.Controls.Add(_txtAddress);
            _pnlOrderDetails.Controls.Add(_lblPrice);
            _pnlOrderDetails.Controls.Add(_lblPriceHint);
            _pnlOrderDetails.Controls.Add(_txtPrice);
            _pnlOrderDetails.Controls.Add(_lblStatus);
            _pnlOrderDetails.Controls.Add(_lblStatusHint);
            _pnlOrderDetails.Controls.Add(_cmbStatus);
            _pnlOrderDetails.Controls.Add(_lblOrderDate);
            _pnlOrderDetails.Controls.Add(_lblDateHint);
            _pnlOrderDetails.Controls.Add(_dtpOrderDate);
            _pnlOrderDetails.Controls.Add(_lblValidation);

            // 
            // _lblSectionTag
            // 
            _lblSectionTag.Location = new Point(18, 12);
            _lblSectionTag.Size = new Size(468, 18);
            _lblSectionTag.Text = "📦  ORDER SPECIFICATIONS";
            _lblSectionTag.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);

            // 
            // _lblCustomerName
            // 
            _lblCustomerName.Location = new Point(18, 36);
            _lblCustomerName.AutoSize = true;
            _lblCustomerName.Text = "Customer Name *";
            _lblCustomerName.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

            // 
            // _lblCustomerHint
            // 
            _lblCustomerHint.Location = new Point(220, 36);
            _lblCustomerHint.Size = new Size(266, 16);
            _lblCustomerHint.Text = "Select or type customer name";
            _lblCustomerHint.Font = new Font("Segoe UI", 8f);
            _lblCustomerHint.TextAlign = ContentAlignment.TopRight;

            // 
            // _cmbCustomer
            // 
            _cmbCustomer.Location = new Point(18, 56);
            _cmbCustomer.Size = new Size(468, 28);
            _cmbCustomer.Font = new Font("Segoe UI", 9.5f);
            _cmbCustomer.DropDownStyle = ComboBoxStyle.DropDown;
            _cmbCustomer.TabIndex = 0;

            // 
            // _lblAddress
            // 
            _lblAddress.Location = new Point(18, 92);
            _lblAddress.AutoSize = true;
            _lblAddress.Text = "City / Delivery Address *";
            _lblAddress.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

            // 
            // _lblAddressHint
            // 
            _lblAddressHint.Location = new Point(220, 92);
            _lblAddressHint.Size = new Size(266, 16);
            _lblAddressHint.Text = "Required for fulfillment";
            _lblAddressHint.Font = new Font("Segoe UI", 8f);
            _lblAddressHint.TextAlign = ContentAlignment.TopRight;

            // 
            // _txtAddress
            // 
            _txtAddress.Location = new Point(18, 112);
            _txtAddress.Size = new Size(468, 28);
            _txtAddress.Font = new Font("Segoe UI", 9.5f);
            _txtAddress.PlaceholderText = "Enter city or delivery address (e.g. London or Chicago)";
            _txtAddress.BorderStyle = BorderStyle.FixedSingle;
            _txtAddress.TabIndex = 1;

            // 
            // _lblPrice
            // 
            _lblPrice.Location = new Point(18, 148);
            _lblPrice.AutoSize = true;
            _lblPrice.Text = "Order Price (₹) *";
            _lblPrice.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

            // 
            // _lblPriceHint
            // 
            _lblPriceHint.Location = new Point(18, 166);
            _lblPriceHint.Size = new Size(224, 14);
            _lblPriceHint.Text = "Positive INR amount";
            _lblPriceHint.Font = new Font("Segoe UI", 7.8f);

            // 
            // _txtPrice
            // 
            _txtPrice.Location = new Point(18, 184);
            _txtPrice.Size = new Size(224, 28);
            _txtPrice.Font = new Font("Segoe UI", 9.5f);
            _txtPrice.PlaceholderText = "e.g. 950";
            _txtPrice.BorderStyle = BorderStyle.FixedSingle;
            _txtPrice.TabIndex = 2;

            // 
            // _lblStatus
            // 
            _lblStatus.Location = new Point(262, 148);
            _lblStatus.AutoSize = true;
            _lblStatus.Text = "Order Status *";
            _lblStatus.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

            // 
            // _lblStatusHint
            // 
            _lblStatusHint.Location = new Point(262, 166);
            _lblStatusHint.Size = new Size(224, 14);
            _lblStatusHint.Text = "Initial state";
            _lblStatusHint.Font = new Font("Segoe UI", 7.8f);

            // 
            // _cmbStatus
            // 
            _cmbStatus.Location = new Point(262, 184);
            _cmbStatus.Size = new Size(224, 28);
            _cmbStatus.Font = new Font("Segoe UI", 9.5f);
            _cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbStatus.FlatStyle = FlatStyle.Flat;
            _cmbStatus.TabIndex = 3;

            // 
            // _lblOrderDate
            // 
            _lblOrderDate.Location = new Point(18, 220);
            _lblOrderDate.AutoSize = true;
            _lblOrderDate.Text = "Order Date *";
            _lblOrderDate.Font = new Font("Segoe UI", 9f, FontStyle.Bold);

            // 
            // _lblDateHint
            // 
            _lblDateHint.Location = new Point(220, 220);
            _lblDateHint.Size = new Size(266, 16);
            _lblDateHint.Text = "Order placement date";
            _lblDateHint.Font = new Font("Segoe UI", 8f);
            _lblDateHint.TextAlign = ContentAlignment.TopRight;

            // 
            // _dtpOrderDate
            // 
            _dtpOrderDate.Location = new Point(18, 240);
            _dtpOrderDate.Size = new Size(468, 28);
            _dtpOrderDate.Font = new Font("Segoe UI", 9.5f);
            _dtpOrderDate.Format = DateTimePickerFormat.Custom;
            _dtpOrderDate.CustomFormat = "dd.MM.yyyy";
            _dtpOrderDate.TabIndex = 4;

            // 
            // _lblValidation
            // 
            _lblValidation.Location = new Point(18, 280);
            _lblValidation.Size = new Size(468, 44);
            _lblValidation.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            _lblValidation.TextAlign = ContentAlignment.MiddleLeft;
            _lblValidation.Padding = new Padding(10, 0, 10, 0);
            _lblValidation.Visible = false;

            // 
            // _btnClear
            // 
            _btnClear.Location = new Point(18, 458);
            _btnClear.Size = new Size(96, 36);
            _btnClear.Text = "↺ Clear";
            _btnClear.Font = new Font("Segoe UI", 9f);
            _btnClear.BorderRadius = 8;
            _btnClear.IsOutlined = false;
            _btnClear.TabIndex = 7;

            // 
            // _btnCancel
            // 
            _btnCancel.Location = new Point(292, 458);
            _btnCancel.Size = new Size(96, 36);
            _btnCancel.Text = "Cancel";
            _btnCancel.Font = new Font("Segoe UI", 9f);
            _btnCancel.BorderRadius = 8;
            _btnCancel.IsOutlined = false;
            _btnCancel.TabIndex = 6;

            // 
            // _btnSave
            // 
            _btnSave.Location = new Point(398, 458);
            _btnSave.Size = new Size(124, 36);
            _btnSave.Text = "Save Order";
            _btnSave.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            _btnSave.BorderRadius = 8;
            _btnSave.IsOutlined = false;
            _btnSave.TabIndex = 5;

            // 
            // AddCustomerOrder Form
            // 
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(540, 510);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            Text = "Add New Customer Order";
            DoubleBuffered = true;
            ShowIcon = false;
            ShowInTaskbar = false;

            Controls.Add(_pnlHeader);
            Controls.Add(_pnlOrderDetails);
            Controls.Add(_btnClear);
            Controls.Add(_btnCancel);
            Controls.Add(_btnSave);

            AcceptButton = _btnSave;
            CancelButton = _btnCancel;

            _pnlHeader.ResumeLayout(false);
            _pnlOrderDetails.ResumeLayout(false);
            _pnlOrderDetails.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private RoundedPanel _pnlHeader;
        private Panel _pnlIconBadge;
        private Label _lblHeaderTitle;
        private Label _lblHeaderSubtitle;
        private RoundedPanel _pnlOrderDetails;
        private Label _lblSectionTag;
        private Label _lblCustomerName;
        private Label _lblCustomerHint;
        private ComboBox _cmbCustomer;
        private Label _lblAddress;
        private Label _lblAddressHint;
        private TextBox _txtAddress;
        private Label _lblPrice;
        private Label _lblPriceHint;
        private TextBox _txtPrice;
        private Label _lblStatus;
        private Label _lblStatusHint;
        private ComboBox _cmbStatus;
        private Label _lblOrderDate;
        private Label _lblDateHint;
        private DateTimePicker _dtpOrderDate;
        private Label _lblValidation;
        private ModernButton _btnClear;
        private ModernButton _btnCancel;
        private ModernButton _btnSave;
    }
}
