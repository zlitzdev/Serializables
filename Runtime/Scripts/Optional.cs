using System;
using System.Collections.Generic;

using UnityEngine;

namespace Zlitz.General.Serializables
{
    [Serializable]
    public struct Optional<T>
    {
        [SerializeField]
        private bool m_hasValue;

        [SerializeField]
        private T m_value;

        public bool hasValue => m_hasValue;

        public T value
        {
            get
            {
                Debug.Assert(m_hasValue, $"Optional<T> doesn't contain a value");
                return m_value;
            }
        }

        public bool TryGetValue(out T value)
        {
            if (m_hasValue)
            {
                value = m_value;
                return true;
            }
            value = default(T);
            return false;
        }

        public static Optional<T> None() => new Optional<T>();

        public static Optional<T> Of(T value) => new Optional<T> { m_hasValue = true, m_value = value };

        public static bool operator==(Optional<T> lhs, Optional<T> rhs)
        {
            return
                (!lhs.m_hasValue && !rhs.m_hasValue) ||
                (lhs.m_hasValue && rhs.m_hasValue && EqualityComparer<T>.Default.Equals(lhs.m_value, rhs.m_value));
        }

        public static bool operator!=(Optional<T> lhs, Optional<T> rhs)
        {
            return !(lhs == rhs);
        }

        public override int GetHashCode()
        {
            if (!m_hasValue)
            {
                return 0;
            }
            return (typeof(T), m_value).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is Optional<T> optional)
            {
                return this == optional;
            }
            return false;
        }

        public override string ToString()
        {
            if (m_hasValue)
            {
                return $"Optional<{typeof(T).FullName}>.Of({m_value})";
            }
            return $"Optional<{typeof(T).FullName}>.None";
        }
    }
}
