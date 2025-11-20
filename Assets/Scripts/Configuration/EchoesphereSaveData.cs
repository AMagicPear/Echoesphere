using System;
using System.Collections.Generic;
using System.IO;
using Helpers;
using UnityEngine;

namespace Configuration
{
    [Serializable]
    public class EchoesphereSaveData {
        public Dictionary<string, object> Objects;
        public int saveVersion;
    
        public EchoesphereSaveData(int saveVersion) {
            this.saveVersion = saveVersion;
            Objects = new Dictionary<string, object>();
        }

        public void WriteToFile() {
            var json = JsonUtility.ToJson(this);
            var compressed = CompressionHelper.GZipCompress(json);
            File.WriteAllBytes(Application.persistentDataPath + "/saveData.gz", compressed);
        }
        
        public static EchoesphereSaveData ReadFromFile() {
            var path = Application.persistentDataPath + "/saveData.gz";
            if (!File.Exists(path)) {
                return new EchoesphereSaveData(0);
            }
            var compressed = File.ReadAllBytes(path);
            var json = CompressionHelper.GZipDecompress(compressed);
            return JsonUtility.FromJson<EchoesphereSaveData>(json);
        }
    }
}