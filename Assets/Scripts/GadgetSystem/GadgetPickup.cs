using CarScripts;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace GadgetSystem {
    public class GadgetPickup : MonoBehaviour {
        
        private                                                            Controller       player;
        [FormerlySerializedAs("playerInventory")] [SerializeField] private GadgetController playerController;
        [SerializeField]                                           private Transform        gadgetTransform;
        [SerializeField]                                           private LayerMask        interactableLayerMask;
        [SerializeField]                                           private float            pickupRange = 2f;

        private const int MAX_PICKUP_COUNT = 5;
        private Collider[] hitColliders;

        public void Initialize(Controller p) {
            player = p;
            player.GetInputs().pickUp.started += _ => TryPickupNearbyGadget();
        }


        #region InputSystem

        void OnEnable() {
            
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

            //Physics.Raycast(player.playerCamera.transform.position, player.playerCamera.transform.forward, out var hit, pickupRange, interactableLayerMask);
            Ray baseCast = new Ray(player.playerCamera.transform.position, player.playerCamera.transform.forward);
            Physics.SphereCast(baseCast, 0.25f, out var hit, pickupRange, interactableLayerMask);
            
            //Debug.Log(closestObj.name + "est le plus proche : " + Vector3.Distance(transform.position, closestObj.position));
            //Debug.Log(hit.collider.gameObject.name);
            
            if (hit.collider.TryGetComponent(out CarController car))
            {
                player.SetCarController(car);
                player.PlayerStateMachine.ChangeState(Controller.ControlerState.Driving);
                return;
            }
            
            Transform hitted = hit.collider.transform;
            if (playerController.AddGadget(hit.collider.GetComponent<IGadget>()) 
                && hit.collider.gameObject != playerController.handledObject) 
            {
                hitted.position = gadgetTransform.position;
                hitted.forward = gadgetTransform.forward;
                playerController.handledObject = hitted.gameObject;
                hitted.SetParent(gadgetTransform);
                hitted.GetComponent<Gadget>().OnPickup();
                //Debug.Log("Ramassé:" + gadget.Name);
            }
            else {
                Debug.Log("vous possedez dez déjà cet objet !");
            }
        }

        void OnDrawGizmos() {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, pickupRange);
            Gizmos.color = Color.red;
            //Gizmos.DrawWireSphere(gadgetTransform.position, 0.5f);
            Gizmos.DrawWireCube(gadgetTransform.position, Vector3.one);
            Gizmos.color = Color.green;
            if(player != null )Gizmos.DrawRay(player.playerCamera.transform.position, player.playerCamera.transform.forward * pickupRange);
        }
    }
}