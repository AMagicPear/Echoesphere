using Echoesphere.Runtime.Agent;
using Echoesphere.Runtime.Configuration.Providers;
using Echoesphere.Runtime.UI.MusicNote;
using UnityEngine;
using UnityEngine.Serialization;

namespace Echoesphere.Runtime.Configuration {
    [DefaultExecutionOrder(-100)]   // 使得GameRoot提早执行以防止引用错误
    public class GameRoot : MonoBehaviour {
        public static GameRoot Instance { get; private set; }

        [Header("Managers")]
        public SaveManager saveManager;
        public AgentCommunicator agentCommunicator;
        public MusicNoteController musicNoteController;
        [FormerlySerializedAs("echoEventCenter")] public EchoGameEvents echoGameEvents;
        public GameStateManager gameStateManager;

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