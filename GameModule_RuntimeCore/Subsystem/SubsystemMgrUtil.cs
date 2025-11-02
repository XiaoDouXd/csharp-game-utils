using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using XD.Common.Log;
using XD.Common.ScopeUtil;
using XD.GameModule.Module;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnassignedField.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace XD.GameModule.Subsystem
{
    public enum ESubsystemLifecycleEvent
    {
        OnSubsystemInitializationFinished,
        OnSubsystemAsyncInitializeFinished,
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SubsystemLazyAttribute : Attribute {}
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SubsystemAttribute : Attribute { public SubsystemAttribute(Type parent) {} public sbyte Layer; }
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SubsystemInstAttribute : Attribute { public sbyte Layer; }
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class SubsystemInstDisposeOnlyAttribute : Attribute {}

    public static class SubsystemMgrUtil
    {
        public static async void Init(Type sysGroup, CancellationTokenSource? ctx = null)
        {
            try
            {
                var subSystems = GetSubsystems(sysGroup, true);
                if (subSystems.Count <= 0) return;

                ctx ??= new CancellationTokenSource();
                var taskList = new List<Task>();
                foreach (var sysList in subSystems.Values)
                {
                    if (sysList is not { Count: > 0 }) continue;
                    foreach (var sys in sysList)
                    {
                        sys.OnInitializeBase();
                        taskList.Add(Task.Run(() => sys.OnAsyncInitializeBase(ctx)));
                    }
                }
                // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
                E.Event?.BroadcastFrameAsync(ESubsystemLifecycleEvent.OnSubsystemInitializationFinished, sysGroup);
                await Task.WhenAll(taskList);
                E.Event?.BroadcastFrameAsync(ESubsystemLifecycleEvent.OnSubsystemAsyncInitializeFinished, sysGroup);
            }
            catch (Exception e)
            {
                Log.Error($"failure to init subsystem group {sysGroup.Name}, exception: {e}");
            }
        }

        public static void DeInit(Type sysGroup)
        {
            try
            {
                var subSystems = GetSubsystems(sysGroup, false);
                if (subSystems.Count <= 0) return;

                foreach (var sysList in subSystems.Values)
                {
                    if (sysList is not { Count: > 0 }) continue;
                    foreach (var sys in sysList) sys.Dispose();
                }
            }
            catch (Exception e)
            {
                Log.Error($"failure to dispose subsystem group {sysGroup.Name}, exception: {e}");
            }
        }

        private static SortedDictionary<sbyte, List<SubsystemBase>> GetSubsystems(in Type sysContainer, bool initValue)
        {
            var ret = new SortedDictionary<sbyte, List<SubsystemBase>>();
            var staticFields = sysContainer.GetRuntimeFields();
            foreach (var field in staticFields)
            {
                var fieldType = field.FieldType;
                if (!fieldType.IsSubclassOf(typeof(SubsystemBase))) continue;

                var attr = field.GetCustomAttribute<SubsystemInstAttribute>();
                if (attr == null)
                {
                    if (initValue) continue;
                    var disposeAttr = field.GetCustomAttribute<SubsystemInstDisposeOnlyAttribute>();
                    if (disposeAttr == null) continue;
                }

                var defaultConstructor = fieldType.GetConstructor(Array.Empty<Type>());
                SubsystemBase? subSys = null;
                try
                {
                    if (!initValue)
                    {
                        if (field.GetValue(null) is SubsystemBase subsystem)
                        {
                            subSys = subsystem;
                            field.SetValue(null, null);
                        }
                        else continue;
                    }
                    else if (defaultConstructor != null)
                        field.SetValue(null, subSys = Activator.CreateInstance(fieldType) as SubsystemBase);
                    else Log.Error($"failed to create instance of {field.Name}, can't find valid constructor");
                }
                catch (Exception e)
                {
                    Log.Error($"failed to create instance of {field.Name}, exception: {e}");
                    continue;
                }

                if (subSys == null)
                {
                    Log.Error($"failed to create instance of {field.Name}");
                    continue;
                }

                var layer = attr?.Layer ?? 0;
                if (!ret.TryGetValue(layer, out var sysList))
                    ret[layer] = sysList = new List<SubsystemBase>();
                sysList.Add(subSys);
            }
            return ret;
        }
    }

    public abstract partial class SubsystemBase : XDObject
    {
        internal void OnInitializeBase()
        {
            OnInitialize();
        }

        internal void OnAsyncInitializeBase(CancellationTokenSource? ctx)
        {
            OnAsyncInitialize(ctx);
        }

        public override void Dispose()
        {
            if (IsDisposed) return;
            OnUninitialize();
            base.Dispose();
        }
    }
}