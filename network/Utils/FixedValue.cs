using network.Utils;

namespace network.Data_Object;

public class FixedValue : IEquatable<FixedValue>
{
    public struct Cmp : IEqualityComparer<FixedValue>
    {
        public bool Equals(FixedValue x, FixedValue y)
        {
            return x.value == y.value;
        }

        public int GetHashCode(FixedValue obj)
        {
            return obj.value;
        }
    }

    /// <summary>
    /// 构造字典的时候必须传递这个匹配器
    /// </summary>
    public static readonly Cmp DefaultCmp = new Cmp();
    /// <summary>
    /// 数值
    /// </summary>
    public LFixed value;
    /// <summary>
    /// 接口方法
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(FixedValue other)
    {
        return this.value == other.value;
    }
    /// <summary>
    /// 将数据转化成int
    /// </summary>
    /// <param name="i"></param>
    public static implicit operator LFixed(FixedValue i)
    {
        return i.value;
    }

}