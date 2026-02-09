using System.Collections;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager instance;

    void Awake()
    {
        instance = this;
    }
    public IEnumerator SmoothSpawnEffect(GameObject go, float duration = 0.25f)
    {
            Vector3 wantedScale = Vector3.one;
           go.transform.localScale = Vector3.zero;
           float   elapsed     = 0;
           while (elapsed < duration)
           {
               go.transform.localScale =  Vector3.Lerp(go.transform.localScale, wantedScale, elapsed / duration);
               elapsed              += Time.deltaTime;
               yield return null;
           }
           go.transform.localScale = wantedScale;
    }
    
    public IEnumerator SmoothDespawnEffect(GameObject go, float duration = 0.25f) 
    {
        
        float   elapsed     = 0;
        while (elapsed < duration)
        {
            go.transform.localScale =  Vector3.Lerp(go.transform.localScale, Vector3.zero, elapsed / duration);
            elapsed                 += Time.deltaTime;
            yield return null;
        }
        go.transform.localScale = Vector3.zero;
        Destroy(go);
    }
}