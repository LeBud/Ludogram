using System.Collections.Generic;
using Enemies;
using UnityEngine;

namespace Manager {
    public class EnemyManager : MonoBehaviour {
        private HashSet<EnemyController> enemies = new();
        private HashSet<ManholeCover> manholeCover = new();

        public void RegisterEnemy(EnemyController newEnemy) {
            enemies.Add(newEnemy);
        }

        public void DeregisterEnemy(EnemyController removedEnemy) {
            if(enemies.Contains(removedEnemy))
                enemies.Remove(removedEnemy);
        }
        
        public void RegisterManhole(ManholeCover manHole) {
            manholeCover.Add(manHole);
        }

        public void DeregisterManhole(ManholeCover manHole) {
            if(manholeCover.Contains(manHole))
                manholeCover.Remove(manHole);
        }

        public HashSet<ManholeCover> GetManholeCover() {
            return manholeCover;
        }

        public void UpdatePlayerList() {
            foreach (var enemy in enemies) {
                enemy.movement.GetPlayers();
            }
        }
    }
}