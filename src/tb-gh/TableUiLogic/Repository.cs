using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TableUiLogic
{
    internal class Repository
    {
        private string _target;
        private int _expire;
        private string _auth;

        private Strategy repoStrategy;

        private string response;

        public Repository(string target, int expire, string authorization)
        {
            _target = target;
            _expire = expire;
            _auth = authorization;
        }

        public void set_strategy<T>() where T : Strategy
        {
            repoStrategy = RepoStrategy<T>.Instance;
        }

        public string get()
        {
            return repoStrategy?.execute(_target, _expire, _auth);
        }

        public void close()
        {
            if (repoStrategy is RepoStrategyUdpReceive)
            {
                RepoStrategyUdpReceive.Instance.close();
            }
        }
    }
}
