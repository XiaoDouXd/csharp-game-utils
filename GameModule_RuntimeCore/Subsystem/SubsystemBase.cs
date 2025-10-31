using System.Threading;

// ReSharper disable UnusedParameter.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable VirtualMemberNeverOverridden.Global

namespace XD.GameModule.Subsystem
{
    public partial class SubsystemBase
    {
        public virtual void OnInitialize() {}
        public virtual void OnAsyncInitialize(CancellationTokenSource? cts) {}
        public virtual void OnUninitialize() {}
    }
}