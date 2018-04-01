﻿using System;
using System.Threading.Tasks;
using Rocket.API.Plugin;

namespace Rocket.ConsoleHost
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new ConsoleImplementation.ConsoleHost();
            host.Start();
        }
    }
}