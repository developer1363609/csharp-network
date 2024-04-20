using network.Event;

namespace network.Core;

public class EventArgumentsF
{
    /// <summary>
    /// 派发事件的时候传递的参数
    /// </summary>
    public readonly object param;
    /// <summary>
    /// 事件的派发器
    /// </summary>
    public readonly FastNotifier sender;
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
    internal EventArgumentsF(object param_,
        FastNotifier sender_,
        string type_)
    {
        param = param_;
        sender = sender_;
        type = type_;
    }
    public static readonly EventArgumentsF Default
        = new EventArgumentsF(null, null, null);
}