using System.IO;
using System.IO.Compression;
using System.Text;

namespace Echoesphere.Runtime.Helpers {
    /// <summary>
    /// 压缩帮助类
    /// </summary>
    public static class CompressionHelper {
        /// <summary>
        /// 压缩字符串
        /// </summary>
        /// <param name="text">要压缩的字符串</param>
        /// <returns>压缩后的字节数组</returns>
        public static byte[] GZipCompress(string text) {
            var raw = Encoding.UTF8.GetBytes(text);
            using var ms = new MemoryStream();
            using (var gzip = new GZipStream(ms, CompressionMode.Compress)) {
                gzip.Write(raw, 0, raw.Length);
            }

            return ms.ToArray();
        }

        /// <summary>
        /// 解压缩字节数组
        /// </summary>
        /// <param name="compressed">要解压缩的字节数组</param>
        /// <returns>解压缩后的字符串</returns>
        public static string GZipDecompress(byte[] compressed) {
            using var input = new MemoryStream(compressed);
            using var gzip = new GZipStream(input, CompressionMode.Decompress);
            using var output = new MemoryStream();
            gzip.CopyTo(output);
            return Encoding.UTF8.GetString(output.ToArray());
        }
    }
}