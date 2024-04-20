using network.Utils;

namespace network;

public class GlobalLogic
{
    /// <summary>
    /// 全局的计时器
    /// </summary>
    public static readonly XTimer gTimer = new XTimer();
    /// <summary>
    /// 延迟更新的处理
    /// </summary>
    public static readonly XTimer gLateTimer = new XTimer();
    /// <summary>
    /// 这个全局的计时器不能在切换场景的时候被干掉
    /// </summary>
    public static readonly XTimer gGlobalTimer = new XTimer();


    private static SecondsTimer sSecondTimer { get; set; }

    /// <summary>
    /// 全局的秒步进器
    /// </summary>
    public static SecondsTimer GlobalSecondTimer
    {
        get
        {
            if (null == sSecondTimer)
            {
                sSecondTimer = new SecondsTimer(1f);
            }
            return sSecondTimer;
        }
    }


    /// <summary>
    /// 用来注册网络回包消息
    /// </summary>
    public static void SetUpProtobufMsg()
    {
        /*
        //登录
        PBReflectorZone.Regist<LoginReq>(NetZoneCmd.MAIN_LOGIN_REQ);
        PBReflectorZone.Regist<LoginRsp>(NetZoneCmd.MAIN_LOGIN_RSP);
        //登出
        // PBReflectorZone.Regist<LogoutReq>(E_NetZoneCmd.MAIN_LOGOUT_REQ);
        // PBReflectorZone.Regist<LogoutRsp>(E_NetZoneCmd.MAIN_LOGOUT_RSP);
        //心跳
        PBReflectorZone.Regist<HeartBeatReq>(NetZoneCmd.MAIN_HEARTBEAT_REQ);
        PBReflectorZone.Regist<HeartBeatRsp>(NetZoneCmd.MAIN_HEARTBEAT_RSP);
        */
    }
}