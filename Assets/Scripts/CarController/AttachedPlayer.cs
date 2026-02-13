using System.Collections.Generic;
using Manager;
using Player;
using UnityEngine;

namespace CarScripts {
    public class AttachedPlayer : MonoBehaviour {
        [SerializeField] private Transform carRef;
        
        private HashSet<Controller> controllers = new();
        
        private void Update() {
            FollowCar();
        }

        private void FollowCar() {
            transform.position = carRef.position;
            transform.rotation = carRef.rotation;
        }

        private void OnTriggerEnter(Collider other) {
            if (other.transform.TryGetComponent(out Controller player)) {
                controllers.Add(player);
                player.SetPlayerInCar(transform);
                if(player.pickUp.gadgetController.selectedGadget as MoneyBag)
                    GameManager.instance.moneyManager.RegisterBagInCar(player.pickUp.gadgetController.selectedGadget as MoneyBag);
            }

            if (other.transform.TryGetComponent(out MoneyBag bag)) {
                GameManager.instance.moneyManager.RegisterBagInCar(bag);
            }
        }

        private void OnTriggerExit(Collider other) {
            if (other.transform.TryGetComponent(out Controller player)) {
                if(controllers.Contains(player)) controllers.Remove(player);
                player.RemovePlayerFromCar();
                if(player.pickUp.gadgetController.selectedGadget as MoneyBag)
                    GameManager.instance.moneyManager.DeregisterBagInCar(player.pickUp.gadgetController.selectedGadget as MoneyBag);
            }
            
            if (other.transform.TryGetComponent(out MoneyBag bag)) {
                GameManager.instance.moneyManager.DeregisterBagInCar(bag);
            }
        }

        public void ApplyForceToController(Vector3 force) {
            foreach (var c in controllers) {
                c.GetRB().AddForce(force, ForceMode.VelocityChange);
            }
        }
    }
}