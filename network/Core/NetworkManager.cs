using System.Net;
using System.Net.Sockets;
using Google.Protobuf;
using network.Config;
using network.Event;
using network.IO;
using network.Message;
using ProtoBuf;

namespace network.Core
{
    /// <summary>
    /// 记录网络连接的状态
    /// </summary>
    public enum E_NetStatus
    {
        /// <summary>
        /// 没连接到网络
        /// </summary>
        DISCONNECT,
        /// <summary>
        /// 已经连接上了网络
        /// </summary>
        CONNECTED,
        /// <summary>
        /// 正在连接网络
        /// </summary>
        CONNECTING,
    }

    /// <summary>
    /// 网络延迟状态
    /// </summary>
    public enum E_NetDelayState
    {
        /// <summary>
        /// 默认状态
        /// </summary>
        INIT = 4,
        /// <summary>
        /// 网络状态良好
        /// </summary>
        WELL = 3,
        /// <summary>
        /// 网络状态一般
        /// </summary>
        SIMPLE = 2,
        /// <summary>
        /// 网络状态一般
        /// </summary>
        BAD = 1,
        /// <summary>
        /// 网络无法连接
        /// </summary>
        NONE = 0
    }

    public class ClientHeader
    {
        public const int SIZE = 32 + 4;
        /// <summary>
        /// 冯文说加上这2个字段避免被端口扫描的程序误伤
        /// </summary>
        public ushort protocol_version = 0;
        public ushort pass_code = 0x7C7A;
        /// <summary>
        /// 用户id
        /// </summary>
        public long uid;
        /// <summary>
        /// 
        /// </summary>
        public int zone;

        public int ip;

        public int version;

        public int seq;

        public int cmd;

        public int data_len;
        /// <summary>
        /// 将数据写入数据流
        /// </summary>
        /// <param name="st"></param>
        public void Encode(StreamTool st)
        {
            var writer = st.Out;
            writer.Write(IPAddress.HostToNetworkOrder((short)protocol_version));
            writer.Write(IPAddress.HostToNetworkOrder((short)pass_code));
            writer.Write(IPAddress.HostToNetworkOrder(seq));
            writer.Write(IPAddress.HostToNetworkOrder(uid));
            writer.Write(IPAddress.HostToNetworkOrder(zone));
            writer.Write(IPAddress.HostToNetworkOrder(ip));
            writer.Write(IPAddress.HostToNetworkOrder(version));
            writer.Write(IPAddress.HostToNetworkOrder(cmd));
            writer.Write(IPAddress.HostToNetworkOrder(data_len));
        }
        
        ///// <summary>
        /// 从二进制数据里面读取数据
        /// </summary>
        /// <param name="data"></param>
        public void Decode(byte[] data)
        {
            var offset = 4;//偏移了protocol_version和pass_code
            var pos = 0;

            seq = BitConverter.ToInt32(data, pos+ offset);
            seq = IPAddress.NetworkToHostOrder(seq);

            pos += 4;//sizeof(seq);

            uid = BitConverter.ToInt64(data, pos+ offset);
            uid = IPAddress.NetworkToHostOrder(uid);

            pos += 8;//sizeof(uid);

            zone = BitConverter.ToInt32(data, pos+ offset);
            zone = IPAddress.NetworkToHostOrder(zone);

            pos += 4;//sizeof(zone);

            //UnityEngine.Debug.Log("zone:" + zone);

            ip = BitConverter.ToInt32(data, pos+ offset);
            ip = IPAddress.NetworkToHostOrder(ip);

            pos += 4;// sizeof(ip);

            version = BitConverter.ToInt32(data, pos+ offset);
            version = IPAddress.NetworkToHostOrder(version);

            pos += 4;//sizeof(version);


            cmd = BitConverter.ToInt32(data, pos + offset);
            cmd = IPAddress.NetworkToHostOrder(cmd);

            pos += 4;//sizeof(cmd)


            data_len = BitConverter.ToInt32(data, pos + offset);
            data_len = IPAddress.NetworkToHostOrder(data_len);

            pos += 4;//sizeof(data_len)

        }

        /// <summary>
        /// 不允许外部实例化对象
        /// </summary>
        public ClientHeader()
        {

        }
        
    }

    /// <summary>
    /// 网络管理器
    /// </summary>
    public class NetworkManager : SimpleSingleton<NetworkManager> , INetwork
    {

        public int LogErrorChance = 10;
        private static ManualResetEvent TimeoutObject = new ManualResetEvent(false);
        private const int TIME_OUT = 5000;
        public void Dispose(bool sendEvent = false)
        {
            lock (this)
            {
                Close();
            }
        }
        /// <summary>
        /// 消息头的长度
        /// </summary>
        public const int HEAD_SIZE = ClientHeader.SIZE;
        /// <summary>
        /// 记录当前的网络状态
        /// </summary>
        public E_NetStatus Status { get; private set; }
        /// <summary>
        /// 发送数据交换用的数据缓冲区
        /// </summary>
        private MemoryStream mStreamCached = new MemoryStream();
        /// <summary>
        /// 用来写数据的
        /// </summary>
        private StreamTool mByteArray = new StreamTool(HEAD_SIZE);
        /// <summary>
        /// 用来发送数据用的
        /// </summary>
        private byte[] mSendBuffer = new byte[GameConfig.BUFFER_SIZE];
        /// <summary>
        /// 用来交换数据缓冲的缓冲区
        /// </summary>
        private byte[] mSwapBuffer = new byte[GameConfig.BUFFER_SIZE];
        /// <summary>
        /// 用来收数据的缓冲区
        /// </summary>
        private byte[] mRecvBuffer = new byte[GameConfig.BUFFER_SIZE];
        /// <summary>
        /// 用来解数据包的工具类
        /// </summary>
        private NetstreamDecoder mDecoder;

        public NetstreamDecoder GetDecoder()
        {
            return mDecoder;
        }

        private int curSeq;

        /// <summary>
        /// 构造方法
        /// </summary>
        public NetworkManager()
        {
            SuspendThis(true);
            Status = E_NetStatus.DISCONNECT;
         
            mStreamCached.SetLength(0);
            mStreamCached.Capacity = GameConfig.BUFFER_SIZE;
            mDecoder = new NetstreamDecoder().SetUp();
           
        }

        /// <summary>
        /// 用来收发数据用的类
        /// </summary>
        private TcpClient mTcpClient;

        /// <summary>
        /// 序列化工具
        /// </summary>
        private Google.Protobuf.MessageParser<ParseObject> _messageParser = new MessageParser<ParseObject>(() =>
        {
            return null;
        });

        public static readonly serializer sSerializer = new serializer();

        #region 当网络处于未连接时用来缓存指令用
        private CacheInstruction mCacheInstructions = new CacheInstruction();

        private class Instruction
        {
            public uint Cmd { get; private set; }
            public IExtensible Msg { get; private set; }

            public Instruction(uint cmd, IExtensible msg)
            {
                Cmd = cmd;
                Msg = msg;
            }
        }

        private class CacheInstruction
        {
            private Queue<Instruction> mInstructions = new Queue<Instruction>();

            public Instruction Dequeue()
            {
                if (mInstructions.Count > 0)
                {
                    return mInstructions.Dequeue();
                }
                else
                {
                    return null;
                }
            }

            public void Enqueue(Instruction instruction)
            {
                mInstructions.Enqueue(instruction);
            }

            public void Clear()
            {
                mInstructions.Clear();
            }
        }
        #endregion

        //清空缓存的指令
        public void ClearCacheInstructions()
        {
            mCacheInstructions.Clear();
        }

        //刷新缓存的指令
        public bool FlushCacheInstructions()
        {
            bool ret = false;
            do
            {
                if( NetworkOK )
                {
                    Instruction instruction = mCacheInstructions.Dequeue();

                    while( instruction != null )
                    {
                        instruction = mCacheInstructions.Dequeue();
                        if( !SendImpl( instruction.Cmd , instruction.Msg ) )
                        {
                            return false;
                        }
                    }

                    ret = true;
                }
            } while( false );
            return ret;
        }

        private List<uint> mCmdCache = new List<uint>();

        private List<IExtensible> mMsgCache = new List<IExtensible>();


        /// <summary>
        /// 发送pb消息
        /// </summary>
        public bool SendCmd( uint cmd , IExtensible msg )
        {
            var ret = false;
            do
            {
             

                if ( !NetworkOK )
                {
                    mCacheInstructions.Enqueue( new Instruction( cmd , msg ) );
                    break;
                }

                if( !FlushCacheInstructions() )
                {
                    return false;
                }

                if( !SendImpl( cmd , msg ) )
                {
                    return false;
                }
      
                ret = true;
            } while( false );
            return ret;
        }

        private bool SendImpl( uint cmd , IExtensible msg )
        {
            var ret = false;
            do
            {
                try
                {
                    mStreamCached.SetLength( 0 );
                    mStreamCached.Position = 0;
                    sSerializer.Serialize( mStreamCached , msg );
                    mStreamCached.Position = 0;
                    var msgSize = (int) mStreamCached.Length;
                    //if(msgSize <= 0)
                    //{
                    //    Console.WriteLineError("消息长度为0，消息号为:{0}", cmd);
                    //    break;
                    //}
                    if( mSendBuffer.Length < msgSize + HEAD_SIZE )
                    {
                        Array.Resize<byte>( ref mSendBuffer , msgSize + HEAD_SIZE );
                    }
                    WriteHead( cmd , msgSize );
                    mStreamCached.Read( mSendBuffer , HEAD_SIZE , msgSize );
                    DoSend( msgSize );

                    ret = true;
                }
                catch( Exception e )
                {
                    Console.Error.WriteLine( e.ToString() );
                    Dispose( true );
                    break;
                }
            } while( false );
            return ret;
        }

        public bool ReSendCmd( uint cmd , IExtensible msg )
        {
            Console.WriteLine("ResendCmd" + cmd);
            var ret = false;
            do
            {
                if( !NetworkOK )
                {
                    break;
                }
                try
                {
                    mStreamCached.SetLength( 0 );
                    mStreamCached.Position = 0;
                    sSerializer.Serialize( mStreamCached , msg );
                    mStreamCached.Position = 0;
                    var msgSize = (int) mStreamCached.Length;
                    if( mSendBuffer.Length < msgSize + HEAD_SIZE )
                    {
                        Array.Resize<byte>( ref mSendBuffer , msgSize + HEAD_SIZE );
                    }
                    WriteHead( cmd , msgSize , true );
                    mStreamCached.Read( mSendBuffer , HEAD_SIZE , msgSize );
                    DoSend( msgSize );
                }
                catch( Exception e )
                {
                    Console.Error.WriteLine( e.ToString() );
                    Dispose( true );
                    break;
                }
                ret = true;
            } while (false);
            return ret;
        }

        public readonly ClientHeader SendHead = new ClientHeader();

        /// <summary>
        /// 将消息头写入到数据流
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="msgSize"></param>
        private void WriteHead(uint cmd,int msgSize, bool resend = false)
        {
            var head = SendHead;
            head.uid = (long)GameConfig.uid;
            head.cmd = (int)cmd;
            head.data_len = msgSize;


            if (!resend)
            {
                ++curSeq;
            }
            head.seq = curSeq;

            mByteArray.Position = 0;
            head.Encode(mByteArray);
            mByteArray.Position = 0;
            mByteArray.In.Read(mSendBuffer, 0, HEAD_SIZE);
        }
        /// <summary>
        /// 判断当前网络状态是否为可用状态g
        /// </summary>
        public bool NetworkOK
        {
            get
            {
                if (E_NetStatus.CONNECTED != Status)
                {
                    return false;
                }
                if (null == mTcpClient)
                {
                    return false;
                }
                if (!mTcpClient.Connected)
                {
                    return false;
                }
                return true;
            }
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        private void DoSend(int msgSize)
        {
            mTcpClient.GetStream().Write(mSendBuffer, 0, HEAD_SIZE + msgSize);
        }
        /// <summary>
        /// 是否已经启动了
        /// </summary>
        private bool mSetUp = false;
        /// <summary>
        /// 初始化
        /// </summary>
        public void SetUp()
        {
            if(!mSetUp)
            {
                mSetUp = true;
            }
        }

        /// <summary>
        /// 关闭网络管理器
        /// </summary>
        public void ShutDown()
        {
            if(mSetUp)
            {
                mSetUp = false;
                instance = null!;
            }
        }

        /// <summary>
        /// 关闭网络套接字
        /// </summary>
        private void CloseSocket()
        {
            if(Status != E_NetStatus.DISCONNECT)
            {
                Status = E_NetStatus.DISCONNECT;

                if(null != mStreamCached)
                {
                    mStreamCached.Dispose();
                    mStreamCached = null;
                }
                if(null != mByteArray)
                {
                    mByteArray.Dispose();
                    mByteArray = null;
                }
                if(null != mTcpClient)
                {
                    mTcpClient.Close();
                    mTcpClient = null;
                }
                if(null != stream)
                {
                    stream.Dispose();
                    stream = null;
                }
                if(null != mDecoder)
                {
                    mDecoder.Dispose();
                    mDecoder = null;
                }
                ClearCacheInstructions();
            }
        }

        /// <summary>
        /// 关闭网络套接字
        /// </summary>
        private void Close()
        {
            lock(this)
            {
                CloseSocket();
                if(mDecoder != null)
                {
                    mDecoder.Dispose();
                    mDecoder = null;
                }
                Status = E_NetStatus.DISCONNECT;
            }//lock
        }//Close

        /// <summary>
        /// 连接服务器的处理
        /// </summary>
        /// <param name="ir"></param>
        private void OnConnectedServer(IAsyncResult ir)
        {
            try
            {
                TcpClient tcpclient = ir.AsyncState as TcpClient;
                if (tcpclient.Client != null)
                {
                    mTcpClient.EndConnect(ir);
                    Status = E_NetStatus.CONNECTED;

                    StartRecv();
                    NetMsg.sPipe.DispatchCmd(NetMsg.CONNECTED_ZONE, null);
                    GameEvent.sNotifier.DispatchCmd(GameEvent.CONNECT_SERVER);
                }
                else
                {
                    Console.WriteLine("CONNECT_FAILED_ZONE");

                    NetMsg.sPipe.DispatchCmd(NetMsg.CONNECT_FAILED_ZONE, null);
                }
                NetMsg.sPipe.DispatchCmd(NetMsg.PANEL_NET_WAITING_CLOSE, null);
                // UIPanelManager.HideWaitingPanel();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                NetMsg.sPipe.DispatchCmd(NetMsg.CONNECT_FAILED_ZONE, null);
            }
            finally
            {
                Console.WriteLine(" TimeoutObject.Set();");
                TimeoutObject.Set();
            }
        }

        /// <summary>
        /// 开始收数据包
        /// </summary>
        private void StartRecv()
        {
            if(! NetworkOK)
            {
                return;
            }
            stream = mTcpClient.GetStream();
            if(!stream.CanRead)
            {

            }
            if(null == mOnReadEnd)
            {
                mOnReadEnd = OnReadEnd;
            }
            stream.BeginRead(mRecvBuffer, 0, mRecvBuffer.Length, mOnReadEnd, null);
        }

        private AsyncCallback mOnReadEnd;
        /// <summary>
        /// 读取数据完成的处理
        /// </summary>
        /// <param name="ir"></param>
        private void OnReadEnd(IAsyncResult ir)
        {
            lock(this)
            {
                try
                {
                    if (Status != E_NetStatus.CONNECTED)
                    {
                        return;
                    }
                    if (null == stream)
                    {
                        return;
                    }
                    int recvSize = stream.EndRead(ir);
                    if (recvSize <= 0)
                    {
                        return;
                    }
                    mDecoder.Buffer.Push(mRecvBuffer, 0, recvSize);
                }
                catch (Exception e)
                {
                    Console.Error.Write(e.ToString());
                    Dispose(true);
                }
                StartRecv();
            }
        }

        /// <summary>
        /// 网络数据流，用来收数据的
        /// </summary>
        private NetworkStream stream;
        /// <summary>
        /// 登录服务器
        /// </summary>
        public TcpClient LoginServer()
        {
            if (Status == E_NetStatus.CONNECTED)
            {
                Console.Error.WriteLine("当前网络已经链接，请先断开网络");
                GameEvent.sNotifier.DispatchCmd(GameEvent.CONNECT_SERVER);
                return mTcpClient;
            }

            AddressFamily af = AddressFamily.Unspecified;
            try
            {
                ClearCacheInstructions();
                TimeoutObject.Reset();
               
                IPAddress[] aList = Dns.GetHostAddresses("ipxxxx");
                if(aList != null && aList.Length > 0)
                {
                    IPAddress a = aList[0];
                    af = a.AddressFamily;
                }

                int port = 8080;
                mTcpClient = new TcpClient(af);
                mTcpClient.BeginConnect("", port, new AsyncCallback(OnConnectedServer), mTcpClient);
                if (TimeoutObject.WaitOne(TIME_OUT, false))
                {
                    if (Status == E_NetStatus.CONNECTED)
                    {
                        return mTcpClient;
                    }
                }
                else
                {
                    mTcpClient.Close();
                    return null;
                }
            }
            catch(Exception e)
            {
                Console.Error.WriteLine("链接服务器失败LoginServer",e.ToString());
            }
            return mTcpClient;
        }

        bool INetwork.SendCmd(ushort cmd, StreamTool stream)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// 返回当前机器是用的什么网段（一般是IPV4或IPV6）
        /// </summary>
        /// <returns></returns>
        public AddressFamily GetHomeAdressType()
        {
            try
            {
                string HostName = Dns.GetHostName();//得到主机名
                IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
                string targetHost = IpEntry.HostName;//当前使用中的Host
                for(int i=0,n= IpEntry.AddressList.Length;i<n;++i)
                {
                    var a = IpEntry.AddressList[i];
                    if(a.ToString().Equals(targetHost))
                    {
                        //返回当前使用的网段
                        return a.AddressFamily;
                    }
                }
            }
            catch(Exception ex)
            {
                //默认链接IPV4
                return AddressFamily.InterNetwork;
            }

            return AddressFamily.InterNetwork;
        }
    }
}


