using CarScripts;
using Player;
using UnityEngine;

namespace GadgetSystem {
    public class GadgetPickup : MonoBehaviour {
        private                  Controller       player;
        [SerializeField] public GadgetController gadgetController;
        [SerializeField] private Transform        gadgetTransform;
        public static Transform        gadgetStaticTransform;
        [SerializeField] private LayerMask        interactableLayerMask;
        [SerializeField] private float            pickupRange = 2f;
        
        private Collider[] hitColliders;
        
        public void Initialize(Controller p) {
            player = p;
        }

        void Start()
        {
            gadgetStaticTransform = gadgetTransform;
        }
        #region InputSystem

        void OnEnable() {
            
        }

        void OnDisable() {
            player.GetInputs().pickUp.performed -= _ => TryPickupNearbyGadget();
        }

        #endregion


        [ContextMenu("Pickup")]
        public void TryPickupNearbyGadget() {
            if(player.isSeated || player.isDriving) return;
            
            var baseCast = new Ray(player.playerCamera.transform.position, player.playerCamera.transform.forward);
            //Physics.SphereCast(baseCast, 0.25f, out var hit, pickupRange, interactableLayerMask);
            Physics.Raycast(baseCast, out var hit, pickupRange, interactableLayerMask);
            
            if(!hit.collider) return;
            
            if (hit.collider.TryGetComponent(out CarSeat seat) && !seat.playerAlreadySeated) {
                seat.SetDriver(player);
                return;
            }
            
            var hitted = hit.collider.transform;
            if (gadgetController.AddGadget(hit.collider.GetComponent<IGadget>())) {
                Gadget gadget = hitted.GetComponent<Gadget>();
                gadgetController.gadgetObject = hit.collider.gameObject;
                gadget.target                 = gadgetTransform;
                gadget.transform.forward      = gadgetTransform.forward;
                gadget.OnPickup();
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