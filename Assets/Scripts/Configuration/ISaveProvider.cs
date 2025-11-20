namespace Configuration {
    public interface ISaveProvider {
        object CaptureState();
        
        void RestoreState(object state);
        
        string UniqueId { get; }
    }
}