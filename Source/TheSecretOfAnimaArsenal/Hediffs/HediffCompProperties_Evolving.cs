using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace tsoa.arsenal;

public class HediffCompProperties_Evolving : HediffCompProperties
{
    public EvolvingImplantDef evolvingImplantDef;

    public HediffCompProperties_Evolving()
    {
        this.compClass = typeof(HediffComp_Evolving);
    }
}
