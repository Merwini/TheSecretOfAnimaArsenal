//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Verse;
//using RimWorld;
//using CombatExtended;
//using HarmonyLib;
//using System.Reflection;
//using System.Reflection.Emit;

//namespace nuff.witches.arsenal
//{
//    [StaticConstructorOnStartup]
//    public class CE_HarmonyPatches
//    {
//        static CE_HarmonyPatches()
//        {
//            Harmony harmony = new Harmony("nuff.witches.arsenal.ce");

//            var targetMethod = AccessTools.Method(typeof(Verb_MeleeAttackCE), "DamageInfosToApply");
//            var meleePostfix = AccessTools.Method(typeof(VanillaHarmonyPatches), "MeleeDamagePostfix");
//            harmony.Patch(targetMethod, postfix: new HarmonyMethod(meleePostfix));

//            harmony.PatchAll();
//        }


//        [HarmonyPatch(typeof(StatWorker_MeleeDamage), "GetFinalDisplayValue")]
//        public static class StatWorker_MeleeDamage_GetFinalDisplayValue_Patch
//        {
//            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
//            {
//                var codes = instructions.ToList();
//                var getAdjustedDamage = AccessTools.Method(typeof(StatWorker_MeleeDamage), "GetAdjustedDamage");
//                var helperMethod = AccessTools.Method(typeof(CE_HarmonyPatches), nameof(CE_HarmonyPatches.StatWorker_MeleeDamage_GetFinalDisplayValue_Patch_Helper));

//                int endFinallyIndex = -1;

//                for (int i = 0; i < codes.Count; i++)
//                {
//                    if (codes[i].opcode == OpCodes.Endfinally)
//                    {
//                        endFinallyIndex = i;
//                    }
//                }

//                if (endFinallyIndex > 0)
//                {
//                    List<CodeInstruction> newCodes = new List<CodeInstruction>();

//                    newCodes.Add(new CodeInstruction(OpCodes.Ldloc_S, 4)); // load min damage
//                    newCodes.Add(new CodeInstruction(OpCodes.Ldarg_1)); // load optionalReq
//                    newCodes.Add(new CodeInstruction(OpCodes.Call, helperMethod)); // consume the damage and Thing to return a new damage
//                    newCodes.Add(new CodeInstruction(OpCodes.Stloc_S, 4)); // store the new damage

//                    newCodes.Add(new CodeInstruction(OpCodes.Ldloc_S, 5)); // load max damage
//                    newCodes.Add(new CodeInstruction(OpCodes.Ldarg_1)); // load optionalReq
//                    newCodes.Add(new CodeInstruction(OpCodes.Call, helperMethod)); // consume the damage and Thing to return a new damage
//                    newCodes.Add(new CodeInstruction(OpCodes.Stloc_S, 5)); // store the new damage

//                    codes.InsertRange(endFinallyIndex, newCodes);
//                }
//                else
//                {
//                    Log.Error("nuff.witches.arsenal.CE_HarmonyPatches failed to find index");
//                }

//                return codes.AsEnumerable();
//            }
//        }
//        static float StatWorker_MeleeDamage_GetFinalDisplayValue_Patch_Helper(float baseDamage, StatRequest req)
//        {
//            var weapon = req.Thing;
//            if (weapon == null)
//                return baseDamage;

//            var extension = weapon?.def?.GetModExtension<PsyWeaponExtension>();
//            if (extension == null)
//                return baseDamage;

//            var pawn = (weapon.ParentHolder as Pawn_EquipmentTracker)?.pawn;
//            if (pawn == null)
//                return baseDamage;

//            float sensitivity = pawn.GetStatValue(StatDefOf.PsychicSensitivity);
//            float damageScalingFactor = (sensitivity - 1) * extension.damageScaling;

//            if (!extension.canScaleDown && damageScalingFactor < 0f)
//                damageScalingFactor = 0f;

//            return baseDamage + baseDamage * damageScalingFactor;
//        }
//    }
//}
