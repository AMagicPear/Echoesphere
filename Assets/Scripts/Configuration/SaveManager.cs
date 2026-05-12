using System;
using System.Linq;
using UnityEngine;

namespace Echoesphere.Runtime.Configuration {
    public class SaveManager : MonoBehaviour {
        public EchoesphereSaveData Current { get; private set; }
        public bool loadOnStart = true;

        private void Start() {
            if (loadOnStart) {
                Load();
            }
        }

        public void Save() {
            Current = CollectAllState();
            Current.WriteToFile();
        }

        public void Load() {
            Current = EchoesphereSaveData.ReadFromFile();
            ApplyAllState(Current);
        }

        private static EchoesphereSaveData CollectAllState() {
            var data = new EchoesphereSaveData(0);
            var providers = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .OfType<ISaveProvider>();
            foreach (var provider in providers) {
                string id = provider.UniqueId;
                object state = provider.CaptureState();
                data.objects[id] = state;
            }

            return data;
        }

        private static void ApplyAllState(EchoesphereSaveData data) {
            var providers = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .OfType<ISaveProvider>();
            foreach (var provider in providers) {
                string id = provider.UniqueId;
                if (data.objects.TryGetValue(id, out var state)) {
                    provider.RestoreState(state);
                }
            }
        }
    }
}