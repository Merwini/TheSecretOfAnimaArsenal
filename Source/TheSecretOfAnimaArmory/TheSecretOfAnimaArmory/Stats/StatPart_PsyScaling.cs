using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace nuff.tsoa.armory
{
    class StatPart_PsyScaling : StatPart
    {
        public override void TransformValue(StatRequest req, ref float val)
        {
            Thing thing = req.Thing;
            if (thing == null)
                return;

            var extension = thing.def?.GetModExtension<PsyScalingExtension>();
            if (extension == null)
                return;

            Pawn pawn = null;
            if (thing.ParentHolder is Pawn_EquipmentTracker eq)
                pawn = eq.pawn;
            else if (thing.ParentHolder is Pawn_ApparelTracker ap)
                pawn = ap.pawn; 
            if (pawn == null)
                return;

            float sensitivity = pawn.GetStatValue(StatDefOf.PsychicSensitivity);

            val = Armory_Utils.GetPsyScaledValue(val, sensitivity, extension.scalingMultiplier, extension.canScaleDown);
        }

        public override string ExplanationPart(StatRequest req)
        {
            Thing thing = req.Thing;
            PsyScalingExtension extension = thing?.def?.GetModExtension<PsyScalingExtension>();
            Pawn pawn = (thing?.ParentHolder as Pawn_EquipmentTracker)?.pawn;

            if (thing == null || extension == null || pawn == null)
                return null;

            float sensitivity = pawn.GetStatValue(StatDefOf.PsychicSensitivity);
            float multiplier = Armory_Utils.GetPsyScaledMultiplier(sensitivity, extension.scalingMultiplier, extension.canScaleDown);

            return "Psychic Sensitivity Scaling: x" + multiplier.ToStringPercent();
        }
    }
}
