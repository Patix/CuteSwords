using System;
using System.Collections.Generic;
using UnityEngine;

namespace Imported
{
    
    /// <summary>
    /// Dictionary that can serialize keys and values as other types
    /// </summary>
    /// <typeparam name="K">The key type</typeparam>
    /// <typeparam name="V">The value type</typeparam>
    [Serializable]
    public abstract class SerializedDictionary<K, V> : Dictionary<K, V>, ISerializationCallbackReceiver
    {
        [SerializeField]
        List<K> m_Keys = new List<K>();

        [SerializeField]
        List<V> m_Values = new List<V>();

        /// <summary>
        /// OnBeforeSerialize implementation.
        /// </summary>
        public void OnBeforeSerialize()
        {
            m_Keys.Clear();
            m_Values.Clear();

            foreach (var kvp in this)
            {
                m_Keys.Add(kvp.Key);
                m_Values.Add(kvp.Value);
            }
        }

        /// <summary>
        /// OnAfterDeserialize implementation.
        /// </summary>
        public void OnAfterDeserialize()
        {
            Clear();
            
            for (int i = 0; i < m_Keys.Count; i++)
                Add(m_Keys[i], m_Values[i]);

            m_Keys.Clear();
            m_Values.Clear();
        }
    }
}
