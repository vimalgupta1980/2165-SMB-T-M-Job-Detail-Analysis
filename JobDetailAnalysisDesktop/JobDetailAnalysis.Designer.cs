namespace JobDetailAnalysisDesktop
{
    partial class JobDetailAnalysis
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JobDetailAnalysis));
            this.txtDataDir = new System.Windows.Forms.TextBox();
            this.btnRunReport = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewAuditLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectNonBillableCostCodesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectTMJobTypesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectTemplateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.onlineHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.activateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.demoLabel = new System.Windows.Forms.Label();
            this.radioShowTMJobs = new System.Windows.Forms.RadioButton();
            this.radioShowAllJobs = new System.Windows.Forms.RadioButton();
            this.dteTransactionDate = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.chkUnbilled = new System.Windows.Forms.CheckBox();
            this.btnSMBDir = new System.Windows.Forms.Button();
            this.cmbStartingPeriod = new SysconCommon.GUI.SearchableComboBox();
            this.cmbEndPeriod = new SysconCommon.GUI.SearchableComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkSumCustomer = new SysconCommon.GUI.SysconCheckBox();
            this.chkSumPeriod = new SysconCommon.GUI.SysconCheckBox();
            this.chkSumJob = new SysconCommon.GUI.SysconCheckBox();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtDataDir
            // 
            this.txtDataDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDataDir.Location = new System.Drawing.Point(15, 47);
            this.txtDataDir.Name = "txtDataDir";
            this.txtDataDir.ReadOnly = true;
            this.txtDataDir.Size = new System.Drawing.Size(359, 20);
            this.txtDataDir.TabIndex = 2;
            this.txtDataDir.TextChanged += new System.EventHandler(this.txtDataDir_TextChanged);
            // 
            // btnRunReport
            // 
            this.btnRunReport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRunReport.Location = new System.Drawing.Point(393, 306);
            this.btnRunReport.Name = "btnRunReport";
            this.btnRunReport.Size = new System.Drawing.Size(75, 23);
            this.btnRunReport.TabIndex = 4;
            this.btnRunReport.Text = "&Run Report";
            this.btnRunReport.UseVisualStyleBackColor = true;
            this.btnRunReport.Click += new System.EventHandler(this.btnRunReport_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(248, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "End Period";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 84);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Starting Period";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(9, 4);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(150, 24);
            this.menuStrip1.TabIndex = 12;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importFileToolStripMenuItem,
            this.viewAuditLogToolStripMenuItem,
            this.quitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // importFileToolStripMenuItem
            // 
            this.importFileToolStripMenuItem.Name = "importFileToolStripMenuItem";
            this.importFileToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
            this.importFileToolStripMenuItem.Text = "&Import T&&M Updates to SMB";
            this.importFileToolStripMenuItem.Click += new System.EventHandler(this.importFileToolStripMenuItem_Click);
            // 
            // viewAuditLogToolStripMenuItem
            // 
            this.viewAuditLogToolStripMenuItem.Name = "viewAuditLogToolStripMenuItem";
            this.viewAuditLogToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
            this.viewAuditLogToolStripMenuItem.Text = "View &Audit Log";
            this.viewAuditLogToolStripMenuItem.Click += new System.EventHandler(this.viewAuditLogToolStripMenuItem_Click);
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(228, 22);
            this.quitToolStripMenuItem.Text = "E&xit";
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectNonBillableCostCodesToolStripMenuItem,
            this.selectTMJobTypesToolStripMenuItem,
            this.selectTemplateToolStripMenuItem,
            this.settingsToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "&Options";
            // 
            // selectNonBillableCostCodesToolStripMenuItem
            // 
            this.selectNonBillableCostCodesToolStripMenuItem.Name = "selectNonBillableCostCodesToolStripMenuItem";
            this.selectNonBillableCostCodesToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
            this.selectNonBillableCostCodesToolStripMenuItem.Text = "Select Non-Billable Cost Codes";
            this.selectNonBillableCostCodesToolStripMenuItem.Click += new System.EventHandler(this.selectNonBillableCostCodesToolStripMenuItem_Click);
            // 
            // selectTMJobTypesToolStripMenuItem
            // 
            this.selectTMJobTypesToolStripMenuItem.Name = "selectTMJobTypesToolStripMenuItem";
            this.selectTMJobTypesToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
            this.selectTMJobTypesToolStripMenuItem.Text = "Select T&M Job Types";
            this.selectTMJobTypesToolStripMenuItem.Click += new System.EventHandler(this.selectTMJobTypesToolStripMenuItem_Click);
            // 
            // selectTemplateToolStripMenuItem
            // 
            this.selectTemplateToolStripMenuItem.Name = "selectTemplateToolStripMenuItem";
            this.selectTemplateToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
            this.selectTemplateToolStripMenuItem.Text = "Select &Template";
            this.selectTemplateToolStripMenuItem.Click += new System.EventHandler(this.selectTemplateToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(237, 22);
            this.settingsToolStripMenuItem.Text = "&Settings";
            this.settingsToolStripMenuItem.Click += new System.EventHandler(this.settingsToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.onlineHelpToolStripMenuItem,
            this.aboutToolStripMenuItem,
            this.activateToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // onlineHelpToolStripMenuItem
            // 
            this.onlineHelpToolStripMenuItem.Name = "onlineHelpToolStripMenuItem";
            this.onlineHelpToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.onlineHelpToolStripMenuItem.Text = "Online Help";
            this.onlineHelpToolStripMenuItem.Click += new System.EventHandler(this.onlineHelpToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // activateToolStripMenuItem
            // 
            this.activateToolStripMenuItem.Name = "activateToolStripMenuItem";
            this.activateToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.activateToolStripMenuItem.Text = "Activate";
            this.activateToolStripMenuItem.Click += new System.EventHandler(this.activateToolStripMenuItem_Click);
            // 
            // demoLabel
            // 
            this.demoLabel.AutoSize = true;
            this.demoLabel.Location = new System.Drawing.Point(208, 15);
            this.demoLabel.Name = "demoLabel";
            this.demoLabel.Size = new System.Drawing.Size(81, 13);
            this.demoLabel.TabIndex = 14;
            this.demoLabel.Text = "This goes away";
            // 
            // radioShowTMJobs
            // 
            this.radioShowTMJobs.AutoSize = true;
            this.radioShowTMJobs.Checked = true;
            this.radioShowTMJobs.Location = new System.Drawing.Point(151, 175);
            this.radioShowTMJobs.Name = "radioShowTMJobs";
            this.radioShowTMJobs.Size = new System.Drawing.Size(126, 17);
            this.radioShowTMJobs.TabIndex = 15;
            this.radioShowTMJobs.TabStop = true;
            this.radioShowTMJobs.Text = "Show T&&M Jobs Only";
            this.radioShowTMJobs.UseVisualStyleBackColor = true;
            // 
            // radioShowAllJobs
            // 
            this.radioShowAllJobs.AutoSize = true;
            this.radioShowAllJobs.Location = new System.Drawing.Point(283, 175);
            this.radioShowAllJobs.Name = "radioShowAllJobs";
            this.radioShowAllJobs.Size = new System.Drawing.Size(91, 17);
            this.radioShowAllJobs.TabIndex = 16;
            this.radioShowAllJobs.Text = "Show All Jobs";
            this.radioShowAllJobs.UseVisualStyleBackColor = true;
            // 
            // dteTransactionDate
            // 
            this.dteTransactionDate.Location = new System.Drawing.Point(145, 132);
            this.dteTransactionDate.Name = "dteTransactionDate";
            this.dteTransactionDate.Size = new System.Drawing.Size(200, 20);
            this.dteTransactionDate.TabIndex = 17;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 138);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(118, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "Up to Transaction Date";
            // 
            // chkUnbilled
            // 
            this.chkUnbilled.AutoSize = true;
            this.chkUnbilled.Location = new System.Drawing.Point(15, 175);
            this.chkUnbilled.Name = "chkUnbilled";
            this.chkUnbilled.Size = new System.Drawing.Size(131, 17);
            this.chkUnbilled.TabIndex = 19;
            this.chkUnbilled.Text = "Unbilled Records Only";
            this.chkUnbilled.UseVisualStyleBackColor = true;
            this.chkUnbilled.CheckedChanged += new System.EventHandler(this.chkUnbilled_CheckedChanged);
            // 
            // btnSMBDir
            // 
            this.btnSMBDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSMBDir.Location = new System.Drawing.Point(393, 45);
            this.btnSMBDir.Name = "btnSMBDir";
            this.btnSMBDir.Size = new System.Drawing.Size(75, 23);
            this.btnSMBDir.TabIndex = 20;
            this.btnSMBDir.Text = "&Browse";
            this.btnSMBDir.UseVisualStyleBackColor = true;
            this.btnSMBDir.Click += new System.EventHandler(this.btnSMBDir_Click);
            // 
            // cmbStartingPeriod
            // 
            this.cmbStartingPeriod.ConfigVarName = null;
            this.cmbStartingPeriod.FormattingEnabled = true;
            this.cmbStartingPeriod.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12"});
            this.cmbStartingPeriod.Location = new System.Drawing.Point(104, 81);
            this.cmbStartingPeriod.Name = "cmbStartingPeriod";
            this.cmbStartingPeriod.Size = new System.Drawing.Size(121, 21);
            this.cmbStartingPeriod.TabIndex = 9;
            this.cmbStartingPeriod.SelectedIndexChanged += new System.EventHandler(this.cmbStartingPeriod_SelectedIndexChanged);
            // 
            // cmbEndPeriod
            // 
            this.cmbEndPeriod.ConfigVarName = null;
            this.cmbEndPeriod.FormattingEnabled = true;
            this.cmbEndPeriod.Items.AddRange(new object[] {
            "0",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12"});
            this.cmbEndPeriod.Location = new System.Drawing.Point(347, 81);
            this.cmbEndPeriod.Name = "cmbEndPeriod";
            this.cmbEndPeriod.Size = new System.Drawing.Size(121, 21);
            this.cmbEndPeriod.TabIndex = 7;
            this.cmbEndPeriod.SelectedIndexChanged += new System.EventHandler(this.cmbEndPeriod_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 210);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 13);
            this.label2.TabIndex = 21;
            this.label2.Text = "Summarize By:";
            // 
            // chkSumCustomer
            // 
            this.chkSumCustomer.AutoSize = true;
            this.chkSumCustomer.ConfigSettingName = "chkSumCustomer";
            this.chkSumCustomer.Location = new System.Drawing.Point(34, 232);
            this.chkSumCustomer.Name = "chkSumCustomer";
            this.chkSumCustomer.Size = new System.Drawing.Size(70, 17);
            this.chkSumCustomer.TabIndex = 22;
            this.chkSumCustomer.Text = "Customer";
            this.chkSumCustomer.UseVisualStyleBackColor = true;
            // 
            // chkSumPeriod
            // 
            this.chkSumPeriod.AutoSize = true;
            this.chkSumPeriod.ConfigSettingName = "chkSumPeriod";
            this.chkSumPeriod.Location = new System.Drawing.Point(34, 278);
            this.chkSumPeriod.Name = "chkSumPeriod";
            this.chkSumPeriod.Size = new System.Drawing.Size(113, 17);
            this.chkSumPeriod.TabIndex = 23;
            this.chkSumPeriod.Text = "Accounting Period";
            this.chkSumPeriod.UseVisualStyleBackColor = true;
            // 
            // chkSumJob
            // 
            this.chkSumJob.AutoSize = true;
            this.chkSumJob.ConfigSettingName = "chkSumJob";
            this.chkSumJob.Location = new System.Drawing.Point(34, 255);
            this.chkSumJob.Name = "chkSumJob";
            this.chkSumJob.Size = new System.Drawing.Size(43, 17);
            this.chkSumJob.TabIndex = 24;
            this.chkSumJob.Text = "Job";
            this.chkSumJob.UseVisualStyleBackColor = true;
            // 
            // JobDetailAnalysis
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(495, 358);
            this.Controls.Add(this.chkSumJob);
            this.Controls.Add(this.chkSumPeriod);
            this.Controls.Add(this.chkSumCustomer);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnSMBDir);
            this.Controls.Add(this.chkUnbilled);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dteTransactionDate);
            this.Controls.Add(this.radioShowAllJobs);
            this.Controls.Add(this.radioShowTMJobs);
            this.Controls.Add(this.demoLabel);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cmbStartingPeriod);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cmbEndPeriod);
            this.Controls.Add(this.btnRunReport);
            this.Controls.Add(this.txtDataDir);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MinimumSize = new System.Drawing.Size(503, 251);
            this.Name = "JobDetailAnalysis";
            this.Text = "Syscon ReportsPlus Detailed T&M Analysis";
            this.Load += new System.EventHandler(this.JobDetailAnalysis_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtDataDir;
        private System.Windows.Forms.Button btnRunReport;
        private SysconCommon.GUI.SearchableComboBox cmbEndPeriod;
        private System.Windows.Forms.Label label3;
        private SysconCommon.GUI.SearchableComboBox cmbStartingPeriod;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem onlineHelpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem activateToolStripMenuItem;
        private System.Windows.Forms.Label demoLabel;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectNonBillableCostCodesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectTMJobTypesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectTemplateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
        private System.Windows.Forms.RadioButton radioShowTMJobs;
        private System.Windows.Forms.RadioButton radioShowAllJobs;
        private System.Windows.Forms.DateTimePicker dteTransactionDate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripMenuItem importFileToolStripMenuItem;
        private System.Windows.Forms.CheckBox chkUnbilled;
        private System.Windows.Forms.Button btnSMBDir;
        private System.Windows.Forms.ToolStripMenuItem viewAuditLogToolStripMenuItem;
        private System.Windows.Forms.Label label2;
        private SysconCommon.GUI.SysconCheckBox chkSumCustomer;
        private SysconCommon.GUI.SysconCheckBox chkSumPeriod;
        private SysconCommon.GUI.SysconCheckBox chkSumJob;


    }
}

