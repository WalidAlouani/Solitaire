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
    }
}