using Solitaire.Domain;
using UnityEngine;

namespace Solitaire.Presentation
{
    /// <summary>
    /// Holds all visual assets for a card theme: suit sprites, face card art, and colors.
    /// Assign as a single asset in the Inspector to swap themes without code changes.
    /// Replaces scattered Resources.Load calls with a centralized, designer-friendly config.
    /// </summary>
    [CreateAssetMenu(fileName = "CardTheme", menuName = "Solitaire/Card Theme")]
    public class CardThemeSO : ScriptableObject
    {
        [Header("Suit Sprites")]
        [SerializeField] private Sprite _clubs;
        [SerializeField] private Sprite _diamonds;
        [SerializeField] private Sprite _hearts;
        [SerializeField] private Sprite _spades;

        [Header("Face Card Art — Red")]
        [SerializeField] private Sprite _redJack;
        [SerializeField] private Sprite _redQueen;
        [SerializeField] private Sprite _redKing;

        [Header("Face Card Art — Black")]
        [SerializeField] private Sprite _blackJack;
        [SerializeField] private Sprite _blackQueen;
        [SerializeField] private Sprite _blackKing;

        [Header("Card Colors")]
        [SerializeField] private Color _redColor = Color.red;
        [SerializeField] private Color _blackColor = Color.black;

        public Color RedColor => _redColor;
        public Color BlackColor => _blackColor;

        /// <summary>
        /// Returns the small suit icon for the given suit.
        /// </summary>
        public Sprite GetSuitSprite(Suit suit)
        {
            switch (suit)
            {
                case Suit.Clubs:    return _clubs;
                case Suit.Diamonds: return _diamonds;
                case Suit.Hearts:   return _hearts;
                case Suit.Spades:   return _spades;
                default:
                    Debug.LogWarning($"CardThemeSO: Unknown suit {suit}");
                    return null;
            }
        }

        /// <summary>
        /// Returns the center/main art for a card.
        /// Face cards (J, Q, K) get their unique art; all others get the suit sprite.
        /// </summary>
        public Sprite GetCenterSprite(Rank rank, Suit suit, bool isRed)
        {
            switch (rank)
            {
                case Rank.Jack:  return isRed ? _redJack  : _blackJack;
                case Rank.Queen: return isRed ? _redQueen : _blackQueen;
                case Rank.King:  return isRed ? _redKing  : _blackKing;
                default:         return GetSuitSprite(suit);
            }
        }

        /// <summary>
        /// Returns the display text for a rank (A, 2-10, J, Q, K).
        /// </summary>
        public string GetRankDisplayText(Rank rank)
        {
            switch (rank)
            {
                case Rank.Ace:   return "A";
                case Rank.Jack:  return "J";
                case Rank.Queen: return "Q";
                case Rank.King:  return "K";
                default:         return ((int)rank).ToString();
            }
        }

        /// <summary>
        /// Returns the appropriate text color for a card.
        /// </summary>
        public Color GetCardColor(bool isRed)
        {
            return isRed ? _redColor : _blackColor;
        }
    }
}
