using System.Collections;

namespace EpubReader.Collections
{
  public interface IOrderedDictionary : IDictionary, ICollection, IEnumerable
  {
    object this[int index] { get; set; }

    IDictionaryEnumerator GetEnumerator();

    void Insert(int index, object key, object value);

    void RemoveAt(int index);
  }
}
