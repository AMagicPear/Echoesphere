using System;
using UnityEngine;

namespace Echoesphere.Runtime.Stuff {
    [Serializable]
    public class HandData {
        public int h;
        public float x, y, v;
    }
    
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

        public void OnReceiveTcpMessage(string json) {
            var data = JsonUtility.FromJson<HandData>(json);
            // if (data.h > 0) {
            //     // 1. 坐标平滑处理
            //     Vector2 targetPos = new Vector2(data.x, data.y);
            //     smoothedPos = Vector2.Lerp(smoothedPos, targetPos, smoothing);
            //
            //     // 2. 映射到山峦逻辑
            //     // 如果 data.v 超过阈值，直接触发崩解
            //     if (data.v > 0.08f) { 
            //         mountainScript.TriggerPathBreak(); 
            //     } else {
            //         // 稳定滑动：通过速度反向驱动平复过程
            //         // 速度越小，平复越快
            //         float calmFactor = Mathf.Clamp01(1.0f - (data.v / 0.05f));
            //         mountainScript.ApplyCalm(calmFactor);
            //     }
            // } else {
            //     // 手消失了，山峦重新回归迷茫抖动
            //     mountainScript.ResetToJitter();
            // }
        }
    }
}