using UnityEngine;

namespace GadgetSystem.Gadgets_Var
{
    public class SlowingGadget : Gadget
    {
        private                  Camera     currentCamera;
        [SerializeField] private Rigidbody  rb;
        [SerializeField] private LayerMask  surfaceLayer;
        [SerializeField] private float      slowFactor;
        [SerializeField] private GameObject slowZone;
        [SerializeField] private float      launchSpeed;
        private                  bool       isUsed;
        
        protected override void OnUse()
        {
            isUsed = true;
            transform.SetParent(null);
            rb.isKinematic                  = false;
            gadgetController.selectedGadget = null;
            Ray ray = new Ray(currentCamera.transform.position, currentCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                rb.AddForce((ray.direction + Vector3.up) * launchSpeed, ForceMode.Impulse);
            }
            else
            {
                rb.AddForce((ray.direction + Vector3.up), ForceMode.Impulse);
            }
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (!isUsed) return;
            Debug.Log("Collision");
            if (collision.gameObject.layer == surfaceLayer)
            {
                GameObject newSlowZone = Instantiate(this.slowZone,  collision.contacts[0].point, Quaternion.identity);
                //StartCoroutine(EffectManager.instance.SmoothSpawnEffect(newSlowZone, 0.25f));
                Destroy(gameObject);
                Debug.Log("Create SlowZone");
            }
            
        }
        
    
        public override void OnPickup(GadgetController gc)
        {
            currentCamera    = gc.player.playerCamera;
            gadgetController = gc;
            col.enabled = false;
            rb.isKinematic   = true;
        }

        public override void Drop()
        {
            base.Drop();
            transform.SetParent(null);
            rb.isKinematic = false;
            col.enabled = true;
            rb.AddForce((Vector3.up + transform.forward)* 5, ForceMode.Impulse);
        }
    }
}