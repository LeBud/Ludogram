using System.Collections.Generic;
using UnityEngine;

namespace Manager {
    public class EnemyManager : MonoBehaviour {
        private HashSet<Robber> enemies = new();

        public void RegisterEnemy(Robber newEnemy) {
            enemies.Add(newEnemy);
        }

        public void DeregisterEnemy(Robber removedEnemy) {
            enemies.Remove(removedEnemy);
        }

        public void UpdatePlayerList() {
            foreach (var enemy in enemies) {
                enemy.movement.GetPlayers();
            }
        }
    }
}