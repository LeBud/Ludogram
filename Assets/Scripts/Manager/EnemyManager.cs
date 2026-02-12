using System.Collections.Generic;
using Enemies;
using UnityEngine;

namespace Manager {
    public class EnemyManager : MonoBehaviour {
        private HashSet<EnemyController> enemies = new();
        private HashSet<ManholeCover> manholeCover = new();
        public HashSet<MoneyBag> targetedBag = new();
        
        private HashSet<SpawnFrog> frogSpawner = new();
        
        public void RegisterEnemy(EnemyController newEnemy) {
            enemies.Add(newEnemy);
        }

        public void DeregisterEnemy(EnemyController removedEnemy) {
            foreach (var spawnFrog in frogSpawner) {
                spawnFrog.RemoveFrogFromSpawned(removedEnemy);
            }
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
        
        public void RegisterTarget(MoneyBag bag) {
            targetedBag.Add(bag);
        }

        public void DeregisterTarget(MoneyBag bag) {
            if(bag == null) return;
            
            if(targetedBag.Contains(bag))
                targetedBag.Remove(bag);
        }

        public void RegisterSpawner(SpawnFrog frog) {
            frogSpawner.Add(frog);
        }

        public void DeregisterCarFromSpawner(AiCarDriver ai) {
            foreach (var spawnFrog in frogSpawner) {
                spawnFrog.RemoveCarFromSpawned(ai);
            }
        }
    }
}