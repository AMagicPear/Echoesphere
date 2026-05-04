using System;
using System.Collections.Generic;
using System.IO;
using Echoesphere.Runtime.Helpers;
using Newtonsoft.Json;
using UnityEngine;

namespace Echoesphere.Runtime.Configuration {
    [Serializable]
    public class EchoesphereSaveData {
        public Dictionary<string, object> Objects;
        public int saveVersion;
        public static string FilePath => Application.persistentDataPath + "/saveData.gz";

        private static readonly JsonSerializerSettings SerializerSettings = new() {
            TypeNameHandling = TypeNameHandling.Auto
        };

        public EchoesphereSaveData(int saveVersion) {
            this.saveVersion = saveVersion;
            Objects = new Dictionary<string, object>();
        }

        public void WriteToFile() {
            var json = JsonConvert.SerializeObject(this, SerializerSettings);
            var compressed = CompressionHelper.GZipCompress(json);
            File.WriteAllBytes(FilePath, compressed);
        }

        public static EchoesphereSaveData ReadFromFile() {
            if (!File.Exists(FilePath)) {
                return new EchoesphereSaveData(0);
            }

            var compressed = File.ReadAllBytes(FilePath);
            var json = CompressionHelper.GZipDecompress(compressed);
            return JsonConvert.DeserializeObject<EchoesphereSaveData>(json, SerializerSettings);
        }
    }
}