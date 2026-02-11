using System;
using UnityEngine;

namespace CarScripts {
    public class CarDoors : MonoBehaviour {
        
        public static CarDoors instance {get; private set;}

        [Header("DoorSettings")] 
        [SerializeField] private Animator anim;
        
        public bool areDoorsOpen {get; private set;}

        private void Awake() {
            if(instance == null) instance = this;
            else Destroy(this);
        }

        public void UseDoor() {
            areDoorsOpen = !areDoorsOpen;

            if (areDoorsOpen) anim.SetBool("Open", true);
            else anim.SetBool("Open", false);
        }
    }
}