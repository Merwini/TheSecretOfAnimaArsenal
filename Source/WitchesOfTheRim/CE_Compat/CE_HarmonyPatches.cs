using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using CombatExtended;
using HarmonyLib;
using rep.witches;
using System.Reflection;

namespace nuff.witches
{
    [StaticConstructorOnStartup]
    public class CE_HarmonyPatches
    {
        static CE_HarmonyPatches()
        {
            Harmony harmony = new Harmony("nuff.witches.ce");

            var targetMethod = AccessTools.Method(typeof(Verb_MeleeAttackCE), "DamageInfosToApply");
            var meleePostfix = AccessTools.Method(typeof(VanillaHarmonyPatches), "MeleeDamagePostfix");

            harmony.Patch(targetMethod, postfix: new HarmonyMethod(meleePostfix));
        }

    }
}
