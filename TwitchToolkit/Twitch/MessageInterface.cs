using ToolkitCore;
using ToolkitCore.Models;
using TwitchToolkit.Votes;
using Verse;

namespace TwitchToolkit.Twitch
{
    public class MessageInterface : TwitchInterfaceBase
    {
        public MessageInterface(Game game)
        { }

        public override void ParseCommand(MessageDetails message)
        {
            if (Helper.ModActive) 
                CommandsHandler.CheckCommand(message);

            if (VoteHandler.voteActive && int.TryParse(message.Message, out int voteId)) 
                VoteHandler.currentVote.RecordVote(ViewerStates.GetState(message.Viewer).id, voteId - 1);

            TwitchToolkit_MainTabWindow.LogChatMessage(message);
        }
    }
}
