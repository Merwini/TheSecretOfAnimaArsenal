using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace nuff.witches
{
    class PsyWeaponExtension : DefModExtension
    {
        public float damageScaling = 1f;

        public float penetrationScaling = 0f;

        public bool canScaleDown = false;

        public float entropyCost = 0f;
    }
}
