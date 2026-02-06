using UnityEngine;

namespace GadgetSystem.Gadgets_Var
{
    public class SlowingGadget : Gadget
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private float     slowFactor;
        
        protected override void OnUse()
        {
            
        }
    
    
        public override void Release()
        {
            
        }
    
        public override void OnPickup()
        {
            rb.isKinematic = true;
        }

        public override void Drop()
        {
            transform.SetParent(null);
            rb.isKinematic = false;
            rb.AddForce((Vector3.up + transform.forward)* 5, ForceMode.Impulse);
        }
    }
}