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
        public bool isLimitedDuration;
        public int targetClickAmount;
    }

    class Scheduler
    {
        private static bool _running = false;
        private static Action _onFinishAction;
        private static Parameters _parameters;

        public static void Start(Parameters parameters, Action OnFinish)
        {
            if (_running) return;

            _running = true;
            _parameters = parameters;
            _onFinishAction = OnFinish;
            new Thread(RunLooop).Start(_parameters);
        }

        public static void Stop() 
        {
            _running = false;
        }

        private static void RunLooop(object? p)
        {
            int clickCount = 0;
            Parameters @params = (Parameters)p!;
            while (_running)
            {
                InputSynthesizer.Click(@params.button, @params.target, @params.isStaticTarget);
                if(@params.isLimitedDuration)
                {
                    clickCount++;
                    _running = clickCount < @params.targetClickAmount;
                }
                if(@params.isRandomInterval)
                {
                    int millis = (int)(new Random().NextDouble() * (@params.randomRange.End - @params.randomRange.Start)) + @params.randomRange.Start;
                    Thread.Sleep(millis);
                } else
                {
                    Thread.Sleep(@params.sleepMillis);
                }
            }
            _onFinishAction();
        }
    }
}
