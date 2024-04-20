using network.IO;

namespace network.Core;

public interface IBridge
{
    /// <summary>
    /// 注册网络消息，针对网络指令的
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="fn"></param>
    void Regist(uint cmd, Action<StreamTool> fn);

    /// <summary>
    /// 注册回调方法,针对protobuffer的
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="fn"></param>
    void Regist(uint cmd, Action<ProtoBuf.IExtensible> fn);
    /// <summary>
    /// 移除网络消息
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="fn"></param>
    void Remove(uint cmd, Action<StreamTool> fn);
    /// <summary>
    /// 移除网络消息
    /// </summary>
    /// <param name="cmd"></param>
    /// <param name="fn"></param>
    void Remove(uint cmd, Action<ProtoBuf.IExtensible> fn);
}