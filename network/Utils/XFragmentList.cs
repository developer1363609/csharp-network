namespace network.Utils;

public class XFragmentList<T> where T : class
    {

        public T[] mImpl;
        /// <summary>
        /// 构造函数
        /// </summary>
        public XFragmentList()
        {
            mImpl = new T[4];
            Capacity = 4;
        }//XFragmentList

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="capacity"></param>
        public XFragmentList(int capacity)
        {
            Capacity = capacity;
            mImpl = new T[capacity];
        }//XFragmentList

        /// <summary>
        /// 清除掉全部的元素
        /// </summary>
        public void Clear()
        {
            for(int i = 0,n=Count;i<n;++i)
            {
                mImpl[i] = null;
            }
            Count = 0;
            DeleteCount = 0;
        }//Clear
#if dev
        public int DeleteCount{get;private set;}
#else
        /// <summary>
        /// 删除的元素的个数
        /// </summary>
        public int DeleteCount;
#endif

        /// <summary>
        /// 删除某个位置的元素,O(1)
        /// </summary>
        /// <param name="i"></param>
        public void RemoveAt(int i)
        {
            mImpl[i] = null;
            ++DeleteCount;
        }
        /// <summary>
        /// 删除某个元素,O(N)
        /// </summary>
        /// <param name="item"></param>
        public void Remove(T item)
        {
            if (item == null)
                return;

            for(int i=0,n=Count;i<n;++i)
            {
                if(item.Equals(mImpl[i]))
                {
                    mImpl[i] = null;
                    ++DeleteCount;
                }
            }//for
        }//Remove
        /// <summary>
        /// 列表里面是否包含元素
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(T item)
        {
            if (item == null)
                return false;

            for (int i = 0, n = Count; i < n; ++i)
            {
                if (item.Equals(mImpl[i]))
                {
                    return true;
                }
            }//for
            return false;
        }

        /// <summary>
        /// 删除所有为Null的元素,最优和最差的情况时间复杂度都是O(N)
        /// </summary>
        public void Flush()
        {
            if (DeleteCount <= 0) { return; }
            int slow = 0;
            int fast = 0;
            int N = 0;
            bool cond = true;
            for(int i=0,n=Count;i<n;)
            {
                cond = true;
                if(mImpl[i] == null)
                {
                    slow = i;
                    for(fast = slow+1;true;++fast)
                    {
                        if(fast >= Count)
                        {
                            cond = false;
                            break;
                        }
                        if(mImpl[fast] != null)
                        {
                            mImpl[slow] = mImpl[fast];
                            mImpl[fast] = null;
                            ++i;
                            break;
                        }
                    }//for fast
                }//if
                else
                {
                    ++i;
                }
                if (!cond)
                {
                    break;
                }
            }//for i
            for(int i=0;i<Count;++i)
            {
                if(null != mImpl[i])
                {
                    ++N;
                }
            }
            Count = N;
            DeleteCount = 0;
        }//Flush

        /// <summary>
        /// 取值
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public T this[int i]
        {
            get
            {
                return mImpl[i]; ;
            }//get
        }//[]
        /// <summary>
        /// 在数组末尾添加元素，O(1)
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            if(Count >=Capacity)
            {
                MakeCapacity(Count + 1);
            }
            mImpl[Count] = item;
            ++Count;
        }//Add
#if dev
        public int Count{get;private set;}
#else
        /// <summary>
        /// 当前最大的元素个数，中间可能为null
        /// </summary>
        public int Count;
#endif

        public T First
        {
            get
            {
                for(int i=0;i<Count;++i)
                {
                    var it = mImpl[i];
                    if (null == it) { continue; }
                    return it;
                }
                return null;
            }
        }

#if dev
        public int Capacity{get;private set;}
#else
        /// <summary>
        /// 当前的最大容量
        /// </summary>
        public int Capacity;
#endif
        /// <summary>
        /// 创建足够的数据容量
        /// </summary>
        /// <param name="cap"></param>
        private void MakeCapacity(int cap)
        {
            if(cap>Capacity)
            {
                while (Capacity < cap)
                {
                    Capacity += Capacity;
                }
                System.Array.Resize(ref mImpl, Capacity);
            }//if
        }//MakeCapacity



    }