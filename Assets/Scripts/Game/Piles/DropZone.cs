using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DropZone : MonoBehaviour
{
    public PileView PileView;
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
        {
            Debug.LogWarning("DropZone found a child without CardView!");
            return;
        }

        float y = PileView.GetCardPosition(cardView.Model);

        _boxCollider.size = new Vector2(_initialSize.x, _initialSize.y - y);
        _boxCollider.offset = new Vector2(0, y / 2);
    }
}