namespace network.Utils;

public class QueueBufferBlock : MemoryStream ,IDisposable
{
    /// <summary>
    /// 标记对象是否已经被销毁了
    /// </summary>
    private bool mDisposed;

    public new void Dispose()
    {
        if (mDisposed) return;
        mDisposed = true;
        base.Dispose();
    }

    ~QueueBufferBlock() { Dispose(); }

    /// <summary>
    /// 下一个节点
    /// </summary>
    public QueueBufferBlock mNext;
    /// <summary>
    /// 上一个节点
    /// </summary>
    public QueueBufferBlock mPrev;
    /// <summary>
    /// 重置数据
    /// </summary>
    /// <returns></returns>
    public QueueBufferBlock Reset()
    {
        mNext = null;
        mPrev = null;
        mBytesAvailable = 0;
        return this;
    }
    /// <summary>
    /// 连接另外一个缓冲区块
    /// </summary>
    /// <param name="other"></param>
    public void Connect(QueueBufferBlock other)
    {
        mNext = other;
        other.mPrev = this;
    }

    private QueueBufferBlock(int capacity = BLOCK_SIZE)
        : base(capacity)
    { }

    public QueueBufferBlock() : this(BLOCK_SIZE) { }
    /// <summary>
    /// 检测当前的区块写满以后还需要多到的存储空间来写数据
    /// 如果返回0，则表示当前的块足以写入size字节的全部内容s
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public int CheckWrite(int size)
    {
        //计算剩余的空间
        var n = (int)(Capacity - Length);
        if (n >= size)
        {
            return 0;
        }

        return size - n;
    }
    
    /// <summary>
    /// 检测当前区块的缓冲区是否可以读取size字节的数据
    /// 如果返回0，则表示当前的缓冲区可以读取size字节的数据
    /// 如果返回的是n，则表明剩余的数据需要从后面的内存块读取
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public int CheckRead(int size)
    {
        //计算剩余的空间
        var n = mBytesAvailable;
        if (n >= size)
        {
            return 0;
        }
        return size - n;
    }

    /// <summary>
    /// 调试接口，打印从pos位置的size字节的字符串内容
    /// </summary>
    /// <param name="size"></param>
    /// <param name="sb"></param>
    public void GetRawString(int size,int front, System.Text.StringBuilder sb)
    {
        const int PER_LINE = 16;
        var i = 0;
        var next = this;
        var left = size;
        next = this;
        while(left > 0 && null != next)
        {
            var pos = next.Position;
            next.Position = front;
            var nAvaliable = next.mBytesAvailable;
            for (int j = 0, nj = left; j < nj; ++j)
            {
                if (0 == nAvaliable)
                {
                    break;
                }
                --nAvaliable;
                --left;
                ++i;
                sb.Append(' ').Append(next.ReadByte().ToString("x"));
                if (i % PER_LINE == 0)
                {
                    sb.AppendLine();
                }
            }
            next.Position = pos;//重置位置
            next = next.mNext;
        }
    }

    /// <summary>
    /// 已经写进缓冲区的字节数
    /// </summary>
    private int mBytesAvailable;

    public override void Write(byte[] buffer, int offset, int count)
    {
        mBytesAvailable += count;
        base.Write(buffer, offset, count);
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (offset < 0 || count < 0)
        {
            //this.PrintError("1 QueueBufferBlock.Read出错 offset=" + offset + " count=" + count);// + " m_testCount=" + m_testCount);
        }
        if (base.Length < count)
        {
            //this.PrintError("2 QueueBufferBlock.Read出错 base.Length=" + base.Length + " offset=" + offset + " count=" + count + " bytesAvailable=" + bytesAvailable);// + " m_testCount=" + m_testCount);
        }
        mBytesAvailable -= count;
        return base.Read(buffer, offset, count);
    }

    /// <summary>
    /// 每一个池的默认大小
    /// </summary>
    public const int BLOCK_SIZE = 1024 * 8;
    
    /// <summary>
    /// 工厂方法
    /// </summary>
    /// <returns></returns>
    public static QueueBufferBlock Create()
    {
        return new QueueBufferBlock(BLOCK_SIZE);
    }

}