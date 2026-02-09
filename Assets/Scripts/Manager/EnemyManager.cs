using System.Collections.Generic;
using UnityEngine;

namespace Manager {
    public class EnemyManager : MonoBehaviour {
        private HashSet<EnemyController> enemies = new();

        public void RegisterEnemy(EnemyController newEnemy) {
            enemies.Add(newEnemy);
        }

        public void DeregisterEnemy(EnemyController removedEnemy) {
            enemies.Remove(removedEnemy);
        }

        public void UpdatePlayerList() {
            foreach (var enemy in enemies) {
                enemy.movement.GetPlayers();
            }
        }
    }
}