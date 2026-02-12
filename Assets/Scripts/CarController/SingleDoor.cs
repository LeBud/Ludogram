using UnityEngine;

namespace CarScripts {
    public class SingleDoor : MonoBehaviour {
        [SerializeField] private CarDoors controller;

        public void UseDoor() {
            controller.UseDoor();
        }
    }
}