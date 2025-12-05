using System.Collections.Generic;

public class MoveReverseCommand : ICommand
{
    private readonly CardPile _from;
    private readonly CardPile _to;
    private readonly int _count;
    private readonly List<Card> _moved = new List<Card>();

    public MoveReverseCommand(CardPile from, CardPile to, int count = 1)
    {
        _from = from;
        _to = to;
        _count = count;
    }

    public void Execute()
    {
        _moved.Clear();
        var tmp = new List<Card>();
        for (int i = 0; i < _count; i++) 
            tmp.Add(_from.Pop());

        foreach (var c in tmp) 
        { 
            _to.Push(c); 
            _moved.Add(c);
        }
    }

    public void Undo()
    {
        // pop moved items from to (assumes they are on top)
        for (int i = 0; i < _moved.Count; i++) 
            _to.Pop();

        _moved.Reverse();

        foreach (var c in _moved)
        {
            _from.Push(c);
        }
    }
}