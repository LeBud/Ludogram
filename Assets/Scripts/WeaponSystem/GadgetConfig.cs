using UnityEngine;

[CreateAssetMenu(fileName = "Gadget Config", menuName = "Gadget/Gadget Config")]
public class GadgetConfig : ScriptableObject
{
    [TextArea] public string GadgetName;
    
    public float gadgetSpeed;
    public float gadgetRange;
    public float gadgetDamage;

    public float activationTime;
    

}
