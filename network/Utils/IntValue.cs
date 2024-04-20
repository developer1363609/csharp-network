namespace network.Utils;

public class IntValue : IEquatable<IntValue>
{

    public struct Cmp : IEqualityComparer<IntValue>
    {
        public bool Equals(IntValue? x, IntValue? y)
        {
            return y != null && x != null && x.Value == y.Value;
        }

        public int GetHashCode(IntValue obj)
        {
            return obj.Value;
        }
    }
    /// <summary>
    /// 构造字典的时候必须传递这个匹配器
    /// </summary>
    public static readonly Cmp DefaultCmp = new Cmp();
    /// <summary>
    /// 数值
    /// </summary>
    public int Value;
    /// <summary>
    /// 接口方法
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(IntValue? other)
    {
        return other != null && this.Value == other.Value;
    }
    /// <summary>
    /// 将数据转化成int
    /// </summary>
    /// <param name="i"></param>
    public static implicit operator int(IntValue i)
    {
        return i.Value;
    }

}