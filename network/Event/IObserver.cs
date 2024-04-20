using network.Core;

namespace network.Event;

public interface IObserver
{
    /// <summary>
    /// 派发事件后会收到这个回调，可以通过e.type来区分事件类型
    /// </summary>
    /// <param name="e"></param>
    void OnMessage(ref EventArgumentsF e);
}