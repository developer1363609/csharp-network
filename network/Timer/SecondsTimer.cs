namespace network.Utils;

public class SecondsTimer : IDisposable
{
    public SecondsTimer() : this(1f) { }

    public SecondsTimer(float during = 1f)
    {
        mTotalTime = during;
        GlobalLogic.gGlobalTimer.AddUpdateHandler(OnUpdate);
    }

    bool mDisposed = false;
    /// <summary>
    /// 已经流逝的时间
    /// </summary>
    private float mElapsedTime = 0f;
    /// <summary>
    /// 每次回调的时间
    /// </summary>
    public float mTotalTime = 1f;
    /// <summary>
    /// 更新游戏逻辑
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    private bool OnUpdate(float t)
    {
        mElapsedTime += t;
        if (mElapsedTime >= mTotalTime)
        {
            mElapsedTime -= mTotalTime;
            mItems.Flush();
            for (int i = 0, n = mItems.Count; i < n; ++i)
            {
                var it = mItems[i];
                it();
            }
            mItems.Flush();
        }

        return false;
    }
    /// <summary>
    /// 清除资源
    /// </summary>
    public void Dispose()
    {
        if (!mDisposed)
        {
            mDisposed = true;
            GlobalLogic.gGlobalTimer.RemoveUpdateHandler(OnUpdate);
        }
    }
    /// <summary>
    /// 需要更新的原件
    /// </summary>
    private XArray<Action> mItems = new XArray<Action>();
    /// <summary>
    /// 设置计时器
    /// </summary>
    /// <param name="fn"></param>
    public void SetTimer(Action fn)
    {
        mItems.AddDelay(fn);
    }
    /// <summary>
    /// 移除计时器
    /// </summary>
    /// <param name="fn"></param>
    public void RemoveTimer(Action fn)
    {
        mItems.RemoveDelay(fn);
    }

}