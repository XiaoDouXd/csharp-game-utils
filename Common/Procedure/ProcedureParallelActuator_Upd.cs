#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using XD.Common.ScopeUtil;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace XD.Common.Procedure
{
    /// <summary>
    /// 并行程序流轮询执行器 (依靠 Unity update 轮询执行每一个程序流)
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class ProcedureParallelActuator_Upd : IProcedure, IList<IProcedure?>, IReadOnlyList<IProcedure?>, IUpdate
    {
        public IProcedure? this[int index]
        {
            get => _procedures[index]?.Item;
            set
            {
                if (CheckLock()) return;
                if (Contains(value) && value != null)
                {
                    Log.Log.Error("can't set same procedure into the actuator");
                    return;
                }

                _procedures[index] = new ProcedureInfo
                {
                    IsRunning = false,
                    Item = value
                };
            }
        }

        public bool IsReadOnly => false;
        public int Count => _procedures.Count;

        public float Process
        {
            get
            {
                var i = 0;
                var process = 0f;
                // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
                foreach (var info in _procedures)
                {
                    if (info.Item == null) continue;
                    i++;
                    process += Math.Clamp(info.Item.Process, 0f, 1f);
                }
                return i <= 0 ? 0 : process / i;
            }
        }

        public bool Running { get; private set; }

        public event Action<IProcedure, IProcedure.EndType, Exception?>? OnEnd;

        public void Abort()
        {
            if (!CheckLock(false)) return;
            if (_procedures.Count <= 0) return;

            for (var i = _procedures.Count - 1; i >= 0; i--)
            {
                var info = _procedures[i];
                var item = info.Item;

                if (item == null) continue;
                item.OnEnd -= DoEnd;
                if (info.IsRunning) item.Abort();
                info.IsRunning = false;
            }

            _curCompletedCnt = _curRunningCnt = 0;
            Running = false;
        }

        public IProcedure.RetInfo Init() => IProcedure.RetInfo.Success;

        public void OnUpdate(float dt, float rdt)
        {
            if (!Running) return;
            if (_curRunningCnt <= 0 || _curCompletedCnt < _curRunningCnt) return;
            OnEnd?.Invoke(this, IProcedure.EndType.Success, null);
            _curCompletedCnt = _curRunningCnt = 0;
            Running = false;
        }

        public void Do()
        {
            lock (this)
            {
                if (CheckLock()) return;
                Running = true;
            }
            if (_procedures.Count <= 0)
            {
                OnEnd?.Invoke(this, IProcedure.EndType.Success, null);
                return;
            }

            // 装载事件
            var runCnt = _curCompletedCnt = _curRunningCnt = 0;
            for (var i = _procedures.Count - 1; i >= 0; i--)
            {
                var info = _procedures[i];
                info.IsRunning = false;
                var item = info.Item;

                if (item == null) continue;

                var initInfo = item.Init();
                switch (initInfo.EndType)
                {
                    case IProcedure.EndType.Abort:
                        DoEnd(this, IProcedure.EndType.Abort, initInfo.Exception);
                        return;
                    case IProcedure.EndType.Warning:
                        if (initInfo.Exception != null) Log.Log.Warning(initInfo.Exception);
                        break;
                    case IProcedure.EndType.Success:
                    default: break;
                }

                runCnt++;
                info.IsRunning = true;
                item.OnEnd += DoEnd;
            }

            if (runCnt <= 0)
            {
                DoEnd(this, IProcedure.EndType.Abort, new ArgumentException("no procedure can run"));
                return;
            }

            _curCompletedCnt = 0;
            _curRunningCnt = runCnt;
            // ReSharper disable once ForCanBeConvertedToForeach
            for (var i = 0; i < _procedures.Count; i++)
            {
                var info = _procedures[i];
                var item = info.Item;
                if (item == null || !info.IsRunning) continue;
                item.Do();
            }
        }

        public void Add(IProcedure? item) => Add(item, string.Empty);
        public void Add(IProcedure? procedure, string name)
        {
            if (CheckLock()) return;
            if (Contains(procedure) && procedure != null) return;
            _procedures.Add(new ProcedureInfo
            {
                Item = procedure,
                IsRunning = false
            });
        }

        public void Clear()
        {
            if (CheckLock()) return;
            _procedures.Clear();
        }

        public bool Contains(IProcedure? item) => IndexOf(item) >= 0;

        public void CopyTo(IProcedure?[] array, int arrayIndex)
        {
            for (int i = 0, j = arrayIndex; i < _procedures.Count && j < array.Length; i++, j++)
                array[j] = _procedures[i].Item;
        }

        public bool Remove(IProcedure? item)
        {
            if (CheckLock()) return false;
            var idx = IndexOf(item);
            if (idx < 0) return false;
            RemoveAt(idx);
            return true;
        }

        public int IndexOf(IProcedure? item)
        {
            for (var i = 0; i < _procedures.Count; i++)
            {
                var p = _procedures[i].Item;
                if (p == null && item == null) return i;
                if (p == null || item == null) continue;
                if (ReferenceEquals(item, p)) return i;
            }
            return -1;
        }

        public void Insert(int index, IProcedure? item)
        {
            if (CheckLock()) return;
            if (Contains(item) && item != null)
            {
                Log.Log.Error("can't set same procedure into the actuator");
                return;
            }

            _procedures.Insert(index, new ProcedureInfo
            {
                Item = item,
                IsRunning = false
            });
        }

        public void RemoveAt(int index)
        {
            if (CheckLock()) return;
            _procedures.RemoveAt(index);
        }

        public IEnumerator<IProcedure?> GetEnumerator()
        {
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var info in _procedures)
                yield return info.Item;
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #region Do Procedures

        private void DoEnd(IProcedure? procedure, IProcedure.EndType type, Exception? exception)
        {
            lock (_procedures)
            {
                if (ReferenceEquals(this, procedure))
                {
                    for (var i = _procedures.Count - 1; i >= 0; i--)
                    {
                        var info = _procedures[i];
                        var item = info.Item;

                        if (item == null) continue;
                        item.OnEnd -= DoEnd;
                        if (info.IsRunning) item.Abort();
                        info.IsRunning = false;
                    }

                    _curCompletedCnt = _curRunningCnt = 0;
                    Running = false;
                }
                else
                {
                    var idx = IndexOf(procedure);
                    if (idx < 0) return;

                    var info = _procedures[idx];
                    if (info.Item == null) return;

                    info.Item.OnEnd -= OnEnd;
                    _curCompletedCnt++;
                }
            }
        }

        private bool CheckLock(bool log = true)
        {
            if (!Running)
                return false;
            if (log) Log.Log.Warning("try to modify locked procedure");
            return true;
        }

        #endregion

        private int _curRunningCnt;
        private int _curCompletedCnt;
        private readonly List<ProcedureInfo> _procedures = new();

        private class ProcedureInfo
        {
            public bool IsRunning;
            public IProcedure? Item;
        }
    }
}