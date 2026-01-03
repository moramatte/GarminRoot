using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Infrastructure.Collections;

/// <summary>
/// Wrapper for List<T> where we want its contents compared and not the object reference
/// </summary>
[Serializable]
public struct EquatableList<T> : IEquatable<EquatableList<T>>, IEnumerable<T>
{
    private List<T> _list;

    public EquatableList()
    {
        _list = [];
    }

    public EquatableList(IEnumerable<T> items)
    {
        _list = items?.ToList() ?? [];
    }

    public readonly List<T> List => _list;

    public readonly bool Equals(EquatableList<T> other)
    {
        if (_list == null && other._list == null) return true;
        if (_list == null || other._list == null) return false;
        if (_list.Count != other._list.Count) return false;

        // Multiset comparison
        var dict = new Dictionary<T, int>();
        foreach (var item in _list)
        {
            dict.TryGetValue(item, out int count);
            dict[item] = count + 1;
        }

        foreach (var item in other._list)
        {
            if (!dict.TryGetValue(item, out int count)) return false;
            if (count == 1) dict.Remove(item);
            else dict[item] = count - 1;
        }

        return dict.Count == 0;
    }

    public override readonly bool Equals(object obj) => obj is EquatableList<T> vl && Equals(vl);

	public readonly IEnumerator<T> GetEnumerator()
	{
		return ((IEnumerable<T>)_list).GetEnumerator();
	}

	public override readonly int GetHashCode() => _list?.GetHashCode() ?? 0;

	readonly IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)_list).GetEnumerator();
	}

	// This method is required for serialization of this class to work. See https://stackoverflow.com/questions/16846898/c-to-be-xml-serializable-types-which-inherit-from-ienumerable-must-have-an-im
	public readonly void Add(T item) => _list.Add(item);

	public static bool operator ==(EquatableList<T> left, EquatableList<T> right) => left.Equals(right);
    public static bool operator !=(EquatableList<T> left, EquatableList<T> right) => !left.Equals(right);
}
