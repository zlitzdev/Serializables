using System;

using UnityEngine;

namespace Zlitz.General.Serializables
{
    [Serializable]
    public struct Pair<TKey, TValue>
    {
        [SerializeField]
        private TKey m_key;

        [SerializeField]
        private TValue m_value;

        public TKey key => m_key;

        public TValue value => m_value;

        public Pair(TKey key, TValue value)
        {
            m_key   = key;
            m_value = value;
        }
    }
}
