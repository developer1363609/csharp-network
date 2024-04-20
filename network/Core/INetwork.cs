using network.IO;

namespace network.Core;

public interface INetwork
{
    /// <summary>
    /// 发送pb消息
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="msg"></param>
    /// <returns></returns>
    bool SendCmd(uint cmd, ProtoBuf.IExtensible msg);
    /// <summary>
    /// 发送指令消息
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="stream"></param>
    /// <returns></returns>
    bool SendCmd(ushort cmd, StreamTool stream);
}