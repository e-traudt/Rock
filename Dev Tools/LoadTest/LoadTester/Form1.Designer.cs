namespace LoadTester
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
        protected override void Dispose( bool disposing )
        {
            if ( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea5 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.CustomLabel customLabel9 = new System.Windows.Forms.DataVisualization.Charting.CustomLabel();
            System.Windows.Forms.DataVisualization.Charting.CustomLabel customLabel10 = new System.Windows.Forms.DataVisualization.Charting.CustomLabel();
            System.Windows.Forms.DataVisualization.Charting.Legend legend5 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnStart = new System.Windows.Forms.Button();
            this.tbUrl = new System.Windows.Forms.TextBox();
            this.tbClientCount = new System.Windows.Forms.TextBox();
            this.lblClientCount = new System.Windows.Forms.Label();
            this.pgbRequestCount = new System.Windows.Forms.ProgressBar();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.cbDownloadHeaderSrcElements = new System.Windows.Forms.CheckBox();
            this.cbDownloadBodySrcElements = new System.Windows.Forms.CheckBox();
            this.tbStats = new System.Windows.Forms.TextBox();
            this.lblThreadCount = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tbRequestsDelayMS = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.radPOST = new System.Windows.Forms.RadioButton();
            this.radGET = new System.Windows.Forms.RadioButton();
            this.tbPostBody = new System.Windows.Forms.TextBox();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.lblStatus = new System.Windows.Forms.Label();
            this.pgbResponseCount = new System.Windows.Forms.ProgressBar();
            this.btnStop = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.tbTestDurationSecs = new System.Windows.Forms.TextBox();
            this.tbStartWindowMS = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbExceptions = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(430, 227);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(60, 24);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // tbUrl
            // 
            this.tbUrl.Location = new System.Drawing.Point(12, 8);
            this.tbUrl.Name = "tbUrl";
            this.tbUrl.Size = new System.Drawing.Size(704, 20);
            this.tbUrl.TabIndex = 1;
            this.tbUrl.Text = "http://localhost:6229/Webhooks/TextToWorkflowTwilio.ashx";
            this.tbUrl.TextChanged += new System.EventHandler(this.tbUrl_TextChanged);
            // 
            // tbClientCount
            // 
            this.tbClientCount.Location = new System.Drawing.Point(436, 92);
            this.tbClientCount.Name = "tbClientCount";
            this.tbClientCount.Size = new System.Drawing.Size(45, 20);
            this.tbClientCount.TabIndex = 3;
            this.tbClientCount.Text = "255";
            // 
            // lblClientCount
            // 
            this.lblClientCount.AutoSize = true;
            this.lblClientCount.Location = new System.Drawing.Point(432, 76);
            this.lblClientCount.Margin = new System.Windows.Forms.Padding(0);
            this.lblClientCount.Name = "lblClientCount";
            this.lblClientCount.Size = new System.Drawing.Size(64, 13);
            this.lblClientCount.TabIndex = 4;
            this.lblClientCount.Text = "Client Count";
            // 
            // pgbRequestCount
            // 
            this.pgbRequestCount.Location = new System.Drawing.Point(12, 260);
            this.pgbRequestCount.Name = "pgbRequestCount";
            this.pgbRequestCount.Size = new System.Drawing.Size(960, 10);
            this.pgbRequestCount.TabIndex = 9;
            // 
            // chart1
            // 
            this.chart1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            chartArea5.Area3DStyle.Inclination = 45;
            chartArea5.Area3DStyle.LightStyle = System.Windows.Forms.DataVisualization.Charting.LightStyle.Realistic;
            customLabel9.GridTicks = System.Windows.Forms.DataVisualization.Charting.GridTickTypes.TickMark;
            customLabel9.Text = "@0ms";
            customLabel9.ToPosition = 10D;
            customLabel10.FromPosition = 10D;
            customLabel10.Text = "b";
            customLabel10.ToPosition = 20D;
            chartArea5.AxisX.CustomLabels.Add(customLabel9);
            chartArea5.AxisX.CustomLabels.Add(customLabel10);
            chartArea5.AxisX.IntervalAutoMode = System.Windows.Forms.DataVisualization.Charting.IntervalAutoMode.VariableCount;
            chartArea5.AxisX.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Milliseconds;
            chartArea5.AxisX.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Milliseconds;
            chartArea5.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea5);
            legend5.Enabled = false;
            legend5.Name = "Legend1";
            this.chart1.Legends.Add(legend5);
            this.chart1.Location = new System.Drawing.Point(12, 301);
            this.chart1.Name = "chart1";
            this.chart1.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.Grayscale;
            this.chart1.Size = new System.Drawing.Size(408, 279);
            this.chart1.TabIndex = 11;
            this.chart1.Text = "chart1";
            // 
            // cbDownloadHeaderSrcElements
            // 
            this.cbDownloadHeaderSrcElements.AutoSize = true;
            this.cbDownloadHeaderSrcElements.Location = new System.Drawing.Point(566, 34);
            this.cbDownloadHeaderSrcElements.Name = "cbDownloadHeaderSrcElements";
            this.cbDownloadHeaderSrcElements.Size = new System.Drawing.Size(151, 17);
            this.cbDownloadHeaderSrcElements.TabIndex = 12;
            this.cbDownloadHeaderSrcElements.Text = "GET Header Src Elements";
            this.cbDownloadHeaderSrcElements.UseVisualStyleBackColor = true;
            // 
            // cbDownloadBodySrcElements
            // 
            this.cbDownloadBodySrcElements.AutoSize = true;
            this.cbDownloadBodySrcElements.Location = new System.Drawing.Point(566, 58);
            this.cbDownloadBodySrcElements.Name = "cbDownloadBodySrcElements";
            this.cbDownloadBodySrcElements.Size = new System.Drawing.Size(140, 17);
            this.cbDownloadBodySrcElements.TabIndex = 13;
            this.cbDownloadBodySrcElements.Text = "GET Body Src Elements";
            this.cbDownloadBodySrcElements.UseVisualStyleBackColor = true;
            // 
            // tbStats
            // 
            this.tbStats.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbStats.Location = new System.Drawing.Point(727, 16);
            this.tbStats.Multiline = true;
            this.tbStats.Name = "tbStats";
            this.tbStats.Size = new System.Drawing.Size(245, 156);
            this.tbStats.TabIndex = 14;
            // 
            // lblThreadCount
            // 
            this.lblThreadCount.AutoSize = true;
            this.lblThreadCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblThreadCount.Location = new System.Drawing.Point(723, 185);
            this.lblThreadCount.Name = "lblThreadCount";
            this.lblThreadCount.Size = new System.Drawing.Size(66, 24);
            this.lblThreadCount.TabIndex = 15;
            this.lblThreadCount.Text = "lblInfo";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(432, 116);
            this.label1.Margin = new System.Windows.Forms.Padding(0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(93, 13);
            this.label1.TabIndex = 17;
            this.label1.Text = "Start Window (ms)";
            // 
            // tbRequestsDelayMS
            // 
            this.tbRequestsDelayMS.Location = new System.Drawing.Point(436, 176);
            this.tbRequestsDelayMS.Name = "tbRequestsDelayMS";
            this.tbRequestsDelayMS.Size = new System.Drawing.Size(45, 20);
            this.tbRequestsDelayMS.TabIndex = 16;
            this.tbRequestsDelayMS.Text = "0";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(563, 93);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(43, 13);
            this.label2.TabIndex = 18;
            this.label2.Text = "Method";
            // 
            // radPOST
            // 
            this.radPOST.AutoSize = true;
            this.radPOST.Checked = true;
            this.radPOST.Location = new System.Drawing.Point(566, 109);
            this.radPOST.Name = "radPOST";
            this.radPOST.Size = new System.Drawing.Size(54, 17);
            this.radPOST.TabIndex = 19;
            this.radPOST.TabStop = true;
            this.radPOST.Text = "POST";
            this.radPOST.UseVisualStyleBackColor = true;
            // 
            // radGET
            // 
            this.radGET.AutoSize = true;
            this.radGET.Location = new System.Drawing.Point(626, 109);
            this.radGET.Name = "radGET";
            this.radGET.Size = new System.Drawing.Size(47, 17);
            this.radGET.TabIndex = 20;
            this.radGET.Text = "GET";
            this.radGET.UseVisualStyleBackColor = true;
            // 
            // tbPostBody
            // 
            this.tbPostBody.Font = new System.Drawing.Font("Consolas", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbPostBody.Location = new System.Drawing.Point(12, 34);
            this.tbPostBody.Multiline = true;
            this.tbPostBody.Name = "tbPostBody";
            this.tbPostBody.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbPostBody.Size = new System.Drawing.Size(404, 220);
            this.tbPostBody.TabIndex = 21;
            this.tbPostBody.Text = resources.GetString("tbPostBody.Text");
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // lblStatus
            // 
            this.lblStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblStatus.AutoSize = true;
            this.lblStatus.BackColor = System.Drawing.Color.Black;
            this.lblStatus.Font = new System.Drawing.Font("Consolas", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.Color.White;
            this.lblStatus.Location = new System.Drawing.Point(564, 144);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(80, 22);
            this.lblStatus.TabIndex = 22;
            this.lblStatus.Text = "STOPPED";
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // pgbResponseCount
            // 
            this.pgbResponseCount.Location = new System.Drawing.Point(12, 276);
            this.pgbResponseCount.Name = "pgbResponseCount";
            this.pgbResponseCount.Size = new System.Drawing.Size(960, 10);
            this.pgbResponseCount.TabIndex = 23;
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(496, 227);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(60, 24);
            this.btnStop.TabIndex = 24;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(432, 32);
            this.label3.Margin = new System.Windows.Forms.Padding(0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(102, 13);
            this.label3.TabIndex = 26;
            this.label3.Text = "Test Duration (secs)";
            // 
            // tbTestDurationSecs
            // 
            this.tbTestDurationSecs.Location = new System.Drawing.Point(436, 48);
            this.tbTestDurationSecs.Margin = new System.Windows.Forms.Padding(0);
            this.tbTestDurationSecs.Name = "tbTestDurationSecs";
            this.tbTestDurationSecs.Size = new System.Drawing.Size(45, 20);
            this.tbTestDurationSecs.TabIndex = 25;
            this.tbTestDurationSecs.Text = "180";
            // 
            // tbStartWindowMS
            // 
            this.tbStartWindowMS.Location = new System.Drawing.Point(436, 132);
            this.tbStartWindowMS.Name = "tbStartWindowMS";
            this.tbStartWindowMS.Size = new System.Drawing.Size(56, 20);
            this.tbStartWindowMS.TabIndex = 27;
            this.tbStartWindowMS.Text = "5000";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(432, 160);
            this.label4.Margin = new System.Windows.Forms.Padding(0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(104, 13);
            this.label4.TabIndex = 28;
            this.label4.Text = "Requests Delay (ms)";
            // 
            // tbExceptions
            // 
            this.tbExceptions.Location = new System.Drawing.Point(436, 300);
            this.tbExceptions.Multiline = true;
            this.tbExceptions.Name = "tbExceptions";
            this.tbExceptions.Size = new System.Drawing.Size(540, 288);
            this.tbExceptions.TabIndex = 29;
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(984, 592);
            this.Controls.Add(this.tbExceptions);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.tbStartWindowMS);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbTestDurationSecs);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.pgbResponseCount);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.tbPostBody);
            this.Controls.Add(this.radGET);
            this.Controls.Add(this.radPOST);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbRequestsDelayMS);
            this.Controls.Add(this.lblThreadCount);
            this.Controls.Add(this.tbStats);
            this.Controls.Add(this.cbDownloadBodySrcElements);
            this.Controls.Add(this.cbDownloadHeaderSrcElements);
            this.Controls.Add(this.chart1);
            this.Controls.Add(this.pgbRequestCount);
            this.Controls.Add(this.lblClientCount);
            this.Controls.Add(this.tbClientCount);
            this.Controls.Add(this.tbUrl);
            this.Controls.Add(this.btnStart);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Load Tester";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.TextBox tbUrl;
        private System.Windows.Forms.TextBox tbClientCount;
        private System.Windows.Forms.Label lblClientCount;
        private System.Windows.Forms.ProgressBar pgbRequestCount;
        private System.Windows.Forms.DataVisualization.Charting.Chart chart1;
        private System.Windows.Forms.CheckBox cbDownloadHeaderSrcElements;
        private System.Windows.Forms.CheckBox cbDownloadBodySrcElements;
        private System.Windows.Forms.TextBox tbStats;
        private System.Windows.Forms.Label lblThreadCount;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbRequestsDelayMS;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RadioButton radPOST;
        private System.Windows.Forms.RadioButton radGET;
        private System.Windows.Forms.TextBox tbPostBody;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ProgressBar pgbResponseCount;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbTestDurationSecs;
        private System.Windows.Forms.TextBox tbStartWindowMS;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbExceptions;
        private System.Windows.Forms.Timer timer1;
    }
}

