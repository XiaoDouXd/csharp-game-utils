
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
    /// 串行程序流轮询执行器 (依靠 Unity update 轮询执行, 不开线程)
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class ProcedureSerialActuator_Upd : IProcedure, IList<IProcedure?>, IReadOnlyList<IProcedure?>, IUpdate
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
                    NextIndex = index,
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
            _curWaitingInfo = null;

            var lastIdxInner = -1;
            for (var i = _procedures.Count - 1; i >= 0; i--)
            {
                var info = _procedures[i];
                var item = info.Item;

                if (item == null) continue;
                item.OnEnd -= lastIdxInner < 0 ? DoEnd : DoNext;

                info.NextIndex = lastIdxInner;
                lastIdxInner = i;
            }

            _curRunningInfo?.Item?.Abort();
            _curRunningInfo = null;

            Running = false;
        }

        public IProcedure.RetInfo Init() => IProcedure.RetInfo.Success;

        public void OnUpdate(float dt, float rdt)
        {
            if (_curWaitingInfo == null) return;
            _curWaitingInfo?.Item?.Do();
            _curWaitingInfo = null;
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
            var nextIndex = -1;
            for (var i = _procedures.Count - 1; i >= 0; i--)
            {
                var info = _procedures[i];
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

                item.OnEnd += nextIndex < 0 ? DoEnd : DoNext;

                info.NextIndex = nextIndex;
                nextIndex = i;
            }

            if (nextIndex < 0)
            {
                DoEnd(this, IProcedure.EndType.Abort, new ArgumentException("no procedure can run"));
                return;
            }
            DoInner(nextIndex);
        }

        public void Add(IProcedure? procedure)
        {
            if (CheckLock()) return;
            if (Contains(procedure) && procedure != null)
            {
                Log.Log.Error("can't set same procedure into the actuator");
                return;
            }

            _procedures.Add(new ProcedureInfo
            {
                Item = procedure,
                NextIndex = _procedures.Count
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
                NextIndex = index
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

        private void DoNext(IProcedure _, IProcedure.EndType type, Exception? exception)
        {
            if (_curRunningInfo == null)
            {
                DoEnd(this, IProcedure.EndType.Abort, new ArgumentNullException(nameof(_curRunningInfo), "invalid procedure data"));
                return;
            }

            switch (type)
            {
                case IProcedure.EndType.Abort:
                    DoEnd(this, IProcedure.EndType.Abort, exception);
                    return;
                case IProcedure.EndType.Warning:
                {
                    if (exception != null) Log.Log.Warning(exception);
                    break;
                }
                case IProcedure.EndType.Success:
                default: break;
            }

            DoInner(_curRunningInfo.NextIndex);
        }

        private void DoInner(int idx)
        {
            var item = _procedures[idx];
            if (item.Item == null)
            {
                DoEnd(this, IProcedure.EndType.Abort, new ArgumentOutOfRangeException(nameof(idx), "invalid procedure data"));
                return;
            }
            _curWaitingInfo = _curRunningInfo = item;
        }

        private void DoEnd(IProcedure _, IProcedure.EndType type, Exception? exception)
        {
            // 卸载事件
            var lastIdxInner = -1;
            for (var i = _procedures.Count - 1; i >= 0; i--)
            {
                var info = _procedures[i];
                var item = info.Item;

                if (item == null) continue;
                item.OnEnd -= lastIdxInner < 0 ? DoEnd : DoNext;

                info.NextIndex = lastIdxInner;
                lastIdxInner = i;
            }

            _curRunningInfo = null;
            OnEnd?.Invoke(this, type, exception);
            Running = false;
        }

        private bool CheckLock(bool log = true)
        {
            if (!Running && _curRunningInfo == null && _curWaitingInfo == null)
                return false;
            if (log) Log.Log.Warning("try to modify locked procedure");
            return true;
        }

        #endregion

        private ProcedureInfo? _curWaitingInfo;
        private ProcedureInfo? _curRunningInfo;
        private readonly List<ProcedureInfo> _procedures = new();

        private class ProcedureInfo
        {
            public int NextIndex;
            public IProcedure? Item;
        }
    }
}