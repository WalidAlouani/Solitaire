using System;
using System.Collections.Generic;

namespace Solitaire.Extensions
{
    public static class ListExtensions
    {
        // Shared instance avoids duplicate seeds when called in quick succession.
        private static readonly Random _rng = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = _rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
