using XD.Common.Event;

namespace XD.GameModule.Module.MEvent
{
    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable once ClassNeverInstantiated.Global
    public partial class EventModule
    {
        public EventDispatcherAsync I { get; } = new();

        private void OnTick(float _, float __) => I.OnTick(_, __);
    }
}