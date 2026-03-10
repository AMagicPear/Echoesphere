using UnityEngine;

namespace Echoesphere.Runtime.Stuff {
    public class RotatableAvatar : MonoBehaviour {
        private int _facingAt = 0;

        private int FacingAt {
            get => _facingAt;
            set {
                _facingAt = value;
                transform.rotation = Quaternion.Euler(0, 0, _facingAt * 90);
            }
        }

        public int directionsCount = 4;

        public int RotateForward(int stride = 1) {
            FacingAt = (FacingAt + stride + directionsCount) % directionsCount;
            return FacingAt;
        }
    }
}