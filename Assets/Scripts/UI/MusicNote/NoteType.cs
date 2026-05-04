namespace Echoesphere.Runtime.UI.MusicNote {
    public enum NoteType {
        WaterDrop,
        Crossing,
        Tide,
        Breeze
    }
    
    public static class NoteTypeExtensions {
        public static string ToCommandName(this NoteType noteType) => noteType switch {
            NoteType.WaterDrop => "waterdrop",
            NoteType.Crossing => "crossing",
            NoteType.Tide => "tide",
            NoteType.Breeze => "breeze",
            _ => throw new System.ArgumentOutOfRangeException(nameof(noteType))
        };
    }
}