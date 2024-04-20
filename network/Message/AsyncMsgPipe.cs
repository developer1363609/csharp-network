using Google.Protobuf.WellKnownTypes;
using network.Utils;

namespace network.Message;

//异步消息管道
public class AsyncMsgPipe
{
    public AsyncMsgPipe()
    {
        GlobalLogic.gLateTimer.AddUpdateHandler(Update);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="msg"></param>
    public delegate void Handler(ref PipeMsg msg);
    /// <summary>
    /// 异步消息管道
    /// </summary>
    readonly Dictionary<string, XArray<Handler>> mMap = new Dictionary<string, XArray<Handler>>();
    /// <summary>
    /// 消息队列
    /// </summary>
    private readonly Queue<PipeMsg> mMsgQueue = new Queue<PipeMsg>();
    /// <summary>
    /// 派发消息
    /// </summary>
    /// <param name="type"></param>
    /// <param name="data"></param>
    public void DispatchCmd(string type,object data = null)
    {
        lock(this){
            mMsgQueue.Enqueue(new PipeMsg(data, this, type));
        }
    }
    /// <summary>
    /// 派发消息，避免装箱
    /// </summary>
    /// <param name="type"></param>
    /// <param name="data"></param>
    public void DispatchCmdInt(string type, int data)
    {
        lock(this)
        {
            var iv = intPool.Get();
            iv.Value = data;
            DispatchCmd(type, iv);
        }
    }

    private readonly XPool<IntValue> intPool = new XPool<IntValue>();
    private readonly XPool<FloatValue> floatPool = new XPool<FloatValue>();

    /// <summary>
    /// 派发消息，避免装箱
    /// </summary>
    /// <param name="type"></param>
    /// <param name="data"></param>
    public void DispatchCmdFloat(string type,float data)
    {
        lock(this)
        {
            var iv = floatPool.Get();
            iv.Value = data;
            DispatchCmd(type, iv);
        }
    }
    /// <summary>
    /// 注册事件监听函数
    /// </summary>
    /// <param name="type"></param>
    /// <param name="fn"></param>
    public void Regist(string type, Handler fn)
    {
        XArray<Handler> vec = null;
        if(!mMap.TryGetValue(type,out vec))
        {
            vec = new XArray<Handler>();
            mMap[type] = vec;
        }
        vec.AddDelay(fn);
    }
    /// <summary>
    /// 移除事件监听函数
    /// </summary>
    /// <param name="type"></param>
    /// <param name="fn"></param>
    public void Remove(string type, Handler fn)
    {
        if(mMap.TryGetValue(type,out var vec))
        {
            vec.RemoveDelay(fn);
        }
    }
    /// <summary>
    /// 派发消息
    /// </summary>
    /// <param name="e"></param>
    private void Invoke(ref PipeMsg e)
    {
        if (!mMap.TryGetValue(e.type, out var vec)) return;
        vec.Flush();
        for(int i=0,n=vec.Count;i<n;++i)
        {
            var fn = vec[i];
            fn(ref e);
        }
        vec.Flush();
        switch (e.param)
        {
            case null:
                return;
            case IntValue value:
                intPool.Put(value);
                break;
            case FloatValue value:
                floatPool.Put(value);
                break;
        }
    }
    /// <summary>
    /// 刷新消息队列，被Unity的MonoBehaviour驱动更新
    /// </summary>
    private bool Update(float t)
    {
        lock(this)
        {
            while (mMsgQueue.Count > 0)
            {
                var e = mMsgQueue.Dequeue();
                Invoke(ref e);
            }
        }
        return false;
    }
}