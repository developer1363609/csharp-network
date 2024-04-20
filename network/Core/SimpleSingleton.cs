namespace network.Core;

public class SimpleSingleton<T> : ISingleton where T : class ,new()
{
    /// <summary>
    /// 阻止外部直接实例化
    /// </summary>
    protected SimpleSingleton()
    {
    }
    
    /// <summary>
    /// 静态实例的引用
    /// </summary>
    protected static T instance;
    
    /// <summary>
    /// 获取静态对象实例
    /// </summary>
    public static T Instance
    {
        get
        {
            if (null == instance)
                instance = new T();
            return instance;
        }//get
    }//Instance
    /// <summary>
    /// 获取静态实例，如果为null不创建新的实例
    /// </summary>
    /// <returns></returns>
    public static T TryGetInstance()
    {
        return instance;
    }

    public bool Suspended
    {
        get
        {
            return mSuspended;
        }
    }
    /// <summary>
    /// 标记是否阻止清理
    /// </summary>
    private bool mSuspended;

    public static bool HasInstance()
    {
        return instance != null;
    }//HasInstance

    public void CleanInstance()
    {
        instance = null;
    }//CleanInstance

    /// <summary>
    /// 阻止被粗暴地清理静态单例
    /// </summary>
    /// <param name="b"></param>
    public void SuspendThis(bool b)
    {
        mSuspended = b;
    }
}