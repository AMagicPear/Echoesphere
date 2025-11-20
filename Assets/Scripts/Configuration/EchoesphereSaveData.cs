using System;
using System.Collections.Generic;
using System.IO;
using Helpers;
using UnityEngine;

namespace Configuration {
    [Serializable]
    public class EchoesphereSaveData {
        public Dictionary<string, object> Objects;
        public int saveVersion;
        public static string FilePath => Application.persistentDataPath + "/saveData.gz";

        public EchoesphereSaveData(int saveVersion) {
            this.saveVersion = saveVersion;
            Objects = new Dictionary<string, object>();
        }

        public void WriteToFile() {
            var json = JsonUtility.ToJson(this);
            var compressed = CompressionHelper.GZipCompress(json);
            File.WriteAllBytes(FilePath, compressed);
        }

        public static EchoesphereSaveData ReadFromFile() {
            if (!File.Exists(FilePath)) {
                return new EchoesphereSaveData(0);
            }

            var compressed = File.ReadAllBytes(FilePath);
            var json = CompressionHelper.GZipDecompress(compressed);
            return JsonUtility.FromJson<EchoesphereSaveData>(json);
        }
    }
}