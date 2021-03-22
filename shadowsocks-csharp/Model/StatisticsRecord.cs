using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shadowsocks.Model
{
    // Simple processed records for a short period of time
    public class StatisticsRecord
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string ServerIdentifier { get; set; }

        // in ping-only records, these fields would be null
        public int? AverageLatency;
        public int? MinLatency;
        public int? MaxLatency;

        private bool EmptyLatencyData => (AverageLatency == null) && (MinLatency == null) && (MaxLatency == null);

        /// <summary>
        /// note the speed is in KB/s here. While the overall traffic speed data from controller is in B/s
        /// </summary>
        public int? AverageInboundSpeed;
        public int? MinInboundSpeed;
        public int? MaxInboundSpeed;

        private bool EmptyInboundSpeedData
            => (AverageInboundSpeed == null) && (MinInboundSpeed == null) && (MaxInboundSpeed == null);

        public int? AverageOutboundSpeed;
        public int? MinOutboundSpeed;
        public int? MaxOutboundSpeed;

        private bool EmptyOutboundSpeedData
            => (AverageOutboundSpeed == null) && (MinOutboundSpeed == null) && (MaxOutboundSpeed == null);

        /// <summary>
        /// ping response time in ms.
        /// if user disabled ping test, response would be null
        /// </summary>
        public int? AverageResponse;
        public int? MinResponse;
        public int? MaxResponse;
        
        /// <summary>
        /// actually is the ratio of packets that succeeded.
        /// </summary>
        public float? PingPassRate;

        private bool EmptyResponseData
            => (AverageResponse == null) && (MinResponse == null) && (MaxResponse == null) && (PingPassRate == null);

        public bool IsEmptyData() {
            return EmptyInboundSpeedData && EmptyOutboundSpeedData && EmptyResponseData && EmptyLatencyData
                && (FailCount == null);
        }

        public StatisticsRecord()
        {
        }

        public StatisticsRecord(string identifier, ICollection<int> inboundSpeedRecords, ICollection<int> outboundSpeedRecords, ICollection<int> latencyRecords)
        {
            ServerIdentifier = identifier;
            var inbound = inboundSpeedRecords?.Where(s => s > 0).ToList();
            if (inbound != null && inbound.Any())
            {
                AverageInboundSpeed = (int) inbound.Average();
                MinInboundSpeed = inbound.Min();
                MaxInboundSpeed = inbound.Max();
            }
            var outbound = outboundSpeedRecords?.Where(s => s > 0).ToList();
            if (outbound!= null && outbound.Any())
            {
                AverageOutboundSpeed = (int) outbound.Average();
                MinOutboundSpeed = outbound.Min();
                MaxOutboundSpeed = outbound.Max();
            }
            var latency = latencyRecords?.Where(s => s > 0).ToList();
            if (latency!= null && latency.Any())
            {
                AverageLatency = (int) latency.Average();
                MinLatency = latency.Min();
                MaxLatency = latency.Max();
            }
        }

        public StatisticsRecord(string identifier, ICollection<int?> responseRecords)
        {
            ServerIdentifier = identifier;
            SetResponse(responseRecords);
        }

        public void SetResponse(ICollection<int?> responseRecords)
        {
            if (responseRecords == null) return;
            var records = responseRecords.Where(response => response != null).Select(response => response.Value).ToList();
            if (!records.Any()) return;
            AverageResponse = (int?) records.Average();
            MinResponse = records.Min();
            MaxResponse = records.Max();
            PingPassRate = records.Count / (float) responseRecords.Count;
        }

        /// <summary>
        /// number of failure per second.
        /// this data is not a reliable indicator, since some server may not be fully used.
        /// </summary>
        public float? FailCount;
    }
}
