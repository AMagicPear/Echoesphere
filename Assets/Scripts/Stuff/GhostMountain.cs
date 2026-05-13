using System;
using Echoesphere.Runtime.Agent;
using UnityEngine;

namespace Echoesphere.Runtime.Stuff {
    
    public class GhostMountain : MonoBehaviour {
        private static readonly int WobbleSpeed = Shader.PropertyToID("_WobbleSpeed");
        private static readonly int WobbleIntensity = Shader.PropertyToID("_WobbleIntensity");
        private static readonly int FresnelColor = Shader.PropertyToID("_FresnelColor");
        
        [Header("Mountain Wobble")] public float wobbleSpeed = 0.2f;
        public float initialWobbleSpeed = 0.2f;
        public float wobbleIntensity = 1f;
        public float initialWobbleIntensity = 1f;
        public Material mountainMaterial;

        private float _playerSpeed = 0;
        private AgentCommunicator _agent;

        private void Awake() {
            _agent = FindFirstObjectByType<AgentCommunicator>();
        }

        private void OnEnable() {
            _agent.OnCommandReceived += OnCommandReceived;
        }

        private void OnDisable() {
            _agent.OnCommandReceived -= OnCommandReceived;
        }

        private void OnCommandReceived(JsonMessage msg) {
            if (!msg.data.StartsWith("ghost_mountain:")) return;
            var payload = msg.data.Substring(15); // strip "ghost_mountain:"
            var segments = payload.Split(':');
            if (segments.Length != 2) return;
            var key = segments[0].Trim();
            var value = segments[1].Trim();

            switch (key) {
                case "intensity":
                case "wobble":
                    if (float.TryParse(value, out var intensity)) {
                        wobbleIntensity = intensity;
                        mountainMaterial.SetFloat(WobbleIntensity, intensity);
                    }
                    break;
                case "color":
                    if (ColorUtility.TryParseHtmlString("#" + value, out var color)) {
                        mountainMaterial.SetColor(FresnelColor, color);
                    }
                    break;
            }
        }

        private void Start() {
            wobbleSpeed = initialWobbleSpeed;
            wobbleIntensity = initialWobbleIntensity;
        }

        private void Update() {
            wobbleSpeed = Mathf.Lerp(wobbleSpeed, _playerSpeed+0.2f, Time.deltaTime * 10f);
            mountainMaterial.SetFloat(WobbleSpeed, wobbleSpeed);
            mountainMaterial.SetFloat(WobbleIntensity, wobbleIntensity);
        }

        /// 结束的时候恢复一下防止值被固化在材质上
        private void OnDestroy() {
            mountainMaterial.SetFloat(WobbleSpeed, initialWobbleSpeed);
            mountainMaterial.SetFloat(WobbleIntensity, initialWobbleIntensity);
        }
    }
}