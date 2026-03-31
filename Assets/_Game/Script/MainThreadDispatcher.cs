namespace Luzart
{
    using System;
    using System.Collections.Generic;

    public class MainThreadDispatcher : Singleton<MainThreadDispatcher>
    {
        private readonly Queue<Action> _queue = new Queue<Action>();

        public void Enqueue(Action action)
        {
            lock (_queue) { _queue.Enqueue(action); }
        }

        private void Update()
        {
            lock (_queue)
            {
                while (_queue.Count > 0)
                    _queue.Dequeue().Invoke();
            }
        }
    }
}
