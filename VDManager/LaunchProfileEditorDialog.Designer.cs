namespace VDManager
{
    partial class LaunchProfileEditorDialog
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
            lblName = new System.Windows.Forms.Label();
            txtName = new System.Windows.Forms.TextBox();
            lblDesc = new System.Windows.Forms.Label();
            txtDesc = new System.Windows.Forms.TextBox();
            chkStartup = new System.Windows.Forms.CheckBox();
            lblHotkey = new System.Windows.Forms.Label();
            txtHotkey = new System.Windows.Forms.TextBox();
            btnEditHotkey = new System.Windows.Forms.Button();
            btnClearHotkey = new System.Windows.Forms.Button();
            lblEntries = new System.Windows.Forms.Label();
            btnAddEntry = new System.Windows.Forms.Button();
            btnEditEntry = new System.Windows.Forms.Button();
            btnDeleteEntry = new System.Windows.Forms.Button();
            btnMoveUp = new System.Windows.Forms.Button();
            btnMoveDown = new System.Windows.Forms.Button();
            dgvEntries = new System.Windows.Forms.DataGridView();
            btnOK = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();

            ((System.ComponentModel.ISupportInitialize)dgvEntries).BeginInit();
            this.SuspendLayout();

            // labelX=15, fieldX=130, rowH=35
            // y values: 15, 50, 85, 120(hotkey), 160(entries label), 180(buttons), 210(grid), 435(OK/Cancel)

            // lblName  (y=15)
            lblName.Text = "Name:";
            lblName.Location = new System.Drawing.Point(15, 15);
            lblName.AutoSize = true;

            // txtName  (y-2=13)
            txtName.Location = new System.Drawing.Point(130, 13);
            txtName.Size = new System.Drawing.Size(300, 25);

            // lblDesc  (y=50)
            lblDesc.Text = "Description:";
            lblDesc.Location = new System.Drawing.Point(15, 50);
            lblDesc.AutoSize = true;

            // txtDesc  (y-2=48)
            txtDesc.Location = new System.Drawing.Point(130, 48);
            txtDesc.Size = new System.Drawing.Size(480, 25);

            // chkStartup  (y=85)
            chkStartup.Text = "Auto-launch this profile when DeskBulldozer starts";
            chkStartup.Location = new System.Drawing.Point(130, 85);
            chkStartup.AutoSize = true;

            // lblHotkey  (y=120)
            lblHotkey.Text = "Hotkey:";
            lblHotkey.Location = new System.Drawing.Point(15, 120);
            lblHotkey.AutoSize = true;

            // txtHotkey  (y-2=118)
            txtHotkey.Location = new System.Drawing.Point(130, 118);
            txtHotkey.Size = new System.Drawing.Size(180, 25);
            txtHotkey.ReadOnly = true;

            // btnEditHotkey  (fieldX+185=315, y-2=118)
            btnEditHotkey.Text = "Set Hotkey";
            btnEditHotkey.Location = new System.Drawing.Point(315, 118);
            btnEditHotkey.Size = new System.Drawing.Size(90, 25);
            btnEditHotkey.Click += BtnEditHotkey_Click;

            // btnClearHotkey  (fieldX+280=410, y-2=118)
            btnClearHotkey.Text = "Clear";
            btnClearHotkey.Location = new System.Drawing.Point(410, 118);
            btnClearHotkey.Size = new System.Drawing.Size(60, 25);
            btnClearHotkey.Click += BtnClearHotkey_Click;

            // lblEntries  (y=160, after rowH+5=40 added)
            lblEntries.Text = "App Entries:";
            lblEntries.Location = new System.Drawing.Point(15, 160);
            lblEntries.AutoSize = true;

            // Entry buttons  (y=180, after +20)
            btnAddEntry.Text = "Add";
            btnAddEntry.Location = new System.Drawing.Point(15, 180);
            btnAddEntry.Size = new System.Drawing.Size(80, 26);
            btnAddEntry.Click += BtnAddEntry_Click;

            btnEditEntry.Text = "Edit";
            btnEditEntry.Location = new System.Drawing.Point(100, 180);
            btnEditEntry.Size = new System.Drawing.Size(80, 26);
            btnEditEntry.Click += BtnEditEntry_Click;

            btnDeleteEntry.Text = "Delete";
            btnDeleteEntry.Location = new System.Drawing.Point(185, 180);
            btnDeleteEntry.Size = new System.Drawing.Size(80, 26);
            btnDeleteEntry.Click += BtnDeleteEntry_Click;

            btnMoveUp.Text = "▲ Up";
            btnMoveUp.Location = new System.Drawing.Point(280, 180);
            btnMoveUp.Size = new System.Drawing.Size(70, 26);
            btnMoveUp.Click += BtnMoveUp_Click;

            btnMoveDown.Text = "▼ Down";
            btnMoveDown.Location = new System.Drawing.Point(355, 180);
            btnMoveDown.Size = new System.Drawing.Size(80, 26);
            btnMoveDown.Click += BtnMoveDown_Click;

            // dgvEntries  (y=210, after +30)
            dgvEntries.Location = new System.Drawing.Point(15, 210);
            dgvEntries.Size = new System.Drawing.Size(625, 220);
            dgvEntries.AllowUserToAddRows = false;
            dgvEntries.AllowUserToDeleteRows = false;
            dgvEntries.AllowUserToResizeRows = false;
            dgvEntries.MultiSelect = false;
            dgvEntries.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            dgvEntries.RowHeadersVisible = false;
            dgvEntries.ColumnHeadersHeight = 28;
            dgvEntries.RowTemplate.Height = 26;
            dgvEntries.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            // btnOK / btnCancel  (y=435, after +225)
            btnOK.Text = "OK";
            btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            btnOK.Location = new System.Drawing.Point(460, 435);
            btnOK.Size = new System.Drawing.Size(85, 30);
            btnOK.Click += BtnOK_Click;

            btnCancel.Text = "Cancel";
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Location = new System.Drawing.Point(555, 435);
            btnCancel.Size = new System.Drawing.Size(85, 30);

            // LaunchProfileEditorDialog
            this.Text = "Launch Profile";
            this.ClientSize = new System.Drawing.Size(660, 480);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.AcceptButton = btnOK;
            this.CancelButton = btnCancel;
            this.Controls.AddRange(new System.Windows.Forms.Control[]
            {
                lblName, txtName,
                lblDesc, txtDesc,
                chkStartup,
                lblHotkey, txtHotkey, btnEditHotkey, btnClearHotkey,
                lblEntries,
                btnAddEntry, btnEditEntry, btnDeleteEntry, btnMoveUp, btnMoveDown,
                dgvEntries,
                btnOK, btnCancel
            });

            ((System.ComponentModel.ISupportInitialize)dgvEntries).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label lblDesc;
        private System.Windows.Forms.TextBox txtDesc;
        private System.Windows.Forms.CheckBox chkStartup;
        private System.Windows.Forms.Label lblHotkey;
        private System.Windows.Forms.TextBox txtHotkey;
        private System.Windows.Forms.Button btnEditHotkey;
        private System.Windows.Forms.Button btnClearHotkey;
        private System.Windows.Forms.Label lblEntries;
        private System.Windows.Forms.DataGridView dgvEntries;
        private System.Windows.Forms.Button btnAddEntry;
        private System.Windows.Forms.Button btnEditEntry;
        private System.Windows.Forms.Button btnDeleteEntry;
        private System.Windows.Forms.Button btnMoveUp;
        private System.Windows.Forms.Button btnMoveDown;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}
