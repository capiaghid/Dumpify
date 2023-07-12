using System.Collections;

namespace Dumpify.Playground;

class Test : ICollection<KeyValuePair<string, int>>
{
    private List<(string key, int value)> _list = new();

    public IEnumerator<KeyValuePair<string, int>> GetEnumerator()
        => _list.Select(l => new KeyValuePair<string, int>(l.key, l.value)).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public void Add(KeyValuePair<string, int> item)
        => _list.Add((item.Key, item.Value));

    public void Clear()
        => _list.Clear();

    public bool Contains(KeyValuePair<string, int> item)
        => _list.Contains((item.Key, item.Value));

    public void CopyTo(KeyValuePair<string, int>[] array, int arrayIndex)
        => throw new NotImplementedException();

    public bool Remove(KeyValuePair<string, int> item)
        => throw new NotImplementedException();

    public int Count => _list.Count;
    public bool IsReadOnly { get; } = false;
}