using System.Runtime.InteropServices;

namespace network.Utils;

public static class Fix
    {
        public static LFixed ToFix(this LFixed l)
        {
            return l;
        }
        public static LFixed ToFix(this byte d)
        {
            var L = new LFixed();
            L.Set(d);
            return L;
        }

        public static LFixed ToFix(this uint i)
        {
            var L = new LFixed();
            L.Set(i);
            return L;
        }

        public static LFixed ToFix(this int i)
        {
            var L = new LFixed();
            L.Set(i);
            return L;
        }

        /// <summary>
        /// 符号位筛选
        /// </summary>
        private const int Float_SignFiltrate = 1 << 31;             //二进制 10000000000000000000000000000000
        private const long Double_SignFiltrate = 1L << 63;             //二进制 |1个1|63个0|

        /// <summary>
        /// 指数筛选
        /// </summary>
        private const int Float_ExponentFiltrate = 2139095040;      // 二进制 01111111100000000000000000000000
        private const long Double_ExponentFiltrate = ((1L << 11) - 1L) << 52;     // 二进制 |1个0|11个1|52个0|
        /// <summary>
        /// 基数筛选
        /// </summary>
        private const int Float_CardinalFiltrate = 8388607;       // 二进制 00000000011111111111111111111111
        private const long Double_CardinalFiltrate = (1L << 52) - 1L;             // 二进制 |1个0|11个0|52个1|
        /// <summary>
        /// 补数
        /// </summary>
        private const int Float_SupplementFiltrate = 8388608;     // 二进制 00000000100000000000000000000000
        private const long Double_SupplementFiltrate = 1L << 52;                 // 二进制 |1个0|10个0|1个1|52个0|

        /// <summary>
        /// 指数偏差
        /// </summary>
        private const int Float_ExponentDelta = -150 + LFixed.Fix_Fracbits;
        private const int Double_ExponentDelta = -1075 + LFixed.Fix_Fracbits;

        //考虑到多线程问题，不使用临时变量
        ////临时变量
        //private static TypeTransform CommonTransverter = new TypeTransform();
        //private static int CommonExponent;
        //private static long Float_CommonCardinal;
        //private static long Double_CommonCardinal;

        unsafe public static LFixed ToFix(this float f)
        {
            int CommonExponent;
            long Float_CommonCardinal;

            int i = *(int*)(&f);

            //获取浮点数二进制的指数位
            CommonExponent = (i & Float_ExponentFiltrate) >> 23;

            //（指数值 = 指数位值 - 127 - 基数位数）   （基数位数 = 23）（指数范围 -127~128 ，即指数字面值为0实际表示的指数值是-127）
            //加LFixed.Fix_Fracbits使最终值可以直接存入LFixed.L
            CommonExponent = CommonExponent + Float_ExponentDelta;

            if (CommonExponent < -24)
            {
                return LFixed.zero;
            }
#if DEBUG
            else if (CommonExponent > 20)
            {
                Console.Error.WriteLine("传入的float值过大");
                return LFixed.maxValue;
            }
#endif

            LFixed rt;

            //获取浮点数二进制的基数位（基数位前要补一个1才是完整的基数）
            Float_CommonCardinal = (i & Float_CardinalFiltrate) | Float_SupplementFiltrate;

            if (CommonExponent >= 0)
            {
                rt.L = Float_CommonCardinal << CommonExponent;
            }
            else
            {
                rt.L = Float_CommonCardinal >> (-CommonExponent);
            }

            //判断符号位
            if ((i & Float_SignFiltrate) != 0)
            {
                rt.L = -rt.L;
            }

            return rt;



            //            TypeTransform CommonTransverter = new TypeTransform();
            //            int CommonExponent;
            //            long Float_CommonCardinal;

            //            CommonTransverter.f = f;

            //            //获取浮点数二进制的指数位
            //            CommonExponent = (CommonTransverter.i & Float_ExponentFiltrate) >> 23;

            //            //（指数值 = 指数位值 - 127 - 基数位数）   （基数位数 = 23）（指数范围 -127~128 ，即指数字面值为0实际表示的指数值是-127）
            //            //加LFixed.Fix_Fracbits使最终值可以直接存入LFixed.L
            //            CommonExponent = CommonExponent + Float_ExponentDelta;

            //            if (CommonExponent < -24)
            //            {
            //                return LFixed.zero;
            //            }
            //#if DEBUG
            //            else if (CommonExponent > 20)
            //            {
            //                Console.Error.WriteLine("传入的float值过大");
            //                return LFixed.maxValue;
            //            }
            //#endif

            //            LFixed rt;

            //            //获取浮点数二进制的基数位（基数位前要补一个1才是完整的基数）
            //            Float_CommonCardinal = (CommonTransverter.i & Float_CardinalFiltrate) | Float_SupplementFiltrate;

            //            if (CommonExponent >= 0)
            //            {
            //                rt.L = Float_CommonCardinal << CommonExponent;
            //            }
            //            else
            //            {
            //                rt.L = Float_CommonCardinal >> (-CommonExponent);
            //            }

            //            //判断符号位
            //            if ((CommonTransverter.i & Float_SignFiltrate) != 0)
            //            {
            //                rt.L = -rt.L;
            //            }

            //            return rt;
        }
        unsafe public static LFixed ToFix(this double d)
        {
            int CommonExponent;
            long Double_CommonCardinal;

            long l = *(long*)(&d);

            //获取浮点数二进制的指数位
            CommonExponent = (int)((l & Double_ExponentFiltrate) >> 52);

            //（指数值 = 指数位值 - 1023 - 基数位数）   （基数位数 = 52）（指数范围 -1023~1024 ，即指数字面值为0实际表示的指数值是-1023）
            //加LFixed.Fix_Fracbits使最终值可以直接存入LFixed.L
            CommonExponent = CommonExponent + Double_ExponentDelta;

            if (CommonExponent < -63)
            {
                return LFixed.zero;
            }
#if DEBUG
            else if (CommonExponent > 63)
            {
                Console.Error.WriteLine("传入的double值过大");
                return LFixed.maxValue;
            }
#endif

            LFixed rt;

            //获取浮点数二进制的基数位（基数位前要补一个1才是完整的基数）
            Double_CommonCardinal = (l & Double_CardinalFiltrate) | Double_SupplementFiltrate;

            if (CommonExponent >= 0)
            {
                rt.L = Double_CommonCardinal << CommonExponent;
            }
            else
            {
                rt.L = Double_CommonCardinal >> (-CommonExponent);
            }

            //判断符号位
            if ((l & Double_SignFiltrate) != 0)
            {
                rt.L = -rt.L;
            }
            return rt;
        }

        public static LFixed ReadFixed(this BinaryReader BR)
        {
            var l = BR.ReadInt64();
            var L = new LFixed();
            L.L = l;
            return L;
        }

        public static void WriteFixed(this BinaryWriter BW, LFixed L)
        {
            BW.Write(L.L);
        }

        [StructLayout(LayoutKind.Explicit, Size = 8)]
        public struct TypeTransform
        {
            [FieldOffset(0)]
            public float f;

            [FieldOffset(0)]
            public int i;

            [FieldOffset(0)]
            public double d;

            [FieldOffset(0)]
            public long l;

            //[FieldOffset(0)]
            //public byte b1;

            //[FieldOffset(1)]
            //public byte b2;

            //[FieldOffset(2)]
            //public byte b3;

            //[FieldOffset(3)]
            //public byte b4;
        }

        /// <summary>
        /// 保留2位小数
        /// </summary>
        /// <param name="L"></param>
        /// <returns></returns>
        public static float RoundFix2(this LFixed L)
        {
            float f = L;
            int i = (int)Math.Round(f * 100);
            f = i / 100F;
            return f;
        }

        /// <summary>
        /// 保留2位小数
        /// </summary>
        /// <param name="L"></param>
        /// <returns></returns>
        public static float RoundFix3(this LFixed L)
        {
            float f = L;
            int i = (int)Math.Round(f * 1000);
            f = i / 1000F;
            return f;
        }
    }