﻿using RimWorld;
using System.Collections.Generic;
using ToolkitCore;
using ToolkitCore.Models;
using TwitchToolkit.Incidents;
using TwitchToolkit.Store;
using Verse;

namespace TwitchToolkit.IncidentHelpers.Raids
{
    public class Raid : IncidentHelperVariables
    {
        public override bool IsPossible(MessageDetails message, ViewerState viewer, bool separateChannel = false)
        {
            this.separateChannel = separateChannel;
            this.Viewer = viewer;
            string[] command = message.Message.Split(' ');
            if (command.Length < 3)
            {
                message.Reply($"Syntax is {this.storeIncident.syntax}");
                return false;
            }

            if (!VariablesHelpers.PointsWagerIsValid(command[2], message, viewer, ref pointsWager, ref storeIncident, separateChannel))
                return false;

            target = Current.Game.AnyPlayerHomeMap;
            if (target == null)
                return false;

            parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatSmall, target);
            parms.points = IncidentHelper_PointsHelper.RollProportionalGamePoints(storeIncident, pointsWager, parms.points);
            parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;

            worker = new Incidents.IncidentWorker_RaidEnemy
            {
                def = IncidentDefOf.RaidEnemy
            };

            return worker.CanFireNow(parms);
        }

        public override void TryExecute()
        {
            if (worker.TryExecute(parms))
            {
                Viewer.TakeViewerCoins(pointsWager);
                Viewer.CalculateNewKarma(this.storeIncident.karmaType, pointsWager);
                VariablesHelpers.SendPurchaseMessage($"Starting raid with {pointsWager} points wagered and {(int)parms.points} raid points purchased by {Viewer.username}");
                return;
            }
            TwitchWrapper.SendChatMessage("Could not generate parameters for raid.");
        }

        public int pointsWager = 0;
        public IIncidentTarget target = null;
        public IncidentWorker worker = null;
        public IncidentParms parms = null;
        private bool separateChannel = false;

        public override ViewerState Viewer { get; set; }
    }

    public class DropRaid : IncidentHelperVariables
    {
        public override bool IsPossible(MessageDetails message, ViewerState viewer, bool separateChannel = false)
        {
            this.separateChannel = separateChannel;
            this.Viewer = viewer;
            string[] command = message.Message.Split(' ');
            if (command.Length < 3)
            {
                message.Reply($"Syntax is {this.storeIncident.syntax}");
                return false;
            }

            if (!VariablesHelpers.PointsWagerIsValid(command[2], message, viewer, ref pointsWager, ref storeIncident, separateChannel))
                return false;

            Helper.Log("Finding target");

            target = Current.Game.AnyPlayerHomeMap;
            if (target == null)
                return false;

            parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatSmall, target);
            parms.points = IncidentHelper_PointsHelper.RollProportionalGamePoints(storeIncident, pointsWager, parms.points);
            parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            parms.raidArrivalMode = PawnsArrivalModeDefOf.CenterDrop;
            parms.faction = Find.FactionManager.RandomEnemyFaction(false, false, false, TechLevel.Industrial);

            worker = new Incidents.IncidentWorker_RaidEnemy
            {
                def = IncidentDefOf.RaidEnemy
            };

            return worker.CanFireNow(parms);
        }

        public override void TryExecute()
        {
            if (worker.TryExecute(parms))
            {
                Viewer.TakeViewerCoins(pointsWager);
                Viewer.CalculateNewKarma(this.storeIncident.karmaType, pointsWager);
                VariablesHelpers.SendPurchaseMessage(
                    $"Starting drop raid with {pointsWager} points wagered and {(int)parms.points} raid points purchased by {Viewer.username}", 
                separateChannel);
                return;
            }

            TwitchWrapper.SendChatMessage($"Could not generate parameters for drop raid.");
        }

        public int pointsWager = 0;
        public IIncidentTarget target = null;
        public IncidentWorker worker = null;
        public IncidentParms parms = null;
        private bool separateChannel = false;

        public override ViewerState Viewer { get; set; }
    }

    public class SapperRaid : IncidentHelperVariables
    {
        public override bool IsPossible(MessageDetails message, ViewerState viewer, bool separateChannel = false)
        {
            this.separateChannel = separateChannel;
            this.Viewer = viewer;
            string[] command = message.Message.Split(' ');
            if (command.Length < 3)
            {
                message.Reply($"Syntax is {this.storeIncident.syntax}");
                return false;
            }

            if (!VariablesHelpers.PointsWagerIsValid(command[2], message, viewer, ref pointsWager, ref storeIncident, separateChannel))
                return false;

            target = Current.Game.AnyPlayerHomeMap;
            if (target == null)
                return false;

            parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatSmall, target);
            parms.points = IncidentHelper_PointsHelper.RollProportionalGamePoints(storeIncident, pointsWager, parms.points);
            parms.raidStrategy = DefDatabase<RaidStrategyDef>.GetNamed("ImmediateAttackSappers");
            parms.faction = Find.FactionManager.RandomEnemyFaction(false, false, false, TechLevel.Industrial);

            worker = new Incidents.IncidentWorker_RaidEnemy
            {
                def = IncidentDefOf.RaidEnemy
            };

            return worker.CanFireNow(parms);
        }

        public override void TryExecute()
        {
            if (worker.TryExecute(parms))
            {
                Viewer.TakeViewerCoins(pointsWager);
                Viewer.CalculateNewKarma(this.storeIncident.karmaType, pointsWager);
                VariablesHelpers.SendPurchaseMessage($"Starting sapper raid with {pointsWager} points wagered and {(int)parms.points} raid points purchased by {Viewer.username}");
                return;
            }
            TwitchWrapper.SendChatMessage($"Could not generate parameters for sapper raid.");
        }

        public int pointsWager = 0;
        public IIncidentTarget target = null;
        public IncidentWorker worker = null;
        public IncidentParms parms = null;
        private bool separateChannel = false;

        public override ViewerState Viewer { get; set; }
    }

    public class SiegeRaid : IncidentHelperVariables
    {
        public override bool IsPossible(MessageDetails message, ViewerState viewer, bool separateChannel = false)
        {
            this.separateChannel = separateChannel;
            this.Viewer = viewer;
            string[] command = message.Message.Split(' ');
            if (command.Length < 3)
            {
                message.Reply($"Syntax is {this.storeIncident.syntax}");
                return false;
            }

            if (!VariablesHelpers.PointsWagerIsValid(command[2], message, viewer, ref pointsWager, ref storeIncident, separateChannel))
                return false;

            target = Current.Game.AnyPlayerHomeMap;
            if (target == null)
                return false;

            parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatSmall, target);
            parms.points = IncidentHelper_PointsHelper.RollProportionalGamePoints(storeIncident, pointsWager, parms.points);
            parms.raidStrategy = DefDatabase<RaidStrategyDef>.GetNamed("Siege");
            //parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeDrop;
            parms.faction = Find.FactionManager.RandomEnemyFaction(false, false, false, TechLevel.Industrial);

            worker = new Incidents.IncidentWorker_RaidEnemy
            {
                def = IncidentDefOf.RaidEnemy
            };

            return worker.CanFireNow(parms);
        }

        public override void TryExecute()
        {
            if (worker.TryExecute(parms))
            {
                Viewer.TakeViewerCoins(pointsWager);
                Viewer.CalculateNewKarma(this.storeIncident.karmaType, pointsWager);
                VariablesHelpers.SendPurchaseMessage($"Starting siege raid with {pointsWager} points wagered and {(int)parms.points} raid points purchased by {Viewer.username}");
                return;
            }
            TwitchWrapper.SendChatMessage($"Could not generate parameters for siege raid.");
        }

        public int pointsWager = 0;
        public IIncidentTarget target = null;
        public IncidentWorker worker = null;
        public IncidentParms parms = null;
        private bool separateChannel = false;

        public override ViewerState Viewer { get; set; }
    }

    public class MechanoidRaid : IncidentHelperVariables
    {
        public override bool IsPossible(MessageDetails message, ViewerState viewer, bool separateChannel = false)
        {
            this.separateChannel = separateChannel;
            this.Viewer = viewer;
            string[] command = message.Message.Split(' ');
            if (command.Length < 3)
            {
                message.Reply($"Syntax is {this.storeIncident.syntax}");
                return false;
            }

            if (!VariablesHelpers.PointsWagerIsValid(command[2], message, viewer, ref pointsWager, ref storeIncident, separateChannel))
                return false;

            target = Current.Game.AnyPlayerHomeMap;
            if (target == null)
                return false;

            parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatSmall, target);
            parms.points = IncidentHelper_PointsHelper.RollProportionalGamePoints(storeIncident, pointsWager, parms.points);
            parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeDrop;
            parms.faction = Find.FactionManager.OfMechanoids;

            worker = new Incidents.IncidentWorker_RaidEnemy
            {
                def = IncidentDefOf.RaidEnemy
            };

            return worker.CanFireNow(parms);
        }

        public override void TryExecute()
        {
            if (worker.TryExecute(parms))
            {
                Viewer.TakeViewerCoins(pointsWager);
                Viewer.CalculateNewKarma(this.storeIncident.karmaType, pointsWager);
                VariablesHelpers.SendPurchaseMessage($"Starting mechanoid raid with {pointsWager} points wagered and {(int)parms.points} raid points purchased by {Viewer.username}");
                return;
            }
            TwitchWrapper.SendChatMessage($"Could not generate parameters for mechanoid raid.");
        }

        public int pointsWager = 0;
        public IIncidentTarget target = null;
        public IncidentWorker worker = null;
        public IncidentParms parms = null;
        private bool separateChannel = false;

        public override ViewerState Viewer { get; set; }
    }

    public class Infestation : IncidentHelperVariables
    {
        public override bool IsPossible(MessageDetails message, ViewerState viewer, bool separateChannel = false)
        {
            this.separateChannel = separateChannel;
            this.Viewer = viewer;
            string[] command = message.Message.Split(' ');
            if (command.Length < 3)
            {
                message.Reply($"Syntax is {this.storeIncident.syntax}");
                return false;
            }

            if (!VariablesHelpers.PointsWagerIsValid(command[2], message, viewer, ref pointsWager, ref storeIncident, separateChannel))
                return false;

            target = Current.Game.AnyPlayerHomeMap;
            if (target == null)
                return false;

            parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatBig, target);
            parms.points = IncidentHelper_PointsHelper.RollProportionalGamePoints(storeIncident, pointsWager, StorytellerUtility.DefaultThreatPointsNow(target));
            parms.forced = true;

            worker = new IncidentWorker_Infestation
            {
                def = IncidentDef.Named("Infestation")
            };

            bool canFire = worker.CanFireNow(parms, true);

            if (!canFire)
            {
                message.Reply("No suitable location for infestation to occur.");
            }

            return canFire;
        }

        public override void TryExecute()
        {
            if (worker.TryExecute(parms))
            {
                Viewer.TakeViewerCoins(pointsWager);
                Viewer.CalculateNewKarma(this.storeIncident.karmaType, pointsWager);
                VariablesHelpers.SendPurchaseMessage($"Starting infestation raid with {pointsWager} points wagered and {(int)parms.points} raid points purchased by {Viewer.username}");
                return;
            }
            TwitchWrapper.SendChatMessage($"Could not generate parameters for infestation.");
        }

        public int pointsWager = 0;
        public IIncidentTarget target = null;
        public IncidentWorker worker = null;
        public IncidentParms parms = null;
        private bool separateChannel = false;

        public override ViewerState Viewer { get; set; }
    }

    public class ManhunterPack : IncidentHelperVariables
    {
        public override bool IsPossible(MessageDetails message, ViewerState viewer, bool separateChannel = false)
        {
            this.separateChannel = separateChannel;
            this.Viewer = viewer;
            string[] command = message.Message.Split(' ');
            if (command.Length < 3)
            {
                VariablesHelpers.ViewerDidWrongSyntax(message, storeIncident.syntax);
                return false;
            }

            if (!VariablesHelpers.PointsWagerIsValid(command[2], message, viewer, ref pointsWager, ref storeIncident, separateChannel))
                return false;

            target = Current.Game.AnyPlayerHomeMap;
            if (target == null)
                return false;

            parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatSmall, target);
            parms.points = IncidentHelper_PointsHelper.RollProportionalGamePoints(storeIncident, pointsWager, parms.points);

            worker = new IncidentWorker_ManhunterPack
            {
                def = IncidentDefOf.RaidEnemy
            };

            return worker.CanFireNow(parms);
        }

        public override void TryExecute()
        {
            if (worker.TryExecute(parms))
            {
                Viewer.TakeViewerCoins(pointsWager);
                Viewer.CalculateNewKarma(this.storeIncident.karmaType, pointsWager);
                VariablesHelpers.SendPurchaseMessage($"Starting manhunterpack with {pointsWager} points wagered and {(int)parms.points} raid points purchased by {Viewer.username}");
                return;
            }
            TwitchWrapper.SendChatMessage($"Could not generate parameters for manhunter pack.");
        }

        public int pointsWager = 0;
        public IIncidentTarget target = null;
        public IncidentWorker worker = null;
        public IncidentParms parms = null;
        private bool separateChannel = false;

        public override ViewerState Viewer { get; set; }
    }

    public class Predators : IncidentHelperVariables
    {
        public override bool IsPossible(MessageDetails message, ViewerState viewer, bool separateChannel = false)
        {
            this.separateChannel = separateChannel;
            this.Viewer = viewer;
            string[] command = message.Message.Split(' ');
            if (command.Length < 3)
            {
                VariablesHelpers.ViewerDidWrongSyntax(message, storeIncident.syntax);
                return false;
            }

            if (!VariablesHelpers.PointsWagerIsValid(command[2], message, viewer, ref pointsWager, ref storeIncident, separateChannel))
                return false;

            target = Current.Game.AnyPlayerHomeMap;
            if (target == null)
                return false;

            parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.ThreatSmall, target);
            parms.points = IncidentHelper_PointsHelper.RollProportionalGamePoints(storeIncident, pointsWager, parms.points);

            List<string> animals = new List<string>()
            { "Bear_Grizzly", "Bear_Polar", "Rhinoceros", "Elephant", "Megasloth", "Thrumbo" };

            animals.Shuffle();

            ThingDef def = ThingDef.Named(animals[0]);
            float averagePower = 0;
            if (def != null && def.race != null)
            {
                foreach (Tool t in def.tools)
                {
                    averagePower += t.power;
                }
                averagePower = averagePower / def.tools.Count;
            }

            float animalCount = 2.5f;
            if (averagePower > 18)
            {
                animalCount = 2.0f;
            }

            worker = new IncidentWorker_SpecificAnimalsWanderIn("TwitchStoriesLetterLabelPredators", PawnKindDef.Named(animals[0]), false, (int)animalCount, true, true);
            worker.def = IncidentDef.Named("HerdMigration");

            return worker.CanFireNow(parms);
        }

        public override void TryExecute()
        {
            if (worker.TryExecute(parms))
            {
                Viewer.TakeViewerCoins(pointsWager);
                Viewer.CalculateNewKarma(this.storeIncident.karmaType, pointsWager);
                VariablesHelpers.SendPurchaseMessage($"Starting predator pack with {pointsWager} points wagered and {(int)parms.points} raid points purchased by {Viewer.username}");
                return;
            }
            TwitchWrapper.SendChatMessage($"Could not generate parameters for manhunter pack.");
        }

        public int pointsWager = 0;
        public IIncidentTarget target = null;
        public IncidentWorker worker = null;
        public IncidentParms parms = null;
        private bool separateChannel = false;

        public override ViewerState Viewer { get; set; }
    }
}
