namespace Echoesphere.Runtime.Configuration {
    public interface ISaveProvider {
        object CaptureState();
        
        void RestoreState(object state);
        
        string UniqueId { get; }
    }
}