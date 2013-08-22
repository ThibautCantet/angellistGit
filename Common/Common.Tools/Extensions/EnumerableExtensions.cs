// <copyright file="EnumerableExtensions.cs" company="AntVoice">
// Copyright (c)  All rights reserved.
// </copyright>

using System.Linq;

namespace Common.Tools.Extensions
{
    using System;
    using System.Collections.Generic;

    public static class EnumerableExtensions
    {
        public static List<List<T>> Split<T>(this List<T> source, int size)
        {
            var ret = new List<List<T>>();
            for (int i = 0; i < source.Count; i += size)
            {
                ret.Add(source.GetRange(i, Math.Min(size, source.Count - i)));
            }
            return ret;
        }

        public static IEnumerable<int> Range(int min, int max, int step)
        {
            if (max != 0 && step != 0)
            {
                for (int i = min; i <= max; i += step)
                {
                    yield return i;
                }
            }
        }

        public static double Multiply(this IEnumerable<double> source)
        {
            return source.Aggregate<double, double>(1, (current, v) => current*v);
        }

        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            return source == null || !source.Any();
        }
    }
}
