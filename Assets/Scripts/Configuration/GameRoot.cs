using UnityEngine;

namespace Configuration {
    public class GameRoot : MonoBehaviour {
        public static GameRoot Instance { get; private set; }

        [Header("Managers")]
        public SaveManager saveManager;
        public RaspberryPi.RaspberryPiCommunicator rasPiCommunicator;

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