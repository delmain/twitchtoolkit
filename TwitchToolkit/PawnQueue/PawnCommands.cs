using ToolkitCore;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolkitCore.Models;
using TwitchToolkit.Store;
using Verse;

namespace TwitchToolkit.PawnQueue
{
    public class PawnCommands : TwitchInterfaceBase
    {
        public PawnCommands(Game game)
        {

        }

        public override void ParseCommand(MessageDetails msg)
        {
            ViewerState viewer = ViewerStates.GetState(msg.Viewer);
            GameComponentPawns component = Current.Game.GetComponent<GameComponentPawns>();
            
            if (msg.Message.StartsWith("!mypawnskills") && CommandsHandler.AllowCommand(msg))
            {
                
                if (!component.HasUserBeenNamed(msg.Viewer.Username))
                {
                    msg.Reply("You are not in the colony.");
                    return;
                }

                Pawn pawn = component.PawnAssignedToUser(msg.Viewer.Username);
                var output = new StringBuilder($"{pawn.Name.ToStringShort.CapitalizeFirst()}'s skill levels are ");

                List<SkillRecord> skills = pawn.skills.skills;

                for (int i = 0; i < skills.Count; i++)
                {
                    if (skills[i].TotallyDisabled)
                    {
                        output.Append(skills[i].def.LabelCap).Append(": -");
                    }
                    else
                    {
                        output.Append(skills[i].def.LabelCap).Append(": ").Append(skills[i].levelInt);
                    }

                    if (skills[i].passion == Passion.Minor) 
                        output.Append("+");

                    if (skills[i].passion == Passion.Major) 
                        output.Append("++");

                    if (i != skills.Count - 1)
                        output.Append(", ");
                }

                msg.Reply(output.ToString());
            }

            if (msg.Message.StartsWith("!mypawnstory") && CommandsHandler.AllowCommand(msg))
            {
                if (!component.HasUserBeenNamed(msg.Viewer.Username))
                {
                    msg.Reply("You are not in the colony.");
                    return;
                }

                Pawn pawn = component.PawnAssignedToUser(msg.Viewer.Username);

                var output = new StringBuilder($"About {pawn.Name.ToStringShort.CapitalizeFirst()}: ");
                output.Append(string.Join(", ", pawn.story.AllBackstories.Select(b => b.title)));
                output.Append(" | ").Append(pawn.gender);

                output.Append(" | Incapable of: ");
                var combinedDisabledWorkTags = pawn.story.DisabledWorkTagsBackstoryAndTraits;
                if (combinedDisabledWorkTags == WorkTags.None)
                    output.Append("(None)");
                else
                    output.Append(string.Join(", ", WorkTagsFrom(combinedDisabledWorkTags).Select(t => t.LabelTranslated())));

                output.Append(" | Traits: ");
                output.Append(string.Join(", ", pawn.story.traits.allTraits.Select(t => t.LabelCap)));

                msg.Reply(output.ToString());
            }

            if (msg.Message.StartsWith("!changepawnname") && CommandsHandler.AllowCommand(msg))
            {
                string[] command = msg.Message.Split(' ');

                if (command.Length < 2) 
                    return;

                string newName = command[1];

                if (!component.HasUserBeenNamed(viewer.username))
                {
                    msg.Reply("You are not in the colony.");
                    return;
                }

                if (newName == null || newName == "" || newName.Length > 16)
                {
                    msg.Reply("Your name can be up to 16 characters.");
                    return;
                }

                if (!Purchase_Handler.CheckIfViewerHasEnoughCoins(msg, viewer, 500, true)) 
                    return;

                viewer.TakeViewerCoins(500);
                nameRequests.Add(msg.Viewer.Username, newName);
                TwitchWrapper.SendChatMessage($"@{ToolkitSettings.Channel}, @{msg.Viewer.DisplayName} has requested to be named {newName}, use !approvename @{msg.Viewer.DisplayName} or !declinename @{msg.Viewer.DisplayName}");
            }

            if (viewer.IsModerator || viewer.IsBroadcaster)
            {
                if (msg.Message.StartsWith("!unstickpeople"))
                {
                    Purchase_Handler.viewerNamesDoingVariableCommands = new List<string>();
                }

                if (msg.Message.StartsWith("!approvename"))
                {
                    string[] command = msg.Message.Split(' ');
                    
                    if (command.Length < 2) 
                        return;

                    string username = command[1].Replace("@", "");

                    if (username == null || username == "" || !nameRequests.ContainsKey(username))
                    {
                        msg.Reply($"Invalid username");
                        return;
                    }

                    if (!component.HasUserBeenNamed(username)) 
                        return;

                    Pawn pawn = component.PawnAssignedToUser(username);
                    NameTriple old = pawn.Name as NameTriple;
                    pawn.Name = new NameTriple(old.First, nameRequests[username], old.Last);
                    TwitchWrapper.SendChatMessage($"@{msg.Viewer.DisplayName} approved request for name change from {old} to {pawn.Name}");
                }

                if (msg.Message.StartsWith("!declinename"))
                {
                    string[] command = msg.Message.Split(' ');
                    
                    if (command.Length < 2) 
                        return;

                    string username = command[1].Replace("@", "");

                    if (username == null || username == "" || !nameRequests.ContainsKey(username))
                    {
                        msg.Reply("Invalid username");
                        return;
                    }

                    if (!component.HasUserBeenNamed(username)) 
                        return;

                    var user = Viewers.FindByUsername(username)?.DisplayName ?? username;
                    nameRequests.Remove(username);
                    TwitchWrapper.SendChatMessage($"@{msg.Viewer.DisplayName} declined name change request from {user}");
                }
            }

            Store_Logger.LogString("Parsed pawn command");
        }

        private static IEnumerable<WorkTags> WorkTagsFrom(WorkTags tags)
        {
            foreach (WorkTags workTag in tags.GetAllSelectedItems<WorkTags>())
            {
                if (workTag != WorkTags.None)
                {
                    yield return workTag;
                }
            }
            yield break;
        }

        public Dictionary<string, string> nameRequests = new Dictionary<string, string>();
    }
}
