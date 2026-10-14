namespace PROJECT_SBV1.Forms
{
    partial class LoginForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            // Main TableLayoutPanel Container (40% Left Branding / 60% Right Login)
            mainTableLayout = new TableLayoutPanel();
            leftPanel = new Panel();
            brandTitle = new Label();
            brandSub = new Label();
            rightPanel = new Panel();
            heading = new Label();
            subHeading = new Label();
            lblUser = new Label();
            userContainer = new PROJECT_SBV1.Controls.RoundedPanel();
            _txtUsername = new TextBox();
            lblPass = new Label();
            passContainer = new PROJECT_SBV1.Controls.RoundedPanel();
            _txtPassword = new TextBox();
            _chkShowPassword = new CheckBox();
            _lblError = new Label();
            btnSignIn = new PROJECT_SBV1.Controls.ModernButton();
            demoPill = new PROJECT_SBV1.Controls.RoundedPanel();
            lblHint = new Label();

            mainTableLayout.SuspendLayout();
            leftPanel.SuspendLayout();
            rightPanel.SuspendLayout();
            userContainer.SuspendLayout();
            passContainer.SuspendLayout();
            demoPill.SuspendLayout();
            SuspendLayout();

            // ----------------------------------------------------
            // mainTableLayout: 2 Columns (40% / 60%), Dock = Fill
            // ----------------------------------------------------
            mainTableLayout.ColumnCount = 2;
            mainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            mainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            mainTableLayout.RowCount = 1;
            mainTableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainTableLayout.Controls.Add(leftPanel, 0, 0);
            mainTableLayout.Controls.Add(rightPanel, 1, 0);
            mainTableLayout.Dock = DockStyle.Fill;
            mainTableLayout.Location = new Point(0, 0);
            mainTableLayout.Margin = new Padding(0);
            mainTableLayout.Name = "mainTableLayout";
            mainTableLayout.Size = new Size(920, 520);
            mainTableLayout.TabIndex = 0;

            // ----------------------------------------------------
            // leftPanel: Branding Panel (Column 0, 40% Width)
            // ----------------------------------------------------
            leftPanel.BackColor = Color.FromArgb(24, 26, 36);
            leftPanel.Dock = DockStyle.Fill;
            leftPanel.Location = new Point(0, 0);
            leftPanel.Margin = new Padding(0);
            leftPanel.Name = "leftPanel";
            leftPanel.Padding = new Padding(35, 30, 35, 30);
            leftPanel.Size = new Size(368, 520);
            leftPanel.TabIndex = 0;
            leftPanel.Paint += LeftPanel_Paint;

            // brandTitle: "Business Analytics"
            brandTitle.AutoSize = true;
            brandTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            brandTitle.ForeColor = Color.White;
            brandTitle.Location = new Point(35, 80);
            brandTitle.Name = "brandTitle";
            brandTitle.Size = new Size(230, 32);
            brandTitle.TabIndex = 0;
            brandTitle.Text = "Business Analytics";
            leftPanel.Controls.Add(brandTitle);

            // brandSub: Platform Description
            brandSub.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            brandSub.ForeColor = Color.FromArgb(150, 155, 195);
            brandSub.Location = new Point(35, 125);
            brandSub.Name = "brandSub";
            brandSub.Size = new Size(295, 75);
            brandSub.TabIndex = 1;
            brandSub.Text = "Comprehensive analytics platform for monitoring orders, revenues, subscriptions and real-time activity.";
            leftPanel.Controls.Add(brandSub);

            // Feature List inside leftPanel
            string[] features = {
                "✔ Live Light & Dark Theme",
                "✔ Real-time Metrics & Donut Charts",
                "✔ Sales Dynamics & User Activity",
                "✔ Visual Studio .NET 8.0 Windows Forms"
            };
            int featY = 220;
            for (int i = 0; i < features.Length; i++)
            {
                var lblFeat = new Label
                {
                    AutoSize = true,
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                    ForeColor = Color.FromArgb(170, 180, 220),
                    Location = new Point(35, featY),
                    Name = $"lblFeat_{i}",
                    Size = new Size(220, 15),
                    TabIndex = 2 + i,
                    Text = features[i],
                    UseMnemonic = false
                };
                leftPanel.Controls.Add(lblFeat);
                featY += 28;
            }

            // ----------------------------------------------------
            // rightPanel: Login Form (Column 1, 60% Width)
            // ----------------------------------------------------
            rightPanel.BackColor = Color.FromArgb(19, 20, 27);
            rightPanel.Dock = DockStyle.Fill;
            rightPanel.Location = new Point(368, 0);
            rightPanel.Margin = new Padding(0);
            rightPanel.Name = "rightPanel";
            rightPanel.Padding = new Padding(45, 30, 45, 30);
            rightPanel.Size = new Size(552, 520);
            rightPanel.TabIndex = 1;

            // heading: "Welcome Back"
            heading.AutoSize = true;
            heading.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            heading.ForeColor = Color.White;
            heading.Location = new Point(45, 35);
            heading.Name = "heading";
            heading.Size = new Size(205, 37);
            heading.TabIndex = 0;
            heading.Text = "Welcome Back";
            rightPanel.Controls.Add(heading);

            // subHeading
            subHeading.AutoSize = true;
            subHeading.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            subHeading.ForeColor = Color.FromArgb(142, 146, 188);
            subHeading.Location = new Point(47, 75);
            subHeading.Name = "subHeading";
            subHeading.Size = new Size(345, 15);
            subHeading.TabIndex = 1;
            subHeading.Text = "Enter your username and password to access the dashboard";
            subHeading.UseMnemonic = false;
            rightPanel.Controls.Add(subHeading);

            // lblUser: Username Label
            lblUser.AutoSize = true;
            lblUser.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblUser.ForeColor = Color.FromArgb(200, 205, 230);
            lblUser.Location = new Point(47, 115);
            lblUser.Name = "lblUser";
            lblUser.Size = new Size(64, 15);
            lblUser.TabIndex = 2;
            lblUser.Text = "Username";
            rightPanel.Controls.Add(lblUser);

            // userContainer: Rounded container for Username
            userContainer.BorderRadius = 8;
            userContainer.BorderWidth = 1;
            userContainer.CustomBackgroundColor = Color.FromArgb(29, 31, 42);
            userContainer.CustomBorderColor = Color.FromArgb(45, 48, 68);
            userContainer.Location = new Point(47, 138);
            userContainer.Name = "userContainer";
            userContainer.Size = new Size(380, 42);
            userContainer.TabIndex = 3;

            // _txtUsername
            _txtUsername.BackColor = Color.FromArgb(29, 31, 42);
            _txtUsername.BorderStyle = BorderStyle.None;
            _txtUsername.Font = new Font("Segoe UI", 10.5F);
            _txtUsername.ForeColor = Color.White;
            _txtUsername.Location = new Point(14, 11);
            _txtUsername.Name = "_txtUsername";
            _txtUsername.Size = new Size(350, 19);
            _txtUsername.TabIndex = 0;
            _txtUsername.Text = "admin";
            _txtUsername.KeyDown += OnKeyDownSubmit;
            userContainer.Controls.Add(_txtUsername);
            rightPanel.Controls.Add(userContainer);

            // lblPass: Password Label
            lblPass.AutoSize = true;
            lblPass.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblPass.ForeColor = Color.FromArgb(200, 205, 230);
            lblPass.Location = new Point(47, 195);
            lblPass.Name = "lblPass";
            lblPass.Size = new Size(59, 15);
            lblPass.TabIndex = 4;
            lblPass.Text = "Password";
            rightPanel.Controls.Add(lblPass);

            // passContainer: Rounded container for Password
            passContainer.BorderRadius = 8;
            passContainer.BorderWidth = 1;
            passContainer.CustomBackgroundColor = Color.FromArgb(29, 31, 42);
            passContainer.CustomBorderColor = Color.FromArgb(45, 48, 68);
            passContainer.Location = new Point(47, 218);
            passContainer.Name = "passContainer";
            passContainer.Size = new Size(380, 42);
            passContainer.TabIndex = 5;

            // _txtPassword
            _txtPassword.BackColor = Color.FromArgb(29, 31, 42);
            _txtPassword.BorderStyle = BorderStyle.None;
            _txtPassword.Font = new Font("Segoe UI", 10.5F);
            _txtPassword.ForeColor = Color.White;
            _txtPassword.Location = new Point(14, 11);
            _txtPassword.Name = "_txtPassword";
            _txtPassword.PasswordChar = '•';
            _txtPassword.Size = new Size(350, 19);
            _txtPassword.TabIndex = 0;
            _txtPassword.Text = "admin123";
            _txtPassword.KeyDown += OnKeyDownSubmit;
            passContainer.Controls.Add(_txtPassword);
            rightPanel.Controls.Add(passContainer);

            // _chkShowPassword
            _chkShowPassword.AutoSize = true;
            _chkShowPassword.Font = new Font("Segoe UI", 8.5F);
            _chkShowPassword.ForeColor = Color.FromArgb(142, 146, 188);
            _chkShowPassword.Location = new Point(49, 270);
            _chkShowPassword.Name = "_chkShowPassword";
            _chkShowPassword.Size = new Size(108, 19);
            _chkShowPassword.TabIndex = 6;
            _chkShowPassword.Text = "Show Password";
            _chkShowPassword.CheckedChanged += (s, e) =>
            {
                _txtPassword.PasswordChar = _chkShowPassword.Checked ? '\0' : '•';
            };
            rightPanel.Controls.Add(_chkShowPassword);

            // _lblError
            _lblError.AutoSize = true;
            _lblError.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _lblError.ForeColor = Color.FromArgb(239, 68, 68);
            _lblError.Location = new Point(49, 295);
            _lblError.Name = "_lblError";
            _lblError.Size = new Size(0, 15);
            _lblError.TabIndex = 7;
            _lblError.Visible = false;
            rightPanel.Controls.Add(_lblError);

            // btnSignIn
            btnSignIn.BorderRadius = 10;
            btnSignIn.Cursor = Cursors.Hand;
            btnSignIn.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnSignIn.ForeColor = Color.White;
            btnSignIn.HoverColor = Color.FromArgb(100, 130, 255);
            btnSignIn.Location = new Point(47, 320);
            btnSignIn.Name = "btnSignIn";
            btnSignIn.NormalColor = Color.FromArgb(76, 111, 255);
            btnSignIn.PressedColor = Color.FromArgb(60, 90, 220);
            btnSignIn.Size = new Size(380, 44);
            btnSignIn.TabIndex = 8;
            btnSignIn.Text = "Sign In to Dashboard";
            btnSignIn.UseVisualStyleBackColor = true;
            btnSignIn.Click += BtnSignIn_Click;
            rightPanel.Controls.Add(btnSignIn);

            // demoPill
            demoPill.BorderRadius = 8;
            demoPill.BorderWidth = 1;
            demoPill.CustomBackgroundColor = Color.FromArgb(26, 30, 46);
            demoPill.CustomBorderColor = Color.FromArgb(40, 50, 80);
            demoPill.Location = new Point(47, 380);
            demoPill.Name = "demoPill";
            demoPill.Size = new Size(380, 36);
            demoPill.TabIndex = 9;

            // lblHint
            lblHint.AutoSize = true;
            lblHint.Font = new Font("Segoe UI", 8F);
            lblHint.ForeColor = Color.FromArgb(140, 170, 245);
            lblHint.Location = new Point(14, 11);
            lblHint.Name = "lblHint";
            lblHint.Size = new Size(295, 13);
            lblHint.TabIndex = 0;
            lblHint.Text = "💡 Demo credentials: admin / admin123 (or Hardik / 1234)";
            lblHint.UseMnemonic = false;
            demoPill.Controls.Add(lblHint);
            rightPanel.Controls.Add(demoPill);

            // ----------------------------------------------------
            // Form Configuration
            // ----------------------------------------------------
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(19, 20, 27);
            ClientSize = new Size(920, 520);
            Controls.Add(mainTableLayout);
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimumSize = new Size(880, 500);
            Name = "LoginForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Business Analytics — Sign In";

            mainTableLayout.ResumeLayout(false);
            leftPanel.ResumeLayout(false);
            leftPanel.PerformLayout();
            rightPanel.ResumeLayout(false);
            rightPanel.PerformLayout();
            userContainer.ResumeLayout(false);
            userContainer.PerformLayout();
            passContainer.ResumeLayout(false);
            passContainer.PerformLayout();
            demoPill.ResumeLayout(false);
            demoPill.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel mainTableLayout;
        private Panel leftPanel;
        private Panel rightPanel;
        private Label brandTitle;
        private Label brandSub;
        private Label heading;
        private Label subHeading;
        private Label lblUser;
        private Label lblPass;
        private Controls.RoundedPanel userContainer;
        private Controls.RoundedPanel passContainer;
        private TextBox _txtUsername;
        private TextBox _txtPassword;
        private CheckBox _chkShowPassword;
        private Label _lblError;
        private Controls.ModernButton btnSignIn;
        private Controls.RoundedPanel demoPill;
        private Label lblHint;
    }
}
