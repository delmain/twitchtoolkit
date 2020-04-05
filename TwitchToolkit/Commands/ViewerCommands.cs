using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolkitCore;
using ToolkitCore.Models;
using TwitchToolkit.PawnQueue;
using TwitchToolkit.Store;
using Verse;

namespace TwitchToolkit.Commands.ViewerCommands
{
    public class CheckBalance : CommandDriver
    {
        public override void RunCommand(MessageDetails message)
        {
            ViewerState viewer = ViewerStates.GetState(message.Viewer);

            message.Reply(Helper.ReplacePlaceholder("TwitchToolkitBalanceMessage".Translate(), amount: viewer.Coins.ToString(), karma: viewer.Karma.ToString()));
        }
    }

    public class WhatIsKarma : CommandDriver
    {
        public override void RunCommand(MessageDetails message)
        {
            ViewerState viewer = ViewerStates.GetState(message.Viewer);

            message.Reply($"{"TwitchToolkitWhatIsKarma".Translate()} {viewer.Karma}%");
        }
    }

    public class PurchaseList : CommandDriver
    {
        public override void RunCommand(MessageDetails message)
        {
            message.Reply($"{"TwitchToolkitPurchaseList".Translate()} {ToolkitSettings.CustomPricingSheetLink}");
        }
    }

    public class GiftCoins : CommandDriver
    {
        public override void RunCommand(MessageDetails message)
        {
            ViewerState viewer = ViewerStates.GetState(message.Viewer);

            string[] command = message.Message.Split(' ');

            if (command.Count() < 3)
            {
                Log.Message("command not long enough");
                return;
            }

            string target = command[1].Replace("@", string.Empty);

            bool isNumeric = int.TryParse(command[2], out int amount);
            if (isNumeric && amount > 0)
            {
                // if the viewer hasn't been online this stream, they won't have a Viewer registered
                var giftee = Viewers.FindByUsername(target) ?? new Viewer(DisplayName: target);
                ViewerState gifteeState = ViewerStates.GetViewer(target);

                if (ToolkitSettings.KarmaReqsForGifting)
                {
                    if (gifteeState.Karma < ToolkitSettings.MinimumKarmaToRecieveGifts || viewer.Karma < ToolkitSettings.MinimumKarmaToSendGifts)
                    {
                        return;
                    }
                }

                if (viewer.Coins >= amount)
                {
                    viewer.TakeViewerCoins(amount);
                    gifteeState.GiveViewerCoins(amount);
                    message.Reply(Helper.ReplacePlaceholder("TwitchToolkitGiftCoins".Translate(), amount: amount.ToString(), from: message.Viewer.DisplayName));
                    Store_Logger.LogGiftCoins(message.Viewer.DisplayName, giftee.DisplayName, amount);
                }
            }
        }
    }

    public class JoinQueue : CommandDriver
    {
        public override void RunCommand(MessageDetails message)
        {
            ViewerState viewer = ViewerStates.GetState(message.Viewer);

            GameComponentPawns pawnComponent = Current.Game.GetComponent<GameComponentPawns>();

            if (pawnComponent.HasUserBeenNamed(message.Viewer.Username) || pawnComponent.UserInViewerQueue(message.Viewer.Username))
            {
                return;
            }

            if (ToolkitSettings.ChargeViewersForQueue)
            {
                if (viewer.Coins < ToolkitSettings.CostToJoinQueue)
                {
                    message.Reply($"You do not have enough coins to purchase a ticket, it costs {ToolkitSettings.CostToJoinQueue} and you have {viewer.Coins}.");
                    return;
                }

                viewer.TakeViewerCoins(ToolkitSettings.CostToJoinQueue);
            }

            pawnComponent.AddViewerToViewerQueue(message.Viewer.Username);
            message.Reply("You have purchased a ticket and are in the queue!");
        }
    }

    public class ModInfo : CommandDriver
    {
        public override void RunCommand(MessageDetails message)
        {
            message.Reply("I don't know, ask Delmain, he crashed all this back together after it was abandoned.");
        }
    }

    public class Buy : CommandDriver
    {
        public override void RunCommand(MessageDetails message)
        {
            ViewerState viewer = ViewerStates.GetState(message.Viewer);
            Purchase_Handler.ResolvePurchase(viewer, message);
        }
    }

    public class ModSettings : CommandDriver
    {
        public override void RunCommand(MessageDetails message)
        {
            Command buyCommand = DefDatabase<Command>.GetNamed("Buy");

            string storeon = buyCommand.enabled ? "TwitchToolkitOn".Translate() : "TwitchToolkitOff".Translate();
            string earningcoins = ToolkitSettings.EarningCoins ? "TwitchToolkitOn".Translate() : "TwitchToolkitOff".Translate();

            string stats_message = Helper.ReplacePlaceholder("TwitchToolkitModSettings".Translate(),
                amount: ToolkitSettings.CoinAmount.ToString(),
                first: ToolkitSettings.CoinInterval.ToString(),
                second: storeon,
                third: earningcoins,
                karma: ToolkitSettings.KarmaCap.ToString()
                );

            message.Reply(stats_message);
        }
    }

    public class Instructions : CommandDriver
    {
        public override void RunCommand(MessageDetails message)
        {
            Command allCommandsCommand = DefDatabase<Command>.GetNamed("AvailableCommands");

            message.Reply($"The toolkit is a mod where you earn coins while you watch. Check out the bit.ly/toolkit-guide  or use !{allCommandsCommand.command} for a short list. {ToolkitSettings.Channel.CapitalizeFirst()} has a list of items/events to purchase at {ToolkitSettings.CustomPricingSheetLink}");
        }
    }

    public class AvailableCommands : CommandDriver
    {
        public override void RunCommand(MessageDetails message)
        {
            var commandList = string.Join(", ", DefDatabase<Command>.AllDefs.Where(c => c.enabled && !c.requiresAdmin && !c.requiresMod).Select(cc => $"!{cc.command}"));
            message.Reply($"viewer commands: {commandList}");
        }
    }

    public class InstalledMods : CommandDriver
    {
        public override void RunCommand(MessageDetails message)
        {
            if (message is ChatDetails && (DateTime.Now - Cooldowns.modsCommandCooldown).TotalSeconds <= 15)
            {
                return;
            }

            Cooldowns.modsCommandCooldown = DateTime.Now;
            var sb = new StringBuilder();
            sb.Append("Version: ").Append(Toolkit.Mod.Version).Append(", Mods: ");
            bool first = true;
            foreach(var modName in LoadedModManager.RunningMods.Select((m) => m.Name))
            {
                if (first)
                    first = false;
                else
                    sb.Append(", ");

                if(sb.Length + modName.Length > 240)
                {
                    message.Reply(sb.ToString());
                    sb.Length = 0;
                }

                sb.Append(modName);
            }
            message.Reply(sb.ToString());

            return;
        }
    }

    public static class Cooldowns
    {
        public static DateTime modsCommandCooldown = DateTime.Now;
    }
}
