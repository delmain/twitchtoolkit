using System;
using System.Linq;
using ToolkitCore;
using ToolkitCore.Models;
using TwitchToolkit.Store;
using Verse;

namespace TwitchToolkit.Commands.ModCommands
{
    public class RefreshViewers : CommandDriver
    {
        public override void RunCommand(MessageDetails message)
        {
            TwitchToolkitDev.WebRequest_BeginGetResponse.Main("https://tmi.twitch.tv/group/user/" + ToolkitSettings.Channel.ToLower() + "/chatters", new Func<TwitchToolkitDev.RequestState, bool>(ViewerStates.SaveUsernamesFromJsonResponse));

            message.Reply("Viewers have been refreshed.");
        }
    }

    public class KarmaRound : CommandDriver
    {
        public override void RunCommand(MessageDetails message)
        {
            ViewerStates.AwardViewersCoins();

            message.Reply("Rewarding all active viewers coins.");
        }
    }

    public class GiveAllCoins : CommandDriver
    {
        public override void RunCommand(MessageDetails message)
        {
            try
            {
                string[] command = message.Message.Split(' ');

                if (command.Length < 2)
                {
                    return;
                }

                bool isNumeric = int.TryParse(command[1], out int amount);

                if (isNumeric)
                {
                    foreach (ViewerState vwr in ViewerStates.All)
                    {
                        vwr.GiveViewerCoins(amount);
                    }

                    message.Reply(Helper.ReplacePlaceholder("TwitchToolkitGiveAllCoins".Translate(), amount: amount.ToString()));
                }
            }
            catch (InvalidCastException e)
            {
                Helper.Log("Give All Coins Syntax Error " + e.Message);
            }
        }
    }

    public class GiveCoins : CommandDriver
    {
        public override void RunCommand(MessageDetails message)
        {
            try
            {
                Log.Message($"GiveCoins command from {message.Viewer.Username}");
                string[] command = message.Message.Split(' ');

                if (command.Length < 3)
                {
                    return;
                }

                string receiver = command[1].Replace("@", "");

                if (!message.Viewer.IsBroadcaster && string.Equals(message.Viewer.Username, receiver, StringComparison.CurrentCultureIgnoreCase))
                {
                    message.Reply("TwitchToolkitModCannotGiveCoins".Translate());
                    return;
                }

                int amount;
                bool isNumeric = int.TryParse(command[2], out amount);
                if (isNumeric)
                {
                    // If the user hasn't been online in this stream, there won't be a Viewer object for them, in that case, default to using whatever the gifter put for receiver
                    var gifteeViewer = Viewers.FindByUsername(receiver) ?? new Viewer(DisplayName: receiver);
                    var gifteeState = ViewerStates.GetViewer(receiver);

                    Helper.Log($"Giving viewer {gifteeViewer.DisplayName} {amount} coins");
                    gifteeState.GiveViewerCoins(amount);
                    message.Reply(Helper.ReplacePlaceholder("TwitchToolkitGivingCoins".Translate(), viewer: gifteeViewer.DisplayName, amount: amount.ToString(), newbalance: gifteeState.Coins.ToString()));
                    Store_Logger.LogGiveCoins(message.Viewer.Username, gifteeViewer.DisplayName, amount);
                }
            }
            catch (InvalidCastException e)
            {
                Helper.Log("Invalid Give Viewer Coins Command " + e.Message);
            }
        }
    }

    public class CheckUser : CommandDriver
    {
        public override void RunCommand(MessageDetails message)
        {
            try
            {
                string[] command = message.Message.Split(' ');

                if (command.Length < 2)
                {
                    return;
                }

                string target = command[1].Replace("@", "");

                // If the user hasn't been online this stream, they won't have a viewer object, in that case, use what was entered.
                var targetedViewer = Viewers.FindByUsername(target) ?? new Viewer(DisplayName: target);
                ViewerState targeted = ViewerStates.GetViewer(target);
                message.Reply(Helper.ReplacePlaceholder("TwitchToolkitCheckUser".Translate(), 
                    viewer: targetedViewer.DisplayName, 
                    amount: targeted.Coins.ToString(), 
                    karma: targeted.Karma.ToString()));
            }
            catch (InvalidCastException e)
            {
                Helper.Log("Invalid Check User Command " + e.Message);
            }
        }
    }

    public class SetKarma : CommandDriver
    {
        public override void RunCommand(MessageDetails message)
        {
            try
            {
                string[] command = message.Message.Split(' ');

                if (command.Length < 3)
                {
                    return;
                }

                string target = command[1].Replace("@", "");
                int amount;
                bool isNumeric = int.TryParse(command[2], out amount);
                if (isNumeric)
                {
                    ViewerState targeted = ViewerStates.GetViewer(target);
                    var targetedViewer = Viewers.All.FirstOrDefault(v => string.Equals(target, v.Username, StringComparison.CurrentCultureIgnoreCase));
                    var username = targetedViewer?.DisplayName ?? target;
                    targeted.SetViewerKarma(amount);
                    message.Reply(Helper.ReplacePlaceholder("TwitchToolkitSetKarma".Translate(), viewer: targeted.username, karma: amount.ToString()));
                }
            }
            catch (InvalidCastException e)
            {
                Helper.Log("Invalid Check User Command " + e.Message);
            }
        }
    }

    public class ToggleCoins : CommandDriver
    {
        public override void RunCommand(MessageDetails message)
        {
            if (ToolkitSettings.EarningCoins)
            {
                ToolkitSettings.EarningCoins = false;
                message.Reply($"{"TwitchToolkitEarningCoinsMessage".Translate()} {"TwitchToolkitOff".Translate()}");
            }
            else
            {
                ToolkitSettings.EarningCoins = true;
                message.Reply($"{"TwitchToolkitEarningCoinsMessage".Translate()} {"TwitchToolkitOn".Translate()}");
            }
        }
    }
}
