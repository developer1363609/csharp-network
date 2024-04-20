#if DEBUG
#define dev
#define DEV
#else
#endif

using System.Runtime.InteropServices;
using System.Text;

namespace network.IO
{
    /// <summary>
    /// 数据流的读写工具
    /// </summary>
    public class StreamTool : IDisposable
    {
        public class XBinaryWriter : BinaryWriter
        {
            [StructLayout(LayoutKind.Explicit, Size = 8)]
            public struct Data
            {
                [FieldOffset(0)] private readonly float f;

                [FieldOffset(0)] private readonly int i;

                [FieldOffset(0)] private readonly double d;

                [FieldOffset(0)] private readonly long l;
            }//Data

            private Data tmp;

           
            public XBinaryWriter(Stream output, Encoding encoding):base(output,encoding)
            {

            }

            public XBinaryWriter(Stream output) : base(output, Encoding.UTF8)
            {

            }


            public override unsafe void Write(float value)
            {
                base.Write(*(int*)(&value));
            }

            public override unsafe void Write(double value)
            {
                base.Write(*(long*)(&value));
            }
        }//BinaryWritter

        public class XBinaryReader : System.IO.BinaryReader
        {
            private XBinaryWriter.Data tmp;

            public XBinaryReader(Stream input, Encoding encoding):base(input,encoding)
            {
            }

            public XBinaryReader(Stream input):this(input, Encoding.UTF8)
            {

            }

            public override unsafe float ReadSingle()
            {
                var i = base.ReadInt32();
                return *(float*)(&i);
            }

            public override unsafe double ReadDouble()
            {
                var l = base.ReadInt64();
                return *(double*)(&l);
            }
        }//BinaryReader

        /// <summary>
        /// 标记是否已经被销毁了
        /// </summary>
        private bool mDisposed;
        /// <summary>
        /// 查看是否已经销毁了
        /// </summary>
        public bool Disposed => mDisposed;

        /// <summary>
        /// 销毁游戏资源
        /// </summary>
        public void Dispose()
        {
            if (mDisposed) return;
            mDisposed = true;
            if(null != In)
            {
                In.Close();
                In = null;
            }
            if(null != Out)
            {
                Out.Close();
                Out = null;
            }

            if (null == Buffer) return;
            Buffer.Close();
            Buffer.Dispose();
            Buffer = null;
        }
        /// <summary>
        /// 清除数据
        /// </summary>
        public StreamTool Clear()
        {
            Buffer.SetLength(0);
            return this;
        }
        /// <summary>
        /// 保存数据
        /// </summary>
        /// <returns></returns>
        public StreamTool Flush()
        {
            Buffer.Flush();
            return this;
        }
       
        /// <summary>
        /// 读取多个字符串
        /// </summary>
        /// <param name="output"></param>
        /// <returns></returns>
        public void ReadStrings(List<string> output = null)
        {
            var reader = In;
            var n = reader.ReadInt32();
            for(var i = 0; i < n;++i)
            {
                var s = reader.ReadString();
                output.Add(s);
            }
        }
        /// <summary>
        /// 写入多个字符串
        /// </summary>
        /// <param name="list"></param>
        public void WriteStrings(List<String> list)
        {
            var writter = Out;
            writter.Write(list.Count);
            for(int i=0,n=list.Count;i<n;++i)
            {
                var s = list[i];
                writter.Write(s);
            }
        }
        /// <summary>
        /// 将后面size字节的数据拷贝到st中
        /// </summary>
        /// <param name="st"></param>
        /// <param name="size"></param>
        public void CopyTo(StreamTool st,int size,byte[] swap)
        {
            var total = size;
            var batchN = swap.Length;

            while(total > 0)
            {
                var step = total > batchN ? batchN : total;
                var read = In.Read(swap, 0, step);
                st.Out.Write(swap, 0, step);
                total -= step;
            }
        }

        /// <summary>
        /// 读写的位置
        /// </summary>
        public int Position
        {
            get
            {
                return (int)Buffer.Position;
            }
            set
            {
                Buffer.Position = value;
            }
        }
#if dev
        /// <summary>
        /// 读数据用的
        /// </summary>
        public XBinaryReader In { get; private set; }
        /// <summary>
        /// 写数据用的
        /// </summary>
        public XBinaryWriter Out { get; private set; }
        /// <summary>
        /// 数据缓冲区
        /// </summary>
        public Stream Buffer { get; private set; }
#else
                /// <summary>
        /// 读数据用的
        /// </summary>
        public BinaryReader In;
        /// <summary>
        /// 写数据用的
        /// </summary>
        public BinaryWriter Out;
        /// <summary>
        /// 数据缓冲区
        /// </summary>
        public Stream Buffer;
#endif
        /// <summary>
        /// 
        /// </summary>
        public StreamTool() : this(16) { }

        /// <summary>
        /// 根据空间大小来创建对象
        /// </summary>
        /// <param name="capacity"></param>
        public StreamTool(int capacity)
        {
            Buffer = new MemoryStream(capacity);
            Init(true, true);
        }
        /// <summary>
        /// 根据数组构造对象
        /// </summary>
        /// <param name="vec"></param>
        public StreamTool(byte[] vec)
        {
            Buffer = new MemoryStream(vec);
            Init(true, true);
        }
        /// <summary>
        /// 根据MemoryStream来构造对象
        /// </summary>
        /// <param name="stream"></param>
        public StreamTool(MemoryStream stream)
        {
            Buffer = stream;
            Init(true, true);
        }
        /// <summary>
        /// 根据文件流来构造对象
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="write"></param>
        public StreamTool(FileStream stream,bool write)
        {
            Buffer = stream;
            if(write)
            {
                Init(false, true);
            }
            else
            {
                Init(true, false);
            }
        }
        /// <summary>
        /// 根据文件路径来构造对象
        /// </summary>
        /// <param name="path"></param>
        /// <param name="write"></param>
        public StreamTool(string path,bool write)
        {
            if(write)
            {
                Buffer = new FileStream(path, 
                    FileMode.OpenOrCreate, FileAccess.ReadWrite);
                Init(true, true);
            }
            else
            {
                Buffer = File.OpenRead(path);
                Init(true, false);
            }
        }//StreamTool

        /// <summary>
        /// 避免忘记手动调用Dispose的时候出错
        /// </summary>
        ~StreamTool()
        {
            Dispose();
        }

        /// <summary>
        /// 初始化reader和writter
        /// </summary>
        /// <param name="readEnable"></param>
        /// <param name="writeEnable"></param>
        private void Init(bool readEnable,bool writeEnable)
        {
            if(readEnable)
            {
                In = new XBinaryReader(Buffer,Encoding.UTF8);
            }
            if(writeEnable)
            {
                Out = new XBinaryWriter(Buffer,Encoding.UTF8);
            }
        }

        /// <summary>
        /// 打开并且读取数据
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="cached"></param>
        /// <returns></returns>
        public static byte[] OpenRead(string filePath,byte[] cached = null)
        {
            if(!File.Exists(filePath))
            {
                Console.Write("File not found:{0}",filePath);
            }
            var stream = File.OpenRead(filePath);
            stream.Position = 0;
            int N = (int)stream.Length;
            if(null == cached)
            {
                cached = new byte[N];
            }
            else
            {
                if(cached.Length < N)
                {
                    Array.Resize(ref cached, N);
                }
            }
            stream.Read(cached, 0, N);
            stream.Close();
            stream.Dispose();
            return cached;
        }
        /// <summary>
        /// 打开文件并将数据写入文件中
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="date"></param>
        public static void OpenWrite(string filePath,byte[] data)
        {
            var stream = File.OpenWrite(filePath);
            stream.SetLength(0);//清除旧的数据
            stream.Write(data, 0, data.Length);
            stream.Flush();
            stream.Close();
            stream.Dispose();
        }

        public static void MakeSureFolderExist(string filePath)
        {
            var index = filePath.LastIndexOf("/");
            if (index >= 0)
            {
                var folder = filePath.Substring(0, index);
                if (Directory.Exists(folder) == false)
                {
                    Directory.CreateDirectory(folder);
                }
            }
        }
    }
}

