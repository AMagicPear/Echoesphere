using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Echoesphere.Runtime.UI {
    public class SceneLoader : MonoBehaviour {
        private static readonly int FadeIn = Animator.StringToHash("FadeIn");
        private static readonly int FadeOut = Animator.StringToHash("FadeOut");
        public Button buttonA;
        public Button buttonB;
        public Animator animator;

        private void Start() {
            buttonA.onClick.AddListener(LoadSceneA);
            buttonB.onClick.AddListener(LoadSceneB);
        }

        private void LoadSceneA() {
            StartCoroutine(LoadScene(1));
        }

        private void LoadSceneB() {
            StartCoroutine(LoadScene(2));
        }


        private IEnumerator LoadScene(int index) {
            animator.SetBool(FadeIn, true);
            animator.SetBool(FadeOut, false);
            yield return new WaitForSeconds(1);
            var asyncOperation = SceneManager.LoadSceneAsync(index);
            if (asyncOperation != null) asyncOperation.completed += OnLoadedScene;
        }

        private void OnLoadedScene(AsyncOperation obj) {
            animator.SetBool(FadeIn, false);
            animator.SetBool(FadeOut, true);
        }
    }
}