using System.Collections;

namespace network.Utils;

[Serializable]
    public class XDictionary<K, V> : IEnumerable<KeyValuePair<K, V>>
    {
        /// <summary>
        /// 字典
        /// </summary>
        private Dictionary<K, int> mMap;
        /// <summary>
        /// 顺序表
        /// </summary>
        private XList<KeyValuePair<K, V>> mList;

        public int Count
        {
            get
            {
                return mList.Count;
            }
        }

        public XList<KeyValuePair<K, V>> GetList()
        {
            return mList;
        }

        public XDictionary(int capacity)
        {
            mMap = new Dictionary<K, int>(capacity);
            mList = new XList<KeyValuePair<K, V>>(capacity);
        }

        public XDictionary(IEqualityComparer<K> comparer)
        {
            mMap = new Dictionary<K, int>(comparer);
            mList = new XList<KeyValuePair<K, V>>();
        }

        public XDictionary(int capacity,
            IEqualityComparer<K> comparer)
        {
            mMap = new Dictionary<K, int>(capacity, comparer);
            mList = new XList<KeyValuePair<K, V>>(capacity);
        }
        public XDictionary()
        {
            mMap = new Dictionary<K, int>();
            mList = new XList<KeyValuePair<K, V>>();
        }

        /// <summary>
        /// 添加，时间复杂度O(1)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public void Add(K key, V val)
        {
            if (!mMap.ContainsKey(key))
            {
                mList.Add(new KeyValuePair<K, V>(key, val));
                mMap.Add(key, mList.Count - 1);
            }//if
            else
            {
                var i = mMap[key];
                mList[i] = new KeyValuePair<K, V>(key, val);
            }
        }//Add
        /// <summary>
        /// 移除，时间复杂度O(1)
        /// </summary>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public void Remove(K key)
        {
            if (mMap.ContainsKey(key))
            {
                var i = mMap[key];
                var n = mList.Count;
                if (n > 1)
                {
                    var last = n - 1;
                    var lastItem = mList[last];
                    mList[i] = lastItem;
                    mList.RemoveAt(n - 1);//remove last
                    mMap[lastItem.Key] = i;
                    mMap.Remove(key);
                }//if
                else
                {
                    mList.RemoveAt(i);
                    mMap.Remove(key);
                }//else
            }//if
        }//Remove
        /// <summary>
        /// 存取器
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public V this[K key]
        {
            get
            {
                var i = 0;
                if (mMap.TryGetValue(key, out i))
                {
                    return mList[i].Value;
                }
                return default(V);
            }
            set
            {
                var i = 0;
                if (mMap.TryGetValue(key, out i))
                {
                    mList[i] = new KeyValuePair<K, V>(key, value);
                }
                else
                {
                    Add(key, value);
                }
            }
        }//[]

        public bool ContainsKey(K key)
        {
            return mMap.ContainsKey(key);
        }
        /// <summary>
        /// 原则上不允许随意用这个接口，开销比较大
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool ContainsValue(V value)
        {
            var ret = false;
            for (int i = 0, n = mList.Count; i < n; ++i)
            {
                if (value.Equals(mList[i].Value))
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }
        /// <summary>
        /// 取值
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(K key, out V value)
        {
            var ret = false;
            value = default(V);
            var i = 0;
            ret = mMap.TryGetValue(key, out i);
            if (ret)
            {
                value = mList[i].Value;
            }//if
            return ret;
        }//TryGetValue
        /// <summary>
        /// 清除所有的元素
        /// </summary>
        public void Clear()
        {
            mMap.Clear();
            mList.Clear();
        }//Clear

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            return mList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }