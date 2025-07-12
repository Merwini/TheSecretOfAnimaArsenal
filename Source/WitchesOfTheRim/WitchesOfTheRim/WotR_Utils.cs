using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace nuff.witches
{
    class WotR_Utils
    {
        public static PsyWeaponExtension GetPsyExtension(Verb_MeleeAttack verb)
        {
            return verb.EquipmentSource?.def.GetModExtension<PsyWeaponExtension>()
                ?? verb.HediffSource?.def.GetModExtension<PsyWeaponExtension>();
        }

    }
}
