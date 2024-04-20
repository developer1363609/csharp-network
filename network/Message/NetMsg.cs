namespace network.Message;

public static class NetMsg
    {
        /// <summary>
        /// 用来派发消息的管道 这个类和Notifier的区别是这个类支持跨线程之间的消息投递
        /// 而Notifier不支持跨线程之间的消息派发，
        /// 因为Unity里面约束子线程（网络线程）不允许调度Unity的资源
        /// </summary>
        public static readonly AsyncMsgPipe sPipe
            = new AsyncMsgPipe();

        #region 游戏服务器
        /// <summary>
        /// 断线重连
        /// </summary>
        public const string RECONNECT_BATTLE_SERVER = "RECONNECT_BATTLE_SERVER";
        /// <summary>
        /// 连接上网络 
        /// </summary>
        public const string CONNECTED_ZONE = "CONNECTED_ZONE";
        /// <summary>
        /// 网络被断开
        /// </summary>
        public const string DISCONNECTED_ZONE = "DISCONNECTED_ZONE";

        /// <summary>
        /// 网络错误
        /// </summary>
        public const string NETWORK_ERROR = "NETWORK_ERROR";

        /// <summary>
        /// 正在连接网络
        /// </summary>
        public const string CONNECTING_ZONE = "CONNECTING_ZONE";
        /// <summary>
        /// 连接网络失败
        /// </summary>
        public const string CONNECT_FAILED_ZONE = "CONNECT_FAILED_ZONE";
        /// <summary>
        /// 正在尝试重连
        /// </summary>
        public const string RECONNECTING_ZONE = "RECONNECTING_ZONE";
        /// <summary>
        /// 重连成功
        /// </summary>
        public const string RECONNECT_SUCCESS_ZONE = "RECONNECT_SUCCESS_ZONE";
        /// <summary>
        /// 重连失败
        /// </summary>
        public const string RECONNECT_FAILED_ZONE = "RECONNECT_FAILED_ZONE";
        /// <summary>
        /// 连接超时
        /// </summary>
        public const string CONNECT_TIMEOUT_ZONE = "CONNECT_TIMEOUT_ZONE";

        /// <summary>
        /// 打开网络等待面板
        /// </summary>
        public static string PANEL_NET_WAITING_OPEN = "PANEL_WAITING_OPEN";

        /// <summary>
        /// 关闭网络等待面板
        /// </summary>
        public static string PANEL_NET_WAITING_CLOSE = "PANEL_NET_WAITING_CLOSE";

        #endregion


        #region 战斗服务器

        /// <summary>
        /// 连接上网络 (NetworkManagerMatch内部使用的)
        /// </summary>
        public const string CONNECTED_MATCH_INTERNAL = "CONNECTED_MATCH_INTERNAL";
        /// <summary>
        /// 连接上网络 
        /// </summary>
        public const string CONNECTED_MATCH = "CONNECTED_MATCH";
        /// <summary>
        /// 网络被断开
        /// </summary>
        public const string DISCONNECTED_MATCH = "DISCONNECTED_MATCH";
        /// <summary>
        /// 正在连接网络
        /// </summary>
        public const string CONNECTING_MATCH = "CONNECTING_MATCH";
        /// <summary>
        /// 连接网络失败
        /// </summary>
        public const string CONNECT_FAILED_MATCH = "CONNECT_FAILED_MATCH";
        /// <summary>
        /// 正在尝试重连
        /// </summary>
        public const string RECONNECTING_MATCH = "RECONNECTING_MATCH";
        /// <summary>
        /// 重连成功
        /// </summary>
        public const string RECONNECT_SUCCESS_MATCH = "RECONNECT_SUCCESS_MATCH";
        /// <summary>
        /// 重连失败
        /// </summary>
        public const string RECONNECT_FAILED_MATCH = "RECONNECT_FAILED_MATCH";
        /// <summary>
        /// 连接超时
        /// </summary>
        public const string CONNECT_TIMEOUT_MATCH = "CONNECT_TIMEOUT_MATCH";
        /// <summary>
        /// 链接上了重播服务器
        /// </summary>
        public const string CONNECTED_REPLAY = "CONNECTED_REPLAY";
        /// <summary>
        /// 网络被断开,重播服务器
        /// </summary>
        public const string DISCONNECTED_REPLAY = "DISCONNECTED_REPLAY";

        /// <summary>
        /// 连接网络失败,重播服务器
        /// </summary>
        public const string CONNECT_FAILED_REPLAY = "CONNECT_FAILED_REPLAY";

        /// <summary>
        /// 正在连接网络,重播服务器
        /// </summary>
        public const string CONNECTING_REPLAY = "CONNECTING_REPLAY";

        /// <summary>
        /// 专用检测服务器ping的服务器链接结果消息
        /// </summary>
        public const string CONNECTING_SERVER_FOR_PING_SUCCESS = "CONNECTING_SERVER_FOR_PING_SUCCESS";
        public const string CONNECTING_SERVER_FOR_PING_FAIL = "CONNECTING_SERVER_FOR_PING_FAIL";
        #endregion
    }