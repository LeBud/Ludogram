using UnityEngine;
using UnityEngine.AI;

public class Menottes : Gadget
{
    private Camera currentCamera;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float     launchSpeed;
    private                  bool      isUsed = false;

    protected override void OnUse()
    {
        isUsed = true;
        target = null;
        transform.SetParent(null);
        rb.isKinematic = false;
        Ray ray = new Ray(currentCamera.transform.position, currentCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            rb.AddForce((hit.point - transform.position).normalized * launchSpeed, ForceMode.Impulse);
        }
        else
        {
            rb.AddForce((ray.direction + Vector3.up) * (launchSpeed * 0.5f), ForceMode.Impulse);
        }
    }

    public override void OnPickup(GadgetController gc)
    {
        currentCamera    = gc.player.playerCamera;
        gadgetController = gc;
        rb.isKinematic   = true;
        isUsed           = false;
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
        if (!isUsed) return;
        if (collision.gameObject.TryGetComponent(out NavMeshAgent enemy))
        {
            enemy.speed = 0f;
            StartCoroutine(EffectManager.instance.SmoothDespawnEffect(gameObject, 0.2f));
        }
    }
    
    
}
