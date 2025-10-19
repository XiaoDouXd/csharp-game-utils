#nullable enable

using System;
using System.Collections.Generic;
using XD.Common.ScopeUtil;

namespace XD.Common.AsyncUtil
{
    public class UpdateDelayDriver : XDObject, IUpdate
    {
        public object AddDelay(Action action, double delay)
            => _tasks.AddLast(new Task(action, delay + (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 0.001)));
        public bool RemoveDelay(object? handle)
        {
            if (handle is LinkedListNode<Task> node) _tasks.Remove(node);
            else return false;
            return true;
        }

        public void OnUpdate(float dt, float rdt)
        {
            var currTime = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 0.001);

            var node = _tasks.First;
            while (node != null)
            {
                var next = node.Next;
                if (node.Value.TargetTime < currTime)
                {
                    node.Value.Action.Invoke();
                    _tasks.Remove(node);
                }
                node = next;
            }
        }

        private readonly struct Task
        {
            public readonly Action Action;
            public readonly double TargetTime;

            public Task(Action act, double targetTime)
            {
                Action = act;
                TargetTime = targetTime;
            }
        }
        private readonly LinkedList<Task> _tasks = new();
    }
}