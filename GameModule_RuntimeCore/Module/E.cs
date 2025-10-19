using XD.GameModule.Module.MAsset;
using XD.GameModule.Module.MConfig;
using XD.GameModule.Module.MEvent;
using XD.GameModule.Module.MUpdate;

// ReSharper disable UnusedMember.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable UnusedAutoPropertyAccessor.Local

namespace XD.GameModule.Module
{
    /// <summary> Engine </summary>
    public static partial class E
    {
        /// <summary>
        /// 轮询模块
        /// </summary>
        [EngineModule(1)]
        public static UpdateModule? Upd { get; private set; }

        /// <summary>
        /// 事件模块
        /// </summary>
        [EngineModule(2)]
        public static EventModule? Event { get; private set; }

        /// <summary>
        /// 资源模块
        /// </summary>
        [EngineModule(2)]
        public static AssetModule? Asset { get; private set; }

        /// <summary>
        /// 配置模块
        /// </summary>
        [EngineModule(3)]
        public static ConfigModule? Cfg { get; private set; }
    }
}