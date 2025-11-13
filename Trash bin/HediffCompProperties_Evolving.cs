using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace nuff.tsoa.arsenal
{
    class HediffCompProperties_Evolving : HediffCompProperties
    {
        public StatDef evolvingStat;

        public float lowerThreshold = 0f;
        public float upperThreshold = 999f;

        public HediffDef hediffAbove = null;
        public HediffDef hediffBelow = null;

        public HediffCompProperties_Evolving()
        {
            this.compClass = typeof(HediffComp_Evolving);
        }

    }
}
