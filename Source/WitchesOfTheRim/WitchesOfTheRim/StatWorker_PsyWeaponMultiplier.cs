using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace nuff.witches.arsenal
{
    class StatWorker_PsyWeaponMultiplier : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req)
        {
            return req.HasThing && req.Thing.def?.GetModExtension<PsyWeaponExtension>() != null;
        }

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            Thing thing = req.Thing;
            var extension = thing?.def?.GetModExtension<PsyWeaponExtension>();
            if (thing == null || extension == null)
                return 1f;

            Pawn pawn = (thing.ParentHolder as Pawn_EquipmentTracker)?.pawn;
            if (pawn == null)
                return extension.damageScaling;

            float sensitivity = pawn.GetStatValue(StatDefOf.PsychicSensitivity);
            return Arsenal_Utils.GetPsyScaledMultiplier(sensitivity, extension.damageScaling, extension.canScaleDown);
        }

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            Thing thing = req.Thing;
            var extension = thing?.def?.GetModExtension<PsyWeaponExtension>();
            Pawn pawn = (thing?.ParentHolder as Pawn_EquipmentTracker)?.pawn;

            if (thing == null || extension == null || pawn == null)
                return "";

            float sensitivity = pawn.GetStatValue(StatDefOf.PsychicSensitivity);
            float scale = Arsenal_Utils.GetPsyScaledMultiplier(sensitivity, extension.damageScaling, extension.canScaleDown);

            return $"Psychic sensitivity: {sensitivity.ToString("0.##")}\n" +
                   $"Scaling factor: {extension.damageScaling.ToString("0.##")}\n" +
                   $"Final multiplier: x{scale.ToStringPercent()}";
        }
    }
}
