using System;
using System.Collections.Generic;

namespace GoogleTranslate.Desktop
{
    public delegate void EndingInputHandler(string inputText);
    public class InputQueue : IDisposable
    {
        private readonly Stack<string> _stack = new Stack<string>();

        private System.Timers.Timer Timer { get; }

        public event EndingInputHandler OnStopInput;

        public InputQueue()
        {
            Timer = new System.Timers.Timer(600);
            Timer.Elapsed += (sender, args) =>
            {
                if (_stack.Count <= 0) return;
                var input = _stack.Pop();
                _stack.Clear();
                OnStopInput?.Invoke(input);
            };
            Timer.Start();
        }

        public void In(string input)
        {
            _stack.Push(input);
        }

        public void Dispose()
        {
            Timer.Stop();
            Timer.Dispose();
        }
    }
}
