using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using HarmonyLib;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace TurretSmart
{
    [HarmonyPatch]
    public static class HarmonyPatches
    {
        private static readonly FieldInfo f_Building_TurretGun_verb = AccessTools.Field(typeof(Building_TurretGun), "verb");
        private static readonly FieldInfo f_Projectile_launcher = AccessTools.Field(typeof(Projectile), "launcher");

        [HarmonyPatch(typeof(ShotReport), "HitFactorFromShooter")]
        [HarmonyPostfix]
        public static void ShotReport_HitFactorFromShooter_Postfix(Verb verb, ref float __result)
        {
            if (!TurretSmartMod.settings.enhancedAccuracy)
                return;

            if (verb?.caster is Building_TurretGun turret)
            {
                if (turret.Faction == Faction.OfPlayer)
                {
                    __result *= TurretSmartMod.settings.accuracyMultiplier;
                }
            }
        }

        [HarmonyPatch(typeof(Thing), "TakeDamage")]
        [HarmonyPrefix]
        public static bool TakeDamage_Prefix(Thing __instance, DamageInfo dinfo)
        {
            if (dinfo.Instigator == null)
                return true;

            if (__instance is Pawn pawn && TurretSmartMod.settings.preventFriendlyFire)
            {
                if (IsPlayerTurretProjectileHitFriendly(dinfo, pawn))
                {
                    dinfo.SetAmount(0f);
                    return true;
                }
            }

            if (!IsPlayerTurretProjectile(dinfo.Instigator))
                return true;

            if (!IsWall(__instance))
                return true;

            if (TurretSmartMod.settings.wallDamageMultiplier >= 1f)
                return true;

            float reducedDamage = dinfo.Amount * TurretSmartMod.settings.wallDamageMultiplier;
            dinfo.SetAmount(reducedDamage);
            return true;
        }

        [HarmonyPatch(typeof(CompExplosive), "CheckState")]
        [HarmonyPrefix]
        public static bool CompExplosive_CheckState_Prefix(CompExplosive __instance)
        {
            if (!TurretSmartMod.settings.preventSelfDestruct)
                return true;

            if (__instance.parent is Building_TurretGun turret)
            {
                if (turret.Faction == Faction.OfPlayer || turret.Faction == null)
                {
                    return false;
                }
            }

            return true;
        }

        [HarmonyPatch(typeof(Pawn), "TakeDamage")]
        [HarmonyPrefix]
        public static void Pawn_TakeDamage_Prefix(Pawn __instance, DamageInfo dinfo)
        {
            if (TurretSmartMod.settings.targetBodyPart == "None")
                return;

            if (dinfo.Instigator == null)
                return;

            if (!IsPlayerTurretProjectile(dinfo.Instigator))
                return;

            Building_TurretGun turret = dinfo.Instigator as Building_TurretGun;
            if (turret != null)
            {
                if (turret.Faction != Faction.OfPlayer && turret.Faction != null)
                    return;
            }

            Pawn pawn = __instance;
            if (pawn == null || pawn.Dead)
                return;

            BodyPartRecord targetPart = FindTargetBodyPart(pawn, TurretSmartMod.settings.targetBodyPart);
            if (targetPart != null)
            {
                dinfo.SetHitPart(targetPart);
            }
        }

        private static bool IsPlayerTurretProjectile(Thing instigator)
        {
            if (instigator is Building_TurretGun turretGun)
            {
                return turretGun.Faction == Faction.OfPlayer;
            }

            if (instigator is Projectile projectile)
            {
                Thing launcher = f_Projectile_launcher?.GetValue(projectile) as Thing;
                Building_TurretGun turretFromLauncher = launcher as Building_TurretGun;
                if (turretFromLauncher != null)
                {
                    return turretFromLauncher.Faction == Faction.OfPlayer;
                }
            }

            return false;
        }

        private static bool IsPlayerTurretProjectileHitFriendly(DamageInfo dinfo, Pawn targetPawn)
        {
            if (dinfo.Instigator is Projectile projectile)
            {
                Thing launcher = f_Projectile_launcher?.GetValue(projectile) as Thing;
                Building_TurretGun turret = launcher as Building_TurretGun;
                if (turret != null && turret.Faction == Faction.OfPlayer)
                {
                    return IsFriendlyPawn(targetPawn, turret);
                }
            }

            return false;
        }

        private static BodyPartRecord FindTargetBodyPart(Pawn pawn, string targetPart)
        {
            if (pawn?.RaceProps?.body == null)
                return null;

            BodyDef body = pawn.RaceProps.body;

            switch (targetPart)
            {
                case "Head":
                    return body.AllParts.FirstOrDefault(p => p.def == BodyPartDefOf.Head);

                case "Torso":
                    return body.AllParts.FirstOrDefault(p => p.def == BodyPartDefOf.Torso);

                case "Arms":
                    return body.AllParts.FirstOrDefault(p => p.def == BodyPartDefOf.Arm);

                case "Legs":
                    return body.AllParts.FirstOrDefault(p => p.def == BodyPartDefOf.Leg);

                case "Heart":
                    BodyPartRecord heart = body.AllParts.FirstOrDefault(p => p.def == BodyPartDefOf.Heart);
                    if (heart != null)
                        return heart;
                    Hediff heartHediff = pawn.health.hediffSet.hediffs.FirstOrDefault(h => h.def.defName.ToLower().Contains("heart"));
                    if (heartHediff != null)
                        return heartHediff.Part;
                    return body.AllParts.FirstOrDefault(p => p.def == BodyPartDefOf.Torso);

                case "Brain":
                    BodyPartDef brainDef = DefDatabase<BodyPartDef>.GetNamed("Brain");
                    return body.AllParts.FirstOrDefault(p => p.def == brainDef);

                default:
                    return null;
            }
        }

        private static bool IsWall(Thing thing)
        {
            if (thing is Building building)
            {
                if (building.def.category == ThingCategory.Building)
                {
                    if (building.def.building?.isNaturalRock == true)
                        return true;
                    if (building.def.IsWall)
                        return true;
                }
            }
            return false;
        }

        private static bool IsFriendlyPawn(Pawn target, Building_TurretGun turret)
        {
            if (target == null || turret == null)
                return false;

            if (turret.Faction == null)
                return false;

            if (target.Faction == null)
                return false;

            if (target.Faction == turret.Faction)
                return true;

            FactionRelation relation = turret.Faction.RelationWith(target.Faction);
            return relation.kind == FactionRelationKind.Ally || relation.kind == FactionRelationKind.Neutral;
        }
    }
}