namespace network.Event;

public class GameEvent
{
    /// <summary>
        /// 正在检测版本更新
        /// </summary>
        public const string VERSION_CHECKING = "VERSION_CHECKING";

        /// <summary>
        /// 开始更新版本
        /// </summary>
        public const string VERSION_UPDATE_START = "VERSION_UPDATE_START";

        /// <summary>
        /// 版本更新的进度
        /// </summary>
        public const string VERSION_UPDATE_PROGRESS = "VERSION_UPDATE_PROGRESS";

        /// <summary>
        /// 版本更新完成
        /// </summary>
        public const string VERSION_UPDATE_FINSHED = "VERSION_UPDATE_FINSHED";

        /// <summary>
        /// 开始解压资源
        /// </summary>
        public const string UNPACK_START = "UNPACK_START";

        /// <summary>
        /// 解压完成
        /// </summary>
        public const string UNPACK_FINISHED = "UNPACK_FINISHED";

        /// <summary>
        /// android授权结果
        /// </summary>
        public const string REQUEST_PERMISSION = "REQUEST_PERMISSION";

        /// <summary>
        /// 游戏场景的资源加载中
        /// </summary>
        public const string SCENE_RES_PROGRESS = "SCENE_RES_PROGRESS";

        /// <summary>
        /// 加载场景完成
        /// </summary>
        public const string SCENE_LOAD_OVER = "SCENE_LOAD_OVER";

        /// <summary>
        /// 游戏场景的资源加载完成
        /// </summary>
        public const string SCENE_RES_READY = "SCENE_RES_READY";

        /// <summary>
        /// 配置表加载完成
        /// </summary>
        public const string TABLE_CONFIG_READY = "TABLE_CONFIG_READY";

        /// <summary>
        /// 全部的战斗场景相关的资源，UI，配置都初始化完成，可以等待进入战斗
        /// </summary>
        public const string BATTLE_SCENE_READY = "BATTLE_SCENE_READY";

        /// <summary>
        /// 一切准备就绪，可以进入游戏大厅了
        /// </summary>
        public const string ALL_READY = "ALL_READY";

        /// <summary>
        /// PVE进入战斗场景
        /// </summary>
        public const string ENTER_BATTLE_PVE = "ENTER_BATTLE_PVE";

        /// <summary>
        /// 战斗真正的开始，战斗加载界面移除后
        /// </summary>
        public const string BATTLE_REALLY_BEGIN = "BATTLE_REALLY_BEGIN";

        /// <summary>
        /// 服务器通知客户端所有客户端都加载好了，可以开始比赛
        /// </summary>
        public const string SERVER_NOTIFY_BEGIN = "SERVER_NOTIFY_BEGIN";

        /// <summary>
        /// 发现比赛
        /// </summary>
        public const string FIND_ACTIVE_GAME = "FIND_ACTIVE_GAME";

        /// <summary>
        /// 失去焦点事件
        /// </summary>
        public const string APPLICATION_LOST_FOCUS = "APPLICATION_LOST_FOCUS";

        /// <summary>
        /// 获得焦点事件
        /// </summary>
        public const string APPLICATION_GET_FOCUS = "APPLICATION_GET_FOCUS";

        /// <summary>
        /// 删除所有Tick
        /// </summary>
        public const string DELET_ALL_TIMER_HANDLER = "DELET_ALL_TIMER_HANDLER";

        /// <summary>
        /// 游戏结束
        /// </summary>
        public const string GAME_OVER = "GAME_OVER";

        /// <summary>
        /// 出生点的门关闭的时候派发
        /// </summary>
        public const string GATE_RESETED = "GATE_RESETED";

        /// <summary>
        /// 出生点的门打开的时候派发
        /// </summary>
        public const string GATE_OPENED = "GATE_OPENED";

        /// <summary>
        /// 清除资源的时候抛这个事件，不需要手动移除
        /// </summary>
        public const string CLEANUP_RES = "CLEANUP_RES";

        /// <summary>
        /// 开始重播
        /// </summary>
        public const string REPLAY_BEGIN = "REPLAY_BEGIN";

        /// <summary>
        /// 重播完成了
        /// </summary>
        public const string REPLAY_FINISHED = "REPLAY_FINISHED";


        /// <summary>
        /// 获得最新战斗服ping
        /// </summary>
        public const string GET_MATCH_SERVERS_PING = "GET_MATCH_SERVERS_PING";

        /// <summary>
        /// 绑定账号成功
        /// </summary>
        public const string BIND_ACCOUNT_SUCCESS = "BIND_ACCOUNT_SUCCESS";

        //add by lihui 触发式引导的监听事件
        /// <summary>
        /// 接近圣坛时，游戏出现触发式引导提示玩家可以在己方圣坛回血
        /// </summary>
        public const string GUIDE_NEAR_BLOOD = "GUIDE_NEAR_BLOOD";
        /// <summary>
        /// 当角色进入草丛时，游戏出现触发式引导提示玩家草丛可以躲藏
        /// </summary>
        public const string GUIDE_ENTER_GRASS = "GUIDE_ENTER_GRASS";
        /// <summary>
        /// 靠近敌方防御塔时提示
        /// </summary>
        public const string GUIDE_NEAR_TOWER = "GUIDE_NEAR_TOWER";
        /// <summary>
        /// 重生时提示带确定按钮的提示
        /// </summary>
        public const string GUIDE_PLAYER_RELIVE = "GUIDE_PLAYER_RELIVE";
        /// <summary>
        /// 新手引导战斗引导阶段结束
        /// </summary>
        public const string BATTLE_GUIDE_END = "BATTLE_GUIDE_END";
        /// <summary>
        ///登录用的
        /// </summary>
        public const string CONNECT_SERVER = "CONNECT_SERVER";

        /// <summary>
        /// 服务器通知游戏模式轮换了
        /// </summary>
        public const string SERVER_GAME_MODE_START_FLOW = "SERVER_GAME_MODE_START_FLOW";

        /// <summary>
        /// 客户端已经从服务器拉取到了新的游戏模式
        /// </summary>
        public const string SERVER_GAME_MODE_FINISH_FLOW = "SERVER_GAME_MODE_FINISH_FLOW";

      

    
        /// 用来派发或者发送消息
        /// </summary>
        public static readonly Notifier sNotifier = new Notifier();
}