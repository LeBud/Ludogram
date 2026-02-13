using UnityEngine;

namespace Player {
    public class SwitchPlayerModel : MonoBehaviour {
        [SerializeField] private SkinnedMeshRenderer bodyRenderer;
        [SerializeField] private Transform chihuahuaHead;
        [SerializeField] private Transform pugHead;

        [SerializeField] private Material chihuahuaMat;
        [SerializeField] private Material pugMat;

        public void SetPlayerMesh(int index) {
            if (index == 0) {
                pugHead.gameObject.SetActive(false);
                chihuahuaHead.gameObject.SetActive(true);
                bodyRenderer.material = chihuahuaMat;
            }
            else {
                pugHead.gameObject.SetActive(true);
                chihuahuaHead.gameObject.SetActive(false);
                bodyRenderer.material = pugMat;
            }
        }
    }
}