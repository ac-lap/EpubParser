using System;
using System.Collections;
using System.Collections.Generic;

namespace EpubReader.Collections
{
  public class OrderedDictionary : IOrderedDictionary, IDictionary, ICollection, IEnumerable
  {
    private Dictionary<object, object> store;
    private List<object> ordered;

    public object this[int index]
    {
      get
      {
        return this.store[this.ordered[index]];
      }
      set
      {
        this.store[this.ordered[index]] = value;
      }
    }

    public bool IsFixedSize
    {
      get
      {
        return ((IDictionary) this.store).IsFixedSize;
      }
    }

    public bool IsReadOnly
    {
      get
      {
        return ((IDictionary) this.store).IsFixedSize;
      }
    }

    public ICollection Keys
    {
      get
      {
        return (ICollection) this.store.Keys;
      }
    }

    public ICollection Values
    {
      get
      {
        return (ICollection) this.store.Values;
      }
    }

    public object this[object key]
    {
      get
      {
        return this.store[key];
      }
      set
      {
        this.store[key] = value;
      }
    }

    public int Count
    {
      get
      {
        return this.store.Count;
      }
    }

    public bool IsSynchronized
    {
      get
      {
        return ((ICollection) this.store).IsSynchronized;
      }
    }

    public object SyncRoot
    {
      get
      {
        return ((ICollection) this.store).SyncRoot;
      }
    }

    public OrderedDictionary()
    {
      this.store = new Dictionary<object, object>();
      this.ordered = new List<object>();
    }

    public OrderedDictionary(IDictionary d)
    {
      this.store = new Dictionary<object, object>();
      this.ordered = new List<object>();
      foreach (object key in (IEnumerable) d.Keys)
      {
        this.store.Add(key, d[key]);
        this.ordered.Add(key);
      }
    }

    public OrderedDictionary(IEqualityComparer equalityComparer)
    {
      this.store = new Dictionary<object, object>(equalityComparer as IEqualityComparer<object>);
      this.ordered = new List<object>();
    }

    public OrderedDictionary(int capacity)
    {
      this.store = new Dictionary<object, object>(capacity);
      this.ordered = new List<object>();
    }

    public void Insert(int index, object key, object value)
    {
      this.ordered.Insert(index, key);
      this.store.Add(key, value);
    }

    public void RemoveAt(int index)
    {
      throw new NotImplementedException();
    }

    public void Add(object key, object value)
    {
      this.store.Add(key, value);
      this.ordered.Add(key);
    }

    public void Clear()
    {
      this.store.Clear();
    }

    public bool Contains(object key)
    {
      return this.store.ContainsKey(key);
    }

    public IDictionaryEnumerator GetEnumerator()
    {
      return (IDictionaryEnumerator) this.store.GetEnumerator();
    }

    public void Remove(object key)
    {
      this.store.Remove(key);
    }

    public void CopyTo(Array array, int index)
    {
      ((ICollection) this.store).CopyTo(array, index);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable) this.store).GetEnumerator();
    }
  }
}
