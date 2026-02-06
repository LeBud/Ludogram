using Player;
using UnityEngine;

namespace CarScripts {
    public class CarSeat : MonoBehaviour {
        [SerializeField] private CarController carController;
        [SerializeField] private bool driverSeat = false;
        [SerializeField] private Transform playerPos;
        
        public bool playerAlreadySeated { get; private set; }
        
        private void Awake() {
            if(carController == null) Debug.LogError("CarController is null, please reference it");
        }

        public void SetDriver(Controller player) {
            if (driverSeat) {
                player.SetCarController(carController, this);
                playerAlreadySeated = true;
                player.isDriving      = true;
            }
            else {
                player.SetCarController(carController, this);
                playerAlreadySeated = true;
                player.isSeated      = true;
            }
            
        }

        public Transform GetPlayerPos() {
            return playerPos;
        }

        public void UnSeatDriver() {
            playerAlreadySeated = false;
        }
        
    }
}