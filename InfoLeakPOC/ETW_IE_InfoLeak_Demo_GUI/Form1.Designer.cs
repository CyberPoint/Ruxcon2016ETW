namespace ETW_IE_InfoLeak_Demo_GUI
{
    partial class Form1
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
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Cookies Stored");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("URLs Accessed");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Requests Made");
            this.btnStartProvider = new System.Windows.Forms.Button();
            this.btnStopProvider = new System.Windows.Forms.Button();
            this.btnStartConsumer = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.providerStatusLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.consumerStatusLabel = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.tabControlResults = new System.Windows.Forms.TabControl();
            this.tabPageTextView = new System.Windows.Forms.TabPage();
            this.tabPageTreeView = new System.Windows.Forms.TabPage();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.tabExtractedData = new System.Windows.Forms.TabPage();
            this.extractedDataTreeView = new System.Windows.Forms.TreeView();
            this.panel2 = new System.Windows.Forms.Panel();
            this.checkBox_autoScrollRequests = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.numDroppedEventsLabel = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.numParsedEventsLabel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.requestsMissingLabel = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnOpenEtlFile = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.tabControlResults.SuspendLayout();
            this.tabPageTextView.SuspendLayout();
            this.tabPageTreeView.SuspendLayout();
            this.tabExtractedData.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnStartProvider
            // 
            this.btnStartProvider.Location = new System.Drawing.Point(13, 12);
            this.btnStartProvider.Name = "btnStartProvider";
            this.btnStartProvider.Size = new System.Drawing.Size(94, 23);
            this.btnStartProvider.TabIndex = 0;
            this.btnStartProvider.Text = "Start Provider";
            this.btnStartProvider.UseVisualStyleBackColor = true;
            this.btnStartProvider.Click += new System.EventHandler(this.btnStartProvider_Click);
            // 
            // btnStopProvider
            // 
            this.btnStopProvider.Enabled = false;
            this.btnStopProvider.Location = new System.Drawing.Point(13, 42);
            this.btnStopProvider.Name = "btnStopProvider";
            this.btnStopProvider.Size = new System.Drawing.Size(94, 23);
            this.btnStopProvider.TabIndex = 1;
            this.btnStopProvider.Text = "Stop Provider";
            this.btnStopProvider.UseVisualStyleBackColor = true;
            this.btnStopProvider.Click += new System.EventHandler(this.btnStopProvider_Click);
            // 
            // btnStartConsumer
            // 
            this.btnStartConsumer.Enabled = false;
            this.btnStartConsumer.Location = new System.Drawing.Point(13, 72);
            this.btnStartConsumer.Name = "btnStartConsumer";
            this.btnStartConsumer.Size = new System.Drawing.Size(94, 23);
            this.btnStartConsumer.TabIndex = 2;
            this.btnStartConsumer.Text = "Start Consumer";
            this.btnStartConsumer.UseVisualStyleBackColor = true;
            this.btnStartConsumer.Click += new System.EventHandler(this.btnStartConsumer_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 102);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Provider Status:";
            // 
            // providerStatusLabel
            // 
            this.providerStatusLabel.AutoSize = true;
            this.providerStatusLabel.Location = new System.Drawing.Point(17, 119);
            this.providerStatusLabel.Name = "providerStatusLabel";
            this.providerStatusLabel.Size = new System.Drawing.Size(47, 13);
            this.providerStatusLabel.TabIndex = 5;
            this.providerStatusLabel.Text = "Stopped";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 150);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(90, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Consumer Status:";
            // 
            // consumerStatusLabel
            // 
            this.consumerStatusLabel.AutoSize = true;
            this.consumerStatusLabel.Location = new System.Drawing.Point(17, 167);
            this.consumerStatusLabel.Name = "consumerStatusLabel";
            this.consumerStatusLabel.Size = new System.Drawing.Size(47, 13);
            this.consumerStatusLabel.TabIndex = 7;
            this.consumerStatusLabel.Text = "Stopped";
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Location = new System.Drawing.Point(3, 3);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox1.Size = new System.Drawing.Size(592, 355);
            this.textBox1.TabIndex = 8;
            this.textBox1.WordWrap = false;
            // 
            // tabControlResults
            // 
            this.tabControlResults.Controls.Add(this.tabPageTextView);
            this.tabControlResults.Controls.Add(this.tabPageTreeView);
            this.tabControlResults.Controls.Add(this.tabExtractedData);
            this.tabControlResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlResults.Location = new System.Drawing.Point(0, 0);
            this.tabControlResults.Name = "tabControlResults";
            this.tabControlResults.SelectedIndex = 0;
            this.tabControlResults.Size = new System.Drawing.Size(606, 387);
            this.tabControlResults.TabIndex = 10;
            // 
            // tabPageTextView
            // 
            this.tabPageTextView.Controls.Add(this.textBox1);
            this.tabPageTextView.Location = new System.Drawing.Point(4, 22);
            this.tabPageTextView.Name = "tabPageTextView";
            this.tabPageTextView.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTextView.Size = new System.Drawing.Size(598, 361);
            this.tabPageTextView.TabIndex = 0;
            this.tabPageTextView.Text = "Text View";
            this.tabPageTextView.UseVisualStyleBackColor = true;
            // 
            // tabPageTreeView
            // 
            this.tabPageTreeView.Controls.Add(this.treeView1);
            this.tabPageTreeView.Location = new System.Drawing.Point(4, 22);
            this.tabPageTreeView.Name = "tabPageTreeView";
            this.tabPageTreeView.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageTreeView.Size = new System.Drawing.Size(598, 361);
            this.tabPageTreeView.TabIndex = 2;
            this.tabPageTreeView.Text = "Tree View";
            this.tabPageTreeView.UseVisualStyleBackColor = true;
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.Location = new System.Drawing.Point(3, 3);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(592, 355);
            this.treeView1.TabIndex = 0;
            this.treeView1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.treeView_CopySelectedNodeText);
            // 
            // tabExtractedData
            // 
            this.tabExtractedData.Controls.Add(this.extractedDataTreeView);
            this.tabExtractedData.Controls.Add(this.panel2);
            this.tabExtractedData.Location = new System.Drawing.Point(4, 22);
            this.tabExtractedData.Name = "tabExtractedData";
            this.tabExtractedData.Padding = new System.Windows.Forms.Padding(3);
            this.tabExtractedData.Size = new System.Drawing.Size(598, 361);
            this.tabExtractedData.TabIndex = 3;
            this.tabExtractedData.Text = "Extracted Data";
            this.tabExtractedData.UseVisualStyleBackColor = true;
            // 
            // extractedDataTreeView
            // 
            this.extractedDataTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.extractedDataTreeView.Location = new System.Drawing.Point(3, 3);
            this.extractedDataTreeView.Name = "extractedDataTreeView";
            treeNode1.Name = "CookiesStoredNode";
            treeNode1.Text = "Cookies Stored";
            treeNode2.Name = "URLsAccessedNode";
            treeNode2.Text = "URLs Accessed";
            treeNode3.Name = "RequestsMadeNode";
            treeNode3.Text = "Requests Made";
            this.extractedDataTreeView.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode3});
            this.extractedDataTreeView.ShowNodeToolTips = true;
            this.extractedDataTreeView.Size = new System.Drawing.Size(592, 327);
            this.extractedDataTreeView.TabIndex = 0;
            this.extractedDataTreeView.MouseClick += new System.Windows.Forms.MouseEventHandler(this.treeView_CopySelectedNodeText);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Transparent;
            this.panel2.Controls.Add(this.checkBox_autoScrollRequests);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(3, 330);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(592, 28);
            this.panel2.TabIndex = 1;
            // 
            // checkBox_autoScrollRequests
            // 
            this.checkBox_autoScrollRequests.AutoSize = true;
            this.checkBox_autoScrollRequests.Location = new System.Drawing.Point(0, 11);
            this.checkBox_autoScrollRequests.Name = "checkBox_autoScrollRequests";
            this.checkBox_autoScrollRequests.Size = new System.Drawing.Size(141, 17);
            this.checkBox_autoScrollRequests.TabIndex = 0;
            this.checkBox_autoScrollRequests.Text = "Auto-scroll new requests";
            this.checkBox_autoScrollRequests.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.numDroppedEventsLabel);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.numParsedEventsLabel);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.requestsMissingLabel);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.btnOpenEtlFile);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.btnStartProvider);
            this.panel1.Controls.Add(this.consumerStatusLabel);
            this.panel1.Controls.Add(this.btnStopProvider);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.btnStartConsumer);
            this.panel1.Controls.Add(this.providerStatusLabel);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel1.Location = new System.Drawing.Point(606, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(121, 387);
            this.panel1.TabIndex = 11;
            // 
            // numDroppedEventsLabel
            // 
            this.numDroppedEventsLabel.AutoSize = true;
            this.numDroppedEventsLabel.Location = new System.Drawing.Point(17, 316);
            this.numDroppedEventsLabel.Name = "numDroppedEventsLabel";
            this.numDroppedEventsLabel.Size = new System.Drawing.Size(13, 13);
            this.numDroppedEventsLabel.TabIndex = 14;
            this.numDroppedEventsLabel.Text = "0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 303);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(87, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Dropped Events:";
            // 
            // numParsedEventsLabel
            // 
            this.numParsedEventsLabel.AutoSize = true;
            this.numParsedEventsLabel.Location = new System.Drawing.Point(17, 210);
            this.numParsedEventsLabel.Name = "numParsedEventsLabel";
            this.numParsedEventsLabel.Size = new System.Drawing.Size(13, 13);
            this.numParsedEventsLabel.TabIndex = 12;
            this.numParsedEventsLabel.Text = "0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 197);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(92, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "Received Events:";
            // 
            // requestsMissingLabel
            // 
            this.requestsMissingLabel.AutoSize = true;
            this.requestsMissingLabel.Location = new System.Drawing.Point(17, 281);
            this.requestsMissingLabel.Name = "requestsMissingLabel";
            this.requestsMissingLabel.Size = new System.Drawing.Size(13, 13);
            this.requestsMissingLabel.TabIndex = 10;
            this.requestsMissingLabel.Text = "0";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(14, 238);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 43);
            this.label2.TabIndex = 9;
            this.label2.Text = "Number of incomplete requests skipped:";
            // 
            // btnOpenEtlFile
            // 
            this.btnOpenEtlFile.Location = new System.Drawing.Point(13, 352);
            this.btnOpenEtlFile.Name = "btnOpenEtlFile";
            this.btnOpenEtlFile.Size = new System.Drawing.Size(94, 23);
            this.btnOpenEtlFile.TabIndex = 8;
            this.btnOpenEtlFile.Text = "Open ETL File";
            this.btnOpenEtlFile.UseVisualStyleBackColor = true;
            this.btnOpenEtlFile.Click += new System.EventHandler(this.btnOpenEtlFile_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "ETL files|*.etl";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "etl";
            this.saveFileDialog1.Filter = "ETL files|*.etl";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(727, 387);
            this.Controls.Add(this.tabControlResults);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.Text = "ETW IE Information Leak";
            this.tabControlResults.ResumeLayout(false);
            this.tabPageTextView.ResumeLayout(false);
            this.tabPageTextView.PerformLayout();
            this.tabPageTreeView.ResumeLayout(false);
            this.tabExtractedData.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnStartProvider;
        private System.Windows.Forms.Button btnStopProvider;
        private System.Windows.Forms.Button btnStartConsumer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label providerStatusLabel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label consumerStatusLabel;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TabControl tabControlResults;
        private System.Windows.Forms.TabPage tabPageTextView;
        private System.Windows.Forms.TabPage tabPageTreeView;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TabPage tabExtractedData;
        private System.Windows.Forms.TreeView extractedDataTreeView;
        private System.Windows.Forms.Button btnOpenEtlFile;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        //private System.Windows.Forms.Button btnStartAndSave;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label requestsMissingLabel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label numParsedEventsLabel;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.CheckBox checkBox_autoScrollRequests;
        private System.Windows.Forms.Label numDroppedEventsLabel;
        private System.Windows.Forms.Label label5;
    }
}

