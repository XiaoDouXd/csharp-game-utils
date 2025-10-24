using System.Collections.Concurrent;
using System.Collections.Generic;

namespace XD.GameModule.Module.MEvent
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public partial class EventModule
    {
        #region task list

        private const int MaxTaskListCache = 100;

        private List<EventHandlerBase> NewTaskList() =>
            _taskListPool.TryPop(out var v) ? v : new List<EventHandlerBase>();

        private void DelTaskList(List<EventHandlerBase> paramList)
        {
            paramList.Clear();
            if (_taskListPool.Count < MaxTaskListCache) _taskListPool.Push(paramList);
        }

        private readonly ConcurrentStack<List<EventHandlerBase>> _taskListPool = new();

        #endregion

        #region param list

        private const int MaxParamListCache = 100;

        private List<ParamPack> NewParamList() => _paramListPool.TryPop(out var v) ? v : new List<ParamPack>();

        private void DelParamList(List<ParamPack> paramList)
        {
            if (ReferenceEquals(paramList, _emptyParamList)) return;
            foreach (var param in paramList) param.Dispose(); // 释放参数包的数据
            paramList.Clear();
            if (_paramListPool.Count < MaxParamListCache) _paramListPool.Push(paramList);
        }

        private static ParamPack NewValuePack<T>(T? obj) => ParamPack.Create(obj);

        private readonly List<ParamPack> _emptyParamList = new();
        private readonly ConcurrentStack<List<ParamPack>> _paramListPool = new();

        #endregion
    }
}