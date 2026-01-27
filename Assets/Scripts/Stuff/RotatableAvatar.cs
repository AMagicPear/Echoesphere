using UnityEngine;

namespace Stuff {
    public class RotatableAvatar : MonoBehaviour {
        private int facingAt = 0;

        public int FacingAt {
            get => facingAt;
            private set {
                facingAt = value;
                transform.rotation = Quaternion.Euler(0, 0, facingAt * 90);
            }
        }

        public int directionsCount = 4;

        public int RotateForward(int stride = 1) {
            FacingAt = (FacingAt + stride + directionsCount) % directionsCount;
            return FacingAt;
        }
    }
}