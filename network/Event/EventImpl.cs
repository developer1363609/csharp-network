namespace network.Event;

public class EventImpl
{
    public event Notifier.Eventhandler mDispatcher;

    /// <summary>
    /// 派发事件
    /// </summary>
    /// <param name="data"></param>
    /// <param name="sender"></param>
    /// <param name="type"></param>
    public void Execute(object data, Notifier sender, string type)
    {
        if (null != mDispatcher)
        {
            var ea = new EventArguments(data, sender, type);
            mDispatcher.Invoke(ref ea);
        }
    }
}