using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace nuff.witches.arsenal
{
    class StatWorker_PsyMultiplier : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req)
        {
            return req.HasThing && req.Thing.def?.GetModExtension<PsyScalingExtension>() != null;
        }

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            Thing thing = req.Thing;
            var extension = thing?.def?.GetModExtension<PsyScalingExtension>();
            if (thing == null || extension == null)
                return 1f;

            Pawn pawn = null;
            if (thing.ParentHolder is Pawn_EquipmentTracker eq)
                pawn = eq.pawn;
            else if (thing.ParentHolder is Pawn_ApparelTracker ap)
                pawn = ap.pawn;
            if (pawn == null)
                return extension.scalingMultiplier;

            float sensitivity = pawn.GetStatValue(StatDefOf.PsychicSensitivity);
            return Arsenal_Utils.GetPsyScaledMultiplier(sensitivity, extension.scalingMultiplier, extension.canScaleDown);
        }

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            Thing thing = req.Thing;
            var extension = thing?.def?.GetModExtension<PsyScalingExtension>();
            Pawn pawn = null;
            if (thing.ParentHolder is Pawn_EquipmentTracker eq)
                pawn = eq.pawn;
            else if (thing.ParentHolder is Pawn_ApparelTracker ap)
                pawn = ap.pawn;
            if (thing == null || extension == null || pawn == null)
                return "";

            float sensitivity = pawn.GetStatValue(StatDefOf.PsychicSensitivity);
            float scale = Arsenal_Utils.GetPsyScaledMultiplier(sensitivity, extension.scalingMultiplier, extension.canScaleDown);

            return $"Psychic sensitivity: {sensitivity.ToString("0.##")}\n" +
                   $"Equipment Scaling factor: {extension.scalingMultiplier.ToString("0.##")}";
        }
    }

    class StatWorker_PsyDamageMultiplier : StatWorker_PsyMultiplier
    {
        public override bool ShouldShowFor(StatRequest req)
        {
            if (!req.HasThing)
                return false;

            ThingDef def = req.Thing.def;
            return def.IsWeapon && def.GetModExtension<PsyScalingExtension>() != null;
        }
    }

    class StatWorker_PsyArmorMultiplier : StatWorker_PsyMultiplier
    {
        public override bool ShouldShowFor(StatRequest req)
        {
            if (!req.HasThing)
                return false;

            ThingDef def = req.Thing.def;
            return def.IsApparel && def.GetModExtension<PsyScalingExtension>() != null;
        }
    }
}
