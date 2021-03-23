using Shadowsocks.Controller;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shadowsocks.Controller.Strategy
{
    class StrategyManager
    {
        List<IStrategy> _strategies;
        public StrategyManager(ShadowsocksController controller)
        {
            _strategies = new List<IStrategy>();
            _strategies.Add(new BalancingStrategy(controller));
            IStrategy ha = new HighAvailabilityStrategy(controller);
            _strategies.Add(ha);
            _strategies.Add(new StatisticsStrategy(controller, ha));
            // TODO: load DLL plugins
        }
        public IList<IStrategy> GetStrategies()
        {
            return _strategies;
        }
    }
}
