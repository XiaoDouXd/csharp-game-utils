using System;
using System.Collections.Generic;
using System.Reflection;
using XD.Common.Log;
using XD.Common.Procedure;
using XD.GameModule.Module.MUpdate;

// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace XD.GameModule.Module
{
    /// <summary>
    /// 引擎模块容器
    /// </summary>
    public static partial class E
    {
        public enum State : byte
        {
            // ReSharper disable IdentifierTypo
            Null = 0,
            Inited,

            Initing,
            Reiniting,
            Deiniting
            // ReSharper restore IdentifierTypo
        }

        #region Tick
        public static void OnTick(float dt, float rdt) => Tick?.Invoke(dt, rdt);
        public static void OnLateTick(float dt, float rdt) => LateTick?.Invoke(dt, rdt);
        public static void OnPhysicalTick(float dt, float rdt) => PhysicalTick?.Invoke(dt, rdt);

        internal static event Action<float, float>? Tick;
        internal static event Action<float, float>? LateTick;
        internal static event Action<float, float>? PhysicalTick;
        #endregion

        public static float InitProcess => _initProcedure?.Process ?? 0f;
        public static float ReinitProcess => _reinitProcedure?.Process ?? 0f;

        internal static IProcedure InitProcedure
        {
            get
            {
                if (Modules.Count <= 0) return EmptyProcedure;
                if (_initProcedure != null) return _initProcedure;

                var moduleProcedure = new ProcedureSerialActuator_Upd();
                OnUpdateInit += moduleProcedure.OnUpdate;
                foreach (var layer in Modules.Values)
                {
                    if (layer.Count <= 0) continue;
                    var layerProcedure = new ProcedureParallelActuator_Upd();
                    foreach (var module in layer) layerProcedure.Add(module.InitProcedure());
                    moduleProcedure.Add(layerProcedure);
                    OnUpdateInit += layerProcedure.OnUpdate;
                }
                return _initProcedure = moduleProcedure;
            }
        }

        internal static IProcedure ReinitProcedure
        {
            get
            {
                if (Modules.Count <= 0) return EmptyProcedure;
                if (_reinitProcedure != null) return _reinitProcedure;

                var moduleProcedure = new ProcedureSerialActuator_Upd();
                OnUpdateReinit += moduleProcedure.OnUpdate;
                foreach (var layer in Modules.Values)
                {
                    if (layer.Count <= 0) continue;
                    var layerProcedure = new ProcedureParallelActuator_Upd();
                    foreach (var module in layer) layerProcedure.Add(module.ReinitProcedure());
                    moduleProcedure.Add(layerProcedure);
                    OnUpdateReinit += layerProcedure.OnUpdate;
                }
                return _reinitProcedure = moduleProcedure;
            }
        }

        public static void Init()
        {
            if (_state != State.Null)
            {
                if (_state == State.Inited)
                {
                    Log.Warning("engine module is already inited, try to re-initialize engine module");
                    Reinit(true);
                    return;
                }
                Log.Error($"can't init engine module which is {_state}");
                return;
            }

            var ee = typeof(E);
            var staticFields = ee.GetFields();
            foreach (var field in staticFields)
            {
                var fieldType = field.FieldType;
                if (!fieldType.IsSubclassOf(typeof(EngineModule))) continue;

                var attr = field.GetCustomAttribute<EngineModuleAttribute>();
                if (attr == null) continue;

                var defaultConstructor = fieldType.GetConstructor(Array.Empty<Type>());

                EngineModule? engineModule = null;
                try
                {
                    if (defaultConstructor != null)
                        field.SetValue(null, engineModule = Activator.CreateInstance(fieldType) as EngineModule);
                    else Log.Error($"failure to create instance of {field.Name}, can't find valid constructor");
                }
                catch (Exception e)
                {
                    Log.Error($"failure to create instance of {field.Name}, exception: {e}");
                    continue;
                }

                if (engineModule == null)
                {
                    Log.Error($"failure to create instance of {field.Name}");
                    continue;
                }

                if (!Modules.TryGetValue(attr.Layer, out var modules))
                    Modules[attr.Layer] = modules = new List<EngineModule>();
                modules.Add(engineModule);
            }

            var properties = ee.GetProperties();
            foreach (var field in properties)
            {
                var fieldType = field.PropertyType;
                if (!fieldType.IsSubclassOf(typeof(EngineModule))) continue;

                var attr = field.GetCustomAttribute<EngineModuleAttribute>();
                if (attr == null) continue;

                var defaultConstructor = fieldType.GetConstructor(Array.Empty<Type>());

                EngineModule? engineModule = null;
                try
                {
                    if (defaultConstructor != null)
                        field.SetValue(null, engineModule = Activator.CreateInstance(fieldType) as EngineModule);
                    else Log.Error($"failure to create instance of {field.Name}, can't find valid constructor");
                }
                catch (Exception e)
                {
                    Log.Error($"failure to create instance of {field.Name}, exception: {e}");
                    continue;
                }

                if (engineModule == null)
                {
                    Log.Error($"failure to create instance of {field.Name}");
                    continue;
                }

                if (!Modules.TryGetValue(attr.Layer, out var modules))
                    Modules[attr.Layer] = modules = new List<EngineModule>();
                modules.Add(engineModule);
            }

            if (Modules.Count <= 0)
            {
                _state = State.Inited;
                OnInited?.Invoke();
                return;
            }

            _state = State.Initing;
            var procedure = InitProcedure;

            Tick += OnUpdateInitCb;
            procedure.OnEnd += OnEnd;
            procedure.Do();
            return;

            void OnEnd(IProcedure pcd, IProcedure.EndType endType, Exception? exception)
            {
                procedure.OnEnd -= OnEnd;
                Tick -= OnUpdateInitCb;
                _state = State.Inited;
                OnInited?.Invoke();
            }
        }

        public static void Reinit(bool isCallOnInited = false)
        {
            if (_state != State.Inited) return;
            _state = State.Reiniting;
            var deinitProcedure = ReinitProcedure;
            Tick += OnUpdateReInitCb;
            deinitProcedure.OnEnd += OnEnd;
            deinitProcedure.Do();
            return;

            void OnEnd(IProcedure pcd, IProcedure.EndType endType, Exception? exception)
            {
                deinitProcedure.OnEnd -= OnEnd;
                _state = State.Inited;

                Tick -= OnUpdateReInitCb;
                OnReinited?.Invoke();
                if (isCallOnInited) OnInited?.Invoke();
            }
        }

        public static void Deinit()
        {
            if (_state != State.Inited) return;
            _state = State.Deiniting;

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var layer in Modules.Values)
            {
                if (layer.Count <= 0) continue;
                foreach (var module in layer) module.DeInitProcedure().Do();
            }

            Modules.Clear();

            _initProcedure = null;
            _reinitProcedure = null;

            OnUpdateInit = null;
            OnUpdateReinit = null;

            _state = State.Null;
            var staticFields = typeof(E).GetFields(BindingFlags.Static);
            foreach (var field in staticFields)
            {
                var fieldType = field.FieldType;
                if (!fieldType.IsSubclassOf(typeof(EngineModule))) continue;

                var attr = field.GetCustomAttribute<EngineModuleAttribute>();
                if (attr == null) continue;
                field.SetValue(null, null);
            }
            Log.Info("deinit engine module success");
        }

        private static void OnUpdateInitCb(float dt, float rdt) => OnUpdateInit?.Invoke(dt, rdt);
        private static void OnUpdateReInitCb(float dt, float rdt) => OnUpdateReinit?.Invoke(dt, rdt);

        // ReSharper disable IdentifierTypo
        public static event Action? OnInited;
        public static event Action? OnReinited;
        // ReSharper restore IdentifierTypo

        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
        private class EngineModuleAttribute : Attribute
        {
            public sbyte Layer { get; }
            public EngineModuleAttribute(sbyte layer) => Layer = layer;
        }

        private static State _state;
        private static ProcedureSerialActuator_Upd? _initProcedure;
        private static ProcedureSerialActuator_Upd? _reinitProcedure;

        private static event UpdateModule.UpdFunc? OnUpdateInit;
        private static event UpdateModule.UpdFunc? OnUpdateReinit;

        public static readonly ProcedureSync EmptyProcedure = new();
        private static readonly SortedDictionary<sbyte, List<EngineModule>> Modules = new();
    }

    /// <summary>
    /// 引擎模块
    /// </summary>
    public abstract class EngineModule
    {
        /// <summary>
        /// 初始化程序流
        /// </summary>
        /// <returns></returns>
        internal abstract IProcedure InitProcedure();

        /// <summary>
        /// 重初始化程序流
        /// </summary>
        /// <returns></returns>
        internal abstract IProcedure ReinitProcedure();

        /// <summary>
        /// 反初始化程序流
        /// </summary>
        /// <returns></returns>
        internal abstract IProcedure DeInitProcedure();
    }
}