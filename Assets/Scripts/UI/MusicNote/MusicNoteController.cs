using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Echoesphere.Runtime.UI.MusicNote {
    public class MusicNoteController : MonoBehaviour {
        [SerializeField] private MusicNote[] musicNotes;

        [Header("Animation")] public float fadeDuration = 0.4f;
        public float connectionLineFadeDuration = 0.25f;
        public float acquireScaleDuration = 0.2f;
        public float acquireScale = 1.3f;
        public float playPulseDuration = 1.0f;
        public float acquiredAlpha = 0.75f;

        [Header("Audio")] [SerializeField] private AudioClip[] acquireSounds;
        [SerializeField] private AudioSource audioSource;

        [Header("Input")] [SerializeField] private InputActionReference noteWaterDropAction;
        [SerializeField] private InputActionReference noteCrossingAction;
        [SerializeField] private InputActionReference noteTideAction;
        [SerializeField] private InputActionReference noteBreezeAction;

        public bool IsAcquired(NoteType noteType) => musicNotes.Any(n => n.noteType == noteType && n.IsAcquired);

        private Dictionary<InputActionReference, Action<InputAction.CallbackContext>> _handlers;

        private void Awake() {
            _handlers = new Dictionary<InputActionReference, Action<InputAction.CallbackContext>> {
                [noteWaterDropAction] = _ => PlayByType(NoteType.WaterDrop),
                [noteCrossingAction] = _ => PlayByType(NoteType.Crossing),
                [noteTideAction] = _ => PlayByType(NoteType.Tide),
                [noteBreezeAction] = _ => PlayByType(NoteType.Breeze),
            };
            foreach (var note in musicNotes) note.controller = this;
        }

        private void OnEnable() {
            foreach (var (actionRef, handler) in _handlers) {
                actionRef.action.performed += handler;
                actionRef.action.Enable();
            }
        }

        private void OnDisable() {
            foreach (var (actionRef, handler) in _handlers)
                actionRef.action.performed -= handler;
        }

        [Header("Debug")] public NoteType debugNoteType;

        [ContextMenu("Acquire Debug Note")]
        private void AcquireDebugNote() => AcquireByType(debugNoteType);

        [ContextMenu("Play Debug Note")]
        private void PlayDebugNote() => PlayByType(debugNoteType);

        public void AcquireByType(NoteType noteType) {
            var note = musicNotes.FirstOrDefault(n => n.noteType == noteType);
            if (note == null || note.IsAcquired) return;
            note.Acquire();
        }

        public void PlayByType(NoteType noteType) {
            musicNotes.FirstOrDefault(n => n.noteType == noteType)?.Play();
        }

        public void PlaySound(AudioClip clip) {
            if (clip != null) audioSource.PlayOneShot(clip);
        }

        public void PlayRandomAcquireSound() {
            var clip = acquireSounds[UnityEngine.Random.Range(0, acquireSounds.Length)];
            PlaySound(clip);
        }
    }
}