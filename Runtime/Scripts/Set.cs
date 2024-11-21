using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Zlitz.General.Serializables
{
    [Serializable]
    public class Set<T> : ISet<T>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private System.Collections.Generic.List<T> m_list = new System.Collections.Generic.List<T>();

        private HashSet<T> m_set;

        #region ISerializationCallbackReceiver

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (m_list == null)
            {
                m_list = new System.Collections.Generic.List<T>();
            }

            HashSet<T> missing = new HashSet<T>(m_set);
            missing.ExceptWith(m_list);
            m_list.AddRange(missing);
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            m_set.Clear();
            m_set.UnionWith(m_list.Where(e => e != null));
        }

        #endregion

        #region ISet<T>

        public Set()
        {
            m_set = new HashSet<T>(EqualityComparer<T>.Default);
        }

        public Set(IEnumerable<T> collection)
        {
            m_set = new HashSet<T>(collection, EqualityComparer<T>.Default);
        }

        public int Count => m_set.Count;

        public bool IsReadOnly => ((ICollection<T>)m_set).IsReadOnly;

        public bool Add(T item)
        {
            if (m_set.Add(item))
            {
                m_list.Add(item);
                return true;
            }

            return false;
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            m_set.ExceptWith(other);
            SyncListOnSetItemsRemoved();
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            m_set.IntersectWith(other);
            SyncListOnSetItemsRemoved();
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            return m_set.IsProperSubsetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            return m_set.IsProperSupersetOf(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            return m_set.IsSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            return m_set.IsSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            return m_set.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            return m_set.SetEquals(other);
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            m_set.SymmetricExceptWith(other);
        }

        public void UnionWith(IEnumerable<T> other)
        {
            m_set.UnionWith(other);
        }

        public void Clear()
        {
            m_set.Clear();
            m_list.Clear();
        }

        public bool Contains(T item)
        {
            return m_set.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            m_set.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            if (m_set.Remove(item))
            {
                m_list.Remove(item);
                return true;
            }

            return false;
        }

        void ICollection<T>.Add(T item)
        {
            m_set.Add(item);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return m_set.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_set.GetEnumerator();
        }

        #endregion

        private void SyncListOnSetItemsRemoved()
        {
            for (int i = m_list.Count - 1; i >= 0; i--)
            {
                if (m_set.Contains(m_list[i]))
                {
                    continue;
                }
                m_list.RemoveAt(i);
            }
        }
    }
}
