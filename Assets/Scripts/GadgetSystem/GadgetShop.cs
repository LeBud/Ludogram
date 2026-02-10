using System.Collections.Generic;
using UnityEngine;

public class GadgetShop : MonoBehaviour
{
    public List<GameObject>    gadgetsPrefabs;
    public List<Transform> targets;

    void Start()
    {
        GetTargets();
        PlaceGadget();
    }

    private void GetTargets()
    {
        foreach (Transform target in transform.GetChild(0))
        {
            targets.Add(target);
        }
    }

    private void PlaceGadget()
    {
        int gadgetIndex = 0;
        for (int i = 0; i < targets.Count; i++)
        {
            gadgetIndex++;
            if (gadgetIndex >= gadgetsPrefabs.Count)
            {
                gadgetIndex = 0;
            }
            GameObject gadget = Instantiate(gadgetsPrefabs[gadgetIndex], targets[i].position, Quaternion.identity);
            gadget.GetComponent<Rigidbody>().isKinematic = true;
        }
    }
}
