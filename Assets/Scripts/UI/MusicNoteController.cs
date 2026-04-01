using System.Linq;
using UnityEngine;

namespace Echoesphere.Runtime.UI {
    public class MusicNoteController : MonoBehaviour {
        [SerializeField] private MusicNote[] musicNotes;
        public int AcquiredNoteCount { get; private set; }

        public void AcquireByIndex(int noteTypeIndex) {
            musicNotes[noteTypeIndex].Acquire();
            AcquiredNoteCount++;
        }

        public void AcquireByType(NoteType noteType) {
            musicNotes.FirstOrDefault(note => note.noteType == noteType).Acquire();
            AcquiredNoteCount++;
        }

        public void PlayByIndex(int noteTypeIndex) {
            musicNotes[noteTypeIndex].Play();
        }

        public void PlayByType(NoteType noteType) {
            musicNotes.FirstOrDefault(note => note.noteType == noteType).Play();
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