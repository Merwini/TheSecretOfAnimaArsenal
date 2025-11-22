using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace nuff.tsoa.arsenal
{
    public class PsyExtraDamageExtension : DefModExtension
    {
        public float heatConsumedPercent = 0.1f;

        public float damagePerHeatConsumed = 4f;

        public DamageDef damageDef;
    }
}
