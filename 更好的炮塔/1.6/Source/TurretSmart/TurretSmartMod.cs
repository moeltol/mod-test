using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace TurretSmart
{
    public class TurretSmartMod : Mod
    {
        public static TurretSmartSettings settings;

        private static readonly string[] bodyPartOptions = new string[]
        {
            "None",
            "Head",
            "Torso",
            "Arms",
            "Legs",
            "Heart",
            "Brain"
        };

        public TurretSmartMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<TurretSmartSettings>();

            Harmony harmony = new Harmony("TurretSmart");
            harmony.PatchAll();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard list = new Listing_Standard();
            list.Begin(inRect);

            list.Label("TurretSmartSettings".Translate());
            list.Gap();

            list.CheckboxLabeled("TurretSmart_EnhancedAccuracy".Translate(), ref TurretSmartMod.settings.enhancedAccuracy);

            if (TurretSmartMod.settings.enhancedAccuracy)
            {
                list.Gap();
                list.Label("TurretSmart_AccuracyMultiplier".Translate(TurretSmartMod.settings.accuracyMultiplier.ToString("F1")));
                TurretSmartMod.settings.accuracyMultiplier = list.Slider(TurretSmartMod.settings.accuracyMultiplier, 1f, 10f);
            }

            list.Gap();

            list.CheckboxLabeled("TurretSmart_PreventFriendlyFire".Translate(), ref TurretSmartMod.settings.preventFriendlyFire);

            list.Gap();

            list.Label("TurretSmart_WallDamageMultiplier".Translate((TurretSmartMod.settings.wallDamageMultiplier * 100).ToString("F0")));
            TurretSmartMod.settings.wallDamageMultiplier = list.Slider(TurretSmartMod.settings.wallDamageMultiplier, 0f, 1f);

            list.Gap();

            list.CheckboxLabeled("TurretSmart_PreventSelfDestruct".Translate(), ref TurretSmartMod.settings.preventSelfDestruct);

            list.Gap();

            list.Label("TurretSmart_TargetBodyPart".Translate(TurretSmartMod.GetBodyPartLabel(TurretSmartMod.settings.targetBodyPart)));
            if (list.ButtonText(TurretSmartMod.GetBodyPartLabel(TurretSmartMod.settings.targetBodyPart)))
            {
                List<FloatMenuOption> options = new List<FloatMenuOption>();
                foreach (string part in bodyPartOptions)
                {
                    string partName = part;
                    options.Add(new FloatMenuOption(TurretSmartMod.GetBodyPartLabel(partName), () =>
                    {
                        TurretSmartMod.settings.targetBodyPart = partName;
                    }));
                }
                Find.WindowStack.Add(new FloatMenu(options));
            }

            list.End();
            base.DoSettingsWindowContents(inRect);
        }

        public static string GetBodyPartLabel(string part)
        {
            switch (part)
            {
                case "None": return "TurretSmart_BodyPart_None".Translate();
                case "Head": return "TurretSmart_BodyPart_Head".Translate();
                case "Torso": return "TurretSmart_BodyPart_Torso".Translate();
                case "Arms": return "TurretSmart_BodyPart_Arms".Translate();
                case "Legs": return "TurretSmart_BodyPart_Legs".Translate();
                case "Heart": return "TurretSmart_BodyPart_Heart".Translate();
                case "Brain": return "TurretSmart_BodyPart_Brain".Translate();
                default: return part;
            }
        }

        public override string SettingsCategory()
        {
            return "TurretSmart".Translate();
        }
    }
}