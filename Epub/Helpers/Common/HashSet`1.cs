using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Helpers.Common
{
  public class HashSet<T> : ISet<T>, ICollection<T>, IEnumerable<T>, IEnumerable
  {
    private Dictionary<T, bool> _data;

    public IEqualityComparer<T> Comparer
    {
      get
      {
        return this._data.Comparer;
      }
    }

    public int Count
    {
      get
      {
        return this._data.Count;
      }
    }

    bool ICollection<T>.IsReadOnly
    {
      get
      {
        return false;
      }
    }

    public HashSet()
    {
      this._data = new Dictionary<T, bool>();
    }

    public HashSet(IEnumerable<T> collection)
    {
      if (collection == null)
        throw new ArgumentNullException("collection", "parameter must not be null");
      this._data = new Dictionary<T, bool>();
      this.AddRange(collection);
    }

    public HashSet(IEqualityComparer<T> comparer)
    {
      this._data = new Dictionary<T, bool>(comparer);
    }

    public HashSet(IEnumerable<T> collection, IEqualityComparer<T> comparer)
    {
      this._data = new Dictionary<T, bool>(comparer);
      this.AddRange(collection);
    }

    public bool Add(T item)
    {
      if (this._data.ContainsKey(item))
        return false;
      this._data.Add(item, true);
      return true;
    }

    public void Clear()
    {
      this._data.Clear();
    }

    public bool Contains(T item)
    {
      return this._data.ContainsKey(item);
    }

    public void CopyTo(T[] array)
    {
      if (array == null)
        throw new ArgumentNullException("array");
      this.AddRange((IEnumerable<T>) array);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
      if (array == null)
        throw new ArgumentNullException("array");
      if (arrayIndex < 0)
        throw new ArgumentOutOfRangeException("arrayIndex");
      if (arrayIndex >= array.Length)
        throw new ArgumentException("arrayIndex must not be greater than or equal to the array's length");
      this.AddRange(Enumerable.Skip<T>((IEnumerable<T>) array, arrayIndex));
    }

    public void CopyTo(T[] array, int arrayIndex, int count)
    {
      if (array == null)
        throw new ArgumentNullException("array");
      if (arrayIndex < 0)
        throw new ArgumentOutOfRangeException("arrayIndex");
      if (count < 0)
        throw new ArgumentOutOfRangeException("count", "count must be greater than zero");
      if (arrayIndex >= array.Length)
        throw new ArgumentException("arrayIndex must not be greater than or equal to the array's length");
      if (arrayIndex + count > array.Length)
        throw new ArgumentException("arrayIndex and count specify more items than possible");
      this.AddRange(Enumerable.Take<T>(Enumerable.Skip<T>((IEnumerable<T>) array, arrayIndex), count));
    }

    public void ExceptWith(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");
      foreach (T key in other)
        this._data.Remove(key);
    }

    public IEnumerator<T> GetEnumerator()
    {
      return (IEnumerator<T>) this._data.Keys.GetEnumerator();
    }

    public void IntersectWith(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");
      List<T> list = Enumerable.ToList<T>(Enumerable.Where<T>((IEnumerable<T>) this._data.Keys, new Func<T, bool>((other.Contains<T>))));
      this._data = new Dictionary<T, bool>();
      this.AddRange((IEnumerable<T>) list);
    }

    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");
      HashSet<T> hashSet = new HashSet<T>(other);
      if (this.Count < hashSet.Count)
        return Enumerable.All<T>((IEnumerable<T>) this._data.Keys, new Func<T, bool>(hashSet.Contains));
      return false;
    }

    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");
      if (this.Count == 0)
        return false;
      HashSet<T> hashSet = new HashSet<T>(other);
      if (this.Count > hashSet.Count)
        return Enumerable.All<T>((IEnumerable<T>) hashSet, new Func<T, bool>(this.Contains));
      return false;
    }

    public bool IsSubsetOf(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");
      if (this.Count == 0)
        return true;
      HashSet<T> hashSet = new HashSet<T>(other);
      if (this.Count <= hashSet.Count)
        return Enumerable.All<T>((IEnumerable<T>) this._data.Keys, new Func<T, bool>(hashSet.Contains));
      return false;
    }

    public bool IsSupersetOf(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");
      return Enumerable.All<T>(other, new Func<T, bool>(this.Contains));
    }

    public bool Overlaps(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");
      return Enumerable.Any<T>(other, new Func<T, bool>(this.Contains));
    }

    public bool Remove(T item)
    {
      if (!this._data.ContainsKey(item))
        return false;
      this._data.Remove(item);
      return true;
    }

    public int RemoveWhere(Predicate<T> match)
    {
      if (match == null)
        throw new ArgumentNullException("match");
      List<T> list = Enumerable.ToList<T>(Enumerable.Where<T>((IEnumerable<T>) this._data.Keys, (Func<T, bool>) (i => match(i))));
      int num = this.Count - list.Count;
      this.Clear();
      this.AddRange((IEnumerable<T>) list);
      return num;
    }

    public bool SetEquals(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");
      HashSet<T> hashSet = new HashSet<T>();
      foreach (T obj in other)
      {
        if (!this.Contains(obj))
          return false;
        hashSet.Add(obj);
      }
      return hashSet.Count == this.Count;
    }

    public void SymmetricExceptWith(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");
      foreach (T obj in Enumerable.Where<T>(other, new Func<T, bool>(this.Contains)))
        this.Remove(obj);
    }

    public void TrimExcess()
    {
    }

    public void UnionWith(IEnumerable<T> other)
    {
      if (other == null)
        throw new ArgumentNullException("other");
      this.AddRange(other);
    }

    private void AddRange(IEnumerable<T> items)
    {
      if (items == null)
        throw new ArgumentNullException("items");
      foreach (T obj in items)
        this.Add(obj);
    }

    void ICollection<T>.Add(T item)
    {
      this.Add(item);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return (IEnumerator) this._data.Keys.GetEnumerator();
    }
  }
}
