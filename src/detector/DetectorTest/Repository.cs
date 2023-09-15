using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectorTest
{
    internal class Repository
    {
        private Dictionary<string, object> data = new Dictionary<string, object>();
        private IRepoStrategy strategy;

        public Repository(string strategyName)
        {
            strategy = RepoStrategyFactory.GetStrategy(strategyName);
        }

        public void AddToData(int markerId, object data)
        {
            string markerIdStr = markerId.ToString();
            if (this.data.ContainsKey(markerIdStr))
            {
                this.data[markerIdStr] = data;
            }
            else
            {
                this.data.Add(markerIdStr, data);
            }
        }

        public void Setup()
        {
            strategy.Setup();
        }

        public void CloseThreads()
        {
            if (!strategy.Terminate)
            {
                strategy.Terminate = true;
            }
        }

        public void SendData()
        {
            strategy.SetData(data);
            data.Clear();
        }

        public bool CheckForTerminate()
        {
            return strategy.Terminate;
        }

        public void Update(Dictionary<string, object> markerJson, int markerId)
        {
            AddToData(markerId, markerJson);
        }
    }

}
