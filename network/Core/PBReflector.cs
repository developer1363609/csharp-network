using System.Diagnostics;
using ProtoBuf;

namespace network.Core
{
    public static class PBReflectorZone
    {
        /// <summary>
        /// 用来构造对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class Maker<T> where T : IExtensible ,new()
        {
            /// <summary>
            /// 构造这个类的对象
            /// </summary>
            /// <returns></returns>
            public IExtensible Invoke()
            {
                return new T();
            }
        }
        
        /// <summary>
        /// 构造器的信息
        /// </summary>
        public class Creator
        {
            // 记录类型信息
            public Type mType;
            
            // 类名称
            public string mName;
            
            // 完整的类名
            public string mFullName;
            
            // 消息号名称
            public uint mCmd;
            
            // 构造器
            public Func<IExtensible> mMaker;
            
            /// <summary>
            /// 尽量使用这个方法构建对象而不是使用new
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="cmd"></param>
            /// <returns></returns>
            public static Creator Make<T>(uint cmd) where T : IExtensible, new()
            {
                var ret = new Creator();
                ret.mType = typeof(T);
                ret.mName = ret.mType.Name;
                ret.mFullName = ret.mType.FullName;
                ret.mCmd = cmd;
                ret.mMaker = new Maker<T>().Invoke;
                return ret;
            }
        }
        
        //end Creator
        /// <summary>
        /// 根据协议号对应构造信息的映射表，以避免反射
        /// </summary>
        public static readonly Dictionary<uint, Creator> mMap
            = new Dictionary<uint, Creator>(128);
        
        /// <summary>
        /// 注册对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd"></param>
        public static void Regist<T>(uint cmd) where T : IExtensible,new()
        {
            if(! mMap.ContainsKey(cmd))
            {
                var creator = Creator.Make<T>(cmd);
                mMap.Add(cmd, creator);
            }
        }
        
        /// <summary>
        /// 查看某个消息号是否已经被注册过
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static bool Contains(uint cmd)
        {
            return mMap.ContainsKey(cmd);
        }
        
        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static IExtensible? MakeInstance(uint cmd,Stream stream)
        {
            IExtensible? ret = null;
            do
            {
                Creator creator = null;
                if(!mMap.TryGetValue(cmd,out creator))
                {
                    break;
                }
                try
                {
                    var obj = creator.mMaker();
                    stream.Position = 0;
                    ret = NetworkManager.sSerializer.Deserialize(
                        stream, obj, creator.mType) as IExtensible;
                    stream.Position = 0;
                }
                catch(Exception e)
                {
                    Console.Error.WriteLine(e.ToString());
                }
            } while (false);
            return ret;
        }
    }

    /// <summary>
    /// protobuf的
    /// </summary>
    public static class PBReflectorMatch
    {
        /// <summary>
        /// 用来构造对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public class Maker<T> where T : IExtensible, new()
        {
            /// <summary>
            /// 构造这个类的对象
            /// </summary>
            /// <returns></returns>
            public IExtensible Invoke()
            {
                return new T();
            }
        }
        /// <summary>
        /// 构造器的信息
        /// </summary>
        public class Creator
        {
            // 记录类型信息
            public Type mType;
            
            // 类名称
            public string mName;
            
            // 完整的类名
            public string mFullName;
            
            // 消息号名称
            public uint mCmd;
            
            /// <summary>
            /// 构造器
            /// </summary>
            public Func<IExtensible> mMaker;
            /// <summary>
            /// 尽量使用这个方法构建对象而不是使用new
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="cmd"></param>
            /// <returns></returns>
            public static Creator Make<T>(uint cmd) where T : IExtensible, new()
            {
                var ret = new Creator();
                ret.mType = typeof(T);
                ret.mName = ret.mType.Name;
                ret.mFullName = ret.mType.FullName;
                ret.mCmd = cmd;
                ret.mMaker = new Maker<T>().Invoke;
                return ret;
            }
        }//end Creator
        /// <summary>
        /// 根据协议号对应构造信息的映射表，以避免反射
        /// </summary>
        public static readonly Dictionary<uint, Creator> mMap
            = new Dictionary<uint, Creator>(128);
        /// <summary>
        /// 注册对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd"></param>
        public static void Regist<T>(uint cmd) where T : IExtensible, new()
        {
            if (!mMap.ContainsKey(cmd))
            {
                var creator = Creator.Make<T>(cmd);
                mMap.Add(cmd, creator);
            }
        }
        /// <summary>
        /// 查看某个消息号是否已经被注册过
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static bool Contains(uint cmd)
        {
            return mMap.ContainsKey(cmd);
        }
        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static IExtensible MakeInstance(uint cmd, Stream stream)
        {
            IExtensible ret = null;
            do
            {
                Creator creator = null;
                if (!mMap.TryGetValue(cmd, out creator))
                {
                    break;
                }
                try
                {
                    var obj = creator.mMaker();
                    stream.Position = 0;
                    ret = NetworkManager.sSerializer.Deserialize(
                        stream, obj, creator.mType) as IExtensible;
                    stream.Position = 0;
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine("cmd:"+ cmd+e);
                }
            } while (false);
            return ret;
        }
    }

   
    public static class PBReflectorReplay
    {
        public class Maker<T> where T : IExtensible, new()
        {
            public IExtensible Invoke()
            {
                return new T();
            }
        }

        public class Creator
        {
            /// <summary>
            /// 记录类型信息
            /// </summary>
            public Type mType;

            public string mName;
            /// <summary>
            /// 完整的类名
            /// </summary>
            public string mFullName;
            /// <summary>
            /// 消息号名称
            /// </summary>
            public uint mCmd;
            /// <summary>
            /// 构造器
            /// </summary>
            public Func<IExtensible> mMaker;
            /// <summary>
            /// 尽量使用这个方法构建对象而不是使用new
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="cmd"></param>
            /// <returns></returns>
            public static Creator Make<T>(uint cmd) where T : IExtensible, new()
            {
                var ret = new Creator
                {
                    mType = typeof(T)
                };
                ret.mName = ret.mType.Name;
                Debug.Assert(ret.mType.FullName != null, "ret.mType.FullName != null");
                ret.mFullName = ret.mType.FullName;
                ret.mCmd = cmd;
                ret.mMaker = new Maker<T>().Invoke;
                return ret;
            }
        }
        
        //end Creator
        /// <summary>
        /// 根据协议号对应构造信息的映射表，以避免反射
        /// </summary>
        public static readonly Dictionary<uint, Creator> mMap
            = new Dictionary<uint, Creator>(128);
        
        /// <summary>
        /// 注册对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd"></param>
        public static void Regist<T>(uint cmd) where T : IExtensible, new()
        {
            if (!mMap.ContainsKey(cmd))
            {
                var creator = Creator.Make<T>(cmd);
                mMap.Add(cmd, creator);
            }
        }
       
        /// <summary>
        /// 查看某个消息号是否已经被注册过
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static bool Contains(uint cmd)
        {
            return mMap.ContainsKey(cmd);
        }
        
        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static IExtensible MakeInstance(uint cmd, Stream stream)
        {
            IExtensible ret = null;
            do
            {
                Creator creator = null;
                if (!mMap.TryGetValue(cmd, out creator))
                {
                    break;
                }
                try
                {
                    var obj = creator.mMaker();
                    stream.Position = 0;
                    ret = NetworkManager.sSerializer.Deserialize(
                        stream, obj, creator.mType) as IExtensible;
                    stream.Position = 0;
                }
                catch (Exception e)
                {
                    Console.Error.Write(e.ToString());
                }
            } while (false);
            return ret;
        }
    }
}


