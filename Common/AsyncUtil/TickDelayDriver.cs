using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using XD.Common.ScopeUtil;
using XDLog = XD.Common.Log.Log;

namespace XD.Common.AsyncUtil
{
    /// <summary>
    /// 基于 Tick 的延时执行驱动.
    /// 线程安全:
    ///   - AddDelay / RemoveDelay 可在任意线程调用 (通过 pending 队列与主线程同步).
    ///   - OnTick 必须由主线程 (驱动线程) 调用.
    /// 时间源:
    ///   - 使用单调累计时间 (基于 Tick 的 dt 累加), 不受系统时间调整 / NTP 同步影响,
    ///     也能自然地跟随游戏时间缩放 / 暂停语义.
    /// </summary>
    public class TickDelayDriver : XDObject, ITick
    {
        /// <summary>
        /// 注册一个延时回调, 返回用于 <see cref="RemoveDelay"/> 的句柄.
        /// 可在任意线程调用; 实际挂入链表发生在下一次 OnTick.
        /// </summary>
        public object AddDelay(Action action, double delay)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            // 立即构建 node, 目标时刻基于当前累计时间 (volatile 读).
            var target = _accTime + Math.Max(0, delay);
            var node = new LinkedListNode<Task>(new Task(action, target));
            _pendingAdd.Enqueue(node);
            return node;
        }

        /// <summary>
        /// 取消一个延时回调. 可在任意线程调用.
        /// </summary>
        public bool RemoveDelay(object? handle)
        {
            if (handle is not LinkedListNode<Task> node) return false;
            // 只做标记 + 入队, 真正移除在主线程 OnTick 内完成, 避免跨线程破坏链表.
            node.Value.MarkCancelled();
            _pendingRemove.Enqueue(node);
            return true;
        }

        public void OnTick(float dt, float rdt)
        {
            // 单调累计时间推进.
            _accTime += Math.Max(0, dt);

            // 1. flush 新增节点
            while (_pendingAdd.TryDequeue(out var node))
            {
                // 若已经被取消, 跳过入链表.
                if (node.Value.IsCancelled) continue;
                _tasks.AddLast(node);
            }

            // 2. flush 取消节点
            while (_pendingRemove.TryDequeue(out var node))
            {
                if (node.List == _tasks) _tasks.Remove(node);
                // else: 可能尚未被 flush 到 _tasks (会在 #1 时因 IsCancelled 被跳过)
            }

            // 3. 执行到期任务
            var node2 = _tasks.First;
            var currTime = _accTime;
            while (node2 != null)
            {
                var next = node2.Next;
                var task = node2.Value;
                if (task.IsCancelled)
                {
                    _tasks.Remove(node2);
                }
                else if (task.TargetTime <= currTime)
                {
                    _tasks.Remove(node2);
                    try { task.Action.Invoke(); }
                    catch (Exception e) { XDLog.Error($"[TickDelayDriver] delay callback exception: {e}"); }
                }
                node2 = next;
            }
        }

        private sealed class Task
        {
            public readonly Action Action;
            public readonly double TargetTime;
            public bool IsCancelled => _cancelled != 0;

            public Task(Action act, double targetTime)
            {
                Action = act;
                TargetTime = targetTime;
                _cancelled = 0;
            }

            public void MarkCancelled() => System.Threading.Interlocked.Exchange(ref _cancelled, 1);

            private int _cancelled;
        }

        private double _accTime;
        private readonly LinkedList<Task> _tasks = new();
        private readonly ConcurrentQueue<LinkedListNode<Task>> _pendingAdd = new();
        private readonly ConcurrentQueue<LinkedListNode<Task>> _pendingRemove = new();
    }
}
