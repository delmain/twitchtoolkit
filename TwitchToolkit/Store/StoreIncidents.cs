using ToolkitCore.Models;
using TwitchToolkit.Incidents;

namespace TwitchToolkit.Store
{
    public abstract class IncidentHelper
    {
        public abstract bool IsPossible();
        public abstract void TryExecute();
        public StoreIncident storeIncident = null;
        public ViewerState Viewer { get; set; } = null;
        public string message;
    }

    public abstract class IncidentHelperVariables
    {
        public abstract bool IsPossible(MessageDetails message, ViewerState viewer, bool separateChannel = false);
        public abstract void TryExecute();
        public StoreIncidentVariables storeIncident = null;
        public abstract ViewerState Viewer { get; set; }
        public string message;
    }
}
