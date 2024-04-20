using System.Runtime.InteropServices;
using network.Core;
using network.IO;

namespace network.Utils;

public class QueueBuffer : IDisposable
{
    private Mutex mMutex = new Mutex();
    /// <summary>
    /// 交换用的缓冲数组，锁模式下用
    /// </summary>
    private readonly byte[] mCachedBuffer = new byte[QueueBufferBlock.BLOCK_SIZE];

    /// <summary>
    /// 交换用的缓冲数组,非锁模式下用
    /// </summary>
    private static readonly byte[] sBuffer = new byte[QueueBufferBlock.BLOCK_SIZE];
    /// <summary>
    /// 头节点
    /// </summary>
    private QueueBufferBlock mListHead;
    /// <summary>
    /// 尾节点
    /// </summary>
    private QueueBufferBlock mListTail;
    /// <summary>
    /// 记录第一个block读到哪个位置了
    /// </summary>
    private int mFront;
    /// <summary>
    /// 记录最后一个block写到哪个位置了
    /// </summary>
    private int mLast;

    public QueueBuffer()
    {
        BytesAvailable = 0;
        AddBlock();
    }
    /// <summary>
    /// 总结器
    /// </summary>
    ~QueueBuffer()
    {
        Dispose();
    }
    /// <summary>
    /// 标记是否已经释放资源了
    /// </summary>
    private bool mDisposed;
    /// <summary>
    /// 手动销毁资源
    /// </summary>
    public void Dispose()
    {
        if (!mDisposed)
        {
            mDisposed = true;
        }
        else
        {
            return;
        }
        if (mMutex.WaitOne())
        {
            while (null != mListHead)
            {
                RemoveFront();
            }
            mFront = mLast = 0;

            //将内存区块放回对象池，以方便循环使用
            mMutex.ReleaseMutex();
        }

        if (null == mMutex) return;
        mMutex.Close();
        mMutex = null;
    }
    
    /// <summary>
    /// 往缓冲区写入数据,线程安全
    /// </summary>
    /// <param name="data"></param>
    /// <param name="offset"></param>
    /// <param name="size"></param>
    public void Push(byte[] data, int offset, int size)
    {
        if (!mMutex.WaitOne()) return;
        //如果所有的区块都已经空了，则新建一个新的区块
        if (null == mListTail)
        {
            AddBlock();
            mLast = 0;
        }
        var n = mListTail.CheckWrite(size);
        if (0 == n)
        {
            mListTail.Position = mLast;
            mListTail.Write(data, offset, size);
            mLast += size;
        }
        else
        {
            mListTail.Position = mLast;
            mListTail.Write(data, offset, size - n);
            AddBlock();
            mLast = 0;
            mListTail.Position = mLast;
            mListTail.Write(data, offset + size - n, n);
            mLast += n;
        }
        BytesAvailable += size;
        mMutex.ReleaseMutex();
    }
    
    /// <summary>
    /// 往缓冲区写入数据,非线程安全
    /// </summary>
    /// <param name="data"></param>
    /// <param name="offset"></param>
    /// <param name="size"></param>
    private void PushWithoutLock(byte[] data,int offset,int size)
    {
        //如果所有的区块都已经空了，则新建一个新的区块
        if (null == mListTail)
        {
            AddBlock();
            mLast = 0;
        }
        var n = mListTail.CheckWrite(size);
        if (0 == n)
        {
            mListTail.Position = mLast;
            mListTail.Write(data, offset, size);
            mLast += size;
        }
        else
        {
            mListTail.Position = mLast;
            mListTail.Write(data, offset, size - n);
            AddBlock();
            mLast = 0;
            mListTail.Position = mLast;
            mListTail.Write(data, offset + size - n, n);
            mLast += n;
        }
        BytesAvailable += size;
    }
    
    /// <summary>
    /// 写入一个整数，无锁
    /// </summary>
    /// <param name="i"></param>
    /// <param name="st"></param>
    /// <param name="swap"></param>
    public void WriteInt(int i,StreamTool st,byte[] swap)
    {
        st.Clear();
        st.Out.Write(i);
        st.Position -= 4;
        st.In.Read(swap, 0, 4);
        PushWithoutLock(swap, 0, 4);
    }
    
    /// <summary>
    /// 读一个整数，无锁
    /// </summary>
    /// <param name="st"></param>
    /// <param name="swap"></param>
    /// <returns></returns>
    public int ReadInt(StreamTool st,byte[] swap)
    {
        st.Clear();
        Read(swap, 0, 4);
        st.Out.Write(swap, 0, 4);
        st.Position -= 4;
        return st.In.ReadInt32();
    }


    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct Converter
    {
        [FieldOffset(0)] private readonly int I;
        [FieldOffset(0)] private readonly uint UI;
        [FieldOffset(0)] private readonly long L;
        [FieldOffset(0)] private readonly ulong UL;
        [FieldOffset(0)] private readonly short S;
        [FieldOffset(0)] private readonly ushort US;
        [FieldOffset(0)] private readonly byte _0;
        [FieldOffset(1)] private readonly byte _1;
        [FieldOffset(2)] private readonly byte _2;
        [FieldOffset(3)] private readonly byte _3;
        [FieldOffset(4)] private readonly byte _4;
        [FieldOffset(5)] private readonly byte _5;
        [FieldOffset(6)] private readonly byte _6;
        [FieldOffset(7)] private readonly byte _7;
        public override bool Equals(object obj)
        {
            throw new Exception("Converter.Equals not allowed!");
        }
        public override int GetHashCode()
        {
            throw new Exception("Converter.GetHashCode not allowed!");
        }
    }

    private readonly XPool<QueueBufferBlock> Pool = new XPool<QueueBufferBlock>(QueueBufferBlock.Create);

    /// <summary>
    /// 从队链表后面插入一个新的数据内存段
    /// </summary>
    private void AddBlock()
    {
        var tmp = Pool.Get();
        tmp.Reset();
        if (null == mListTail)
        {
            tmp.mPrev = null;
            mListTail = tmp;
        }
        else
        {
            mListTail.mNext = tmp;
            tmp.mPrev = mListTail;
        }
        mListTail = tmp;
        mListTail.mNext = null;
        mListTail.SetLength(0);//重置长度
        mListTail.Position = 0;//重置下标
        if (null == mListHead)
        {
            mListHead = mListTail;
        }
    }
    
    /// <summary>
    /// 从链表头移除一个数据段
    /// </summary>
    private void RemoveFront()
    {
        var tmp = mListHead;
        mListHead = mListHead.mNext;
        if (null != mListHead)
        {
            mListHead.mPrev = null;
        }
        else
        {
            mListTail = null;
        }
        tmp.mPrev = null;
        tmp.mNext = null;
        tmp.Reset();
        Pool.Put(tmp);
    }
    
    /// <summary>
    /// 从缓冲区读取数据
    /// </summary>
    /// <param name="data"></param>
    /// <param name="offset"></param>
    /// <param name="size"></param>
    private void Pop(byte[] data, int offset, int size)
    {
        if (!mMutex.WaitOne()) return;
        //如果所有的内存数据都已经空了，则不能再读取数据了
        if (null == mListHead)
        {
            mMutex.ReleaseMutex();
            return;
        }
        var n = mListHead.CheckRead(size);
        if (0 == n)
        {
            mListHead.Position = mFront;
            mListHead.Read(data, offset, size);
            mFront += size;
        }
        else
        {
            mListHead.Position = mFront;
            mListHead.Read(data, offset, size - n);
            RemoveFront();
            mFront = 0;
            mListHead.Position = mFront;
            mListHead.Read(data, offset + size - n, n);
            mFront += n;
        }
        BytesAvailable -= size;
        mMutex.ReleaseMutex();
    }

    /// <summary>
    /// 从缓冲区读取数据,非线程安全
    /// </summary>
    /// <param name="data"></param>
    /// <param name="offset"></param>
    /// <param name="size"></param>
    private void Read(byte[] data, int offset, int size)
    {
        //如果所有的内存数据都已经空了，则不能再读取数据了
        if (null == mListHead)
        {
            return;
        }
        var n = mListHead.CheckRead(size);
        if (0 == n)
        {
            mListHead.Position = mFront;
            mListHead.Read(data, offset, size);
            mFront += size;
        }
        else
        {
            mListHead.Position = mFront;
            mListHead.Read(data, offset, size - n);
            RemoveFront();
            mFront = 0;
            mListHead.Position = mFront;
            mListHead.Read(data, offset + size - n, n);
            mFront += n;
        }
        BytesAvailable -= size;
    }
    
    /// <summary>
    /// 将数据写入到StreamTool,线程安全
    /// </summary>
    /// <param name="ba"></param>
    /// <param name="n"></param>
    public void WriteToStreamTool(StreamTool st, int n)
    {
        var total = n;
        var batchN = mCachedBuffer.Length;
        while(total > 0)
        {
            var step = total > batchN ? batchN : total;
            Pop(mCachedBuffer, 0, step);
            st.Out.Write(mCachedBuffer, 0, step);
            total -= step;
        }
    }

    /// <summary>
    /// 打印码流的信息
    /// </summary>
    /// <param name="n"></param>
    /// <param name="sb"></param>
    public void GetRawInfo(int n,System.Text.StringBuilder sb)
    {
        if (!mMutex.WaitOne()) return;
        mListHead.GetRawString(n,mFront, sb);
        mMutex.ReleaseMutex();
    }//GetRawInfo

    /// <summary>
    /// 将数据写入到StreamTool,非线程安全
    /// </summary>
    /// <param name="ba"></param>
    /// <param name="n"></param>
    public void WriteToStreamToolWithoutLock(StreamTool st, int n)
    {
        var total = n;
        var batchN = sBuffer.Length;
        while (total > 0)
        {
            var step = total > batchN ? batchN : total;
            Read(sBuffer, 0, step);
            st.Out.Write(sBuffer, 0, step);
            total -= step;
        }//while
    }//WriteToStreamToolWithoutLock
    /// <summary>
    /// 读取一个整数
    /// </summary>
    /// <returns></returns>
    public int ReadInt()
    {
        Pop(sBuffer, 0, 4);
        var ret = BitConverter.ToInt32(sBuffer, 0);
        return ret;
    }

    /// <summary>
    /// 读取消息体的数据头，zone server的
    /// </summary>
    /// <param name="len"></param>
    /// <param name="cmd"></param>
    public void ReadHeadZone(ClientHeader head,out int len,out uint cmd)
    {
        if (mMutex.WaitOne())
        {
            Pop(mCachedBuffer, 0, ClientHeader.SIZE);//加上offset对应protocol_version和pass code
            head.Decode(mCachedBuffer);
            len = head.data_len;
            cmd = (uint)head.cmd;
            mMutex.ReleaseMutex();
        }
        else
        {
            len = 0;
            cmd = 0;
        }
    }
    
    /// <summary>
    /// 记录下可以从缓冲区读的字节数
    /// </summary>
    private int BytesAvailable{ get; set;}

    public bool IsLonger(int len)
    {
        var ret = false;
        if (!mMutex.WaitOne()) return ret;
        ret = BytesAvailable >= len;
        mMutex.ReleaseMutex();
        return ret;
    }

    public bool IsShorter(int len)
    {
        var ret = false;
        if (!mMutex.WaitOne()) return ret;
        ret = BytesAvailable < len;
        mMutex.ReleaseMutex();
        return ret;
    }

}