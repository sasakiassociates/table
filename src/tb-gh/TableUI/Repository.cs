﻿using Rhino;
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
        private static Repository instance;

        private string _target;
        private int _expire;
        private string _auth;

        private Strategy repoStrategy;

        public string response;
        public string pastResponse;

        private Repository(string target, int expire, string authorization)
        {
            _target = target;
            _expire = expire;
            _auth = authorization;
        }

        public static Repository Instance(string target, int expire, string authorization)
        {
            if (instance == null)
            {
                instance = new Repository(target, expire, authorization);
            }
            return instance;
        }

        public void set_strategy<T>() where T : Strategy
        {
            repoStrategy = RepoStrategy<T>.Instance;
        }

        public string get()
        {
            response = repoStrategy?.execute(_target, _expire, _auth);
            return response;
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
