using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Echoesphere.Runtime.UI {
    public class MusicNoteController : MonoBehaviour {
        [SerializeField] private MusicNote[] musicNotes;

        [Header("Gamepad Input")]
        [SerializeField] private InputActionReference noteWaterDropAction;
        [SerializeField] private InputActionReference noteCrossingAction;
        [SerializeField] private InputActionReference noteTideAction;
        [SerializeField] private InputActionReference noteBreezeAction;

        public int AcquiredNoteCount { get; private set; }

        private Action<InputAction.CallbackContext> _onWaterDrop;
        private Action<InputAction.CallbackContext> _onCrossing;
        private Action<InputAction.CallbackContext> _onTide;
        private Action<InputAction.CallbackContext> _onBreeze;

        private void Awake() {
            _onWaterDrop = _ => OnNoteActionPerformed(NoteType.WaterDrop);
            _onCrossing = _ => OnNoteActionPerformed(NoteType.Crossing);
            _onTide = _ => OnNoteActionPerformed(NoteType.Tide);
            _onBreeze = _ => OnNoteActionPerformed(NoteType.Breeze);
        }

        private void OnEnable() {
            Subscribe(noteWaterDropAction, _onWaterDrop);
            Subscribe(noteCrossingAction, _onCrossing);
            Subscribe(noteTideAction, _onTide);
            Subscribe(noteBreezeAction, _onBreeze);
        }

        private void OnDisable() {
            Unsubscribe(noteWaterDropAction, _onWaterDrop);
            Unsubscribe(noteCrossingAction, _onCrossing);
            Unsubscribe(noteTideAction, _onTide);
            Unsubscribe(noteBreezeAction, _onBreeze);
        }

        private static void Subscribe(InputActionReference actionRef, Action<InputAction.CallbackContext> handler) {
            if (actionRef == null) return;
            actionRef.action.performed += handler;
            actionRef.action.Enable();
        }

        private static void Unsubscribe(InputActionReference actionRef, Action<InputAction.CallbackContext> handler) {
            if (actionRef == null) return;
            actionRef.action.performed -= handler;
            actionRef.action.Disable();
        }

        private void OnNoteActionPerformed(NoteType noteType) {
            AcquireByType(noteType);
            PlayByType(noteType);
        }

        public void AcquireByIndex(int noteTypeIndex) {
            var note = musicNotes[noteTypeIndex];
            if (note.IsAcquired) return;
            note.Acquire();
            AcquiredNoteCount++;
        }

        public void AcquireByType(NoteType noteType) {
            var note = musicNotes.FirstOrDefault(n => n.noteType == noteType);
            if (note == null || note.IsAcquired) return;
            note.Acquire();
            AcquiredNoteCount++;
        }

        public void PlayByIndex(int noteTypeIndex) {
            musicNotes[noteTypeIndex].Play();
        }

        public void PlayByType(NoteType noteType) {
            var note = musicNotes.FirstOrDefault(n => n.noteType == noteType);
            note?.Play();
        }

        public void AcquireNextNote() {
            if (AcquiredNoteCount >= musicNotes.Length) return;
            AcquireByIndex(AcquiredNoteCount);
        }

        public void PlayCurrentNote() {
            if (AcquiredNoteCount <= 0) return;
            PlayByIndex(AcquiredNoteCount - 1);
        }

        public void ResetAll() {
            foreach (var note in musicNotes) {
                note.Reset();
            }
            AcquiredNoteCount = 0;
        }
    }
}
