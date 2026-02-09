using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


namespace Manager {
    public class GameManager : MonoBehaviour {

        public static GameManager instance { get; private set; }
        public EnemyManager enemyManager { get; private set; }

        private HashSet<PlayerInput> players = new();

        private void Awake() {
            if (instance == null) instance = this;
            else Destroy(this);

            if (TryGetComponent(out EnemyManager enemy)) enemyManager = enemy;
            else Debug.LogError("No Enemy Manager found");
        }

        private void Start() {
            PlayerInputManager.instance.onPlayerJoined += HandlePlayerJoining;
        }

        private void HandlePlayerJoining(PlayerInput input) {
            players.Add(input);
            SetupCamera();

            enemyManager.UpdatePlayerList();
        }

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

        public HashSet<PlayerInput> GetPlayers() {
            return players;
        }
    }
}