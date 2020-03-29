using RimWorld;
using System.Collections.Generic;
using Verse;

namespace TwitchToolkit
{

    public class StorytellerCompProperties_CustomRandomStoryTeller : StorytellerCompProperties
    {

        public StorytellerCompProperties_CustomRandomStoryTeller() => compClass = typeof(StorytellerComp_CustomRandomStoryTeller);

        public float mtbDays;

        public List<IncidentCategoryEntry> categoryWeights = new List<IncidentCategoryEntry>();

        public float maxThreatBigIntervalDays = 99999f;

        public FloatRange randomPointsFactorRange = new FloatRange(0.5f, 1.5f);

        public bool skipThreatBigIfRaidBeacon;
    }
}
