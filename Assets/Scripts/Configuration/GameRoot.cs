using Echoesphere.Runtime.Agent;
using Echoesphere.Runtime.UI;
using UnityEngine;

namespace Echoesphere.Runtime.Configuration {
    [DefaultExecutionOrder(-100)]   // 使得GameRoot提早执行以防止引用错误
    public class GameRoot : MonoBehaviour {
        public static GameRoot Instance { get; private set; }

        [Header("Managers")]
        public SaveManager saveManager;
        public Agent.AgentCommunicator agentCommunicator;
        public MusicNoteController musicNoteController;
        public EchoEventCenter echoEventCenter;

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