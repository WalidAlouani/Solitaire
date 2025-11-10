using System;
using System.Collections.Generic;

public static class ListExt
{
    public static void Shuffle<T>(this IList<T> list)
    {
        var rng = new Random();
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}