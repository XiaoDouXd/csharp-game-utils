using System;
using XD.Common.Procedure;

namespace XD.GameModule.Module.MEvent
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public partial class EventModule : EngineModule
    {
        #region lifecycle
        internal override IProcedure InitProcedure() => new ProcedureSync(Init);

        internal override IProcedure ReinitProcedure() => new ProcedureSync(Init);

        internal override IProcedure DeInitProcedure() => new ProcedureSync(DeInit);

        private IProcedure.RetInfo Init()
        {
            _eventMap.Clear();
            _listenerMap.Clear();

            if (E.Upd == null) return (IProcedure.EndType.Abort, new ArgumentNullException(nameof(E.Upd)));
            E.Upd.OnUpdateDirect += OnUpdate;
            return IProcedure.RetInfo.Success;
        }

        private IProcedure.RetInfo DeInit()
        {
            if (E.Upd != null) E.Upd.OnUpdateDirect -= OnUpdate;
            return IProcedure.RetInfo.Success;
        }

        #endregion
    }
}