using network.Core;
using network.Data_Object;
using network.Utils;

namespace network.Event;

 public class FastNotifier
    {
        enum E_Operation
        {
            ADD,
            REMOVE
        }//E_Operation

        /// <summary>
        /// 存储了所有的事件和事件监听对象的映射表
        /// </summary>
        private XDictionary<string, XHashSet<IObserver>> mMap
            = new XDictionary<string, XHashSet<IObserver>>();

        #region 添加和移除的缓存列表
        /// <summary>
        /// 操作队列
        /// </summary>
        private List<E_Operation> mOperationList = new List<E_Operation>();
        /// <summary>
        /// 操作对象队列
        /// </summary>
        private List<IObserver> mObjectList = new List<IObserver>();
        /// <summary>
        /// 消息类型的队列
        /// </summary>
        private List<string> mMsgTypeList = new List<string>();
        #endregion
        /// <summary>
        /// 清除所有的对象
        /// </summary>
        public void Clear()
        {
            mAllocator.ReleaseItems(mMap);
            mMap.Clear();
            mOperationList.Clear();
            mObjectList.Clear();
            mMsgTypeList.Clear();
            mAllocator.ReleaseItems(mReflection);
            mReflection.Clear();
        }//Clear

        private Allocator mAllocator = new Allocator();

        /// <summary>
        /// 定制的Allocator
        /// </summary>
        private class Allocator
        {
            private List<XHashSet<IObserver>> mL1 = new List<XHashSet<IObserver>>();
            public XHashSet<IObserver> GetObserverSet()
            {
                XHashSet<IObserver> ret = null;
                var n = mL1.Count - 1;
                if(n>=0)
                {
                    ret = mL1[n];
                    mL1.RemoveAt(n);
                }
                else
                {
                    return new XHashSet<IObserver>();
                }
                return ret;
            }//GetObserverSet

            private List<XHashSet<string>> mL2 = new List<XHashSet<string>>();

            public XHashSet<string> GetStringSet()
            {
                XHashSet<string> ret = null;
                var n = mL2.Count - 1;
                if (n >= 0)
                {
                    ret = mL2[n];
                    mL2.RemoveAt(n);
                }
                else
                {
                    return new XHashSet<string>();
                }
                return ret;
            }


            public void ReleaseItems(XDictionary<string, XHashSet<IObserver>> M)
            {
                var L = M.GetList();
                for(int i = 0,n=L.Count;i<n;++i)
                {
                    var it = L[i].Value;
                    it.Clear();
                    mL1.Add(it);
                }//for
            }//ReleaseItems

            public void ReleaseItems(XDictionary<IObserver, XHashSet<string>> M)
            {
                var L = M.GetList();
                for (int i = 0, n = L.Count; i < n; ++i)
                {
                    var it = L[i].Value;
                    it.Clear();
                    mL2.Add(it);
                }//for
            }//ReleaseItems

        }//Allocator

        /// <summary>
        /// 为了避免装箱强制约束只能传递非值类型的参数
        /// 如果要传递值类型的数据，请自定义一个Cache对象
        /// 并且声明为全局可取，派发事件之前存在那个地方
        /// 相应的对象从对应的地方取
        /// 派发完以后消除引用
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="msgType"></param>
        /// <param name="param"></param>
        public void DispatchCmd<T>(string msgType,T param)
        {
            if (mDirty)
            {
                Flush();
            }

            if(mMap.TryGetValue(msgType,out var HS))
            {
                var L = HS.GetList();
                var e = new EventArgumentsF(param, this, msgType);
                foreach (var it in L)
                {
                    it.OnMessage(ref e);
                }
            }

            if (mDirty)
            {
                Flush();
            }//if
        }//DispatchCmd
        /// <summary>
        /// 无参数的事件调用
        /// </summary>
        /// <param name="msgType"></param>
        public void DispatchCmd(string msgType)
        {
            if (mDirty)
            {
                Flush();
            }//if

            if (mMap.TryGetValue(msgType, out var HS))
            {
                var L = HS.GetList();
                var e = new EventArgumentsF(null, this, msgType);
                foreach (var it in L)
                {
                    it.OnMessage(ref e);
                }
            }//if

            if (mDirty)
            {
                Flush();
            }//if
        }

        /// <summary>
        /// 派发事件，提供了非装箱的重载
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        public void DispatchCmdInt(string msgType, int data)
        {
            var i = mIntValPool.Get();
            i.Value = data;
            DispatchCmd(msgType, i);
            mIntValPool.Put(i);
        }
        private XPool<IntValue> mIntValPool = new XPool<IntValue>();

        public void DispatchCmdFixed(string msgType, LFixed data)
        {
            var f = mFixedValPool.Get();
            f.value = data;
            DispatchCmd(msgType, f);
            mFixedValPool.Put(f);
        }

        private XPool<FixedValue> mFixedValPool = new XPool<FixedValue>();

        /// <summary>
        /// 可以找到某个对象监听了哪些事件，以方便做清理移除
        /// 
        /// </summary>
        private XDictionary<IObserver, XHashSet<string>> mReflection
             = new XDictionary<IObserver, XHashSet<string>>();
        /// <summary>
        /// 监听事件
        /// </summary>
        /// <param name="msgType"></param>
        /// <param name="observer"></param>
        public void Regist(string msgType,IObserver observer)
        {
            mDirty = true;
            mObjectList.Add(observer);
            mOperationList.Add(E_Operation.ADD);
            mMsgTypeList.Add(msgType);
        }//Regist

        /// <summary>
        /// 移除事件
        /// </summary>
        /// <param name="msgType"></param>
        /// <param name="observer"></param>
        public void Remove(string msgType,IObserver observer)
        {
            mDirty = true;
            mObjectList.Add(observer);
            mOperationList.Add(E_Operation.REMOVE);
            mMsgTypeList.Add(msgType);
        }//Remove

        /// <summary>
        /// 标记当前是否添加或者移除过元素
        /// </summary>
        private bool mDirty;

        /// <summary>
        /// 移除某个对象监听过的所有的事件
        /// 通常用于模块的结束的时候的处理
        /// </summary>
        /// <param name="observer"></param>
        public void RemoveTarget(IObserver observer)
        {
            if (!mReflection.TryGetValue(observer, out var strSet)) return;
            var L = strSet.GetList();
            for(int i=0,n=L.Count;i<n;++i)
            {
                Remove(L[i], observer);
            }//for
        }//RemoveTarget

        /// <summary>
        /// 将添加和移除的事件加入处理，以为是队列的形式存在，因此可以保证逻辑上的一致
        /// </summary>
        private void Flush()
        {
            mDirty = false;
            var N = mOperationList.Count;
            for(var i = 0;i < N; ++i)
            {
                var msg = mMsgTypeList[i];
                var op = mOperationList[i];
                var handler = mObjectList[i];
                switch (op)
                {
                    case E_Operation.ADD:
                    {
                        #region 添加处理逻辑
                        //标记这个observer添加了哪些事件
                        if(!mReflection.TryGetValue(handler,out var strSet))
                        {
                            strSet = mAllocator.GetStringSet(); //new XHashSet<string>();
                            mReflection.Add(handler, strSet);
                        }
                        if(strSet.Contains(msg))
                        {
                            //如果已经监听过这个事件的情况，continue
                            continue;
                        }
                        strSet.Add(msg);

                        if(!mMap.TryGetValue(msg,out var HS))
                        {
                            HS = mAllocator.GetObserverSet();//new XHashSet<IObserver>();
                            mMap.Add(msg, HS);
                        }//if
                        if(!HS.Contains(handler))
                        {
                            HS.Add(handler);
                        }//if
                        //if--add

                        #endregion

                        break;
                    }
                    case E_Operation.REMOVE:
                    {
                        #region 删除处理逻辑
                        var strSet = default(XHashSet<string>);
                        if(!mReflection.TryGetValue(handler,out strSet))
                        {
                            //如果这里不包含了一定是没有监听过事件
                            continue;
                        }//if

                        if(!strSet.Contains(msg))
                        {
                            //如果没有监听过这个事件
                            continue;
                        }//if

                        strSet.Remove(msg);

                        if(mMap.TryGetValue(msg,out var observerSet))
                        {
                            if(observerSet.Contains(handler))
                            {
                                observerSet.Remove(handler);
                            }//if
                        }//if
                        //else -remove

                        #endregion

                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }//for i-N

            //清理掉缓存队列
            mOperationList.Clear();
            mObjectList.Clear();
            mMsgTypeList.Clear();
        }//Flush
    }//FastNotifier
