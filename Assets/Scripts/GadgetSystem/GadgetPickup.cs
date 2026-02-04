using System;
using CarScripts;
using UnityEngine;

namespace GadgetSystem {
    public class GadgetPickup : MonoBehaviour {
        private Controller player;
        [SerializeField] private GadgetInventory playerInventory;
        [SerializeField] private Transform gadgetTransform;
        [SerializeField] private LayerMask interactableLayerMask;
        [SerializeField] private float pickupRange = 2f;

        private const int MAX_PICKUP_COUNT = 5;
        private Collider[] hitColliders;

        private void Awake() {
            if (TryGetComponent(out player)) {
                
            }
            else 
                Debug.LogError("Player component not found!");
        }


        #region InputSystem

        void OnEnable() {
            player.GetInputs().pickUp.started += _ => TryPickupNearbyGadget();
        }

        void OnDisable() {
            player.GetInputs().pickUp.started -= _ => TryPickupNearbyGadget();
        }

        #endregion


        [ContextMenu("Pickup")]
        private void TryPickupNearbyGadget() {
            //Ray

            // hitColliders = new Collider[MAX_PICKUP_COUNT];
            // int numColliders =
            //     Physics.OverlapSphereNonAlloc(transform.position, pickupRange, hitColliders, gadgetLayerMask);
            //
            // Transform closestObj = hitColliders[0].transform;
            // IGadget gadget = closestObj ? closestObj.GetComponent<IGadget>() : null;
            //
            // for (int i = 0; i < numColliders; i++) {
            //     if (closestObj == null) continue;
            //     if (Vector3.Distance(transform.position, hitColliders[i].transform.position) <
            //         Vector3.Distance(transform.position, closestObj.position)
            //         && !playerInventory.gadgets.Contains(gadget)) {
            //         closestObj = hitColliders[i].transform;
            //         gadget = closestObj.GetComponent<IGadget>();
            //     }
            // }

            Physics.Raycast(player.playerCamera.transform.position, player.playerCamera.transform.forward, out var hit, pickupRange, interactableLayerMask);
            
            //Debug.Log(closestObj.name + "est le plus proche : " + Vector3.Distance(transform.position, closestObj.position));

            if (hit.collider.TryGetComponent(out CarController car)) {
                player.PlayerStateMachine.ChangeState(Controller.ControlerState.Driving);
                player.SetCarController(car);
                return;
            }
            
            Transform hitted = hit.collider.transform;
            if (playerInventory.AddGadget(hit.collider.GetComponent<IGadget>())) {
                hitted.position = gadgetTransform.position;
                hitted.forward = gadgetTransform.forward;
                hitted.SetParent(gadgetTransform);
                hitted.GetComponent<Gadget>().OnPickup();
                //Debug.Log("Ramassé:" + gadget.Name);
            }
            else {
                Debug.Log("Inventaire plein !");
            }
        }

        void OnDrawGizmos() {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, pickupRange);
            Gizmos.color = Color.red;
            //Gizmos.DrawWireSphere(gadgetTransform.position, 0.5f);
            Gizmos.DrawWireCube(gadgetTransform.position, Vector3.one);
        }
    }
}