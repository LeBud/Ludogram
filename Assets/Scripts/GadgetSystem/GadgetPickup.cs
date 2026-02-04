using DefaultNamespace.Player;
using UnityEngine;

public class GadgetPickup : MonoBehaviour
{
    public                  Controller      player;
    [SerializeField] private GadgetInventory playerInventory;
    [SerializeField] private Transform       gadgetTransform;
    [SerializeField] private LayerMask       gadgetLayerMask;
    [SerializeField] private float           pickupRange = 2f;
    
    private const    int                 MAX_PICKUP_COUNT = 5;
    private          Collider[]          hitColliders;

   
    #region InputSystem

    void OnEnable()
    {
       player.GetInputs().pickUp.started += _ =>TryPickupNearbyGadget();
    }

    void OnDisable()
    {
        player.GetInputs().pickUp.started -= _ =>TryPickupNearbyGadget();
    }

    #endregion
    
    
    [ContextMenu("Pickup")]
    private void TryPickupNearbyGadget()
    {
        //Ray
        
        hitColliders = new Collider[MAX_PICKUP_COUNT];
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, pickupRange, hitColliders, gadgetLayerMask);

        Transform closestObj = hitColliders[0].transform;
        IGadget    gadget     = closestObj ? closestObj.GetComponent<IGadget>() : null;
        
        for (int i = 0; i < numColliders; i++)
        {
            if (closestObj == null) continue;
            if (Vector3.Distance(transform.position, hitColliders[i].transform.position) < Vector3.Distance(transform.position, closestObj.position)
                &&  playerInventory.selectedGadget != hitColliders[i].GetComponent<IGadget>())
            {
                closestObj = hitColliders[i].transform;
                gadget     = closestObj.GetComponent<IGadget>();
            }
        }
        
        //Debug.Log(closestObj.name + "est le plus proche : " + Vector3.Distance(transform.position, closestObj.position));
        
        if (playerInventory.AddGadget(closestObj.GetComponent<IGadget>()))
        {
            closestObj.position = gadgetTransform.position;
            closestObj.forward = gadgetTransform.forward;
            closestObj.SetParent(gadgetTransform);
            closestObj.GetComponent<Gadget>().OnPickup();
            Debug.Log("Ramassé:" + gadget.Name);
        }
        else
        {
            Debug.Log("Inventaire plein !");
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
        Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(gadgetTransform.position, 0.5f);
        Gizmos.DrawWireCube(gadgetTransform.position, Vector3.one);
    }
}
