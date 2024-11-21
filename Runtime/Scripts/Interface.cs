using System;
using System.Collections.Generic;

using UnityEngine;

namespace Zlitz.General.Serializables
{
    [Serializable]
    public struct Interface<T> where T : class
    {
        [SerializeField]
        private bool m_useUnityObject;

        [SerializeField]
        private UnityEngine.Object m_unityObject;

        [SerializeField]
        private long m_managedReferenceId;

        [SerializeReference]
        private T m_value;

        public T value
        {
            get
            {
                if (m_useUnityObject)
                {
                    if (m_unityObject is T unityObjectInterface)
                    {
                        return unityObjectInterface;
                    }
                    return null;
                }

                return m_value;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Interface<T> other)
            {
                return EqualityComparer<T>.Default.Equals(value, other.value);
            }
            return obj == null && value == null;
        }

        public override int GetHashCode()
        {
            return value?.GetHashCode() ?? 0;
        }
    }
}
