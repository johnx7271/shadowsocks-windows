namespace Shadowsocks.View
{
    partial class StatisticsStrategyConfigurationForm
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series3 = new System.Windows.Forms.DataVisualization.Charting.Series();
            this.StatisticsChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.PingCheckBox = new System.Windows.Forms.CheckBox();
            this.bindingConfiguration = new System.Windows.Forms.BindingSource(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.chartModeSelector = new System.Windows.Forms.GroupBox();
            this.allMode = new System.Windows.Forms.RadioButton();
            this.dayMode = new System.Windows.Forms.RadioButton();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.dataCollectionMinutesNum = new System.Windows.Forms.NumericUpDown();
            this.StatisticsEnabledCheckBox = new System.Windows.Forms.CheckBox();
            this.choiceKeptMinutesNum = new System.Windows.Forms.NumericUpDown();
            this.byHourOfDayCheckBox = new System.Windows.Forms.CheckBox();
            this.repeatTimesNum = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.calculationContainer = new System.Windows.Forms.FlowLayoutPanel();
            this.CleanButton = new System.Windows.Forms.Button();
            this.serverSelector = new System.Windows.Forms.ComboBox();
            this.CancelButton = new System.Windows.Forms.Button();
            this.OKButton = new System.Windows.Forms.Button();
            this.CalculatinTip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.StatisticsChart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingConfiguration)).BeginInit();
            this.chartModeSelector.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataCollectionMinutesNum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.choiceKeptMinutesNum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repeatTimesNum)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.SuspendLayout();
            // 
            // StatisticsChart
            // 
            this.StatisticsChart.BackColor = System.Drawing.Color.Transparent;
            chartArea1.AxisX.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chartArea1.AxisX.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisX.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisX.IsReversed = true;
            chartArea1.AxisX.LabelStyle.ForeColor = System.Drawing.Color.LightGray;
            chartArea1.AxisX.LineColor = System.Drawing.Color.White;
            chartArea1.AxisX.MajorGrid.Enabled = false;
            chartArea1.AxisX.MajorTickMark.LineColor = System.Drawing.Color.White;
            chartArea1.AxisX.MajorTickMark.Size = 2F;
            chartArea1.AxisX.MaximumAutoSize = 100F;
            chartArea1.AxisX.Minimum = 0D;
            chartArea1.AxisX.Title = "Time From Now";
            chartArea1.AxisX.TitleAlignment = System.Drawing.StringAlignment.Far;
            chartArea1.AxisX.TitleForeColor = System.Drawing.Color.LightGray;
            chartArea1.AxisY.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chartArea1.AxisY.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisY.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisY.LabelStyle.ForeColor = System.Drawing.Color.LightGray;
            chartArea1.AxisY.LineColor = System.Drawing.Color.White;
            chartArea1.AxisY.MajorGrid.Enabled = false;
            chartArea1.AxisY.MajorTickMark.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisY.MajorTickMark.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisY.MajorTickMark.LineColor = System.Drawing.Color.White;
            chartArea1.AxisY.MajorTickMark.TickMarkStyle = System.Windows.Forms.DataVisualization.Charting.TickMarkStyle.InsideArea;
            chartArea1.AxisY.MaximumAutoSize = 100F;
            chartArea1.AxisY.Minimum = 0D;
            chartArea1.AxisY.Title = "Speed(KB/s)";
            chartArea1.AxisY.TitleAlignment = System.Drawing.StringAlignment.Near;
            chartArea1.AxisY.TitleForeColor = System.Drawing.Color.White;
            chartArea1.AxisY2.Crossing = -1.7976931348623157E+308D;
            chartArea1.AxisY2.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.True;
            chartArea1.AxisY2.IntervalOffsetType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisY2.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisY2.LabelStyle.ForeColor = System.Drawing.Color.LightGray;
            chartArea1.AxisY2.LineColor = System.Drawing.Color.White;
            chartArea1.AxisY2.MajorGrid.Enabled = false;
            chartArea1.AxisY2.MajorTickMark.LineColor = System.Drawing.Color.LightGray;
            chartArea1.AxisY2.MajorTickMark.TickMarkStyle = System.Windows.Forms.DataVisualization.Charting.TickMarkStyle.InsideArea;
            chartArea1.AxisY2.Minimum = 0D;
            chartArea1.AxisY2.TextOrientation = System.Windows.Forms.DataVisualization.Charting.TextOrientation.Rotated270;
            chartArea1.AxisY2.Title = "dummy";
            chartArea1.AxisY2.TitleForeColor = System.Drawing.Color.Gainsboro;
            chartArea1.BackColor = System.Drawing.Color.Transparent;
            chartArea1.BorderColor = System.Drawing.Color.LightGray;
            chartArea1.BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
            chartArea1.InnerPlotPosition.Auto = false;
            chartArea1.InnerPlotPosition.Height = 92F;
            chartArea1.InnerPlotPosition.Width = 80F;
            chartArea1.InnerPlotPosition.X = 12F;
            chartArea1.Name = "DataArea";
            chartArea1.Position.Auto = false;
            chartArea1.Position.Height = 90F;
            chartArea1.Position.Width = 76F;
            chartArea1.Position.X = 1F;
            chartArea1.Position.Y = 2F;
            this.StatisticsChart.ChartAreas.Add(chartArea1);
            this.StatisticsChart.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.BackColor = System.Drawing.Color.Transparent;
            legend1.ForeColor = System.Drawing.Color.YellowGreen;
            legend1.Name = "ChartLegend";
            this.StatisticsChart.Legends.Add(legend1);
            this.StatisticsChart.Location = new System.Drawing.Point(0, 0);
            this.StatisticsChart.Margin = new System.Windows.Forms.Padding(5);
            this.StatisticsChart.Name = "StatisticsChart";
            this.StatisticsChart.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.None;
            series1.ChartArea = "DataArea";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series1.Color = System.Drawing.Color.LightGreen;
            series1.Legend = "ChartLegend";
            series1.MarkerSize = 6;
            series1.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Diamond;
            series1.Name = "Speed";
            series1.ToolTip = "#VALX\\nInbound: #VAL KiB/s";
            series1.XValueMember = "Key";
            series1.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series1.YValueMembers = "Speed";
            series1.YValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Double;
            series2.ChartArea = "DataArea";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            series2.Color = System.Drawing.Color.Crimson;
            series2.CustomProperties = "EmptyPointValue=Zero";
            series2.Legend = "ChartLegend";
            series2.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Square;
            series2.Name = "FailRate";
            series2.ToolTip = "#VALX\\nFailRate*Scale: #VAL";
            series2.XValueMember = "Key";
            series2.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series2.YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
            series2.YValueMembers = "FailRate";
            series2.YValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Single;
            series3.BorderColor = System.Drawing.Color.Yellow;
            series3.ChartArea = "DataArea";
            series3.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Bubble;
            series3.Color = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(64)))));
            series3.Legend = "ChartLegend";
            series3.MarkerStyle = System.Windows.Forms.DataVisualization.Charting.MarkerStyle.Circle;
            series3.Name = "Ping";
            series3.ToolTip = "#VALX\\n#VALY1 ms\\nLoss: #VALY2";
            series3.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.Int32;
            series3.YAxisType = System.Windows.Forms.DataVisualization.Charting.AxisType.Secondary;
            series3.YValuesPerPoint = 2;
            this.StatisticsChart.Series.Add(series1);
            this.StatisticsChart.Series.Add(series2);
            this.StatisticsChart.Series.Add(series3);
            this.StatisticsChart.Size = new System.Drawing.Size(978, 429);
            this.StatisticsChart.TabIndex = 2;
            // 
            // PingCheckBox
            // 
            this.PingCheckBox.AutoSize = true;
            this.PingCheckBox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.bindingConfiguration, "Ping", true));
            this.PingCheckBox.Location = new System.Drawing.Point(13, 54);
            this.PingCheckBox.Margin = new System.Windows.Forms.Padding(5, 10, 5, 10);
            this.PingCheckBox.Name = "PingCheckBox";
            this.PingCheckBox.Size = new System.Drawing.Size(124, 31);
            this.PingCheckBox.TabIndex = 5;
            this.PingCheckBox.Text = "Ping Test";
            this.PingCheckBox.UseVisualStyleBackColor = true;
            this.PingCheckBox.CheckedChanged += new System.EventHandler(this.PingCheckBox_CheckedChanged);
            // 
            // bindingConfiguration
            // 
            this.bindingConfiguration.DataSource = typeof(Shadowsocks.Model.StatisticsStrategyConfiguration);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 206);
            this.label2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(167, 27);
            this.label2.TabIndex = 8;
            this.label2.Text = "Keep choice for ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(286, 206);
            this.label3.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(87, 27);
            this.label3.TabIndex = 9;
            this.label3.Text = "minutes";
            // 
            // chartModeSelector
            // 
            this.chartModeSelector.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chartModeSelector.Controls.Add(this.allMode);
            this.chartModeSelector.Controls.Add(this.dayMode);
            this.chartModeSelector.Location = new System.Drawing.Point(729, 122);
            this.chartModeSelector.Margin = new System.Windows.Forms.Padding(5, 10, 5, 10);
            this.chartModeSelector.Name = "chartModeSelector";
            this.chartModeSelector.Padding = new System.Windows.Forms.Padding(5, 10, 5, 10);
            this.chartModeSelector.Size = new System.Drawing.Size(234, 103);
            this.chartModeSelector.TabIndex = 3;
            this.chartModeSelector.TabStop = false;
            this.chartModeSelector.Text = "Chart Mode";
            // 
            // allMode
            // 
            this.allMode.AutoSize = true;
            this.allMode.Location = new System.Drawing.Point(11, 61);
            this.allMode.Margin = new System.Windows.Forms.Padding(5, 10, 5, 10);
            this.allMode.Name = "allMode";
            this.allMode.Size = new System.Drawing.Size(156, 31);
            this.allMode.TabIndex = 1;
            this.allMode.Text = "chart by day";
            this.allMode.UseVisualStyleBackColor = true;
            this.allMode.CheckedChanged += new System.EventHandler(this.allMode_CheckedChanged);
            // 
            // dayMode
            // 
            this.dayMode.AutoSize = true;
            this.dayMode.Checked = true;
            this.dayMode.Location = new System.Drawing.Point(11, 29);
            this.dayMode.Margin = new System.Windows.Forms.Padding(5, 10, 5, 10);
            this.dayMode.Name = "dayMode";
            this.dayMode.Size = new System.Drawing.Size(220, 31);
            this.dayMode.TabIndex = 0;
            this.dayMode.TabStop = true;
            this.dayMode.Text = "last day per 20 min";
            this.dayMode.UseVisualStyleBackColor = true;
            this.dayMode.CheckedChanged += new System.EventHandler(this.dayMode_CheckedChanged);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(5, 10, 5, 10);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.CleanButton);
            this.splitContainer1.Panel2.Controls.Add(this.serverSelector);
            this.splitContainer1.Panel2.Controls.Add(this.CancelButton);
            this.splitContainer1.Panel2.Controls.Add(this.OKButton);
            this.splitContainer1.Panel2.Controls.Add(this.chartModeSelector);
            this.splitContainer1.Panel2.Controls.Add(this.StatisticsChart);
            this.splitContainer1.Size = new System.Drawing.Size(978, 744);
            this.splitContainer1.SplitterDistance = 305;
            this.splitContainer1.SplitterWidth = 10;
            this.splitContainer1.TabIndex = 12;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.IsSplitterFixed = true;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(5, 10, 5, 10);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.label9);
            this.splitContainer2.Panel1.Controls.Add(this.label8);
            this.splitContainer2.Panel1.Controls.Add(this.dataCollectionMinutesNum);
            this.splitContainer2.Panel1.Controls.Add(this.StatisticsEnabledCheckBox);
            this.splitContainer2.Panel1.Controls.Add(this.choiceKeptMinutesNum);
            this.splitContainer2.Panel1.Controls.Add(this.byHourOfDayCheckBox);
            this.splitContainer2.Panel1.Controls.Add(this.repeatTimesNum);
            this.splitContainer2.Panel1.Controls.Add(this.label6);
            this.splitContainer2.Panel1.Controls.Add(this.label2);
            this.splitContainer2.Panel1.Controls.Add(this.PingCheckBox);
            this.splitContainer2.Panel1.Controls.Add(this.label3);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer2.Size = new System.Drawing.Size(978, 305);
            this.splitContainer2.SplitterDistance = 384;
            this.splitContainer2.SplitterWidth = 5;
            this.splitContainer2.TabIndex = 7;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(9, 164);
            this.label9.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(162, 27);
            this.label9.TabIndex = 20;
            this.label9.Text = "Collect data per";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft YaHei", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(286, 165);
            this.label8.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(87, 27);
            this.label8.TabIndex = 19;
            this.label8.Text = "minutes";
            // 
            // dataCollectionMinutesNum
            // 
            this.dataCollectionMinutesNum.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.bindingConfiguration, "DataCollectionMinutes", true));
            this.dataCollectionMinutesNum.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.dataCollectionMinutesNum.Location = new System.Drawing.Point(177, 162);
            this.dataCollectionMinutesNum.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dataCollectionMinutesNum.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
            this.dataCollectionMinutesNum.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.dataCollectionMinutesNum.Name = "dataCollectionMinutesNum";
            this.dataCollectionMinutesNum.Size = new System.Drawing.Size(100, 34);
            this.dataCollectionMinutesNum.TabIndex = 18;
            this.dataCollectionMinutesNum.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // StatisticsEnabledCheckBox
            // 
            this.StatisticsEnabledCheckBox.AutoSize = true;
            this.StatisticsEnabledCheckBox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.bindingConfiguration, "StatisticsEnabled", true));
            this.StatisticsEnabledCheckBox.Location = new System.Drawing.Point(13, 12);
            this.StatisticsEnabledCheckBox.Margin = new System.Windows.Forms.Padding(5, 10, 5, 10);
            this.StatisticsEnabledCheckBox.Name = "StatisticsEnabledCheckBox";
            this.StatisticsEnabledCheckBox.Size = new System.Drawing.Size(189, 31);
            this.StatisticsEnabledCheckBox.TabIndex = 17;
            this.StatisticsEnabledCheckBox.Text = "Enable Statistics";
            this.StatisticsEnabledCheckBox.UseVisualStyleBackColor = true;
            // 
            // choiceKeptMinutesNum
            // 
            this.choiceKeptMinutesNum.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.bindingConfiguration, "ChoiceKeptMinutes", true));
            this.choiceKeptMinutesNum.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.choiceKeptMinutesNum.Location = new System.Drawing.Point(177, 204);
            this.choiceKeptMinutesNum.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.choiceKeptMinutesNum.Maximum = new decimal(new int[] {
            120,
            0,
            0,
            0});
            this.choiceKeptMinutesNum.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.choiceKeptMinutesNum.Name = "choiceKeptMinutesNum";
            this.choiceKeptMinutesNum.Size = new System.Drawing.Size(100, 34);
            this.choiceKeptMinutesNum.TabIndex = 16;
            this.choiceKeptMinutesNum.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // byHourOfDayCheckBox
            // 
            this.byHourOfDayCheckBox.AutoSize = true;
            this.byHourOfDayCheckBox.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.bindingConfiguration, "ByHourOfDay", true));
            this.byHourOfDayCheckBox.Location = new System.Drawing.Point(13, 127);
            this.byHourOfDayCheckBox.Margin = new System.Windows.Forms.Padding(5, 10, 5, 10);
            this.byHourOfDayCheckBox.Name = "byHourOfDayCheckBox";
            this.byHourOfDayCheckBox.Size = new System.Drawing.Size(180, 31);
            this.byHourOfDayCheckBox.TabIndex = 15;
            this.byHourOfDayCheckBox.Text = "By hour of day";
            this.byHourOfDayCheckBox.UseVisualStyleBackColor = true;
            // 
            // repeatTimesNum
            // 
            this.repeatTimesNum.DataBindings.Add(new System.Windows.Forms.Binding("Value", this.bindingConfiguration, "RepeatTimesNum", true));
            this.repeatTimesNum.Location = new System.Drawing.Point(34, 84);
            this.repeatTimesNum.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.repeatTimesNum.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.repeatTimesNum.Name = "repeatTimesNum";
            this.repeatTimesNum.Size = new System.Drawing.Size(99, 34);
            this.repeatTimesNum.TabIndex = 14;
            this.repeatTimesNum.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft YaHei", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(139, 86);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(184, 27);
            this.label6.TabIndex = 13;
            this.label6.Text = "packets everytime";
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer3.IsSplitterFixed = true;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Margin = new System.Windows.Forms.Padding(5, 10, 5, 10);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.calculationContainer);
            this.splitContainer3.Size = new System.Drawing.Size(589, 305);
            this.splitContainer3.SplitterDistance = 42;
            this.splitContainer3.SplitterWidth = 1;
            this.splitContainer3.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 9);
            this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(137, 27);
            this.label1.TabIndex = 0;
            this.label1.Text = "Final Score =";
            this.CalculatinTip.SetToolTip(this.label1, "(The server with the highest score would be choosen)");
            // 
            // calculationContainer
            // 
            this.calculationContainer.AutoScroll = true;
            this.calculationContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.calculationContainer.Location = new System.Drawing.Point(0, 0);
            this.calculationContainer.Margin = new System.Windows.Forms.Padding(5, 10, 5, 10);
            this.calculationContainer.Name = "calculationContainer";
            this.calculationContainer.Size = new System.Drawing.Size(589, 262);
            this.calculationContainer.TabIndex = 1;
            // 
            // CleanButton
            // 
            this.CleanButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CleanButton.Location = new System.Drawing.Point(754, 338);
            this.CleanButton.Name = "CleanButton";
            this.CleanButton.Size = new System.Drawing.Size(206, 37);
            this.CleanButton.TabIndex = 7;
            this.CleanButton.Text = "History Clean";
            this.CalculatinTip.SetToolTip(this.CleanButton, "Remove data older than 7 days");
            this.CleanButton.UseVisualStyleBackColor = true;
            this.CleanButton.Click += new System.EventHandler(this.CleanButton_Click);
            // 
            // serverSelector
            // 
            this.serverSelector.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.serverSelector.FormattingEnabled = true;
            this.serverSelector.Location = new System.Drawing.Point(729, 85);
            this.serverSelector.Name = "serverSelector";
            this.serverSelector.Size = new System.Drawing.Size(233, 35);
            this.serverSelector.TabIndex = 6;
            this.serverSelector.SelectionChangeCommitted += new System.EventHandler(this.serverSelector_SelectionChangeCommitted);
            // 
            // CancelButton
            // 
            this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CancelButton.Location = new System.Drawing.Point(861, 253);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(101, 41);
            this.CancelButton.TabIndex = 5;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKButton.Location = new System.Drawing.Point(754, 253);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(101, 41);
            this.OKButton.TabIndex = 4;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // StatisticsStrategyConfigurationForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 27F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(978, 744);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("Microsoft YaHei", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(5, 10, 5, 10);
            this.MinimumSize = new System.Drawing.Size(1000, 800);
            this.Name = "StatisticsStrategyConfigurationForm";
            this.Text = "StatisticsStrategyConfigurationForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.StatisticsStrategyConfigurationForm_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.StatisticsChart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingConfiguration)).EndInit();
            this.chartModeSelector.ResumeLayout(false);
            this.chartModeSelector.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataCollectionMinutesNum)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.choiceKeptMinutesNum)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repeatTimesNum)).EndInit();
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel1.PerformLayout();
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.DataVisualization.Charting.Chart StatisticsChart;
        private System.Windows.Forms.CheckBox PingCheckBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox chartModeSelector;
        private System.Windows.Forms.RadioButton allMode;
        private System.Windows.Forms.RadioButton dayMode;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.FlowLayoutPanel calculationContainer;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.NumericUpDown repeatTimesNum;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox byHourOfDayCheckBox;
        private System.Windows.Forms.NumericUpDown choiceKeptMinutesNum;
        private System.Windows.Forms.CheckBox StatisticsEnabledCheckBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown dataCollectionMinutesNum;
        private System.Windows.Forms.BindingSource bindingConfiguration;
        private new System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.ComboBox serverSelector;
        private System.Windows.Forms.ToolTip CalculatinTip;
        private System.Windows.Forms.Button CleanButton;
    }
}