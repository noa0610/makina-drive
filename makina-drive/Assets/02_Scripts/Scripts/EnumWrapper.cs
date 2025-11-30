using System;
using System.Collections.Generic;
using System.Linq;

public static class EnumWrapper
{
    /// <summary>
    /// enum値から対応する文字列キーを参照
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="skipDefault"></param>
    /// <returns></returns>
    public static Dictionary<T, string> GetValueNameMap<T>(bool skipDefault = true) where T : struct, Enum =>
            GetEnumerable<T>(skipDefault).ToDictionary(v => v, v => Enum.GetName(typeof(T), v)!);

    public static IEnumerable<T> GetEnumerable<T>(bool skipDefault) where T : struct, Enum
    {
#if NET5_0_OR_GREATER
            var values = Enum.GetValues<T>();
#else
        var values = (T[])Enum.GetValues(typeof(T));
#endif
        return skipDefault
            ? values.Where(v => Convert.ToInt64(v) != 0) // “0=デフォルト(None)” を除外
            : values;
    }
}
