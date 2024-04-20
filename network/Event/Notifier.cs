using Google.Protobuf.WellKnownTypes;
using network.Data_Object;
using network.Utils;

namespace network.Event;

public class Notifier
{
    public delegate void Eventhandler(ref EventArguments arguments);
    
    private Dictionary<string, EventImpl> mMap = new Dictionary<string, EventImpl>();
    public void DispatchCmd(string type, object data = default(object))
    {
        try
        {
#if DEBUG
            if(data!=null)
            {
                var t = data.GetType();
                if(t == typeof(int) || t == typeof(float))
                {
                    Console.Error.WriteLine("不允许在参数使用int或者float");
                }
            }
#endif
            if (mMap.ContainsKey(type))
            {
                var impl = mMap[type];
                impl.Execute(data, this, type);
            }
        }
        catch (System.Exception e)
        {
#if DEBUG
            Console.Error.WriteLine(e.ToString());
#endif
        }
    }
    
    public void Remove(string type, Eventhandler handler)
    {
        if (mMap.ContainsKey(type))
        {
            var impl = mMap[type];
            impl.mDispatcher -= handler;
        }
    }
   
    public void DispatchCmdInt(string type,int data)
    {
        var i = mIntPool.Get();
        i.Value = data;
        DispatchCmd(type, i);
        mIntPool.Put(i);
    }

    public void Clear()
    {
        mMap.Clear();
    }
    public void DispatchCmdLong(string type, long data)
    {
        var i = mLongPool.Get();
        i.Value = data;
        DispatchCmd(type, i);
        mLongPool.Put(i);
    }

    public void DispatchCmdFloat(string type,float data)
    {
        var f = mFloatPool.Get();
        f.Value = data;
        DispatchCmd(type, f);
        mFloatPool.Put(f);
    }

    private XPool<IntValue> mIntPool = new XPool<IntValue>();

    private XPool<FloatValue> mFloatPool = new XPool<FloatValue>();

    private XPool<LongValue> mLongPool = new XPool<LongValue>();

    public void DispatchAndRemove(string type, object data = default(object))
    {
        DispatchCmd(type, data);
        RemoveAll(type);
    }
    
    private void RemoveAll(string type)
    {
        if (mMap.ContainsKey(type))
        {
            mMap.Remove(type);
        }
    }
    
    
    public void DispatchAndRemove(string type, int data)
    {
        var i = mIntPool.Get();
        i.Value = data;
        DispatchAndRemove(type, i);
        mIntPool.Put(i);
    }
   
    public void DispatchAndRemove(string type, float data)
    {
        var f = mFloatPool.Get();
        f.Value = data;
        DispatchAndRemove(type, f);
        mFloatPool.Put(f);
    }
}