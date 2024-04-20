using System.Collections;

namespace network.Utils;

    /// <summary>
    /// 对象池
    /// </summary>
    public class XPool<T> : IEnumerable<T>, IDisposable,IPool
        where T : class, new()
    {
        /// <summary>
        /// 在AutoCleanup的时候是否自动被清理
        /// </summary>
        bool IPool.RemoveWhenAutoCleanup { get { return mRemoveWhenAutoCleanup; } }
        /// <summary>
        /// 设置在被清理的时候自动Cleanup
        /// </summary>
        IPool IPool.SetRemoveWhenAutoCleanup()
        {
            mRemoveWhenAutoCleanup = true;
            return this;
        }
        /// <summary>
        /// 设置成在AutoCleanUp的时候从PoolManager中移除以
        /// 消除强引用
        /// </summary>
        /// <returns></returns>
        public XPool<T> SetRemoveWhenAutoCleanup()
        {
            mRemoveWhenAutoCleanup = true;
            return this;
        }//SetRemoveWhenAutoCleanup

        /// <summary>
        /// 标记是否在AutoCleanup的时候从管理器里面移除
        /// </summary>
        private bool mRemoveWhenAutoCleanup;

        private XHashSet<T> mSet;
        /// <summary>
        /// 默认的初始化容量
        /// </summary>
        private const int DEFAULT_SIZE = 4;
        /// <summary>
        /// 存放数据的容器
        /// </summary>
        public List<T> mImpl;
        /// <summary>
        /// 标记是否已经销毁过了
        /// </summary>
        private bool mDisposed = false;

        ///// <summary>
        ///// 切换战斗场景的时候被自动清零
        ///// </summary>
        //void IPool.AutoCleanup()
        //{
        //    PoolManager.AddToAutoCleanUpList(this);
        //}
        ///// <summary>
        ///// 切换战斗场景的时候被自动清零
        ///// </summary>
        ///// <returns></returns>
        //public XPool<T> AutoCleanup()
        //{
        //    PoolManager.AddToAutoCleanUpList(this);
        //    return this;
        //}

        /// <summary>
        /// 销毁资源
        /// </summary>
        public void Dispose()
        {
            if (!mDisposed)
            {
                mDisposed = true;
            }
            Clear();
            mCreator = null;//释放资源
            mPutHandler = null;//释放强引用
            mGetHandler = null;//释放强引用
        }//Dispose
        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="capacity"></param>
        public XPool(int capacity) 
        {
            Init(null, capacity);
        }
        /// <summary>
        /// 根据构造器来构造
        /// </summary>
        /// <param name="fnCreator"></param>
        public XPool(Func<T> fnCreator)
        {
            Init(fnCreator, DEFAULT_SIZE);
        }
        /// <summary>
        /// 不带任何参数的构造方法
        /// </summary>
        public XPool()
        {
            Init(null, DEFAULT_SIZE);
        }
        /// <summary>
        /// 初始化对象
        /// </summary>
        /// <param name="fnCreator"></param>
        /// <param name="capacity"></param>
        private void Init(Func<T> fnCreator, int capacity)
        {
            mImpl = new List<T>(capacity);
            mSet = new XHashSet<T>();
            mCreator = fnCreator;
            mIsIDisposable = typeof(T).IsAssignableFrom(typeof(IDisposable));
            mIsIReleaseable = typeof(T).IsAssignableFrom(typeof(IReleaseable));

//#if TRACKING_MAX
//            var t = typeof(T);
//            if(t.IsSubclassOf(typeof(BaseAction)))
//            {
//                mTrackingInfo = new PoolTrackingInfo(PoolTrackingInfo.E_Type.ACTION, t.Name);
//            }//if
//            else if(t==typeof(SkillEntity))
//            {
//                mTrackingInfo = new PoolTrackingInfo(PoolTrackingInfo.E_Type.SKILL_ENTITY, t.Name);
//            }//else if
//            else if(t==typeof(AudioObject))
//            {
//                mTrackingInfo = new PoolTrackingInfo(PoolTrackingInfo.E_Type.AUDIO, t.Name);
//            }//else
//#endif//TRACKING_MAX
        }

//#if TRACKING_MAX
//        PoolTrackingInfo mTrackingInfo;
//#endif//TRACKING_MAX


        /// <summary>
        /// 根据构造器和初始容量来构造对象
        /// </summary>
        /// <param name="fnCreator"></param>
        /// <param name="capacity"></param>
        public XPool(Func<T> fnCreator, int capacity)
        {
            Init(fnCreator, capacity);
        }
        /// <summary>f
        /// 用来构造对象的delegate
        /// </summary>
        public Func<T> mCreator = null;
        /// <summary>
        /// 获取的时候的处理器
        /// </summary>
        public Action<T> mGetHandler = null;
        /// <summary>
        /// 放回去的时候的处理器
        /// </summary>
        public Action<T> mPutHandler = null;
        /// <summary>
        /// 类型是否为IReleaseable
        /// </summary>
        private bool mIsIReleaseable = false;
        /// <summary>
        /// 类型是否为IDisposable
        /// </summary>
        private bool mIsIDisposable = false;
        /// <summary>
        /// 清除全部的缓存对象
        /// </summary>
        public void Clear()
        {
            mSet.Clear();
            if(mIsIReleaseable)
            {
                for(int i=0,n=mImpl.Count;i<n;++i)
                {
                    var it = mImpl[i];
                    var r = it as IReleaseable;
                    if (null != r)
                    {
                        r.Release();
                    }//if
                }
            }//
            else if(mIsIDisposable)
            {
                for (int i = 0, n = mImpl.Count; i < n; ++i)
                {
                    var it = mImpl[i];
                    var r = it as IDisposable;
                    if (null != r)
                    {
                        r.Dispose();
                    }//if
                }
            }//else
            mImpl.Clear();
        }//Clear
        /// <summary>
        /// 只做引用清理工作
        /// </summary>
        public void ClearReffOnly()
        {
            if(null != mImpl)
            {
                mImpl.Clear();
            }
            if(null!= mSet)
            {
                mSet.Clear();
            }
        }//ClearReffOnly
        /// <summary>
        /// 迭代器
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return mImpl.GetEnumerator();
        }
        /// <summary>
        /// 迭代器
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return mImpl.GetEnumerator();
        }
        /// <summary>
        /// 从对象池中获取对象
        /// </summary>
        /// <returns></returns>
        public T Get()
        {
            T ret = default(T);
            if(0 == mImpl.Count)
            {
                if(null != mCreator)
                {
                    ret = mCreator();
                }
                else
                {
                    ret = new T();
                }
//#if TRACKING_MAX
//                if(null != mTrackingInfo)
//                {
//                    mTrackingInfo.Track(ret);
//                }
//#endif//TRACKING_MAX
            }
            else
            {
                var n = mImpl.Count - 1;
                ret = mImpl[n];
                mImpl.RemoveAt(n);
                if(ret != null && mSet.Contains(ret))
                {
                    mSet.Remove(ret);
                }
            }
            if(null != mGetHandler)
            {
                mGetHandler(ret);
            }
            return ret;
        }
        /// <summary>
        /// 将对象放回对象池
        /// </summary>
        /// <param name="item"></param>
        public void Put(T item)
        {
            if(mSet.Contains(item))
            {
                return;
            }
            mSet.Add(item);
            if(null != mPutHandler)
            {
                mPutHandler(item);
            }
            mImpl.Add(item);
        }

        /// <summary>
        /// 对泛化做兼容处理
        /// </summary>
        /// <param name="item"></param>
        public void PutX(object item)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            if(!(item is T))
            {
                Logger.LogError("type not matched! class:{0}",item.GetType().FullName);
            }
#endif//platform
            Put(item as T);
        }
        /// <summary>
        /// 对泛化做兼容处理
        /// </summary>
        /// <returns></returns>
        public object GetX()
        {
            return Get();
        }
        /// <summary>
        /// 创建若干个对象
        /// </summary>
        /// <param name="capacity"></param>
        public XPool<T> MakeReserve(int capacity)
        {
            capacity = capacity - mImpl.Count;
            for(var i = 0; i < capacity;++i)
            {
                T it = null; ;
                if(null == mCreator)
                {
                    it = new T();
                }
                else
                {
                    it = mCreator();
                }
                mImpl.Add(it);
            }
            return this;
        }

        void IPool.MakeReserveItems(int count)
        {
            MakeReserve(count);
        }//MakeReserve

    }
    