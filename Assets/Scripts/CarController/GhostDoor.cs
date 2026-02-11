using UnityEngine;

namespace CarScripts {
    public class GhostDoor : MonoBehaviour {
        [SerializeField] private Transform targetDoor;

        private void Update() {
            transform.rotation = targetDoor.rotation;
        }
    }
}