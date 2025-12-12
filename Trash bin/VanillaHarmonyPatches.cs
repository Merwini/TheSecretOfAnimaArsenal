//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Verse;
//using RimWorld;
//using UnityEngine;
//using HarmonyLib;

//namespace nuff.tsoa.core
//{
//    [StaticConstructorOnStartup]
//    public class VanillaHarmonyPatches
//    {
//        static VanillaHarmonyPatches()
//        {
//            Harmony harmony = new Harmony("nuff.tsoa.core");

//            // Psy-enhanced melee attacks
//            var targetMethod = AccessTools.Method(typeof(Verb_MeleeAttackDamage), "DamageInfosToApply");
//            var meleePostfix = AccessTools.Method(typeof(VanillaHarmonyPatches), "MeleeDamagePostfix");
//            harmony.Patch(targetMethod, postfix: new HarmonyMethod(meleePostfix));

//            harmony.PatchAll();
//        }

//        public static void MeleeDamagePostfix(Verb_MeleeAttackDamage __instance, ref IEnumerable<DamageInfo> __result)
//        {
//            PsyWeaponExtension extension = WotR_Utils.GetPsyExtension(__instance);
//            if (extension == null)
//                return;

//            if (!(__instance.CasterPawn is Pawn attacker))
//                return;

//            if (extension.entropyCost > 0 && !attacker.HasPsylink)
//            {
//                MoteMaker.ThrowText(attacker.DrawPos, attacker.Map, "WOTR_NotACaster".Translate(), 2);
//                return;
//            }

//            var dinfoList = __result.ToList();

//            float sensitivity = attacker.GetStatValue(StatDefOf.PsychicSensitivity);
//            float damageScalingFactor = (sensitivity - 1) * extension.damageScaling;
//            float penetrationScalingFactor = (sensitivity - 1) * extension.penetrationScaling;

//            if (!extension.canScaleDown && damageScalingFactor < 0f)
//                damageScalingFactor = 0f;

//            if (!extension.canScaleDown && penetrationScalingFactor < 0f)
//                penetrationScalingFactor = 0f;

//            for (int i = 0; i < dinfoList.Count; i++)
//            {
//                var dinfo = dinfoList[i];
//                dinfo.SetAmount(dinfo.Amount + dinfo.Amount * damageScalingFactor);
//                dinfo.armorPenetrationInt = (dinfo.armorPenetrationInt + dinfo.armorPenetrationInt * penetrationScalingFactor);

//                dinfoList[i] = dinfo;
//            }

//            attacker.psychicEntropy.TryAddEntropy(extension.entropyCost);

//            __result = dinfoList;
//        }

//        [HarmonyPatch(typeof(StatWorker_MeleeAverageDPS), nameof(StatWorker_MeleeAverageDPS.GetValueUnfinalized))]
//        public static class StatWorker_MeleeAverageDPS_GetValueUnfinalized_Postfix
//        {
//            public static void Postfix(StatRequest req, bool applyPostProcess, ref float __result)
//            {
//                if (!req.HasThing || !(req.Thing?.ParentHolder is Pawn holder))
//                    return;

//                if (!(req.Thing is ThingWithComps weapon) || weapon.def == null)
//                    return;

//                PsyWeaponExtension extension = weapon.def.GetModExtension<PsyWeaponExtension>();
//                if (extension == null)
//                    return;

//                float sensitivity = holder.GetStatValue(StatDefOf.PsychicSensitivity);
//                float damageScalingFactor = (sensitivity - 1) * extension.damageScaling;

//                if (!extension.canScaleDown && damageScalingFactor < 0f)
//                    damageScalingFactor = 0f;

//                __result += __result * damageScalingFactor;
//            }
//        }
//    }
//}
