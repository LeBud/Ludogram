using Manager;
using UnityEngine;

namespace Enemies {
    public class ManholeCover : MonoBehaviour {
        private void Start() {
            GameManager.instance.enemyManager.RegisterManhole(this);
        }
    }
}