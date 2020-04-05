﻿using ToolkitCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ToolkitCore.Models;
using TwitchToolkit.Incidents;
using Verse;

namespace TwitchToolkit.Store
{
    [StaticConstructorOnStartup]
    public static class Purchase_Handler
    {
        static Purchase_Handler()
        {
            allStoreIncidentsSimple = DefDatabase<StoreIncidentSimple>.AllDefs.ToList();
            allStoreIncidentsVariables = DefDatabase<StoreIncidentVariables>.AllDefs.ToList();

            Helper.Log("trying to load vars after def database loaded");

            Toolkit.Mod.GetSettings<ToolkitSettings>();

            viewerNamesDoingVariableCommands = new List<string>();
        }

        public static void ResolvePurchase(ViewerState viewer, MessageDetails message, bool separateChannel = false)
        {
            List<string> command = message.Message.Split(' ').ToList();

            if (command.Count < 2)
            {
                return;
            }

            if (command[0] == "!levelskill")
            {
                command[0] = "levelskill";
                command.Insert(0, "!buy");
            }

            string productKey = command[1].ToLower();
            string formattedMessage = string.Join(" ", command.ToArray());

            StoreIncidentSimple incident = allStoreIncidentsSimple.Find(s => productKey.ToLower() == s.abbreviation);
            
            if (incident != null)
            {
                ResolvePurchaseSimple(viewer, message, incident, formattedMessage);
                return;
            }

            StoreIncidentVariables incidentVariables = allStoreIncidentsVariables.Find(s => productKey.ToLower() == s.abbreviation);

            if (incidentVariables != null)
            {
                ResolvePurchaseVariables(viewer, message, incidentVariables, formattedMessage);
                return;
            }

            Item item = StoreInventory.items.Find(s => s.abr == productKey);

            Helper.Log($"abr: {productKey} ");

            if (item != null)
            {
                List<String> commandSplit = message.Message.Split(' ').ToList();
                commandSplit.Insert(1, "item");

                if (commandSplit.Count < 4)
                {
                    commandSplit.Add("1");
                }

                if (!int.TryParse(commandSplit[3], out int quantity))
                {
                    commandSplit.Insert(3, "1");
                }

                formattedMessage = string.Join(" ", commandSplit.ToArray());

                ResolvePurchaseVariables(viewer, message, StoreIncidentDefOf.Item, formattedMessage);
            }

            return;
        }

        public static void ResolvePurchaseSimple(ViewerState viewer, MessageDetails message, StoreIncidentSimple incident, string formattedMessage, bool separateChannel = false)
        {
            int cost = incident.cost;

            if (cost < 1) 
                return;

            if (CheckIfViewerIsInVariableCommandList(message)) 
                return;

            if (!CheckIfViewerHasEnoughCoins(message, viewer, cost)) 
                return;

            if (CheckIfKarmaTypeIsMaxed(incident, message)) 
                return;

            if (CheckIfIncidentIsOnCooldown(incident, message)) 
                return;

            IncidentHelper helper = StoreIncidentMaker.MakeIncident(incident);
            
            if (helper == null)
            {
                Helper.Log("Missing helper for incident " + incident.defName);
                return;
            }

            if (!helper.IsPossible())
            {
                message.Reply("TwitchToolkitEventNotPossible".Translate());
                return;
            }

            if (!ToolkitSettings.UnlimitedCoins)
            {
                viewer.TakeViewerCoins(cost);
            }

            Store_Component component = Current.Game.GetComponent<Store_Component>();

            helper.Viewer = viewer;
            helper.message = formattedMessage;

            Ticker.IncidentHelpers.Enqueue(helper);
            Store_Logger.LogPurchase(viewer.username, formattedMessage);
            component.LogIncident(incident);
            viewer.CalculateNewKarma(incident.karmaType, cost);    

            if (ToolkitSettings.PurchaseConfirmations)
            {
                TwitchWrapper.SendChatMessage(
                    Helper.ReplacePlaceholder(
                        "TwitchToolkitEventPurchaseConfirm".Translate(), 
                        first: incident.label.CapitalizeFirst(), 
                        viewer: viewer.username
                        ));
            }
        }

        public static void ResolvePurchaseVariables(ViewerState viewer, MessageDetails message, StoreIncidentVariables incident, string formattedMessage, bool separateChannel = false)
        {
            int cost = incident.cost;

            if (cost < 1 && incident.defName != "Item") 
                return;

            if (CheckIfViewerIsInVariableCommandList(message)) 
                return;

            if (!CheckIfViewerHasEnoughCoins(message, viewer, cost)) 
                return;

            if (incident != DefDatabase<StoreIncidentVariables>.GetNamed("Item"))
            {
                if (CheckIfKarmaTypeIsMaxed(incident, message)) 
                    return;
            }
            else
            {
                if (CheckIfCarePackageIsOnCooldown(message)) 
                    return;
            }

            if (CheckIfIncidentIsOnCooldown(incident, message)) 
                return;

            viewerNamesDoingVariableCommands.Add(viewer.username);

            IncidentHelperVariables helper = StoreIncidentMaker.MakeIncidentVariables(incident);
            
            if (helper == null)
            {
                Helper.Log("Missing helper for incident " + incident.defName);
                return;
            }

            if (!helper.IsPossible(message, viewer))
            {
                if (viewerNamesDoingVariableCommands.Contains(message.Viewer.Username))
                    viewerNamesDoingVariableCommands.Remove(message.Viewer.Username);
                return;
            }

            Store_Component component = Current.Game.GetComponent<Store_Component>();

            helper.Viewer = viewer;
            helper.message = formattedMessage;

            Ticker.IncidentHelperVariables.Enqueue(helper);
            Store_Logger.LogPurchase(viewer.username, message.Message);
            component.LogIncident(incident);
        }

        public static bool CheckIfViewerIsInVariableCommandList(MessageDetails message, bool separateChannel = false)
        {
            if (viewerNamesDoingVariableCommands.Contains(message.Viewer.Username))
            {
                message.Reply("You must wait for the game to unpause to buy something else.");
                return true;
            }
            return false;
        }

        public static bool CheckIfViewerHasEnoughCoins(MessageDetails message, ViewerState viewer, int finalPrice, bool separateChannel = false)
        {
            if (!ToolkitSettings.UnlimitedCoins && viewer.Coins < finalPrice)
            {
                message.Reply(Helper.ReplacePlaceholder(
                    "TwitchToolkitNotEnoughCoins".Translate(),
                    viewer: message.Viewer.DisplayName,
                    amount: finalPrice.ToString(),
                    first: viewer.Coins.ToString()
                ));
                return false;
            }
            return true;
        }

        public static bool CheckIfKarmaTypeIsMaxed(StoreIncident incident, MessageDetails message, bool separateChannel = false)
        {
            bool maxed = CheckTimesKarmaTypeHasBeenUsedRecently(incident);        

            if (maxed)
            {
                Store_Component component = Current.Game.GetComponent<Store_Component>();
                message.Reply($"{incident.label.CapitalizeFirst()} is maxed from karmatype, wait " + component.DaysTillIncidentIsPurchaseable(incident) + " days to purchase.");
            }

            return maxed;
        }

        public static bool CheckTimesKarmaTypeHasBeenUsedRecently(StoreIncident incident)
        {
            // if they have max event setting off always return false
            if (!ToolkitSettings.MaxEvents)
            {
                return false;
            }

            Store_Component component = Current.Game.GetComponent<Store_Component>();

            switch (incident.karmaType)
            {
                case KarmaType.Bad:
                    return component.KarmaTypesInLogOf(incident.karmaType) >= ToolkitSettings.MaxBadEventsPerInterval;
                case KarmaType.Good:
                    return component.KarmaTypesInLogOf(incident.karmaType) >= ToolkitSettings.MaxGoodEventsPerInterval;
                case KarmaType.Neutral:
                    return component.KarmaTypesInLogOf(incident.karmaType) >= ToolkitSettings.MaxNeutralEventsPerInterval;
                case KarmaType.Doom:
                    return component.KarmaTypesInLogOf(incident.karmaType) >= ToolkitSettings.MaxBadEventsPerInterval;
            }

            return false;
        }

        public static bool CheckIfCarePackageIsOnCooldown(MessageDetails message, bool separateChannel = false)
        {
            if (!ToolkitSettings.MaxEvents)
            {
                return false;
            }

            Store_Component component = Current.Game.GetComponent<Store_Component>();
            StoreIncidentVariables incident = DefDatabase<StoreIncidentVariables>.GetNamed("Item");

            if (component.IncidentsInLogOf(incident.abbreviation) >= ToolkitSettings.MaxCarePackagesPerInterval)
            {
                float daysTill = component.DaysTillIncidentIsPurchaseable(incident);
                message.Reply($"Care packages are on cooldown, wait {daysTill} day{(daysTill != 1 ? "s" : "")}.");
                return true;
            }

            return false;
        }

        public static bool CheckIfIncidentIsOnCooldown(StoreIncident incident, MessageDetails message, bool separateChannel = false)
        {
            if (!ToolkitSettings.EventsHaveCooldowns)
            {
                return false;
            }

            Store_Component component = Current.Game.GetComponent<Store_Component>();

            bool maxed = component.IncidentsInLogOf(incident.abbreviation) >= incident.eventCap;        

            if (maxed)
            {
                float days = component.DaysTillIncidentIsPurchaseable(incident);
                message.Reply($"{incident.label.CapitalizeFirst()} is maxed, wait {days} day{(days == 1 ? "" : "s")} to purchase.");
            }

            return maxed;
        }

        public static void QueuePlayerMessage(ViewerState viewer, string message, int variables = 0)
        {
            string colorCode = ViewerState.GetViewerColorCode(viewer.username);
            string userNameTag = $"<color=#{colorCode}>{viewer.username}</color>";
            string[] command = message.Split(' ');
            string output = "\n\n";

            if (command.Length  - 2 == variables)
            {
                output += "<i>from</i> " + userNameTag;
            }
            else
            {
                output += userNameTag + ":";

                for (int i = 2 + variables; i < command.Length; i++)
                {
                    if (viewer.IsBroadcaster)
                    {
                        output += " " + AdminText(command[i]);
                    }
                    else if (viewer.IsSub)
                    {
                        output += " " + SubText(command[i]);
                    }
                    else if (viewer.IsVIP)
                    {
                        output += " " + VIPText(command[i]);
                    }
                    else if (viewer.IsModerator)
                    {
                        output += " " + ModText(command[i]);
                    }
                    else
                    {
                        output += " " + command[i];
                    }
                }
            }

            Helper.playerMessages.Add(output);
        }

        static string AdminText(string input)
        {
            char[] chars = input.ToCharArray();
            StringBuilder output = new StringBuilder();
            output.Append("<size=24>");

            foreach (char str in chars)
            {
                output.Append($"<color=#{Helper.GetRandomColorCode()}>{str}</color>");
            }

            output.Append("</size>");

            return output.ToString();
        }

        static string SubText(string input)
        {
            return "<color=#D9BB25>" + input + "</color>";
        }

        static string VIPText(string input)
        {
            return "<color=#5F49F2>" + input + "</color>";
        }

        static string ModText(string input)
        {
            return "<color=#238C48>" + input + "</color>";
        }

        public static List<StoreIncidentSimple> allStoreIncidentsSimple;
        public static List<StoreIncidentVariables> allStoreIncidentsVariables;

        public static List<string> viewerNamesDoingVariableCommands;
    }
}
