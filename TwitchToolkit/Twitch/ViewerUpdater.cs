using ToolkitCore;
using TwitchLib.Client.Models;
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

        public override void ParseCommand(ChatMessage msg)
        {
            Viewer viewer = Viewers.GetViewer(msg.Username);
            GameComponentPawns component = Current.Game.GetComponent<GameComponentPawns>();

            ToolkitSettings.ViewerColorCodes[msg.Username.ToLower()] = msg.ColorHex;

            if (component.HasUserBeenNamed(msg.Username))
            {
                if (!string.IsNullOrWhiteSpace(msg.ColorHex))
                    component.PawnAssignedToUser(msg.Username).story.hairColor = msg.ColorHex.ToUnityColor();
                else
                    component.PawnAssignedToUser(msg.Username).story.hairColor = 
                        new UnityEngine.Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
            }

            if (msg.IsModerator && !viewer.mod)
            {
                viewer.SetAsModerator();
            }

            if (msg.IsSubscriber && !viewer.IsSub)
            {
                viewer.subscriber = true;
            }
        }
    }
}
