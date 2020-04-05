using RimWorld;
using ToolkitCore;
using System;
using System.Collections.Generic;
using Verse;
using System.Linq;

namespace TwitchToolkit.Votes
{
    public class Vote_ExoticItems : Vote
    {
        public Vote_ExoticItems(Dictionary<int, List<Thing>> thingsOptions) : base(new List<int>(thingsOptions.Keys))
        {
            try
            {
                this.thingsOptions = thingsOptions;
            }
            catch (InvalidCastException e)
            {
                Helper.Log(e.Message);
            }
        }

        public override void EndVote()
        {
            Map map = Helper.AnyPlayerMap;
            Find.WindowStack.TryRemove(typeof(VoteWindow));
            Messages.Message(new Message("Chat voted for: " + VoteKeyLabel(DecideWinner()), MessageTypeDefOf.PositiveEvent), true);
            IntVec3 intVec = DropCellFinder.RandomDropSpot(map);
            DropPodUtility.DropThingsNear(intVec, map, thingsOptions[DecideWinner()], 110, false, true, true);
            Find.LetterStack.ReceiveLetter("LetterLabelCargoPodCrash".Translate(), "CargoPodCrash".Translate(), LetterDefOf.PositiveEvent, new TargetInfo(intVec, map, false), null, null);
        }

        public override void StartVote()
        {
            if (ToolkitSettings.VotingWindow || (!ToolkitSettings.VotingWindow && !ToolkitSettings.VotingChatMsgs))
            {
                VoteWindow window = new VoteWindow(this, "Which exotic item should the colony receive?");
                Find.WindowStack.Add(window);
            }

            if (ToolkitSettings.VotingChatMsgs)
            {
                TwitchWrapper.SendChatMessage("Which exotic item should the colony receive?");
                foreach (KeyValuePair<int, List<Thing>> pair in thingsOptions)
                {
                    TwitchWrapper.SendChatMessage($"[{pair.Key + 1}]  {string.Join(", ", pair.Value.Select(t => t.LabelCap))}");
                }
            }
        }

        public override string VoteKeyLabel(int id)
        {
            return string.Join(", ", thingsOptions[id].Select(t => t.LabelCap));
        }

        Dictionary<int, List<Thing>> thingsOptions = null;
    }
}
