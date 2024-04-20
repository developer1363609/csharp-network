namespace network.Utils;

public struct LFixed : IEquatable<LFixed>, IComparable<LFixed>
    {
        public static LFixed Parse(string s)
        {
            int dot = 0;
            int L = s.Length;
            int sign = 1;

            long number = 0;

            for (int i = 0; i < L; ++i)
            {
                char c = s[i];
                if (i == 0 && c == '-')
                {
                    sign = -1;
                    continue;
                }

                if (c >= '0' && c <= '9')
                {
                    number *= 10;
                    number += (c - '0');

                    if (number >= int.MaxValue)
                    {
#if DEBUG
                        Console.Error.WriteLine("out of range");
#endif
                        return LFixed.zero;
                    }
                }
                else if (c == '.')
                {
                    if (dot == 0)
                    {
                        dot = L - i - 1;
                    }
                    else
                    {
#if DEBUG
                        Console.Error.WriteLine("字符有多个小数点");
#endif
                        return LFixed.zero;
                    }
                }
                else
                {
#if DEBUG
                    Console.Error.WriteLine("无效字符类型");
#endif
                    return LFixed.zero;
                }
            }

            number *= sign;
            var rt = ((int)number).ToFix();
            for (int i = 0; i < dot; ++i)
            {
                rt /= 10;
            }
            return rt;
        }

        public const long LMaxValue = uint.MaxValue;//0xFFFFFFFFL;
        public const long LMinValue = -LMaxValue;//-0xFFFFFFFFL;

        #region 在Dictionary里面的比较器
        public struct CmpImpl : IEqualityComparer<LFixed>
        {
            public bool Equals(LFixed x, LFixed y)
            {
                return x.L == y.L;
            }
            public int GetHashCode(LFixed obj)
            {
                return (int)obj.L;
            }
        }

        public static readonly CmpImpl cmp = new CmpImpl();
        #endregion
        public static readonly LFixed minus = new LFixed().Init(-1);

        public static readonly LFixed one = new LFixed().Init(1);

        public static readonly LFixed zero = new LFixed();

        public static readonly LFixed two = new LFixed().Init(2);
        
        public static readonly LFixed half = 0.5.ToFix();
        
        public static readonly LFixed _01 = 0.1.ToFix();
        public static readonly LFixed _02 = 0.2.ToFix();
        public static readonly LFixed _03 = 0.3.ToFix();
        public static readonly LFixed _04 = 0.4.ToFix();
        public static readonly LFixed _05 = 0.5.ToFix();
        public static readonly LFixed _06 = 0.6.ToFix();
        public static readonly LFixed _07 = 0.7.ToFix();
        public static readonly LFixed _08 = 0.8.ToFix();
        public static readonly LFixed _09 = 0.9.ToFix();

        //千分之一用 value / LFixed._1000，而不是 value * LFixed._0001
        //百分之一用 value / LFixed._100，而不是 value * LFixed._001
        //因为float.ToFixed()有精度丢失，LFixed._0001并不是0.001，而LFixed._1000就是1000
        public static readonly LFixed _10000 = 10000.ToFix();
        public static readonly LFixed _1000 = 1000.ToFix();
        public static readonly LFixed _100 = 100.ToFix();
        public static readonly LFixed _10 = 10.ToFix();

        public static readonly LFixed _14 = 14.ToFix();

        /// <summary>
        /// 圆周度数
        /// </summary>
        public static readonly LFixed _Circumference = 360.ToFix();

        /// <summary>
        /// 允许的精度
        /// </summary>
        public static readonly LFixed kEpsilon = 0.0001.ToFix();

        public static readonly LFixed _15 = 1.5.ToFix();

        public static readonly LFixed maxValue = new LFixed().Init(int.MaxValue);

        public static readonly LFixed minValue = new LFixed().Init(int.MinValue);
        public static LFixed Max(LFixed a, LFixed b)
        {
            return a.L > b.L ? a : b;
        }

        public static LFixed Min(LFixed a, LFixed b)
        {
            return a.L <= b.L ? a : b;
        }

        private LFixed Init(int i)
        {
            long _L = i;
            _L <<= Fix_Fracbits;
            return new LFixed(ref _L);
        }
        /// <summary>
        /// 不小于它的最小整数
        /// </summary>
        public int ceil
        {
            get
            {
                int n = this;
                var f = n.ToFix();
                var delta = this - f;
                if (delta.L >= kEpsilon.L)
                {
                    return n + 1;
                }
                else
                {
                    return n;
                }
            }
        }
        /// <summary>
        /// 不大于它的最大整数
        /// </summary>
        public int floor
        {
            get
            {
                int n = this;
                ++n;
                var f = n.ToFix();
                var delta = f - this;
                if (delta.L >= kEpsilon.L)
                {
                    return n - 1;
                }
                else
                {
                    return n;
                }
            }
        }
        /// <summary>
        /// 4舍5入的计算
        /// </summary>
        public int round
        {
            get
            {
                int n = this;
                var f = n.ToFix();
                var delta = this - f;
                if (delta.L >= _05.L)
                {
                    return n + 1;
                }
                else
                {
                    return n;
                }
            }
        }

        public static LFixed Clamp(LFixed a, LFixed b, LFixed c)
        {
            if (a < b)
            {
                return b;
            }
            if (a > c)
            {
                return c;
            }
            return a;
        }
        public LFixed Abs
        {
            get
            {
                if (L > 0)
                {
                    return this;
                }
                else
                {
                    var l = -L;
                    return new LFixed(ref l);
                }
            }
        }
        /// <summary>
        /// 源生数据
        /// </summary>
        public long L;

        /// <summary>
        /// 半小数位
        /// </summary>
        public const int Fix_Fracbits_Half = Fix_Fracbits / 2;
        /// <summary>
        /// 小数位
        /// </summary>
        public const int Fix_Fracbits = 14;
        /// <summary>
        /// 缩放比例
        /// 2^Fix_Fracbits
        /// </summary>
        public const long POW = 1L << Fix_Fracbits;
        /// <summary>
        /// 缩放比例
        /// </summary>
        public const long POW_2 = POW * POW;
        /// <summary>
        /// 缩放比例
        /// </summary>
        public const long POW_3 = POW * POW * POW;

        /// <summary>
        /// 返回千分位四舍五入的浮点数
        /// </summary>
        /// <returns></returns>
        public float ToRoundFloat()
        {
            float f = this;
            f = (float)(Math.Round(f * 1000) / 1000);
            return f;
        }

        public override string ToString()
        {
            double d = this;
            return d.ToString("f5");
        }

        /// <summary>
        /// 把LFixed的数据Encode
        /// </summary>
        /// <returns></returns>
        public string Encode()
        {
            return L.ToString();
        }

        /// <summary>
        /// 把Encode后的数据转换为LFixed
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static LFixed Decode(string str)
        {
            LFixed lf = new LFixed();
            lf.L = long.Parse(str);

            return lf;
        }

        public string DebugInfo()
        {
            return "LFixed:" + L;
        }

        public LFixed(long l = 0)
        {
            L = l * POW;
        }

        public void Set(double d = 0)
        {
            var F = d.ToFix();
            L = F.L;
            //L = (long)Math.Round((d.Fix5() * POW));
        }

        public void Set(int i = 0)
        {
            L = i;
            L <<= Fix_Fracbits;//L = i << Fix_Fracbits;//L = i * POW;
        }

        public void Set(uint i = 0)
        {
            //L = i << Fix_Fracbits;//L = i * POW;
            L = i;
            L <<= Fix_Fracbits;
        }
        public void Set(short i = 0)
        {
            //L = i << Fix_Fracbits;//L = i * POW;
            L = i;
            L <<= Fix_Fracbits;
        }

        public void Set(ushort i = 0)
        {
            L = i;
            L <<= Fix_Fracbits;
            //L = i << Fix_Fracbits;//L = i * POW;
        }
        public void Set(byte i = 0)
        {
            L = i;
            L <<= Fix_Fracbits;
            //L = i << Fix_Fracbits;//L = i * POW;
        }

        public void Set(float f = 0)
        {
            var F = f.ToFix();
            L = F.L;
            //double d = f;
            //L = (long)Math.Round((d.Fix5() * POW));
        }

        private LFixed(ref LFixed f)
        {
            L = f.L;
        }

        public LFixed(ref long l)
        {
            L = l;
        }

        #region 开平方
        public LFixed Pow(LFixed F)
        {
            var ret = new LFixed();
            ret.L = (long)(Math.Round(Math.Pow(1.0 * L / POW, 1.0 * F.L / POW) * POW));
            return ret;
        }
        
        /// <summary>
        /// 平方
        /// </summary>
        public void Pow_2()
        {
#if DEBUG
            if (L > LMaxValue || L < LMinValue)
            {
                Console.Error.WriteLine("LFixed value overflow");
            }
#endif

            L = L * L >> Fix_Fracbits;
        }

        /// <summary>
        /// 立方
        /// </summary>
        public void Pow_3()
        {
#if DEBUG
            if (L > LMaxValue || L < LMinValue)
            {
                Console.Error.WriteLine("LFixed value overflow");
            }
#endif
            //L = L * L * L / POW_2;
            L = (((L * L) >> Fix_Fracbits) * L) >> Fix_Fracbits;
        }
        /// <summary>
        /// 返回平方
        /// </summary>
        public LFixed pow_2
        {
            get
            {
#if DEBUG
                if (L > LMaxValue || L < LMinValue)
                {
                    Console.Error.WriteLine("LFixed value overflow");
                }
#endif
                //var l = L * L / POW;
                var l = (L * L) >> Fix_Fracbits;
                return new LFixed(ref l);
            }
        }
        /// <summary>
        /// 返回立方
        /// </summary>
        public LFixed pow_3
        {
            get
            {
#if DEBUG
                if (L > LMaxValue || L < LMinValue)
                {
                    Console.Error.WriteLine("LFixed value overflow");
                }
#endif
                //var l = L * L * L / POW_2;
                var l = (((L * L) >> Fix_Fracbits) * L) >> Fix_Fracbits;
                return new LFixed(ref l);
            }
        }
        /// <summary>
        /// 对自己开平方
        /// </summary>
        public void SqrtSelf()
        {
#if DEBUG
            if (L < 0)
            {
                Console.Error.WriteLine("不能对负数开平方");
                return;
            }

            if (L > sqrtMaxValue)
            {
                Console.Error.WriteLine("L的值过大");
                return;
            }
#endif
            long L0 = L << Fix_Fracbits;
            double d;// = L0;
            d = Math.Sqrt(L0);
            long L1 = (long)d;
            long L2 = L1 * L1;

#if DEBUG
            int loopCount = 0;
#endif

            if (L2 < L0)
            {
                do
                {
#if DEBUG
                    loopCount++;
#endif
                    L1 = L1 + 1;
                    L2 = L1 * L1;
                } while (L2 < L0);

                if (L2 > L0)
                {
                    L1 = L1 - 1;
                }
            }
            else if (L2 > L0)
            {
                do
                {
#if DEBUG
                    loopCount++;
#endif
                    L1 = L1 - 1;
                    L2 = L1 * L1;
                } while (L2 > L0);
            }


#if DEBUG
            if (loopCount >= 3)
            {
                Console.Error.WriteLine("sqrt loopCount:" + loopCount);
            }
#endif
            L = L1;
            //l = l << Fix_Fracbits_Half;

            //L = (long)Math.Round(Math.Sqrt(L << Fix_Fracbits));
        }//SqrtThis

        const long sqrtMaxValue = 1L << (64 - Fix_Fracbits - 2);
        /// <summary>
        /// 返回开平方结果
        /// </summary>
        public LFixed sqrt
        {

            get
            {
#if DEBUG
                if (L < 0)
                {
                    Console.Error.WriteLine("不能对负数开平方");
                    return default(LFixed);
                }

                if (L > sqrtMaxValue)
                {
                    Console.Error.WriteLine("L的值过大");
                    return default(LFixed);
                }
#endif
                long L0 = L << Fix_Fracbits;
                double d = L0;
                d = Math.Sqrt(d);
                long L1 = (long)d;
                long L2 = L1 * L1;

#if DEBUG
                int loopCount = 0;
#endif

                if (L2 < L0)
                {
                    do
                    {
#if DEBUG
                        loopCount++;
#endif
                        L1 = L1 + 1;
                        L2 = L1 * L1;
                    } while (L2 < L0);

                    if (L2 > L0)
                    {
                        L1 = L1 - 1;
                    }
                }
                else if (L2 > L0)
                {
                    do
                    {
#if DEBUG
                        loopCount++;
#endif
                        L1 = L1 - 1;
                        L2 = L1 * L1;
                    } while (L2 > L0);
                }


#if DEBUG
                if (loopCount >= 3)
                {
                    Console.Error.WriteLine("sqrt loopCount:" + loopCount);
                }
#endif

                //l = l << Fix_Fracbits_Half;

                //var l = (long)Math.Round(Math.Sqrt(L << Fix_Fracbits));
                return new LFixed(ref L1);
            }
        }//sqrt

        #endregion
        #region 加法
        public static LFixed operator +(LFixed a, LFixed b)
        {
            LFixed ret = new LFixed();
            ret.L = a.L + b.L;
            return ret;
        }

        public static LFixed operator +(LFixed a, int b)
        {
            LFixed ret = new LFixed();
            long bl = b;
            ret.L = a.L + (bl << Fix_Fracbits);
            return ret;
        }

        public static LFixed operator +(int b, LFixed a)
        {
            LFixed ret = new LFixed();
            long bl = b;
            ret.L = a.L + (bl << Fix_Fracbits);
            return ret;
        }

        #endregion

        #region 减法
        public static LFixed operator -(LFixed a)
        {
            LFixed ret = new LFixed();
            ret.L = -a.L;
            return ret;
        }
        public static LFixed operator -(LFixed a, LFixed b)
        {
            LFixed ret = new LFixed();
            ret.L = a.L - b.L;
            return ret;
        }

        public static LFixed operator -(LFixed a, int b)
        {
            LFixed ret = new LFixed();
            long bl = b;
            bl <<= Fix_Fracbits;
            ret.L = a.L - bl;
            return ret;
        }

        public static LFixed operator -(int b, LFixed a)
        {
            LFixed ret = new LFixed();
            long bl = b;
            bl <<= Fix_Fracbits;
            ret.L = bl - a.L;
            return ret;
        }

        #endregion

        #region 乘法
        public static LFixed operator *(LFixed a, LFixed b)
        {
            LFixed ret = new LFixed();
            //ret.L = (a.L >> Fix_Fracbits_Half) * (b.L >> Fix_Fracbits_Half); 
            ret.L = (a.L * b.L) >> Fix_Fracbits;
#if DEBUG

            double ad = a.L >> Fix_Fracbits;
            double bd = b.L;
            double retd = ad * bd;
            //if (a.L > LMaxValue || b.L > LMaxValue || a.L < LMinValue || b.L < LMinValue)
            //if (ret.L > LMaxValue || ret.L < LMinValue)
            if (retd >= long.MaxValue)
            {
                Console.Error.WriteLine("LFixed  overflow .  a.L:" + a.L + "   b.L:" + b.L + "   ret.L:" + ret.L);
            }
#endif

            return ret;
        }

        public static LFixed operator *(LFixed a, int b)
        {
            LFixed ret = new LFixed();
            ret.L = a.L * b;
            return ret;
        }

        public static LFixed operator *(int b, LFixed a)
        {
            LFixed ret = new LFixed();
            ret.L = a.L * b;
            return ret;
        }
        #endregion

        #region 除法

        public static LFixed operator /(LFixed a, LFixed b)
        {
            LFixed ret = new LFixed();
            //ret.L = a.L * POW / b.L;
            ret.L = (a.L << Fix_Fracbits) / b.L;

            //#if DEBUG
            //            if (a.L > LMaxValue || b.L > LMaxValue || ret.L > LMaxValue)
            //            {
            //                Console.Error.WriteLine("LFixed  overflow");
            //            }
            //#endif

            return ret;
        }

        public static LFixed operator /(LFixed a, int b)
        {
            LFixed ret = new LFixed();
            ret.L = a.L / b;
            return ret;
        }

        public static LFixed operator /(int b, LFixed a)
        {
            LFixed ret = new LFixed();
            long bl = b;
            ret.L = (bl << (Fix_Fracbits + Fix_Fracbits)) / a.L;
            return ret;
        }

        #endregion

        #region 比较运算符重载 int,uint,short,ushort,byte,sbyte,float,double

        #region LFixed ~ LFixed
        public static bool operator ==(LFixed lhs, LFixed rhs)
        {
            return lhs.L == rhs.L;
        }
        public static bool operator !=(LFixed lhs, LFixed rhs)
        {
            return lhs.L != rhs.L;
        }

        public static bool operator <(LFixed L1, LFixed L2)
        {
            return L1.L < L2.L;
        }
        public static bool operator >(LFixed L1, LFixed L2)
        {
            return L1.L > L2.L;
        }


        public static bool operator <=(LFixed L1, LFixed L2)
        {
            return L1.L <= L2.L;
        }
        public static bool operator >=(LFixed L1, LFixed L2)
        {
            return L1.L >= L2.L;
        }
        #endregion

        #region LFixed ~ int
        public static bool operator ==(LFixed lhs, int rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L == l;
        }
        public static bool operator !=(LFixed lhs, int rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L != l;
        }
        public static bool operator <(LFixed lhs, int rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L < l;
        }
        public static bool operator >(LFixed lhs, int rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L > l;
        }
        public static bool operator <=(LFixed lhs, int rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L <= l;
        }
        public static bool operator >=(LFixed lhs, int rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L >= l;
        }
        #endregion
        #region int ~ LFixed
        public static bool operator ==(int lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l == rhs.L;
        }
        public static bool operator !=(int lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l != rhs.L;
        }
        public static bool operator <(int lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l < rhs.L;
        }
        public static bool operator >(int lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l > rhs.L;
        }
        public static bool operator <=(int lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l <= rhs.L;
        }
        public static bool operator >=(int lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l >= rhs.L;
        }
        #endregion

        #region LFixed ~ uint
        public static bool operator ==(LFixed lhs, uint rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L == l;
        }
        public static bool operator !=(LFixed lhs, uint rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L != l;
        }
        public static bool operator <(LFixed lhs, uint rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L < l;
        }
        public static bool operator >(LFixed lhs, uint rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L > l;
        }
        public static bool operator <=(LFixed lhs, uint rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L <= l;
        }
        public static bool operator >=(LFixed lhs, uint rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L >= l;
        }
        #endregion
        #region uint ~ LFixed
        public static bool operator ==(uint lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l == rhs.L;
        }
        public static bool operator !=(uint lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l != rhs.L;
        }
        public static bool operator <(uint lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l < rhs.L;
        }
        public static bool operator >(uint lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l > rhs.L;
        }
        public static bool operator <=(uint lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l <= rhs.L;
        }
        public static bool operator >=(uint lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l >= rhs.L;
        }
        #endregion

        #region LFixed ~ short
        public static bool operator ==(LFixed lhs, short rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L == l;
        }
        public static bool operator !=(LFixed lhs, short rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L != l;
        }
        public static bool operator <(LFixed lhs, short rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L < l;
        }
        public static bool operator >(LFixed lhs, short rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L > l;
        }
        public static bool operator <=(LFixed lhs, short rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L <= l;
        }
        public static bool operator >=(LFixed lhs, short rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L >= l;
        }
        #endregion
        #region short ~ LFixed
        public static bool operator ==(short lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l == rhs.L;
        }
        public static bool operator !=(short lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l != rhs.L;
        }
        public static bool operator <(short lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l < rhs.L;
        }
        public static bool operator >(short lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l > rhs.L;
        }
        public static bool operator <=(short lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l <= rhs.L;
        }
        public static bool operator >=(short lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l >= rhs.L;
        }
        #endregion

        #region LFixed ~ ushort
        public static bool operator ==(LFixed lhs, ushort rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L == l;
        }
        public static bool operator !=(LFixed lhs, ushort rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L != l;
        }
        public static bool operator <(LFixed lhs, ushort rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L < l;
        }
        public static bool operator >(LFixed lhs, ushort rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L > l;
        }
        public static bool operator <=(LFixed lhs, ushort rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L <= l;
        }
        public static bool operator >=(LFixed lhs, ushort rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L >= l;
        }
        #endregion
        #region ushort ~ LFixed
        public static bool operator ==(ushort lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l == rhs.L;
        }
        public static bool operator !=(ushort lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l != rhs.L;
        }
        public static bool operator <(ushort lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l < rhs.L;
        }
        public static bool operator >(ushort lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l > rhs.L;
        }
        public static bool operator <=(ushort lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l <= rhs.L;
        }
        public static bool operator >=(ushort lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l >= rhs.L;
        }
        #endregion

        #region LFixed ~ byte
        public static bool operator ==(LFixed lhs, byte rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L == l;
        }
        public static bool operator !=(LFixed lhs, byte rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L != l;
        }
        public static bool operator <(LFixed lhs, byte rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L < l;
        }
        public static bool operator >(LFixed lhs, byte rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L > l;
        }
        public static bool operator <=(LFixed lhs, byte rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L <= l;
        }
        public static bool operator >=(LFixed lhs, byte rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L >= l;
        }
        #endregion
        #region byte ~ LFixed
        public static bool operator ==(byte lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l == rhs.L;
        }
        public static bool operator !=(byte lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l != rhs.L;
        }
        public static bool operator <(byte lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l < rhs.L;
        }
        public static bool operator >(byte lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l > rhs.L;
        }
        public static bool operator <=(byte lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l <= rhs.L;
        }
        public static bool operator >=(byte lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l >= rhs.L;
        }
        #endregion

        #region LFixed ~ sbyte
        public static bool operator ==(LFixed lhs, sbyte rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L == l;
        }
        public static bool operator !=(LFixed lhs, sbyte rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L != l;
        }
        public static bool operator <(LFixed lhs, sbyte rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L < l;
        }
        public static bool operator >(LFixed lhs, sbyte rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L > l;
        }
        public static bool operator <=(LFixed lhs, sbyte rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L <= l;
        }
        public static bool operator >=(LFixed lhs, sbyte rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L >= l;
        }
        #endregion
        #region sbyte ~ LFixed
        public static bool operator ==(sbyte lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l == rhs.L;
        }
        public static bool operator !=(sbyte lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l != rhs.L;
        }
        public static bool operator <(sbyte lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l < rhs.L;
        }
        public static bool operator >(sbyte lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l > rhs.L;
        }
        public static bool operator <=(sbyte lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l <= rhs.L;
        }
        public static bool operator >=(sbyte lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l >= rhs.L;
        }
        #endregion

        #region LFixed ~ float
        [Obsolete("Don't do the \"==\" comparison of \"LFixed\" and \"float\"", true)]
        public static bool operator ==(LFixed lhs, float rhs)
        {
            return lhs == rhs.ToFix();
        }
        [Obsolete("Don't do the \"!=\" comparison of \"LFixed\" and \"float\"", true)]
        public static bool operator !=(LFixed lhs, float rhs)
        {
            return lhs != rhs.ToFix();
        }
        public static bool operator <(LFixed lhs, float rhs)
        {
            return lhs < rhs.ToFix();
        }
        public static bool operator >(LFixed lhs, float rhs)
        {
            return lhs > rhs.ToFix();
        }
        public static bool operator <=(LFixed lhs, float rhs)
        {
            return lhs <= rhs.ToFix();
        }
        public static bool operator >=(LFixed lhs, float rhs)
        {
            return lhs >= rhs.ToFix();
        }
        #endregion
        #region float ~ LFixed
        [Obsolete("Don't do the \"==\" comparison of \"LFixed\" and \"float\"", true)]
        public static bool operator ==(float lhs, LFixed rhs)
        {
            return lhs.ToFix() == rhs;
        }
        [Obsolete("Don't do the \"!=\" comparison of \"LFixed\" and \"float\"", true)]
        public static bool operator !=(float lhs, LFixed rhs)
        {
            return lhs.ToFix() != rhs;
        }
        public static bool operator <(float lhs, LFixed rhs)
        {
            return lhs.ToFix() < rhs;
        }
        public static bool operator >(float lhs, LFixed rhs)
        {
            return lhs.ToFix() > rhs;
        }
        public static bool operator <=(float lhs, LFixed rhs)
        {
            return lhs.ToFix() <= rhs;
        }
        public static bool operator >=(float lhs, LFixed rhs)
        {
            return lhs.ToFix() >= rhs;
        }
        #endregion

        #region LFixed ~ double
        [Obsolete("Don't do the \"==\" comparison of \"LFixed\" and \"double\"", true)]
        public static bool operator ==(LFixed lhs, double rhs)
        {
            return lhs == rhs.ToFix();
        }
        [Obsolete("Don't do the \"!=\" comparison of \"LFixed\" and \"double\"", true)]
        public static bool operator !=(LFixed lhs, double rhs)
        {
            return lhs != rhs.ToFix();
        }
        public static bool operator <(LFixed lhs, double rhs)
        {
            return lhs < rhs.ToFix();
        }
        public static bool operator >(LFixed lhs, double rhs)
        {
            return lhs > rhs.ToFix();
        }
        public static bool operator <=(LFixed lhs, double rhs)
        {
            return lhs <= rhs.ToFix();
        }
        public static bool operator >=(LFixed lhs, double rhs)
        {
            return lhs >= rhs.ToFix();
        }
        #endregion
        #region double ~ LFixed
        [Obsolete("Don't do the \"==\" comparison of \"LFixed\" and \"double\"", true)]
        public static bool operator ==(double lhs, LFixed rhs)
        {
            return lhs.ToFix() == rhs;
        }
        [Obsolete("Don't do the \"!=\" comparison of \"LFixed\" and \"double\"", true)]
        public static bool operator !=(double lhs, LFixed rhs)
        {
            return lhs.ToFix() != rhs;
        }
        public static bool operator <(double lhs, LFixed rhs)
        {
            return lhs.ToFix() < rhs;
        }
        public static bool operator >(double lhs, LFixed rhs)
        {
            return lhs.ToFix() > rhs;
        }
        public static bool operator <=(double lhs, LFixed rhs)
        {
            return lhs.ToFix() <= rhs;
        }
        public static bool operator >=(double lhs, LFixed rhs)
        {
            return lhs.ToFix() >= rhs;
        }
        #endregion

        #region LFixed ~ long
        [Obsolete("Don't compare it to \"LFixed\" and \"long\"", true)]
        public static bool operator ==(LFixed lhs, long rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L == l;
        }
        [Obsolete("Don't compare it to \"LFixed\" and \"long\"", true)]
        public static bool operator !=(LFixed lhs, long rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L != l;
        }
        [Obsolete("Don't compare it to \"LFixed\" and \"long\"", true)]
        public static bool operator <(LFixed lhs, long rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L < l;
        }
        [Obsolete("Don't compare it to \"LFixed\" and \"long\"", true)]
        public static bool operator >(LFixed lhs, long rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L > l;
        }
        [Obsolete("Don't compare it to \"LFixed\" and \"long\"", true)]
        public static bool operator <=(LFixed lhs, long rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L <= l;
        }
        [Obsolete("Don't compare it to \"LFixed\" and \"long\"", true)]
        public static bool operator >=(LFixed lhs, long rhs)
        {
            long l = rhs;
            l = l << LFixed.Fix_Fracbits;
            return lhs.L >= l;
        }
        #endregion
        #region long ~ LFixed
        [Obsolete("Don't compare it to \"LFixed\" and \"long\"", true)]
        public static bool operator ==(long lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l == rhs.L;
        }
        [Obsolete("Don't compare it to \"LFixed\" and \"long\"", true)]
        public static bool operator !=(long lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l != rhs.L;
        }
        [Obsolete("Don't compare it to \"LFixed\" and \"long\"", true)]
        public static bool operator <(long lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l < rhs.L;
        }
        [Obsolete("Don't compare it to \"LFixed\" and \"long\"", true)]
        public static bool operator >(long lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l > rhs.L;
        }
        [Obsolete("Don't compare it to \"LFixed\" and \"long\"", true)]
        public static bool operator <=(long lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l <= rhs.L;
        }
        [Obsolete("Don't compare it to \"LFixed\" and \"long\"", true)]
        public static bool operator >=(long lhs, LFixed rhs)
        {
            long l = lhs;
            l = l << LFixed.Fix_Fracbits;
            return l >= rhs.L;
        }
        #endregion

        #endregion

        #region 隐式转换 double float int short ushort
        public static implicit operator double(LFixed v)
        {
            return 1.0 * v.L / POW;
        }

        public static implicit operator float(LFixed v)
        {
            return 1F * v.L / POW;
        }

        public static implicit operator int(LFixed v)
        {
            return (int)(v.L >> Fix_Fracbits);
        }

        public static implicit operator short(LFixed v)
        {
            return (short)(v.L >> Fix_Fracbits);
        }

        public static implicit operator ushort(LFixed v)
        {
            return (ushort)(v.L >> Fix_Fracbits);
        }
        #endregion

        public bool Equals(LFixed f)
        {
            return L == f.L;
        }

        public override int GetHashCode()
        {
            return (int)(L);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is LFixed))
            {
                return false;
            }
            var F = (LFixed)obj;
            return L == F.L;
        }//Equals

        public int CompareTo(LFixed other)
        {
            return (int)(L - other.L);
        }

        public struct Cmp : IEqualityComparer<LFixed>
        {
            public bool Equals(LFixed x, LFixed y)
            {
                return x.L == y.L;
            }

            public int GetHashCode(LFixed obj)
            {
                return (int)obj.L;
            }
        }

    }