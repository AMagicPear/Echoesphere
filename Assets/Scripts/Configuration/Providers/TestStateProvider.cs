using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Configuration.Providers {
    public class TestStateProvider : MonoBehaviour, ISaveProvider {
        [SerializeField] private string id;
        public string UniqueId => id;
        
        [Serializable]
        public record TestState {
            public int value1;
            public float value2;
            public string value3;
        }
        
        public object CaptureState() {
            return new TestState {
                value1 = Random.Range(int.MinValue, int.MaxValue),
                value2 = Random.Range(float.MinValue, float.MaxValue),
                value3 = Random.Range(0, 10000).ToString(),
            };
        }

        public void RestoreState(object state) {
            var s = (TestState) state;
            Debug.Log($"Restored state: {s.value1}, {s.value2}, {s.value3}");
        }
    }
}