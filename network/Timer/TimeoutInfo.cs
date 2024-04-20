namespace network.Utils;

public class TimeoutInfo
    {
#if dev
        /// <summary>
        /// 不允许XTimer以外的地方修改!!!
        /// </summary>
        public int Seed { get; private set; }
#else
        public int Seed ;
#endif
        public void SetSeed(int seed)
        {
            Seed = seed;
        }//SetSeed


        public XTimer mTimer;
        
        public float mElapsedTime { get; private set; }

        public void SetElapsedTime(float val)
        {
            mElapsedTime = val;
        }
        /// <summary>
        /// 总共的时间 
        /// </summary>
        public float mTotalTime { get; private set; }

        public void SetTotalTime(float t)
        {
            mTotalTime = t;
        }
        /// <summary>
        /// 被回调的方法
        /// </summary>
        public Action<TimeoutInfo> mCallback { get; private set; }

        public void SetCallback(Action<TimeoutInfo> fn)
        {
            mCallback = fn;
        }
        /// <summary>
        /// 可以寄存一个整数
        /// </summary>
        public int intValue { get; set; }
        /// <summary>
        /// 可以寄存一个浮点数
        /// </summary>
        public double doubleValue { get; set; }
        /// <summary>
        /// 可以寄存一个对象
        /// </summary>
        public object objValue { get; set; }
        /// <summary>
        /// 是否取消计时器
        /// </summary>
        public bool cancel { get; private set; }
        /// <summary>
        /// 取消计时器，不会被回调
        /// </summary>
        public TimeoutInfo Cancel()
        {
            cancel = true;
            return null;
        }

        /// <summary>
        /// 停止计时器，并重新开始
        /// </summary>
        public void ReStart()
        {
            Cancel();

            mTimer.SetTimeout(mCallback, mTotalTime);
        }

        /// <summary>
        /// 构造方法
        /// </summary>
        public TimeoutInfo()
        {

        }

        /// <summary>
        /// 强制完成，会被回调
        /// </summary>
        public void ForceFinished()
        {
            //mElapsedTime = mTotalTime;

            cancel = true;
            if (null != mCallback)
            {
                mCallback(this);
            }
            mCallback = null;
        }
        /// <summary>
        /// 重置数据
        /// </summary>
        private TimeoutInfo Reset()
        {
            //mElapsedTime = 0;
            mElapsedTime = 0;
            mTotalTime = 0;
            intValue = 0;
            doubleValue = 0;
            objValue = null;
            mCallback = null;
            cancel = false;
            return this;
        }

        public class Pool
        {
            private List<TimeoutInfo> mImpl = new List<TimeoutInfo>();

            private int mSeed = 1;

            private bool[] mSet = new bool[1024];

            public TimeoutInfo Get()
            {
                TimeoutInfo tmp = null;
                var n = mImpl.Count;
                if(n>0)
                {
                    tmp = mImpl[n - 1];
                    mImpl.RemoveAt(n - 1);
                }
                else
                {
                    tmp = new TimeoutInfo();
                }
                tmp.SetSeed(mSeed++);
                return tmp;
            }
            public void Put(TimeoutInfo ti)
            {
                //找个时间统一改一下
                return;
                var seed = ti.Seed;
                if(seed >= mSet.Length)
                {
                    Array.Resize(ref mSet, mSet.Length * 2);
                    mSet[seed] = true;
                }
                else
                {
                    if (mSet[seed])
                    {
                        return;
                    }
                    else
                    {
                        mSet[seed] = true;
                    }
                }
                ti.Reset();
                mImpl.Add(ti);
            }//Put
        }//Pool

    }