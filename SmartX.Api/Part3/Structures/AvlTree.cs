
namespace SmartX.Api.Part3.Structures;

public sealed class AvlTree<TValue>
{
    private sealed class Node
    {
        public DateTime Timestamp { get; }
        public List<TValue> Values { get; }
        public Node? Left { get; set; }
        public Node? Right { get; set; }
        public int Height { get; set; } = 1;

        public Node(DateTime timestamp, TValue value)
        {
            Timestamp = timestamp;
            Values = new List<TValue> { value };
        }
    }

    private Node? _root;

    public int Count { get; private set; }

    public int Height => GetHeight(_root);

    public long Rotations { get; private set; }

    public void Insert(DateTime timestamp, TValue value)
    {
        _root = InsertNode(_root, timestamp, value);
        Count++;
    }

    public IReadOnlyList<TValue> GetRange(
        DateTime from,
        DateTime to,
        int limit = int.MaxValue,
        bool newestFirst = true)
    {
        if (from > to)
        {
            throw new ArgumentException(
                "The start date must not exceed the end date.");
        }

        if (limit <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(limit));
        }

        var results = new List<TValue>();

        if (newestFirst)
        {
            CollectDescending(_root, from, to, limit, results);
        }
        else
        {
            CollectAscending(_root, from, to, limit, results);
        }

        return results;
    }

    public bool IsBalanced()
    {
        return Validate(_root, null, null, out _);
    }

    private Node InsertNode(
        Node? node,
        DateTime timestamp,
        TValue value)
    {
        if (node is null)
        {
            return new Node(timestamp, value);
        }

        if (timestamp < node.Timestamp)
        {
            node.Left = InsertNode(
                node.Left, timestamp, value);
        }
        else if (timestamp > node.Timestamp)
        {
            node.Right = InsertNode(
                node.Right, timestamp, value);
        }
        else
        {
            node.Values.Add(value);
            return node;
        }

        return Rebalance(node);
    }

    private Node Rebalance(Node node)
    {
        UpdateHeight(node);

        var balance = GetBalance(node);

        if (balance > 1)
        {
            if (GetBalance(node.Left!) < 0)
            {
                node.Left = RotateLeft(node.Left!);
            }

            return RotateRight(node);
        }

        if (balance < -1)
        {
            if (GetBalance(node.Right!) > 0)
            {
                node.Right = RotateRight(node.Right!);
            }

            return RotateLeft(node);
        }

        return node;
    }

    private Node RotateLeft(Node node)
    {
        var newRoot = node.Right!;

        node.Right = newRoot.Left;
        newRoot.Left = node;

        UpdateHeight(node);
        UpdateHeight(newRoot);

        Rotations++;

        return newRoot;
    }

    private Node RotateRight(Node node)
    {
        var newRoot = node.Left!;

        node.Left = newRoot.Right;
        newRoot.Right = node;

        UpdateHeight(node);
        UpdateHeight(newRoot);

        Rotations++;

        return newRoot;
    }

    private static int GetHeight(Node? node)
    {
        return node?.Height ?? 0;
    }

    private static int GetBalance(Node node)
    {
        return GetHeight(node.Left) - GetHeight(node.Right);
    }

    private static void UpdateHeight(Node node)
    {
        node.Height = 1 + Math.Max(
            GetHeight(node.Left),
            GetHeight(node.Right));
    }

    private static void CollectAscending(
        Node? node,
        DateTime from,
        DateTime to,
        int limit,
        List<TValue> results)
    {
        if (node is null || results.Count >= limit)
        {
            return;
        }

        if (node.Timestamp > from)
        {
            CollectAscending(
                node.Left, from, to, limit, results);
        }

        if (node.Timestamp >= from &&
            node.Timestamp <= to)
        {
            foreach (var value in node.Values)
            {
                if (results.Count >= limit)
                {
                    break;
                }

                results.Add(value);
            }
        }

        if (node.Timestamp < to && results.Count < limit)
        {
            CollectAscending(
                node.Right, from, to, limit, results);
        }
    }

    private static void CollectDescending(
        Node? node,
        DateTime from,
        DateTime to,
        int limit,
        List<TValue> results)
    {
        if (node is null || results.Count >= limit)
        {
            return;
        }

        if (node.Timestamp < to)
        {
            CollectDescending(
                node.Right, from, to, limit, results);
        }

        if (node.Timestamp >= from &&
            node.Timestamp <= to)
        {
            for (var i = node.Values.Count - 1; i >= 0; i--)
            {
                if (results.Count >= limit)
                {
                    break;
                }

                results.Add(node.Values[i]);
            }
        }

        if (node.Timestamp > from && results.Count < limit)
        {
            CollectDescending(
                node.Left, from, to, limit, results);
        }
    }

    private static bool Validate(
        Node? node,
        DateTime? minimum,
        DateTime? maximum,
        out int height)
    {
        if (node is null)
        {
            height = 0;
            return true;
        }

        if ((minimum.HasValue &&
             node.Timestamp <= minimum.Value) ||
            (maximum.HasValue &&
             node.Timestamp >= maximum.Value))
        {
            height = 0;
            return false;
        }

        var leftValid = Validate(
            node.Left, minimum, node.Timestamp, out var leftHeight);

        var rightValid = Validate(
            node.Right, node.Timestamp, maximum, out var rightHeight);

        height = 1 + Math.Max(leftHeight, rightHeight);

        return leftValid &&
               rightValid &&
               node.Values.Count > 0 &&
               node.Height == height &&
               Math.Abs(leftHeight - rightHeight) <= 1;
    }
}
