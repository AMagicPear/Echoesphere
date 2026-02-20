using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Echoesphere.Runtime.UI {
    public class ButtonScale : MonoBehaviour {
        [Header("References")] public Selectable[] selectables;
        [Header("Animation")] [SerializeField] protected float selectedScale = 1.1f;
        [SerializeField] protected float scaleDuration = 0.25f;
        
        [Header("Controls")]
        [SerializeField] protected InputActionReference navigateAction;

        private Tween _scaleUpTween;
        private Tween _scaleDownTween;
        
        private Selectable _lastSelected;

        private void Awake() {
            foreach (var selectable in selectables) {
                AddSelectionListener(selectable);
            }
        }

        private void OnEnable() {
            navigateAction.action.performed += OnNavigate;
        }

        private void OnDisable() {
            _scaleUpTween.Kill();
            _scaleDownTween.Kill();
            navigateAction.action.performed -= OnNavigate;
        }

        private void AddSelectionListener(Selectable selectable) {
            // 为每个Selectable添加EventTrigger组件
            var trigger = selectable.gameObject.AddComponent<EventTrigger>();
            // 添加Select事件（适用于手柄）
            var selectEntry = new EventTrigger.Entry {
                eventID = EventTriggerType.Select
            };
            selectEntry.callback.AddListener(OnSelect);
            trigger.triggers.Add(selectEntry);
            // 添加Deselect事件（适用于手柄）
            var deselectEntry = new EventTrigger.Entry {
                eventID = EventTriggerType.Deselect
            };
            deselectEntry.callback.AddListener(OnDeselect);
            trigger.triggers.Add(deselectEntry);
            // 添加OnPointerEnter事件（适用于鼠标）
            var pointerEnterEntry = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerEnter
            };
            pointerEnterEntry.callback.AddListener(OnPointerEnter);
            trigger.triggers.Add(pointerEnterEntry);
            // 添加OnPointerExit事件（适用于鼠标）
            var pointerExitEntry = new EventTrigger.Entry {
                eventID = EventTriggerType.PointerExit
            };
            pointerExitEntry.callback.AddListener(OnPointerExit);
            trigger.triggers.Add(pointerExitEntry);
        }


        private void OnSelect(BaseEventData eventData) {
            _lastSelected = eventData.selectedObject.GetComponent<Selectable>();
            _scaleUpTween = eventData.selectedObject.transform.DOScale(selectedScale, scaleDuration);
        }

        private void OnDeselect(BaseEventData eventData) {
            _scaleDownTween = eventData.selectedObject.transform.DOScale(1.0f, scaleDuration);
        }

        private void OnPointerEnter(BaseEventData eventData) {
            if (eventData is not PointerEventData pointerEventData) return;
            pointerEventData.selectedObject = pointerEventData.pointerEnter;
        }

        private void OnPointerExit(BaseEventData eventData) {
            if (eventData is not PointerEventData pointerEventData) return;
            pointerEventData.selectedObject = null;
        }

        private void OnNavigate(InputAction.CallbackContext context) {
            if (EventSystem.current.currentSelectedGameObject == null && _lastSelected != null) {
                EventSystem.current.SetSelectedGameObject(_lastSelected.gameObject);
            }
        }
    }
}