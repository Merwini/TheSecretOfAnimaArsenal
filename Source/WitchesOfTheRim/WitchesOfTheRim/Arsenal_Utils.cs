using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace nuff.witches.arsenal
{
    class Arsenal_Utils
    {
        public static PsyScalingExtension GetPsyExtension(Verb_MeleeAttack verb)
        {
            return verb.EquipmentSource?.def.GetModExtension<PsyScalingExtension>()
                ?? verb.HediffSource?.def.GetModExtension<PsyScalingExtension>();
        }

        public static float GetPsyScaledValue(float baseValue, float sensitivity, float scaling, bool canScaleDown)
        {
            float multiplier = GetPsyScaledMultiplier(sensitivity, scaling, canScaleDown);

            return baseValue * multiplier;
        }

        public static float GetPsyScaledMultiplier(float sensitivity, float scaling, bool canScaleDown)
        {
            float multiplier = (float)Math.Pow(sensitivity, scaling);
            if (!canScaleDown && multiplier < 1f)
                multiplier = 1f;

            return multiplier;
        }
    }
}
