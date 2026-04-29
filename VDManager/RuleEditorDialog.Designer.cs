using VDManager.Controls;

namespace VDManager
{
    partial class RuleEditorDialog
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
            lblProcessName = new Label();
            cmbProcessName = new ComboBox();
            btnRefreshProcesses = new Button();
            lblTitlePattern = new Label();
            txtTitlePattern = new TextBox();
            chkUseRegex = new CheckBox();
            lblInstanceNumber = new Label();
            nudInstanceNumber = new NumericUpDown();
            lblInstanceHelp = new Label();
            lblPriority = new Label();
            nudPriority = new NumericUpDown();
            lblPriorityHelp = new Label();
            lblDesktop = new Label();
            cmbDesktop = new ComboBox();
            lblMonitor = new Label();
            cmbMonitor = new ComboBox();
            lblDescription = new Label();
            txtDescription = new TextBox();
            chkEnabled = new CheckBox();
            chkEnforcePosition = new CheckBox();
            pnlDivider = new Panel();
            lblQuadrantPanel = new Label();
            visualQuadrantPanel = new VisualQuadrantPanel();
            btnOK = new Button();
            btnCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)nudInstanceNumber).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudPriority).BeginInit();
            SuspendLayout();
            // 
            // lblProcessName
            // 
            lblProcessName.Location = new Point(20, 20);
            lblProcessName.Name = "lblProcessName";
            lblProcessName.Size = new Size(100, 20);
            lblProcessName.TabIndex = 0;
            lblProcessName.Text = "Process Name:";
            // 
            // cmbProcessName
            // 
            cmbProcessName.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            cmbProcessName.AutoCompleteSource = AutoCompleteSource.ListItems;
            cmbProcessName.Location = new Point(130, 18);
            cmbProcessName.Name = "cmbProcessName";
            cmbProcessName.Size = new Size(190, 23);
            cmbProcessName.TabIndex = 1;
            // 
            // btnRefreshProcesses
            // 
            btnRefreshProcesses.Font = new Font("Segoe UI", 12F);
            btnRefreshProcesses.Location = new Point(325, 17);
            btnRefreshProcesses.Name = "btnRefreshProcesses";
            btnRefreshProcesses.Size = new Size(45, 25);
            btnRefreshProcesses.TabIndex = 2;
            btnRefreshProcesses.Text = "↻";
            btnRefreshProcesses.Click += btnRefreshProcesses_Click;
            // 
            // lblTitlePattern
            // 
            lblTitlePattern.Location = new Point(20, 60);
            lblTitlePattern.Name = "lblTitlePattern";
            lblTitlePattern.Size = new Size(100, 20);
            lblTitlePattern.TabIndex = 3;
            lblTitlePattern.Text = "Window Title:";
            // 
            // txtTitlePattern
            // 
            txtTitlePattern.Location = new Point(130, 58);
            txtTitlePattern.Name = "txtTitlePattern";
            txtTitlePattern.PlaceholderText = "Optional filter (e.g., GitHub)";
            txtTitlePattern.Size = new Size(240, 23);
            txtTitlePattern.TabIndex = 4;
            // 
            // chkUseRegex
            // 
            chkUseRegex.Location = new Point(130, 88);
            chkUseRegex.Name = "chkUseRegex";
            chkUseRegex.Size = new Size(100, 20);
            chkUseRegex.TabIndex = 5;
            chkUseRegex.Text = "Use Regex";
            // 
            // lblInstanceNumber
            // 
            lblInstanceNumber.Location = new Point(20, 120);
            lblInstanceNumber.Name = "lblInstanceNumber";
            lblInstanceNumber.Size = new Size(100, 20);
            lblInstanceNumber.TabIndex = 6;
            lblInstanceNumber.Text = "Instance #:";
            // 
            // nudInstanceNumber
            // 
            nudInstanceNumber.Location = new Point(130, 118);
            nudInstanceNumber.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            nudInstanceNumber.Name = "nudInstanceNumber";
            nudInstanceNumber.Size = new Size(80, 23);
            nudInstanceNumber.TabIndex = 7;
            // 
            // lblInstanceHelp
            // 
            lblInstanceHelp.Font = new Font("Segoe UI", 7.5F);
            lblInstanceHelp.ForeColor = Color.Gray;
            lblInstanceHelp.Location = new Point(215, 120);
            lblInstanceHelp.Name = "lblInstanceHelp";
            lblInstanceHelp.Size = new Size(160, 20);
            lblInstanceHelp.TabIndex = 8;
            lblInstanceHelp.Text = "(0 = any, 1 = first, 2 = second, etc.)";
            // 
            // lblPriority
            // 
            lblPriority.Location = new Point(20, 150);
            lblPriority.Name = "lblPriority";
            lblPriority.Size = new Size(100, 20);
            lblPriority.TabIndex = 9;
            lblPriority.Text = "Priority:";
            // 
            // nudPriority
            // 
            nudPriority.Location = new Point(130, 148);
            nudPriority.Name = "nudPriority";
            nudPriority.Size = new Size(80, 23);
            nudPriority.TabIndex = 10;
            // 
            // lblPriorityHelp
            // 
            lblPriorityHelp.Font = new Font("Segoe UI", 7.5F);
            lblPriorityHelp.ForeColor = Color.Gray;
            lblPriorityHelp.Location = new Point(215, 150);
            lblPriorityHelp.Name = "lblPriorityHelp";
            lblPriorityHelp.Size = new Size(160, 20);
            lblPriorityHelp.TabIndex = 11;
            lblPriorityHelp.Text = "(Higher = checked first)";
            // 
            // lblDesktop
            // 
            lblDesktop.Location = new Point(20, 190);
            lblDesktop.Name = "lblDesktop";
            lblDesktop.Size = new Size(100, 20);
            lblDesktop.TabIndex = 12;
            lblDesktop.Text = "Desktop:";
            // 
            // cmbDesktop
            // 
            cmbDesktop.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDesktop.Location = new Point(130, 188);
            cmbDesktop.Name = "cmbDesktop";
            cmbDesktop.Size = new Size(150, 23);
            cmbDesktop.TabIndex = 13;
            // 
            // lblMonitor
            // 
            lblMonitor.Location = new Point(20, 230);
            lblMonitor.Name = "lblMonitor";
            lblMonitor.Size = new Size(100, 20);
            lblMonitor.TabIndex = 14;
            lblMonitor.Text = "Monitor:";
            // 
            // cmbMonitor
            // 
            cmbMonitor.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMonitor.Location = new Point(130, 228);
            cmbMonitor.Name = "cmbMonitor";
            cmbMonitor.Size = new Size(240, 23);
            cmbMonitor.TabIndex = 15;
            // 
            // lblDescription
            // 
            lblDescription.Location = new Point(20, 270);
            lblDescription.Name = "lblDescription";
            lblDescription.Size = new Size(100, 20);
            lblDescription.TabIndex = 16;
            lblDescription.Text = "Description:";
            // 
            // txtDescription
            // 
            txtDescription.Location = new Point(130, 268);
            txtDescription.Name = "txtDescription";
            txtDescription.Size = new Size(240, 23);
            txtDescription.TabIndex = 17;
            // 
            // chkEnabled
            // 
            chkEnabled.Checked = true;
            chkEnabled.CheckState = CheckState.Checked;
            chkEnabled.Location = new Point(130, 308);
            chkEnabled.Name = "chkEnabled";
            chkEnabled.Size = new Size(100, 20);
            chkEnabled.TabIndex = 18;
            chkEnabled.Text = "Enabled";
            // 
            // chkEnforcePosition
            // 
            chkEnforcePosition.Location = new Point(130, 338);
            chkEnforcePosition.Name = "chkEnforcePosition";
            chkEnforcePosition.Size = new Size(250, 20);
            chkEnforcePosition.TabIndex = 19;
            chkEnforcePosition.Text = "Enforce position (snap back if moved)";
            // 
            // pnlDivider
            // 
            pnlDivider.BackColor = SystemColors.ControlDark;
            pnlDivider.Location = new Point(384, 8);
            pnlDivider.Name = "pnlDivider";
            pnlDivider.Size = new Size(1, 420);
            pnlDivider.TabIndex = 22;
            // 
            // lblQuadrantPanel
            // 
            lblQuadrantPanel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblQuadrantPanel.Location = new Point(392, 4);
            lblQuadrantPanel.Name = "lblQuadrantPanel";
            lblQuadrantPanel.Size = new Size(150, 16);
            lblQuadrantPanel.TabIndex = 23;
            lblQuadrantPanel.Text = "Position / Layout:";
            // 
            // visualQuadrantPanel
            // 
            visualQuadrantPanel.BackColor = SystemColors.Control;
            visualQuadrantPanel.BorderStyle = BorderStyle.FixedSingle;
            visualQuadrantPanel.Location = new Point(392, 20);
            visualQuadrantPanel.Name = "visualQuadrantPanel";
            visualQuadrantPanel.Padding = new Padding(8);
            visualQuadrantPanel.SelectedQuadrant = Quadrant.LeftThird;
            visualQuadrantPanel.Size = new Size(317, 499);
            visualQuadrantPanel.TabIndex = 24;
            // 
            // btnOK
            // 
            btnOK.DialogResult = DialogResult.OK;
            btnOK.Location = new Point(200, 430);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(80, 25);
            btnOK.TabIndex = 20;
            btnOK.Text = "OK";
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(290, 430);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(80, 25);
            btnCancel.TabIndex = 21;
            btnCancel.Text = "Cancel";
            // 
            // RuleEditorDialog
            // 
            AcceptButton = btnOK;
            CancelButton = btnCancel;
            ClientSize = new Size(721, 531);
            Controls.Add(lblProcessName);
            Controls.Add(cmbProcessName);
            Controls.Add(btnRefreshProcesses);
            Controls.Add(lblTitlePattern);
            Controls.Add(txtTitlePattern);
            Controls.Add(chkUseRegex);
            Controls.Add(lblInstanceNumber);
            Controls.Add(nudInstanceNumber);
            Controls.Add(lblInstanceHelp);
            Controls.Add(lblPriority);
            Controls.Add(nudPriority);
            Controls.Add(lblPriorityHelp);
            Controls.Add(lblDesktop);
            Controls.Add(cmbDesktop);
            Controls.Add(lblMonitor);
            Controls.Add(cmbMonitor);
            Controls.Add(lblDescription);
            Controls.Add(txtDescription);
            Controls.Add(chkEnabled);
            Controls.Add(chkEnforcePosition);
            Controls.Add(btnOK);
            Controls.Add(btnCancel);
            Controls.Add(pnlDivider);
            Controls.Add(lblQuadrantPanel);
            Controls.Add(visualQuadrantPanel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "RuleEditorDialog";
            StartPosition = FormStartPosition.CenterParent;
            ((System.ComponentModel.ISupportInitialize)nudInstanceNumber).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudPriority).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lblProcessName;
        private System.Windows.Forms.ComboBox cmbProcessName;
        private System.Windows.Forms.Button btnRefreshProcesses;
        private System.Windows.Forms.Label lblTitlePattern;
        private System.Windows.Forms.TextBox txtTitlePattern;
        private System.Windows.Forms.CheckBox chkUseRegex;
        private System.Windows.Forms.Label lblInstanceNumber;
        private System.Windows.Forms.NumericUpDown nudInstanceNumber;
        private System.Windows.Forms.Label lblInstanceHelp;
        private System.Windows.Forms.Label lblPriority;
        private System.Windows.Forms.NumericUpDown nudPriority;
        private System.Windows.Forms.Label lblPriorityHelp;
        private System.Windows.Forms.Label lblDesktop;
        private System.Windows.Forms.ComboBox cmbDesktop;
        private System.Windows.Forms.Label lblMonitor;
        private System.Windows.Forms.ComboBox cmbMonitor;
        private System.Windows.Forms.Label lblDescription;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.CheckBox chkEnabled;
        private System.Windows.Forms.CheckBox chkEnforcePosition;
        private System.Windows.Forms.Panel pnlDivider;
        private System.Windows.Forms.Label lblQuadrantPanel;
        private VDManager.Controls.VisualQuadrantPanel visualQuadrantPanel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}
