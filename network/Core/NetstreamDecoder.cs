using network.IO;
using network.Utils;

namespace network.Core;

public class NetstreamDecoder : IDisposable
    {
        /// <summary>
        /// 消息头的大小
        /// </summary>
        private const int HEAD_SIZE = NetworkManager.HEAD_SIZE;
        /// <summary>
        /// 网络逻辑处理器
        /// </summary>
        private BridgeZone mBridge;
        /// <summary>
        /// 队列缓冲池
        /// </summary>
        private QueueBuffer mBuffer;
        /// <summary>
        /// 数据缓存
        /// </summary>
        private StreamTool mCachedStream;
        /// <summary>
        /// 获取数据队列
        /// </summary>
        public QueueBuffer Buffer
        {
            get
            {
                return mBuffer;
            }
        }
        /// <summary>
        /// 标记是否已经销毁了
        /// </summary>
        private bool mDisposed;
        /// <summary>
        /// 销毁资源
        /// </summary>
        public void Dispose()
        {
            if(! mDisposed)
            {
       
                GlobalLogic.gGlobalTimer.RemoveUpdateHandler(InvokeUpdate);
                mDisposed = true;
                if(null != mBuffer)
                {
                    mBuffer.Dispose();
                    mBuffer = null;
                }
                mBridge = null;
                if(null != mCachedStream)
                {
                    mCachedStream.Dispose();
                    mCachedStream = null;
                }
            }
        }
        /// <summary>
        /// 初始化
        /// </summary>
        public NetstreamDecoder SetUp()
        {
            mBridge = BridgeZone.Instance;
            mBuffer = new QueueBuffer();
            mCachedStream = new StreamTool(QueueBufferBlock.BLOCK_SIZE);
            return this;
        }
        /// <summary>
        /// 当前的消息体的长度
        /// </summary>
        private int mCurMsgSize = 0;
        /// <summary>
        /// 当起那的消息号
        /// </summary>
        private uint mCurCmd = 0;

        public readonly ClientHeader RecvHead = new ClientHeader();

        /// <summary>
        /// 更新协议
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public bool InvokeUpdate(float t)
        {
            Update();
            return false;
        }

        /// <summary>
        /// 解析网络包
        /// </summary>
        public void Update()
        {
            if(null != mBridge)
            {
                while(true)
                {
                    if(0 == mCurMsgSize && 0 == mCurCmd)//如果没读数据头
                    {
                        if (null == mBuffer)
                        {
                            return;
                        }
                        if (mBuffer.IsLonger(HEAD_SIZE))
                        {
                            mBuffer.ReadHeadZone(RecvHead, out mCurMsgSize, out mCurCmd);
                            if(!ProcessBody())
                            {
                                break;
                            }
                        }
                        else//如果数据还没收完
                        {
                            break;
                        }
                    }
                    else//如果已经读取了包头，没读数据体
                    {
                        if(!ProcessBody())
                        {
                            break;
                        }
                    }
                }
            }//end if
        }//end Update
        /// <summary>
        /// 处理消息体
        /// </summary>
        private bool ProcessBody()
        {
            if(0 == mCurCmd || 0 == mCurCmd)
            {
                return false;
            }
            var n = (int)mCurMsgSize;
            if (mBuffer.IsLonger(n))
            {
                mCachedStream.Clear();//清空旧的数据
                mBuffer.WriteToStreamTool(mCachedStream, n);
                mBridge.Invoke(mCurCmd, mCachedStream);//调度业务层处理
                mCurMsgSize = 0;
                mCurCmd = 0;
                return true;
            }
            return false;
        }
    }