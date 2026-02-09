using UnityEngine;

public class Menottes : Gadget
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private int       moneyValue;
    [SerializeField] private LayerMask enemisLayerMask;
    [SerializeField] private float     launchSpeed;
    private                  bool      isUsed = false;

    protected override void OnUse()
    {
        isUsed = true;
        target = null;
        transform.SetParent(null);
        rb.isKinematic = false;
        rb.AddForce(transform.forward * launchSpeed, ForceMode.Impulse);
    }

    public override void OnPickup(GadgetController gc)
    {
        gadgetController = gc;
        rb.isKinematic = true;
        isUsed         = false;
    }

    public override void Drop()
    {
        base.Drop();
        if (isUsed) return;
        transform.SetParent(null);
        rb.isKinematic = false;
        rb.AddForce((Vector3.up + transform.forward)* 5, ForceMode.Impulse);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == enemisLayerMask)
        {
            // if (collision.gameObject.TryGetComponent(out EnemyController robber) && robber.)
            // {
            //     Destroy(gameObject);
            // }
        }
    }
    
    
}
