namespace network.Utils;

[Serializable]
public class XList<T> : List<T>,IEnumerable<T>
{
    public XList() { }
    public XList(IEnumerable<T> collection) : base(collection) { }
    public XList(int capacity) : base(capacity) { }
    /// <summary>
    /// 获取迭代器
    /// </summary>
    /// <returns></returns>
    new public IEnumerator<T> GetEnumerator()
    {
        var ret = ListEnumerator<T>.sPool.Get();
        ret.SetList(this);
        return ret;
    }
}