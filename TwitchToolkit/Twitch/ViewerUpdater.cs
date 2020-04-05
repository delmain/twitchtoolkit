using ToolkitCore;
using ToolkitCore.Models;
using TwitchToolkit.PawnQueue;
using TwitchToolkit.Utilities;
using Verse;

namespace TwitchToolkit.Twitch
{
    public class ViewerUpdater : TwitchInterfaceBase
    {
        public ViewerUpdater(Game game)
        {

        }

        public override void ParseCommand(MessageDetails msg)
        {
            GameComponentPawns component = Current.Game.GetComponent<GameComponentPawns>();

            if (component.HasUserBeenNamed(msg.Viewer.Username))
            {
                if (!string.IsNullOrWhiteSpace(msg.Viewer.ColorHex))
                    component.PawnAssignedToUser(msg.Viewer.Username).story.hairColor = msg.Viewer.ColorHex.ToUnityColor();
            }
        }
    }
}
