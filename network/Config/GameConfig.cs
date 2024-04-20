namespace network.Config;

public class GameConfig
{
    //user id
    public static ulong uid;
    
    // 网络收发包的缓冲区的单元大小
    public const int BUFFER_SIZE = 1024 * 512;
    
    //循环发送网络命令的间隔时间
    public static float RESEND_CMD_INTERVAL_TIME = 3;
}