using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Manager {
    public class GameManager : MonoBehaviour {

        public static GameManager instance { get; private set; }
        public EnemyManager enemyManager { get; private set; }
        public MoneyManager moneyManager { get; private set; }

        private HashSet<PlayerInput> players = new();
        private HashSet<Controller> playerInCar = new();
        private List<Controller> playersRef = new();

        private void Awake() {
            if (instance == null) instance = this;
            else Destroy(this);

            if (TryGetComponent(out EnemyManager enemy)) enemyManager = enemy;
            else Debug.LogError("No Enemy Manager found");
            
            if (TryGetComponent(out MoneyManager money)) moneyManager = money;
            else Debug.LogError("No Money Manager found");
        }

        private void Start() {
            PlayerInputManager.instance.onPlayerJoined += HandlePlayerJoining;
        }

        private void HandlePlayerJoining(PlayerInput input) {
            players.Add(input);
            SetupCamera();

            enemyManager.UpdatePlayerList();
        }

        public void RegisterPlayer(Controller player) {
            playersRef.Add(player);
        }

        public void DeregisterPlayer(Controller player) {
            if(playersRef.Contains(player)) 
                playersRef.Remove(player);
        }
        
        public void RegisterPlayerInCar(Controller player) {
            playerInCar.Add(player);
        }

        public void DeregisterPlayerInCar(Controller player) {
            if(playerInCar.Contains(player))
                playerInCar.Remove(player);
        }
        
        public bool AllPlayerInCars => playerInCar.Count == players.Count;
        
        private void SetupCamera() {
            Debug.Log($"SetupCamera, {players.Count} players connected");

            foreach (var player in players) {
                switch (players.Count) {
                    case 0:
                        Debug.LogWarning("No players found");
                        break;
                    case 1:
                        Debug.Log($"One player only");
                        break;
                    case 2:
                        player.camera.rect = new Rect(player.playerIndex == 0 ? 0f : 0.5f, 0f, 0.5f, 1f);
                        break;
                    case 3:
                        player.camera.rect = new Rect(
                            player.playerIndex == 0 ? 0 : player.playerIndex == 1 ? 0.5f : 0,
                            player.playerIndex < 2 ? 0.5f : 0,
                            player.playerIndex < 2 ? 0.5f : 1,
                            0.5f);
                        break;
                    case 4:
                        player.camera.rect = new Rect(player.playerIndex % 2 * 0.5f, player.playerIndex < 2 ? 0.5f : 0f,
                            0.5f, 0.5f);
                        break;
                }
            }
        }

        public List<Controller> GetPlayers() {
            return playersRef;
        }
    }
}