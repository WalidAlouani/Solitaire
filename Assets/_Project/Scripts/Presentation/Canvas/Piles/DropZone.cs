using UnityEngine;

namespace Solitaire.Presentation.Canvas
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class DropZone : MonoBehaviour
    {
        [SerializeField] private PileView _pileView;
        public PileView PileView => _pileView;

        private BoxCollider2D _boxCollider;
        private Vector2 _initialSize;

        private void Awake()
        {
            _boxCollider = GetComponent<BoxCollider2D>();
            _initialSize = _boxCollider.size;
        }

        private void OnTransformChildrenChanged()
        {
            var childCount = transform.childCount;
            if (childCount <= 1)
            {
                _boxCollider.size = _initialSize;
                _boxCollider.offset = Vector2.zero;
                return;
            }

            var cardView = transform.GetChild(transform.childCount - 1).GetComponent<CardView>();
            if (cardView == null)
                return;

            float y = _pileView.GetCardPosition(cardView.Model);

            _boxCollider.size = new Vector2(_initialSize.x, _initialSize.y - y);
            _boxCollider.offset = new Vector2(0, y / 2);
        }
    }
}