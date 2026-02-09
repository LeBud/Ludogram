using UnityEngine;

public class MoneyBag : Gadget
{
    public  Rigidbody rb;
    public int moneyValue;
    public LayerMask moneyZoneLayerMask;
    public  float     launchSpeed;
    private bool      isUsed = false;

    protected override void OnUse()
    {
        isUsed = true;
        target = null;
        transform.SetParent(null);
        rb.isKinematic = false;
        rb.AddForce(transform.forward * launchSpeed, ForceMode.Impulse);
    }

    public override void OnPickup()
    {
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
        if (collision.gameObject.layer == moneyZoneLayerMask)
        {
            Destroy(gameObject);
        }
    }
}
