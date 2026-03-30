using Verse;

namespace TurretSmart
{
    public class TurretSmartSettings : ModSettings
    {
        public bool enhancedAccuracy = true;
        public float accuracyMultiplier = 3f;
        public bool preventFriendlyFire = true;
        public float wallDamageMultiplier = 0.1f;
        public bool preventSelfDestruct = true;
        public string targetBodyPart = "None";

        public override void ExposeData()
        {
            Scribe_Values.Look(ref enhancedAccuracy, "enhancedAccuracy", true);
            Scribe_Values.Look(ref accuracyMultiplier, "accuracyMultiplier", 3f);
            Scribe_Values.Look(ref preventFriendlyFire, "preventFriendlyFire", true);
            Scribe_Values.Look(ref wallDamageMultiplier, "wallDamageMultiplier", 0.1f);
            Scribe_Values.Look(ref preventSelfDestruct, "preventSelfDestruct", true);
            Scribe_Values.Look(ref targetBodyPart, "targetBodyPart", "None");
            base.ExposeData();
        }
    }
}