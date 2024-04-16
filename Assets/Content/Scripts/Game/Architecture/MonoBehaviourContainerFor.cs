using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Content.Scripts.Game.Architecture
{
    public class MonoBehaviourContainerFor<T> : MonoBehaviour
    {
        [SerializeReference, HideLabel] public T InternalComponent;
    }
}