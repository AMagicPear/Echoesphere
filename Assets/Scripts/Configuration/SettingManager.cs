using UnityEngine;

namespace Echoesphere.Runtime.Configuration {
    public class SettingManager : MonoBehaviour {
        [SerializeField] private int settingVersion;
        
        private int _userSettingValue1;
        private int _userSettingValue2;

        private void Write() {
            PlayerPrefs.SetInt("SettingVersion", settingVersion);
            PlayerPrefs.SetInt("UserSettingValue1", _userSettingValue1);
            PlayerPrefs.SetInt("UserSettingValue2", _userSettingValue2);
            PlayerPrefs.Save();
        }
    }
}