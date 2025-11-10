using System;
using System.Collections.Generic;
using Solitaire.Domain;

namespace Solitaire.Infrastructure
{
    public static class DeckFactory
    {
        public static List<Card> CreateDeck()
        {
            var list = new List<Card>();
            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                    list.Add(new Card(suit, rank));
            return list;
        }

        public static void Shuffle<T>(IList<T> list)
        {
            var random = new Random();
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}