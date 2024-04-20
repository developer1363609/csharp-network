namespace network.Utils;

public interface IPool
{
    
    object GetX();
    void PutX(object item);

    void Clear();
    ///// <summary>
    ///// 自动清零
    ///// </summary>
    //void AutoCleanup();
    /// <summary>
    /// 在AutoCleanup的时候是否自动被清理
    /// </summary>
    bool RemoveWhenAutoCleanup { get; }
    /// <summary>
    /// 设置在被清理的时候自动Cleanup
    /// </summary>
    IPool SetRemoveWhenAutoCleanup();
    /// <summary>
    /// 
    /// </summary>
    void MakeReserveItems(int count);
}