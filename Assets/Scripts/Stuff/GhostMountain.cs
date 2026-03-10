using System;
using UnityEngine;

namespace Echoesphere.Runtime.Stuff {
    public class GhostMountain : MonoBehaviour {
        private static readonly int WobbleSpeed = Shader.PropertyToID("_WobbleSpeed");
        private static readonly int WobbleIntensity = Shader.PropertyToID("_WobbleIntensity");
        [Header("Mountain Wobble")] public float wobbleSpeed = 0.2f;
        public float initialWobbleSpeed = 0.2f;
        public float wobbleIntensity = 1f;
        public float initialWobbleIntensity = 1f;
        public Material mountainMaterial;

        private float _playerSpeed = 0;

        private void Start() {
            wobbleSpeed = initialWobbleSpeed;
            wobbleIntensity = initialWobbleIntensity;
        }

        private void Update() {
            // TODO)) 根据手势获取_playerSpeed
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