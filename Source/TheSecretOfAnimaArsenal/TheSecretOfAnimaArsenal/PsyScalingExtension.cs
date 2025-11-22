using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace nuff.tsoa.arsenal
{
    public class PsyScalingExtension : DefModExtension
    {
        public float scalingMultiplier = 1f;

        public bool canScaleDown = false;

        public float entropyCost = 0f;
    }
}
