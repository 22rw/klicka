using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Klicka
{
    public struct Parameters
    {
        public bool isRandomInterval;
        public (int Start, int End) randomRange;
        public int sleepMillis;
        public bool isStaticTarget;
        public Point target;
        public MouseButton button;
    }

    class Scheduler
    {
        private static bool _running = false;
        private static Parameters _parameters;

        public static void Start(Parameters parameters)
        {
            if (_running) return;
            _running = true;
            _parameters = parameters;
            new Thread(RunLooop).Start(_parameters);
        }

        public static void Stop() 
        {
            _running = false;
        }

        private static void RunLooop(object? p)
        {
            Parameters @params = (Parameters)p!;
            while (_running)
            {
                InputSynthesizer.Click(@params.button, @params.target, @params.isStaticTarget);
                if(@params.isRandomInterval)
                {
                    int millis = (int)(new Random().NextDouble() * (@params.randomRange.End - @params.randomRange.Start)) + @params.randomRange.Start;
                    Thread.Sleep(millis);
                } else
                {
                    Thread.Sleep(@params.sleepMillis);
                }
            }
        }
    }
}
