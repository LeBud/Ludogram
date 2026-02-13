using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Player {
    public class SwitchPlayerModel : MonoBehaviour {
        [SerializeField] private SkinnedMeshRenderer bodyRenderer;
        [SerializeField] private Transform chihuahuaHead;
        [SerializeField] private Transform pugHead;

        [SerializeField] private Material chihuahuaMat;
        [SerializeField] private Material pugMat;
        [SerializeField] private Camera cam;

        public void SetPlayerMesh(int index) {
            if (index == 0) {
                pugHead.gameObject.SetActive(false);
                chihuahuaHead.gameObject.SetActive(true);
                bodyRenderer.material = chihuahuaMat;
                bodyRenderer.gameObject.layer = LayerMask.NameToLayer("Player1");
                chihuahuaHead.gameObject.layer = LayerMask.NameToLayer("Player1");
                
                cam.cullingMask = LayerMask.GetMask("Player2", "Default", "Car", "Enemy", "Gadget", "Road", "UI", "MoneyZone");
            }
            else {
                pugHead.gameObject.SetActive(true);
                chihuahuaHead.gameObject.SetActive(false);
                bodyRenderer.material = pugMat;
                bodyRenderer.gameObject.layer = LayerMask.NameToLayer("Player2");
                pugHead.gameObject.layer = LayerMask.NameToLayer("Player2");
                
                cam.cullingMask = LayerMask.GetMask("Player1", "Default", "Car", "Enemy", "Gadget", "Road", "UI", "MoneyZone");
            }
        }
    }
}