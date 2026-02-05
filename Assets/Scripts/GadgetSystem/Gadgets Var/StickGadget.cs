using System.Collections;
using Player;
using UnityEngine;

public class StickGadget : Gadget
{
    [SerializeField] Rigidbody rb;
    [Header("Basic Stick")]
    [SerializeField] float     setback;
    [SerializeField] float     range;
    [SerializeField] float     movementTime;
    [SerializeField] float     cooldown;
    
    [Header("Heavy Stick")]
    [SerializeField] bool heavyStick;
    [SerializeField] float maxLoadTime;
    [SerializeField] float maxLoadForce;
    
    private int        currentLoadTime;
    private Controller player;
    private bool       canUse = true;
    
    //COOLDOWN
    
    protected override void OnUse()
    {
        // j'attend que les GD choisissent pour en garder qu'un
        switch (heavyStick)
        {
            case true:
                if (!canUse) return;
                StartCoroutine(LoadForce());
                break;
            case false:
                if (!canUse) return;
                StartCoroutine(Cooldown());
                StartCoroutine(AnimateGadget());
                Hit();
                break;
        }
        
    }

    private void Hit()
    {
        Ray baseCast = new Ray(GadgetController.concernedPlayerCamera.transform.position, GadgetController.concernedPlayerCamera.transform.forward);
        RaycastHit[] target = Physics.SphereCastAll(baseCast, 0.25f, range);
        foreach (var hit in target)
        {
            if (hit.rigidbody)
            {
                hit.rigidbody.AddForce(-hit.normal * setback, ForceMode.Impulse);
            }
        }
    }

    IEnumerator Cooldown()
    {
        canUse = false;
        yield return new WaitForSeconds(cooldown);
        canUse = true;
    }

    IEnumerator AnimateGadget()
    {
        transform.localRotation = Quaternion.Euler(-90, 0, 0);
        yield return new WaitForSeconds(0.15f);
        transform.localRotation = Quaternion.Euler(0, 0, 0);
    }

    IEnumerator LoadForce()
    {
        canUse =  false;
        float baseForce = setback;
        float elapsed   = 0;
        while (elapsed < maxLoadTime || !player.GetInputs().use.IsPressed())
        {
            setback =  Mathf.Lerp(baseForce, maxLoadForce, elapsed / maxLoadTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Hit();
        setback = baseForce;
        canUse = true;
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

    public override void OnDepleted()
    {
        base.OnDepleted();
    }
}
