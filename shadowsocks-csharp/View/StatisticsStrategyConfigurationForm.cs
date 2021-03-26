using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Shadowsocks.Controller;
using Shadowsocks.Model;

namespace Shadowsocks.View
{
    /// <summary>
    /// note, inbound speed is the most important.
    /// the coefficient of laterncy should be negative    
    /// </summary>
    public partial class StatisticsStrategyConfigurationForm : Form
    {
        private readonly ShadowsocksController _controller;
        private StatisticsStrategyConfiguration _configuration;
        private readonly DataTable _dataTable = new DataTable();
        private List<string> _servers;
        private readonly Series _speedSeries;
        private readonly Series _failRateSeries;
        private readonly Series _pingSeries;
        private double _y2Max;
        private int _failRateMax;

        public StatisticsStrategyConfigurationForm(ShadowsocksController controller)
        {
            if (controller == null) return;
            InitializeComponent();
            _speedSeries = StatisticsChart.Series["Speed"];
            _failRateSeries = StatisticsChart.Series["FailRate"];
            _pingSeries = StatisticsChart.Series["Ping"];
            _controller = controller;
            _controller.ConfigChanged += controller_ConfigChanged;
            LoadConfiguration();
            Load += (sender, args) => InitData();
        }

        private void controller_ConfigChanged(Object sender, EventArgs args)
        {
            var configs = _controller.GetCurrentConfiguration().configs;
            _servers = configs.Select(server => server.Identifier()).ToList();
            serverSelector.DataSource = _servers;
            serverSelector.SelectedIndex = _servers.Count > 0 ? 0 : -1;
        }

        private void LoadConfiguration()
        {
            var configs = _controller.GetCurrentConfiguration().configs;
            _servers = configs.Select(server => server.Identifier()).ToList();
            _configuration = _controller.StatisticsConfiguration
                             ?? new StatisticsStrategyConfiguration();
            if (_configuration.Calculations == null)
            {
                _configuration = new StatisticsStrategyConfiguration();
            }
        }

        private void InitData()
        {
            bindingConfiguration.Add(_configuration);
            foreach (var kv in _configuration.Calculations)
            {
                var calculation = new CalculationControl(kv.Key, kv.Value);
                calculationContainer.Controls.Add(calculation);
            }

            serverSelector.DataSource = _servers;

            _dataTable.Columns.Add("Key", typeof(Int32));
            _dataTable.Columns.Add("Speed", typeof (int));
            _dataTable.Columns.Add("Ping", typeof(int[]));
            _dataTable.Columns.Add("FailRate", typeof(float));

            StatisticsChart.DataSource = _dataTable;
            LoadChartData();
            BindPingData();
            StatisticsChart.DataBind();            
        }

        

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            foreach (CalculationControl calculation in calculationContainer.Controls)
            {
                _configuration.Calculations[calculation.Value] = calculation.Factor;
            }
            _controller?.SaveStrategyConfigurations(_configuration);
            _controller?.UpdateStatisticsConfiguration(StatisticsEnabledCheckBox.Checked);
            Close();
        }

        private void LoadChartData()
        {
            var serverName = _servers[serverSelector.SelectedIndex];
            _dataTable.Rows.Clear();

            //return directly when no data is usable
            Dictionary<string, List<StatisticsRecord>> sta = _controller.availabilityStatistics?.RawStatistics;
            if ( sta == null) return;
            List<StatisticsRecord> statistics;
            if (!sta.TryGetValue(serverName, out statistics)) return;

            ChartArea ca = this.StatisticsChart.ChartAreas[0];
            IEnumerable<IGrouping<int, StatisticsRecord>> dataGroups;
            DateTime n = DateTime.Now;
            if (allMode.Checked)
            {
                int nd = n.DayOfYear;
                dataGroups = statistics.GroupBy(data => nd - data.Timestamp.DayOfYear);
                ca.AxisX.Title = $"Days from today";                
            }
            else
            {                
                dataGroups = from data in statistics
                             let twentymin = (int) ((n - data.Timestamp).TotalSeconds / 1200)
                             where twentymin <= 24 * 3
                             group data by twentymin;                
                ca.AxisX.Title = $"Time from Now in units of 20 min";
            }

            var finalData = from dataGroup in dataGroups
                            orderby dataGroup.Key descending
                            select new
                            {
                                dataGroup.Key,
                                Speed = dataGroup.Average(data => data.AverageInboundSpeed) ?? 0,
                                Ping = new int[2]{(int)(dataGroup.Average(data => data.AverageResponse) ?? 0),
                                    100 - (int)(dataGroup.Average(data => data.PingPassRate) ?? 1) * 100 },
                                FailRate = dataGroup.Average(data => data.FailCount) ?? -0.001
                            };

            int pingmax = 0;
            double rateMax = 0;
            foreach (var data in finalData.Where(data => data.Speed != 0 || data.Ping[0] != 0 || data.FailRate != -0.001))
            {
                if (data.Ping[0] > pingmax)
                    pingmax = data.Ping[0];
                if (data.FailRate > rateMax)
                    rateMax = data.FailRate;
                _dataTable.Rows.Add(data.Key, data.Speed, data.Ping, data.FailRate);                
            }
            _y2Max = Round(pingmax);
            _failRateMax = (int) Round(rateMax < 0 ? 1 : rateMax);
        }

        private void BindPingData()
        {
            ChartArea ca = this.StatisticsChart.ChartAreas[0];
            Axis y2 = ca.AxisY2;
            y2.Maximum = _y2Max;
            double rate = _y2Max / _failRateMax;
            
            _pingSeries.Points.Clear();
            foreach (DataRow row in _dataTable.Rows)
            {
                object[] r = row.ItemArray;
                int[] ping = (int[])(r[2]);
                _pingSeries.Points.AddXY(r[0], ping[0], ping[1]);
                r[3] = (float)(r[3]) * rate;
                row.ItemArray = r;
            }
            y2.Title = $"Ping in ms\nFailRate * {rate:0.##}";
        }

        /// <summary>
        /// if f<1 return 1; else return a integer that is f round to its highest digit.
        /// </summary>        
        static public double Round(double f)
        {
            int scale = 1;             
            while (scale < f)
                scale *= 10;

            if (scale == 1)
                return 1;
            else {
                scale /= 10;
                return Math.Ceiling(f / scale) * scale;
            }
        }

        private void serverSelector_SelectionChangeCommitted(object sender, EventArgs e)
        {
            LoadChartData();
            BindPingData();
            StatisticsChart.DataBind();            
        }

        private void dayMode_CheckedChanged(object sender, EventArgs e)
        {
            LoadChartData();
            BindPingData();
            StatisticsChart.DataBind();
        }

        private void allMode_CheckedChanged(object sender, EventArgs e)
        {
            LoadChartData();
            BindPingData();
            StatisticsChart.DataBind();            
        }

        private void PingCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            repeatTimesNum.ReadOnly = !PingCheckBox.Checked;
        }

        private void StatisticsStrategyConfigurationForm_FormClosed(object sender, FormClosedEventArgs e)
        {            
            _controller.ConfigChanged -= controller_ConfigChanged;            
        }

        private void CleanButton_Click(object sender, EventArgs e)
        {
            _controller.availabilityStatistics?.CleanseRawStatistics(7);
        }
    }
}
