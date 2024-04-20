namespace network.Message;

public struct PipeMsg : IEquatable<PipeMsg>
    {
        /// <summary>
        /// 派发事件的时候传递的参数
        /// </summary>
        public readonly object param;
        /// <summary>
        /// 事件的派发器
        /// </summary>
        private readonly AsyncMsgPipe sender;
        /// <summary>
        /// 事件的类型
        /// </summary>
        public readonly string type;
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="param_"></param>
        /// <param name="sender_"></param>
        /// <param name="type_"></param>
        internal PipeMsg(object param_,
            AsyncMsgPipe sender_,
            string type_)
        {
            param = param_;
            sender = sender_;
            type = type_;
        }
        public static readonly PipeMsg Default = new PipeMsg(null, null, null);

        public bool Equals(PipeMsg other)
        {
            return param == other.param && 
                sender == other.sender && type == other.type;
        }
    }
