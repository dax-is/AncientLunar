using System;

namespace AncientLunar.Helpers
{
    internal static class DumbConvert
    {
        public static long ToInt64(object param)
        {
            if (param.GetType() == typeof(IntPtr))
                return ((IntPtr)param).ToInt64();

            if (param.GetType() == typeof(ushort))
                return (ushort)param;

            if (param.GetType() == typeof(uint))
                return (uint)param;

            if (param.GetType() == typeof(ulong))
                return (long)(ulong)param;

            if (param.GetType() == typeof(byte))
                return (byte)param;

            if (param.GetType() == typeof(short))
                return (short)param;

            if (param.GetType() == typeof(int))
                return (int)param;

            if (param.GetType() == typeof(long))
                return (long)param;

            if (param.GetType() == typeof(sbyte))
                return (sbyte)param;

            if (param.GetType().IsEnum)
                return Convert.ToInt64(param);

            throw new Exception("What are you doing?");
        }

        public static int ToInt32(object param)
        {
            if (param.GetType() == typeof(IntPtr))
                return ((IntPtr)param).ToInt32();

            if (param.GetType() == typeof(ushort))
                return (ushort)param;

            if (param.GetType() == typeof(uint))
                return (int)(uint)param;

            if (param.GetType() == typeof(ulong))
                return (int)(ulong)param;

            if (param.GetType() == typeof(byte))
                return (byte)param;

            if (param.GetType() == typeof(short))
                return (short)param;

            if (param.GetType() == typeof(int))
                return (int)param;

            if (param.GetType() == typeof(long))
                return (int)(long)param;

            if (param.GetType() == typeof(sbyte))
                return (sbyte)param;

            if (param.GetType().IsEnum)
                return Convert.ToInt32(param);

            throw new Exception("What are you doing?");
        }
    }
}
