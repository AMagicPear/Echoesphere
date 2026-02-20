using System.IO;
using Echoesphere.Runtime.Configuration;
using NUnit.Framework;

namespace Tests {
    public class ConfigurationTest {

        [Test]
        public void WriteConfigurationToFile() {
            var data = new EchoesphereSaveData(0);
            data.WriteToFile();
            Assert.That(File.Exists(EchoesphereSaveData.FilePath), Is.True);
        }
    }
}