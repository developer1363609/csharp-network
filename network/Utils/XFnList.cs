namespace network.Utils;

public class XFnList<T> where T : class
    {
        /// <summary>
        /// 标记是否正在遍历
        /// </summary>
        private bool mLoop = false;
        /// <summary>
        /// 实现容器
        /// </summary>
        private XFragmentList<T> mImpl = new XFragmentList<T>();

        /// <summary>
        /// 在遍历的过程添加的元素
        /// </summary>
        private List<T> mQueue = new List<T>();
        /// <summary>
        /// 上下文
        /// </summary>
        public object Context { get; private set; }
        /// <summary>
        /// 添加元素
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            if(mLoop)
            {
                mQueue.Add(item);
            }
            else
            {
                mImpl.Add(item);
            }
        }//Add

        /// <summary>
        /// 删除元素
        /// </summary>
        /// <param name="item"></param>
        public void Remove(T item)
        {
            for(int i=0,n=mImpl.Count;i<n;++i)
            {
                var it = mImpl[i];
                if(null == it)
                {
                    continue;
                }
                if(it.Equals(item))
                {
                    mImpl.RemoveAt(i);
                }
            }//for
        }//Remove
        /// <summary>
        /// 遍历
        /// </summary>
        /// <param name="h"></param>
        public void ForEach(IIterateHandler h)
        {
            mLoop = true;
            for(int i=0,n=mImpl.Count;i<n;++i)
            {
                var it = mImpl[i];
                if (null == it) continue;
                h.OnIterate(it);
            }//for
            mLoop = false;
            if(mImpl.DeleteCount > 0)
            {
                mImpl.Flush();
            }//if
            if(mQueue.Count > 0)
            {
                for(int i=0,n=mQueue.Count;i<n;++i)
                {
                    mImpl.Add(mQueue[i]);
                }//for
                mQueue.Clear();
            }//if
        }//ForEach

        /// <summary>
        /// 遍历的时候的处理器
        /// </summary>
        public interface IIterateHandler
        {
            /// <summary>
            /// 遍历的时候的处理器
            /// </summary>
            /// <param name="item"></param>
            void OnIterate(T item);
        }//interface IIterateHandler

    }