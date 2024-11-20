using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Zlitz.General.Serializables
{
    [Serializable]
    public class Dict<TKey, TValue> : IDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<Pair<TKey, TValue>> m_list = new List<Pair<TKey, TValue>>();

        private Dictionary<TKey, TValue> m_dict;

        #region ISerializationCallbackReceiver

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (m_list == null)
            {
                m_list = new List<Pair<TKey, TValue>>();
            }

            HashSet<TKey> handledKeys = new HashSet<TKey>(EqualityComparer<TKey>.Default);

            for (int i = 0; i < m_list.Count; i++) 
            {
                TKey key = m_list[i].key;
                if (handledKeys.Add(key))
                {
                    if (!m_dict.TryGetValue(key, out TValue value))
                    {
                        m_list.RemoveAt(i);
                        i--;
                        continue;
                    }
                    m_list[i] = new Pair<TKey, TValue>(key, value);
                }
            }

            HashSet<KeyValuePair<TKey, TValue>> missing = new HashSet<KeyValuePair<TKey, TValue>>(m_dict, new PairComparer());
            missing.ExceptWith(m_list.Select(i => new KeyValuePair<TKey, TValue>(i.key, i.value)));
            m_list.AddRange(missing.Select(i => new Pair<TKey, TValue>(i.Key, i.Value)));
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            m_dict.Clear();
            foreach (Pair<TKey, TValue> item in m_list)
            {
                if (item.key == null)
                {
                    continue;
                }
                m_dict.TryAdd(item.key, item.value);
            }
        }

        #endregion

        #region IDictionary<TKey, TValue>

        public Dict()
        {
            m_dict = new Dictionary<TKey, TValue>(EqualityComparer<TKey>.Default);
        }

        public Dict(IDictionary<TKey, TValue> dictionary)
        {
            m_dict = new Dictionary<TKey, TValue>(dictionary, EqualityComparer<TKey>.Default);
        }

        public Dict(IEnumerable<KeyValuePair<TKey, TValue>> collection)
        {
            m_dict = new Dictionary<TKey, TValue>(collection, EqualityComparer<TKey>.Default);
        }

        public ICollection<TKey> Keys => m_dict.Keys;

        public ICollection<TValue> Values => m_dict.Values;

        public int Count => m_dict.Count;

        public bool IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)m_dict).IsReadOnly;

        public TValue this[TKey key] 
        { 
            get => m_dict[key]; 
            set => m_dict[key] = value; 
        }

        public void Add(TKey key, TValue value)
        {
            if (key == null)
            {
                throw new ArgumentNullException("Value cannot be null. (Parameter 'key')");
            }
            if (m_dict.TryAdd(key, value))
            {
                if (m_list == null)
                {
                    m_list = new List<Pair<TKey, TValue>>();
                }
                m_list.Add(new Pair<TKey, TValue>(key, value));
                return;
            }
            throw new ArgumentException("An item with the same key has already been added.");
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public bool ContainsKey(TKey key)
        {
            return m_dict.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            if (m_dict.Remove(key))
            {
                return true;
            }

            if (m_list != null)
            {
                for (int i = 0; i < m_list.Count; i++)
                {
                    if (EqualityComparer<TKey>.Default.Equals(m_list[i].key, key))
                    {
                        m_list.RemoveAt(i);
                        break;
                    }
                }
            }

            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return m_dict.TryGetValue(key, out value);
        }

        public void Clear()
        {
            m_dict.Clear();
            m_list?.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)m_dict).Contains(item);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)m_dict).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (((ICollection<KeyValuePair<TKey, TValue>>)m_dict).Remove(item))
            {
                if (m_list != null)
                {
                    for (int i = 0; i < m_list.Count; i++)
                    {
                        if (EqualityComparer<TKey>.Default.Equals(m_list[i].key, item.Key))
                        {
                            m_list.RemoveAt(i);
                            break;
                        }
                    }
                }

                return true;
            }

            return false;
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return m_dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_dict.GetEnumerator();
        }

        #endregion

        private class PairComparer : IEqualityComparer<KeyValuePair<TKey, TValue>>
        {
            private EqualityComparer<TKey> m_keyComparer = EqualityComparer<TKey>.Default;

            public bool Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
            {
                return m_keyComparer.Equals(x.Key, y.Key);
            }

            public int GetHashCode(KeyValuePair<TKey, TValue> obj)
            {
                return m_keyComparer.GetHashCode(obj.Key);
            }
        }
    }
}
