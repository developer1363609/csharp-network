using network.Core;
using network.IO;
using ProtoBuf;

namespace network.Utils;

using Fn1 = Action<StreamTool>;
using Fn2 = Action<ProtoBuf.IExtensible>;

public class BridgeZone : SimpleSingleton<BridgeZone> ,IBridge
{
    /// <summary>
    /// 协议号对应的处理函数的映射表，针对指令的
    /// </summary>
    public Dictionary<uint, XFnList<Fn1>> mMap
        = new Dictionary<uint, XFnList<Fn1>>(128);
    /// <summary>
    /// 协议号对应的处理函数的映射表，针对PB的
    /// </summary>
    public Dictionary<uint, XFnList<Fn2>> mMap2
        = new Dictionary<uint, XFnList<Fn2>>(128);
    /// <summary>
    /// 测试接口，模拟服务器回包
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="rsp"></param>
    public void Test(uint cmd, IExtensible rsp)
    {
        XFnList<Fn2> vec = null;
        if(mMap2.TryGetValue(cmd,out vec))
        {
            //vec.Flush();
            //for(int i=0,n=vec.Count;i<n;++i)
            //{
            //    var it = vec[i];
            //    if(null == it) { continue; }
            //    it(rsp);
            //}
            mIter2.rsp = rsp;
            vec.ForEach(mIter2);
        }
    }

    public void TestPayAPPLY_REQ(uint cmd, IExtensible rsp)
    {
        XFnList<Fn2> vec = null;
        if (mMap2.TryGetValue(cmd, out vec))
        {
            mIter2.rsp = rsp;
            vec.ForEach(mIter2);
        }
    }
    private Fn2Iterator mIter2 = new Fn2Iterator();
    private Fn1Iterator mIter1 = new Fn1Iterator();

    public class Fn2Iterator : XFnList<Fn2>.IIterateHandler
    {
        public IExtensible rsp;
        public void OnIterate(Fn2 fn)
        {
            fn(rsp);
        }
    }
    public class Fn1Iterator : XFnList<Fn1>.IIterateHandler
    {
        public StreamTool st;
        public void OnIterate(Fn1 fn)
        {
            fn(st);
        }
    }


    public BridgeZone()
    {
        SuspendThis(true);
    }
    //private int looping = -1;
    /// <summary>
    /// 注册网络消息，针对网络指令的
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="fn"></param>
    public void Regist(uint cmd,Fn1 fn)
    {
        //if (looping == cmd)
        //{
        //    Logger.LogError("Error:" + cmd);
        //}
        XFnList<Fn1> vec;
        if(mMap.TryGetValue(cmd,out vec))
        {
            vec.Add(fn);
        }
        else
        {
            vec = new XFnList<Fn1>();
            vec.Add(fn);
            mMap.Add(cmd, vec);
        }
    }
    /// <summary>
    /// 注册回调方法,针对protobuffer的
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="fn"></param>
    public void Regist(uint cmd,Fn2 fn)
    {
        //if (looping == cmd)
        //{
        //    Logger.LogError("Error:" + cmd);
        //}
        XFnList<Fn2> vec;
        if (mMap2.TryGetValue(cmd, out vec))
        {
            vec.Add(fn);
        }
        else
        {
            vec = new XFnList<Fn2>();
            vec.Add(fn);
            mMap2.Add(cmd, vec);
        }
    }
    /// <summary>
    /// 执行指令
    /// </summary>
    /// <param name="msgLen"></param>
    /// <param name="data"></param>
    public void Invoke(uint cmd, StreamTool stream)
	{
		long dataSize = stream.Buffer.Length;

		InvokeCallback (cmd);
        XFnList<Fn1> vec1 = null;
        XFnList<Fn2> vec2 = null;

		if (mMap2.TryGetValue (cmd, out vec2)) 
		{

            var data = PBReflectorZone.MakeInstance (cmd, stream.Buffer);

            if (null != data) 
			{
              
                mIter2.rsp = data;
                vec2.ForEach(mIter2);
			}
		} 
		else if (mMap.TryGetValue (cmd, out vec1)) 
		{
            mIter1.st = stream;
            vec1.ForEach(mIter1);
        }
	}

    /// <summary>
    /// 测试派发协议
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="stream"></param>
    public void TestInvoke(uint cmd, IExtensible rsp)
    {
        InvokeCallback(cmd);
        XFnList<Fn2> vec2 = null;
        if (mMap2.TryGetValue(cmd, out vec2))
        {
            mIter2.rsp = rsp;
            vec2.ForEach(mIter2);
        }
    }

    private void InvokeCallback(uint rspID)
    {
        if (null != mRspCallbackList)
        {
            for(int i=0,n= mRspCallbackList.Count;i<n;++i)
            {
                var fn = mRspCallbackList[i];
                fn(rspID);
            }
        }
    }

    private XArray<Action<uint>> mRspCallbackList;

    public void SetCallback(Action<uint> fn)
    {
        if(null == mRspCallbackList)
        {
            mRspCallbackList = new XArray<Action<uint>>();
        }
       
        mRspCallbackList.Add(fn);
    }


    /// <summary>
    /// 移除网络消息
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="fn"></param>
    public void Remove(uint cmd, Fn1 fn)
    {

        XFnList<Fn1> vec1 = null;
        if(mMap.TryGetValue(cmd,out vec1))
        {
            vec1.Remove(fn);
        }
    }
    /// <summary>
    /// 移除网络消息
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="fn"></param>
    public void Remove(uint cmd, Fn2 fn)
    {

        XFnList<Fn2> vec2 = null;
        if (mMap2.TryGetValue(cmd, out vec2))
        {
            vec2.Remove(fn);
        }
    }


}