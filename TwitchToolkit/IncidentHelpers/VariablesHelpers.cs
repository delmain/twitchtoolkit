using ToolkitCore;
using System;
using TwitchToolkit.Incidents;
using TwitchToolkit.Store;
using Verse;
using ToolkitCore.Models;

namespace TwitchToolkit.IncidentHelpers
{
    public static class VariablesHelpers
    {
        public static void ViewerDidWrongSyntax(MessageDetails message, string syntax, bool separateChannel = false)
        {
            message.Reply($"Syntax is {syntax}.");
        }

        public static bool PointsWagerIsValid(string wager, MessageDetails message, ViewerState viewer, ref int pointsWager, ref StoreIncidentVariables incident, bool separateChannel = false, int quantity = 1, int maxPrice = 25000)
        {
            try
            {
                if (! int.TryParse( wager, out checked(pointsWager) ) )
                {
                    ViewerDidWrongSyntax(message, incident.syntax);
                    return false;
                }
                pointsWager = checked(pointsWager * quantity);
            }
            catch (OverflowException e)
            {
                Helper.Log(e.Message);
                message.Reply($"Points wager is invalid.");
                return false;
            }

            if (incident.maxWager > 0 && incident.maxWager > incident.cost && pointsWager > incident.maxWager)
            {
                message.Reply($"You cannot spend more than {incident.maxWager} coins on {incident.abbreviation.CapitalizeFirst()}");
                return false;
            }

            if (pointsWager < incident.cost || pointsWager < incident.minPointsToFire)
            {
                message.Reply(Helper.ReplacePlaceholder(
                    "TwitchToolkitMinPurchaseNotMet".Translate(), 
                    amount: pointsWager.ToString(), 
                    first: incident.cost.ToString()
                ));
                return false;
            }

            if (!Purchase_Handler.CheckIfViewerHasEnoughCoins(message, viewer, pointsWager)) 
                return false;

            return true;
        }

        public static void SendPurchaseMessage(string message, bool separateChannel = false)
        {
            if (ToolkitSettings.PurchaseConfirmations)
            {
                TwitchWrapper.SendChatMessage(message);
            }
        }
    }
}
