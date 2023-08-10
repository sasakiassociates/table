using Rhino;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TableUI
{
    internal class Repository
    {
        private string _target;
        private int _expire;
        private string _auth;

        private RepoStrategy repoStrategy;

        private string response;

        public Repository(string target, int expire, string authorization)
        {
            _target = target;
            _expire = expire;
            _auth = authorization;
        }

        public void set_strategy(RepoStrategy strategy)
        {
            repoStrategy = strategy;
        }

        public string get()
        {
            return repoStrategy.execute(_target, _expire, _auth);
        }
    }
}
