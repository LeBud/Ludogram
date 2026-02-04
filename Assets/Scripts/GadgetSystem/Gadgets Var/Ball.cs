using UnityEngine;

public class Ball :  Gadget
{
    bool         isUsed = false;
    public float speed;

    protected override void OnUse()
    {
        isUsed = true;
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, 50f))
        {
            transform.position = Vector3.MoveTowards(transform.position, hit.point, speed * Time.deltaTime);
        }
        else
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }
    

    void OnCollisionEnter(Collision collision)
    {
        if (!isUsed) return;
        Debug.Log(collision.gameObject.name);
        Destroy(gameObject, 2f);
    }

   
}
