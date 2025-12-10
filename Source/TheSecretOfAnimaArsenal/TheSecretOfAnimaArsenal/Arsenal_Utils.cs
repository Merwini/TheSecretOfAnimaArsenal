using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;

namespace nuff.tsoa.arsenal
{
    class Arsenal_Utils
    {
        //was used in previous implementation
        //public static PsyScalingExtension GetPsyExtension(Verb_MeleeAttack verb)
        //{
        //    return verb.EquipmentSource?.def.GetModExtension<PsyScalingExtension>()
        //        ?? verb.HediffSource?.def.GetModExtension<PsyScalingExtension>();
        //}

        //public static float GetPsyScaledValue(float baseValue, float sensitivity, float scaling)
        //{
        //    float multiplier = GetPsyScaledMultiplier(sensitivity, scaling);

        //    return baseValue * multiplier;
        //}

        //public static float GetPsyScaledMultiplier(float sensitivity, float scaling)
        //{
        //    float multiplier = (float)Math.Pow(sensitivity, scaling);
        //    if (!canScaleDown && multiplier < 1f)
        //        multiplier = 1f;

        //    return multiplier;
        //}

        public static float GetPsyScalingFactor(float psyScaling, float sensitivity)
        {
            float val = psyScaling * (sensitivity - 1);
            Log.Warning($"GetPsyScalingFactor value: {val}");
            //return psyScaling * (1 - sensitivity);
            return val;
        }

        public static float GetPsyScaledValue(float initialVal, float psyScaling, float sensitivity)
        {
            float val = initialVal * (1 + GetPsyScalingFactor(psyScaling, sensitivity));
            Log.Warning($"GetPsyScaledValue: {val}");
            return val;
            //return initialVal * GetPsyScalingFactor(psyScaling, sensitivity);
        }
    }
}
