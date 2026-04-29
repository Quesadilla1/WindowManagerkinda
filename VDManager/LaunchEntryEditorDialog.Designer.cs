namespace VDManager
{
    partial class LaunchEntryEditorDialog
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblPreset = new System.Windows.Forms.Label();
            cmbPreset = new System.Windows.Forms.ComboBox();
            lblName = new System.Windows.Forms.Label();
            txtName = new System.Windows.Forms.TextBox();
            lblExe = new System.Windows.Forms.Label();
            txtExe = new System.Windows.Forms.TextBox();
            btnBrowseExe = new System.Windows.Forms.Button();
            lblArgs = new System.Windows.Forms.Label();
            txtArgs = new System.Windows.Forms.TextBox();
            lblWorkDir = new System.Windows.Forms.Label();
            txtWorkDir = new System.Windows.Forms.TextBox();
            btnBrowseWorkDir = new System.Windows.Forms.Button();
            lblDelay = new System.Windows.Forms.Label();
            nudDelay = new System.Windows.Forms.NumericUpDown();
            lblDelayUnit = new System.Windows.Forms.Label();
            lblDesktop = new System.Windows.Forms.Label();
            cmbDesktop = new System.Windows.Forms.ComboBox();
            lblLinkedRule = new System.Windows.Forms.Label();
            cmbLinkedRule = new System.Windows.Forms.ComboBox();
            btnOK = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)nudDelay).BeginInit();
            this.SuspendLayout();

            // labelX=15, fieldX=130, fieldW=280, browseW=70, rowH=35
            // Row y values: 15, 50, 85, 120, 155, 190, 225, 260, 295(buttons)

            // lblPreset  (y=15)
            lblPreset.Text = "Quick Preset:";
            lblPreset.Location = new System.Drawing.Point(15, 15);
            lblPreset.AutoSize = true;

            // cmbPreset  (y-2=13)
            cmbPreset.Location = new System.Drawing.Point(130, 13);
            cmbPreset.Size = new System.Drawing.Size(200, 25);
            cmbPreset.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbPreset.SelectedIndexChanged += CmbPreset_SelectedIndexChanged;

            // lblName  (y=50)
            lblName.Text = "Name:";
            lblName.Location = new System.Drawing.Point(15, 50);
            lblName.AutoSize = true;

            // txtName  (y-2=48)
            txtName.Location = new System.Drawing.Point(130, 48);
            txtName.Size = new System.Drawing.Size(280, 25);

            // lblExe  (y=85)
            lblExe.Text = "Executable:";
            lblExe.Location = new System.Drawing.Point(15, 85);
            lblExe.AutoSize = true;

            // txtExe  (y-2=83)
            txtExe.Location = new System.Drawing.Point(130, 83);
            txtExe.Size = new System.Drawing.Size(280, 25);

            // btnBrowseExe  (fieldX+fieldW+5=415, y-2=83)
            btnBrowseExe.Text = "Browse...";
            btnBrowseExe.Location = new System.Drawing.Point(415, 83);
            btnBrowseExe.Size = new System.Drawing.Size(70, 25);
            btnBrowseExe.Click += BtnBrowseExe_Click;

            // lblArgs  (y=120)
            lblArgs.Text = "Arguments:";
            lblArgs.Location = new System.Drawing.Point(15, 120);
            lblArgs.AutoSize = true;

            // txtArgs  (y-2=118, fieldW+browseW+5=355)
            txtArgs.Location = new System.Drawing.Point(130, 118);
            txtArgs.Size = new System.Drawing.Size(355, 25);

            // lblWorkDir  (y=155)
            lblWorkDir.Text = "Working Dir:";
            lblWorkDir.Location = new System.Drawing.Point(15, 155);
            lblWorkDir.AutoSize = true;

            // txtWorkDir  (y-2=153)
            txtWorkDir.Location = new System.Drawing.Point(130, 153);
            txtWorkDir.Size = new System.Drawing.Size(280, 25);

            // btnBrowseWorkDir  (415, y-2=153)
            btnBrowseWorkDir.Text = "Browse...";
            btnBrowseWorkDir.Location = new System.Drawing.Point(415, 153);
            btnBrowseWorkDir.Size = new System.Drawing.Size(70, 25);
            btnBrowseWorkDir.Click += BtnBrowseWorkDir_Click;

            // lblDelay  (y=190)
            lblDelay.Text = "Startup Delay:";
            lblDelay.Location = new System.Drawing.Point(15, 190);
            lblDelay.AutoSize = true;

            // nudDelay  (y-2=188)
            nudDelay.Location = new System.Drawing.Point(130, 188);
            nudDelay.Size = new System.Drawing.Size(80, 25);
            nudDelay.Minimum = 0;
            nudDelay.Maximum = 300;
            nudDelay.Value = 0;

            // lblDelayUnit  (fieldX+85=215, y=190)
            lblDelayUnit.Text = "seconds  (0 = launch immediately)";
            lblDelayUnit.Location = new System.Drawing.Point(215, 190);
            lblDelayUnit.AutoSize = true;

            // lblDesktop  (y=225)
            lblDesktop.Text = "After launch,\nswitch to:";
            lblDesktop.Location = new System.Drawing.Point(15, 225);
            lblDesktop.AutoSize = true;

            // cmbDesktop  (y-2=223)
            cmbDesktop.Location = new System.Drawing.Point(130, 223);
            cmbDesktop.Size = new System.Drawing.Size(180, 25);
            cmbDesktop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

            // lblLinkedRule  (y=260)
            lblLinkedRule.Text = "Linked Rule:";
            lblLinkedRule.Location = new System.Drawing.Point(15, 260);
            lblLinkedRule.AutoSize = true;

            // cmbLinkedRule  (y-2=258, fieldW+browseW+5=355)
            cmbLinkedRule.Location = new System.Drawing.Point(130, 258);
            cmbLinkedRule.Size = new System.Drawing.Size(355, 25);
            cmbLinkedRule.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

            // btnOK  (y=295)
            btnOK.Text = "OK";
            btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            btnOK.Location = new System.Drawing.Point(310, 295);
            btnOK.Size = new System.Drawing.Size(85, 30);
            btnOK.Click += BtnOK_Click;

            // btnCancel  (y=295)
            btnCancel.Text = "Cancel";
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Location = new System.Drawing.Point(405, 295);
            btnCancel.Size = new System.Drawing.Size(85, 30);

            // LaunchEntryEditorDialog
            this.Text = "Launch Entry";
            this.ClientSize = new System.Drawing.Size(500, 340);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
            this.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                lblPreset, cmbPreset,
                lblName, txtName,
                lblExe, txtExe, btnBrowseExe,
                lblArgs, txtArgs,
                lblWorkDir, txtWorkDir, btnBrowseWorkDir,
                lblDelay, nudDelay, lblDelayUnit,
                lblDesktop, cmbDesktop,
                lblLinkedRule, cmbLinkedRule,
                btnOK, btnCancel
            });

            ((System.ComponentModel.ISupportInitialize)nudDelay).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblPreset;
        private System.Windows.Forms.ComboBox cmbPreset;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblExe;
        private System.Windows.Forms.TextBox txtExe;
        private System.Windows.Forms.Button btnBrowseExe;
        private System.Windows.Forms.Label lblArgs;
        private System.Windows.Forms.TextBox txtArgs;
        private System.Windows.Forms.Label lblWorkDir;
        private System.Windows.Forms.TextBox txtWorkDir;
        private System.Windows.Forms.Button btnBrowseWorkDir;
        private System.Windows.Forms.Label lblDelay;
        private System.Windows.Forms.NumericUpDown nudDelay;
        private System.Windows.Forms.Label lblDelayUnit;
        private System.Windows.Forms.Label lblDesktop;
        private System.Windows.Forms.ComboBox cmbDesktop;
        private System.Windows.Forms.Label lblLinkedRule;
        private System.Windows.Forms.ComboBox cmbLinkedRule;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}
