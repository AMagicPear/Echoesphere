using System;
using Stuff;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Puzzle
{
    public class AvatarRotating : MonoBehaviour
    {
        public RotatableAvatar[] avatars;
        public int[] rotationGroups;

        public void RotateGroup(int groupIndex, bool forward)
        {
            int stride = forward ? 1 : -1;
            foreach (var rotationGroup in rotationGroups)
            {
                avatars[rotationGroup].RotateForward(stride);
            }
        }

        // 测试旋转
        public void OnMove(InputValue value)
        {
            Console.WriteLine($"OnMove: {value}");
            // RotateGroup(0, value.Get<float>() > 0);
        }
    }
}