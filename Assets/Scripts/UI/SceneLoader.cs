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

        [Header("Scene Names")]
        [SerializeField] private string sceneNameA;
        [SerializeField] private string sceneNameB;

        private void Start() {
            buttonA.onClick.AddListener(() => StartCoroutine(LoadScene(sceneNameA)));
            buttonB.onClick.AddListener(() => StartCoroutine(LoadScene(sceneNameB)));
        }

        private IEnumerator LoadScene(string sceneName) {
            animator.SetBool(FadeIn, true);
            animator.SetBool(FadeOut, false);
            yield return new WaitForSeconds(1);
            var asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            asyncOperation.completed += OnLoadedScene;
        }

        private void OnLoadedScene(AsyncOperation obj) {
            animator.SetBool(FadeIn, false);
            animator.SetBool(FadeOut, true);
        }
    }
}