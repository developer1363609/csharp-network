namespace network.Utils;

public class LongValue : IEquatable<LongValue>
{

    public struct Cmp : IEqualityComparer<LongValue>
    {
        public bool Equals(LongValue? x, LongValue? y)
        {
            return y != null && x != null && x.Value == y.Value;
        }

        public int GetHashCode(LongValue obj)
        {
            return (int)obj.Value;
        }
    }
    /// <summary>
    /// 构造字典的时候必须传递这个匹配器
    /// </summary>
    public static readonly Cmp DefaultCmp = new Cmp();
    /// <summary>
    /// 数值
    /// </summary>
    public  long Value;
    /// <summary>
    /// 接口方法
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(LongValue? other)
    {
        return other != null && this.Value == other.Value;
    }
    /// <summary>
    /// 将数据转化成int
    /// </summary>
    /// <param name="i"></param>
    public static implicit operator long(LongValue i)
    {
        return i.Value;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as LongValue);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}