using UnityEngine;

namespace CarScripts {
    public class CarDoors : MonoBehaviour {

        [Header("DoorSettings")] 
        [SerializeField] private Animator anim;
        
        public bool areDoorsOpen = false;
        
        public void UseDoor() {
            areDoorsOpen = !areDoorsOpen;

            if (areDoorsOpen) anim.SetBool("Open", true);
            else anim.SetBool("Open", false);
        }
    }
}