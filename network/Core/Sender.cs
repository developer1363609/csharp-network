using network.Config;
using network.Utils;
using ProtoBuf;

namespace network.Core;

public class Sender
    {
        //当前正在发送的命令字ID
        uint mCurRspId = 0;
        uint mCurReqId = 0;
        IExtensible mCurMsg = null;

        //循环发送网络命令的最大次数
        int mCurSendMsgCount =0;
        //当前是否正在发送
        bool mIsRequesting = false;
        //记录当前需要发送的所有命令字ID
        HashSet<uint> mHasCmd = new HashSet<uint>();
        //记录当前所有需要发送的命令
        Queue<ReqInfo> mQueReqInfo = new Queue<ReqInfo>();

        static Sender mCurrent = null;
        public static Sender Instance {
            get {
                if(null == mCurrent)
                {
                      mCurrent = new Sender();
                }
                return mCurrent;
            }
            private set { mCurrent = value; }
        }

        class ReqInfo
        {
            public uint repId;
            public uint rspId;
            public IExtensible msg;

            public void Set(uint reqId,uint rspId,IExtensible msg)
            {
                this.repId = reqId;
                this.rspId = rspId;
                this.msg = msg;
            }
        }

        public Sender()
        {
            //注册回包回馈
            BridgeZone.Instance.SetCallback(OnRsp);
        }

        /// <summary>
        /// 清除网络命令缓存，断网后应该清理，否则
        /// 可能导致重连后无法发送网络命令
        /// </summary>
        public void ClearCmdCache()
        {
            mIsRequesting = false;
            if(null != mHasCmd)
            {
                mHasCmd.Clear();
            }
            if(null != mQueReqInfo)
            {
                mQueReqInfo.Clear();
            }
            //GlobalLogic.gTimer.RemoveTimeout(ShowWaitingPanel);
        }

        /// <summary>
        /// 发送命令
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="msg"></param>
        public void SendCmd(uint reqId,uint rspId, IExtensible msg)
        {
            if(!NetworkManager.Instance.NetworkOK)
            {
                return;
            }

            //请求过程中不允许多次发送相同的命令
            if (!mHasCmd.Contains(rspId))
            {
                mHasCmd.Add(rspId);
                if (!mIsRequesting)
                {
                    mIsRequesting = true;
                    mCurRspId = rspId;
                    mCurReqId = reqId;
                    mCurMsg = msg;
                    NetworkManager.Instance.SendCmd(reqId, msg);
                    timerHelper = 0;
                    mCurSendMsgCount = 0;
                    SetUpdateHandlerSendCmd(true);
                    OpenNetWorkWaitingPanel();
                    Console.WriteLine("~~~~~~发送命令： {0}", reqId);                
                }
                else
                {
                    ReqInfo req = mReqPool.Get();
                    req.Set(reqId, rspId, msg);
                    mQueReqInfo.Enqueue(req);
                }
            }
        }

        private XPool<ReqInfo> mReqPool = new XPool<ReqInfo>();

        /// <summary>
        /// 回包
        /// </summary>
        /// <param name="rspId"></param>
        void OnRsp(uint rspId)
        {
            if(mCurRspId == rspId)
            {           
                if(mHasCmd.Contains(rspId))
                {
                    mHasCmd.Remove(rspId);
                }
                mCurSendMsgCount = 0;
                mCurRspId = 0;
                mCurMsg = null;
                mCurReqId = 0;
                SetUpdateHandlerSendCmd(false);

                //继续处理下一个网络命令
                if (mQueReqInfo.Count > 0)
                {
                    ReqInfo req = mQueReqInfo.Dequeue();
                    mCurRspId = req.rspId;
                    mCurReqId = req.repId;
                    mCurMsg = req.msg;
                    NetworkManager.Instance.SendCmd(req.repId, req.msg);
                    timerHelper = 0;
                    mCurSendMsgCount = 0;
                    SetUpdateHandlerSendCmd(true);
                }
                else
                {
                    Console.WriteLine("~~~~~~接收命令：{0}", rspId);
                    CloseNetWorkWaitingPanel();
                    mIsRequesting = false;
                }
            }
        }

        /// <summary>
        /// 打开网络等待面板，只有超时2s后才打开
        /// </summary>
        void OpenNetWorkWaitingPanel()
        {
            ShowWaitingPanel();
            //GlobalLogic.gTimer.SetTimeout(ShowWaitingPanel, 2f);
        }

        void ShowWaitingPanel()
        {
            //int panelType = PanelWaitingView.WAITING_TYPE.WAITINGNET_MODE_DELAY;
            //UIPanelManager.ShowWaitingPanel(panelType, ClearCmdCache);
        }

        /// <summary>
        /// 关闭网络等待面板
        /// </summary>
        void CloseNetWorkWaitingPanel()
        {
            //GlobalLogic.gTimer.RemoveTimeout(ShowWaitingPanel);
           // UIPanelManager.HideWaitingPanel();
        }

        /// <summary>
        /// 发送命令后启动一个循环发送器，防止不能收到回包
        /// 导致后面的所有命令无法发送。循环发送N次后丢弃该次
        /// 命令，直接发送下一条网络命令
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        float timerHelper = 0;
        bool ReSendCmd(float t)
        {
            timerHelper += t;
            if (timerHelper >= GameConfig.RESEND_CMD_INTERVAL_TIME)
            {
                timerHelper = 0;
                if (mCurReqId == 0 || mCurMsg == null)
                {
                    return true;
                }
                mCurSendMsgCount++;
                if(mCurSendMsgCount >3)
                {
                    mCurSendMsgCount = 0;
                    OnRsp(mCurRspId);
                    return true;
                }
                NetworkManager.Instance.ReSendCmd(mCurReqId, mCurMsg);
            }
            return false;
        }

        #region 设置循环发送网络命令的定时器
        System.Func<float, bool> mReSendCmd;
        void SetUpdateHandlerSendCmd(bool add)
        {
            if (add)
            {
                if (null == mReSendCmd)
                {
                    mReSendCmd = ReSendCmd;
                    GlobalLogic.gTimer.AddUpdateHandler(mReSendCmd);
                }
            }
            else
            {
                if (null != mReSendCmd)
                {
                    GlobalLogic.gTimer.RemoveUpdateHandler(mReSendCmd);
                    mReSendCmd = null;
                }
            }
        }
        #endregion
    }