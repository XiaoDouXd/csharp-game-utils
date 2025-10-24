namespace XD.Common.ScopeUtil
{
    /// <summary> 可轮询对象 </summary>
    public interface IPolling {}

    public interface ITick : IPolling
    {
        /// <summary>
        /// 刷新间隙
        /// </summary>
        public float TickInterval => 0;

        /// <summary>
        /// 刷新一帧
        /// </summary>
        /// <param name="dt"> 游戏时间增量 </param>
        /// <param name="rdt"> 现实时间增量 </param>
        public void OnTick(float dt, float rdt);
    }

    public interface IPhysicalTicker : IPolling
    {
        /// <summary>
        /// 刷新间隙
        /// </summary>
        public float PhysicalTickInterval => 0;

        /// <summary>
        /// 在 FixedUpdate 里刷新一帧
        /// </summary>
        /// <param name="dt"> 游戏时间增量 </param>
        /// <param name="rdt"> 现实时间增量 </param>
        public void OnPhysicalTick(float dt, float rdt);
    }

    public interface ILateTicker : IPolling
    {
        /// <summary>
        /// 刷新间隙
        /// </summary>
        public float LateTickInterval => 0;

        /// <summary>
        /// 在 LateUpdate 里刷新一帧
        /// </summary>
        /// <param name="dt"> 游戏时间增量 </param>
        /// <param name="rdt"> 现实时间增量 </param>
        public void OnLateTick(float dt, float rdt);
    }

    public interface IInit
    {
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized => false;

        /// <summary>
        /// 初始化时调用
        /// </summary>
        public void OnInit();

        /// <summary>
        /// 反初始化时调用, 和 dispose 的语义不同, 这里保留重新初始化的可能性
        /// </summary>
        public void OnDeinit();
    }
}