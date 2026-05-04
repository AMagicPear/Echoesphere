using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Echoesphere.Runtime.Agent {
    /// <summary>
    /// 录制当前游戏音频并通过 OnAudioFilterRead 获取字节数据
    /// 使用方法：将本脚本挂载到带有 AudioListener 的游戏对象上
    /// </summary>
    public class AudioRecorder : MonoBehaviour {
        [Header("录制设置")]
        [Tooltip("录制时长（秒）")]
        public float recordDuration = 5f;
        [Tooltip("音频通道数（通常为2，即立体声）")]
        public int channels = 2;

        // 用于累积音频数据的列表（音频线程和主线程同时访问，需要加锁）
        private readonly List<float> _recordedData = new();
        private readonly object _recordedDataLock = new();
        private bool _isRecording = false;
        private float _recordTimer = 0f;

        void Update() {
            if (!_isRecording) return;
            _recordTimer += Time.deltaTime;
            if (_recordTimer >= recordDuration) {
                StopRecordingAndSave();
            }
        }

        /// <summary>
        /// 开始录制（可以在外部调用）
        /// </summary>
        public void StartRecording() {
            _recordedData.Clear();
            _isRecording = true;
            _recordTimer = 0f;
            Debug.Log("开始录制...");
        }

        /// <summary>
        /// 手动停止录制并输出字节数组
        /// </summary>
        public void StopRecordingAndSave() {
            if (!_isRecording) return;

            _isRecording = false;
            Debug.Log("录制结束，正在转换数据...");

            List<float> dataCopy;
            lock (_recordedDataLock) {
                dataCopy = new List<float>(_recordedData);
                _recordedData.Clear();
            }

            byte[] audioBytes = ConvertFloatListToPCM16(dataCopy);

            Debug.Log($"音频数据大小: {audioBytes.Length} 字节");

            SaveAsWav(audioBytes, "recorded_audio.wav");
        }

        /// <summary>
        /// OnAudioFilterRead 在音频处理线程中被调用，不要在此执行耗时操作或Unity API
        /// </summary>
        /// <param name="data">音频数据数组，长度 = 采样点数 * 通道数</param>
        /// <param name="channels">通道数，与你在 AudioSettings 中设置的一致</param>
        private void OnAudioFilterRead(float[] data, int channels) {
            if (!_isRecording) return;

            lock (_recordedDataLock) {
                _recordedData.AddRange(data);
            }
        }

        /// <summary>
        /// 将 float 列表转换为 16位 PCM 字节数组
        /// </summary>
        private byte[] ConvertFloatListToPCM16(List<float> floatSamples) {
            int sampleCount = floatSamples.Count;
            byte[] byteArray = new byte[sampleCount * 2]; // 每个采样点2字节

            for (int i = 0; i < sampleCount; i++) {
                // 将 float 范围 [-1.0f, 1.0f] 映射到 short 范围 [-32768, 32767]
                short pcmValue = (short)(Mathf.Clamp(floatSamples[i], -1f, 1f) * 32767f);
                // 以小端序写入两个字节
                byteArray[i * 2] = (byte)(pcmValue & 0xFF);
                byteArray[i * 2 + 1] = (byte)((pcmValue >> 8) & 0xFF);
            }

            return byteArray;
        }

        /// <summary>
        /// 可选：将 PCM 字节数组保存为带 WAV 头的文件（使用 File.WriteAllBytes）
        /// </summary>
        private void SaveAsWav(byte[] pcmData, string filename) {
            int sampleRate = AudioSettings.outputSampleRate;
            int bitsPerSample = 16;
            int byteRate = sampleRate * channels * bitsPerSample / 8;
            Debug.Log(Application.persistentDataPath);

            using (FileStream fs = new FileStream(Application.persistentDataPath + "/" + filename, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(fs)) {
                // RIFF 头
                writer.Write(System.Text.Encoding.UTF8.GetBytes("RIFF"));
                writer.Write(36 + pcmData.Length); // 文件总大小-8
                writer.Write(System.Text.Encoding.UTF8.GetBytes("WAVE"));

                // fmt 块
                writer.Write(System.Text.Encoding.UTF8.GetBytes("fmt "));
                writer.Write(16); // fmt 块大小
                writer.Write((short)1); // PCM 格式
                writer.Write((short)channels);
                writer.Write(sampleRate);
                writer.Write(byteRate);
                writer.Write((short)(channels * bitsPerSample / 8)); // 块对齐
                writer.Write((short)bitsPerSample);

                // data 块
                writer.Write(System.Text.Encoding.UTF8.GetBytes("data"));
                writer.Write(pcmData.Length);
                writer.Write(pcmData);
            }

            Debug.Log("音频已保存为: " + filename);
        }
    }
}