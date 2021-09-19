using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace ScheduleKeeper.WPF.Types.Collections;

public class FuncDictionary<TKey, TValue> :
    IDeserializationCallback,
    ISerializable,
    IReadOnlyCollection<KeyValuePair<TKey, TValue>>,
    IReadOnlyCollection<KeyValuePair<TKey, Func<TValue>>>,
    IEnumerable,
    IEnumerable<KeyValuePair<TKey, TValue>>,
    IEnumerable<KeyValuePair<TKey, Func<TValue>>>,
    IReadOnlyDictionary<TKey, TValue>,
    IReadOnlyDictionary<TKey, Func<TValue>>
    where TKey : notnull
{
    private readonly Dictionary<TKey, Func<TValue>> _dictionary = new();

    public TValue this[TKey key] => _dictionary[key]();
    Func<TValue> IReadOnlyDictionary<TKey, Func<TValue>>.this[TKey key] => _dictionary[key];

    public IEnumerable<TKey> Keys => _dictionary.Keys;

    public int Count => _dictionary.Count;

    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

    public bool IsSynchronized => false;

    public object SyncRoot { get; } = new();

    public bool IsFixedSize => false;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => GetEnumerable().GetEnumerator();
    public IEnumerable<KeyValuePair<TKey, TValue>> GetEnumerable()
    {
        foreach (var x in _dictionary)
            yield return new KeyValuePair<TKey, TValue>(x.Key, x.Value());
    }

    IEnumerator<KeyValuePair<TKey, Func<TValue>>> IEnumerable<KeyValuePair<TKey, Func<TValue>>>.GetEnumerator() => _dictionary.GetEnumerator();

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        value = default;
        if (_dictionary.TryGetValue(key, out var x))
        {
            value = x();
            return true;
        }

        return false;
    }

    public void Add(TKey key, Func<TValue> value) => _dictionary.Add(key, value);

    public bool Remove(TKey key) => _dictionary.Remove(key);

    public void Clear() => _dictionary.Clear();

    public bool Contains(KeyValuePair<TKey, Func<TValue>> item) => _dictionary.Contains(item);

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out Func<TValue> value) => _dictionary.TryGetValue(key, out value);

    public virtual void OnDeserialization(object? sender) => _dictionary.OnDeserialization(sender);

    public void GetObjectData(SerializationInfo info, StreamingContext context) => _dictionary.GetObjectData(info, context);

    public IEnumerable<TValue> Values
    {
        get
        {
            foreach (var value in _dictionary.Values)
                yield return value();
        }
    }

    IEnumerable<Func<TValue>> IReadOnlyDictionary<TKey, Func<TValue>>.Values => _dictionary.Values;
}
