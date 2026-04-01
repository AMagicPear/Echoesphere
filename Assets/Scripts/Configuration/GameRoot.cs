using Echoesphere.Runtime.UI;
using UnityEngine;

namespace Echoesphere.Runtime.Configuration {
    public class GameRoot : MonoBehaviour {
        public static GameRoot Instance { get; private set; }

        [Header("Managers")]
        public SaveManager saveManager;
        public Agent.AgentCommunicator rasPiCommunicator;
        public MusicNoteController musicNoteController;

        private void Awake() {
            if (Instance) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}