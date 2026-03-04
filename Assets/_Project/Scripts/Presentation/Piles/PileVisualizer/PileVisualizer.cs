namespace Solitaire.Presentation.PileVisualizer
{
    /// <summary>
    /// Concrete implementation of PileBehaviour.
    /// Call UpdatePile() manually when cards are added/removed rather than every frame.
    /// </summary>
    public class PileVisualizer : PileBehaviour
    {
        protected override void OnNodeAdded(int index)
        {
        }

        protected override void OnNodeRemoved(int index)
        {
        }

        protected override void OnNodeRemoving(int index)
        {
        }
    }
}