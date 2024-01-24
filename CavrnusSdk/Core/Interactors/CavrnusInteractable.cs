using UnityEngine;
using UnityEngine.Events;

namespace CavrnusSdk.Common
{
    public class CavrnusInteractable : MonoBehaviour
    {
        [SerializeField] private UnityEvent onInteract;

        public void Interact() => onInteract?.Invoke();
    }
}