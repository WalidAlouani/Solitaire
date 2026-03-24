namespace Solitaire.Domain
{
    /// <summary>
    /// Marker interface for piles that serve as a drawable source (e.g. StockPile).
    /// Used by WasteDropRule to check origin without depending on concrete types.
    /// </summary>
    public interface IDrawableSource { }
}
