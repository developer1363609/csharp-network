namespace network.Event;

public class EventArguments
{
    /// <summary>
    /// 派发事件的时候传递的参数
    /// </summary>
    public readonly object param;
    /// <summary>
    /// 事件的派发器
    /// </summary>
    public readonly Notifier sender;
    /// <summary>
    /// 事件的类型
    /// </summary>
    public readonly string type;
    /// <summary>
    /// 构造方法
    /// </summary>
    /// <param name="param_"></param>
    /// <param name="sender_"></param>
    /// <param name="type_"></param>
    internal EventArguments(object param_,
        Notifier sender_,
        string type_)
    {
        param = param_;
        sender = sender_;
        type = type_;
    }
    /// <summary>
    /// 移除事件监听
    /// </summary>
    /// <param name="handler"></param>
    public void Remove(Notifier.Eventhandler handler)
    {
        sender.Remove(type, handler);
    }

    public static readonly EventArguments Default = new EventArguments(null, null, null);
}