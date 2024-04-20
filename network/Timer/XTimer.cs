namespace network.Utils;

public class XTimer
    {
        /// <summary>
        /// 用来缩放时间
        /// </summary>
        public float TimeScale = 1f;

        private XFragmentList<Func<float, bool>> mUpdateItems
            = new XFragmentList<Func<float, bool>>();

        private XFragmentList<TimeoutInfo> mTimeoutlist
            = new XFragmentList<TimeoutInfo>();

        class Timeoutlist_Comparer : IComparer<double>
        {
            public static Timeoutlist_Comparer Get()
            {
                if (null == sIns)
                {
                    sIns = new Timeoutlist_Comparer();
                }
                return sIns;
            }

            private static Timeoutlist_Comparer sIns;

            private Timeoutlist_Comparer() { }
            public int Compare(double x, double y)
            {
                if (x < y)
                    return 1;
                else
                    return -1;
            }//Compare
        }//Timeoutlist_Comparer

        private TimeoutInfo.Pool mTimeoutInfoPool = new TimeoutInfo.Pool();

        /// <summary>
        /// 驱动计时器更新
        /// </summary>
        /// <param name="t"></param>
        public void Tick(float t)
        {
            var rt = t * TimeScale;
#region 更新Update列表

            var L = mUpdateItems;
            for (int i = 0; i < L.Count; ++i)
            {
                var fn = L[i];
                if (null == fn) { continue; }
                var finished = fn(rt);
                if (finished)
                {
                    mUpdateItems.RemoveAt(i);
                }
            }
            if(L.DeleteCount>10)
            {
                L.Flush();
            }
            
#endregion

#region 更新倒计时列表

            for (int i = 0;i< mTimeoutlist.Count; ++i)
            {
                var ti = mTimeoutlist[i];
                if(null == ti) { continue; }
                if (ti.cancel)
                {
                    mTimeoutlist.RemoveAt(i);
                    mTimeoutInfoPool.Put(ti);
                }
                else
                {
                    ti.SetElapsedTime(ti.mElapsedTime + rt);
                    if (ti.mElapsedTime >= ti.mTotalTime)
                    {
                        if (null != ti.mCallback)
                        {
                            ti.mCallback(ti);
                        }
                        mTimeoutlist.RemoveAt(i);
                        mTimeoutInfoPool.Put(ti);
                    }
                }
            }
            if(mTimeoutlist.DeleteCount>10)
            {
                mTimeoutlist.Flush();
            }
#endregion
        }

        private float mCachedTime;
        /// <summary>
        /// 添加更新处理方法
        /// </summary>
        /// <param name="fn"></param>
        public void AddUpdateHandler(Func<float, bool> fn)
        {
            mUpdateItems.Add(fn);
        }
        /// <summary>
        /// 移除更新处理方法
        /// </summary>
        /// <param name="fn"></param>
        public void RemoveUpdateHandler(Func<float, bool> fn)
        {
            mUpdateItems.Remove(fn);
        }

        public bool HaveUpdateHandler(Func<float, bool> fn)
        {
            return mUpdateItems.Contains(fn);
        }

        private void RemoveAllUpdateHander()
        {
            mUpdateItems.Clear();
        }


        private void RemoveAllTimeOutHadner()
        {
            mTimeoutlist.Clear();
        }

        public void RemoveAllHander()
        {
            RemoveAllUpdateHander();
            RemoveAllTimeOutHadner();
        }

        /// <summary>
        /// 最小的时间间隔
        /// </summary>
        readonly float MIN_DELTA = (1f / 60f);

        private void SetTimeout(ref TimeoutInfo ti, Action<TimeoutInfo> fn, float delay)
        {
            ti.mTimer = this;
            ti.SetTotalTime(delay);
            ti.SetCallback(fn);
            ti.SetElapsedTime(0F);
            if (delay <= MIN_DELTA)
            {
                fn(ti);
                mTimeoutInfoPool.Put(ti);
            }
            else
            {
                mTimeoutlist.Add(ti);
            }
        }

        /// <summary>
        /// 设置超时处理
        /// </summary>
        /// <param name="fn"></param>
        /// <param name="delay"></param>
        public TimeoutInfo SetTimeout(Action<TimeoutInfo> fn,
            float delay)
        {
            var ti = mTimeoutInfoPool.Get();
            SetTimeout(ref ti, fn, delay);
            return ti;
        }
        /// <summary>
        /// 设置超时处理
        /// </summary>
        /// <param name="fn"></param>
        /// <param name="delay"></param>
        public TimeoutInfo SetTimeoutObj(Action<TimeoutInfo> fn,
            float delay, object vo)
        {
            var ti = mTimeoutInfoPool.Get();
            ti.objValue = vo;
            SetTimeout(ref ti, fn, delay);
            return ti;
        }
        /// <summary>
        /// 设置超时处理
        /// </summary>
        /// <param name="fn"></param>
        /// <param name="delay"></param>
        /// <param name="vo"></param>
        public void SetTimeout(Action<TimeoutInfo> fn,
            float delay, int vo)
        {
            var ti = mTimeoutInfoPool.Get();
            ti.intValue = vo;
            SetTimeout(ref ti, fn, delay);
        }
        /// <summary>
        /// 设置超时处理
        /// </summary>
        /// <param name="fn"></param>
        /// <param name="delay"></param>
        /// <param name="vo"></param>
        public void SetTimeout(Action<TimeoutInfo> fn,
            float delay, float vo)
        {
            var ti = mTimeoutInfoPool.Get();
            ti.doubleValue = vo;
            SetTimeout(ref ti, fn, delay);
        }
        /// <summary>
        /// 设置超时处理
        /// </summary>
        /// <param name="fn"></param>
        /// <param name="delay"></param>
        /// <param name="vo"></param>
        public void SetTimeout(Action<TimeoutInfo> fn,
            float delay, object vo)
        {
            var ti = mTimeoutInfoPool.Get();
            ti.objValue = vo;
            SetTimeout(ref ti, fn, delay);
        }
        /// <summary>
        /// 移除倒计时，这个函数的，开销非常非常大，慎用
        /// </summary>
        /// <param name="fn"></param>
        public void RemoveTimeout(Action<TimeoutInfo> fn)
        {
            for(int i=0,n=mTimeoutlist.Count;i<n;++i)
            {
                var it = mTimeoutlist[i];
                if(null == it) { continue; }
                if(it.mCallback == fn)
                {
                    it.Cancel();
                }
            }//for

        }//RemoveTimeout
    }