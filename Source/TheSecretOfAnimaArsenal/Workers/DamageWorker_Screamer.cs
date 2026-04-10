using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace tsoa.arsenal;

public class DamageWorker_Screamer : DamageWorker
{
    public override DamageResult Apply(DamageInfo dinfo, Thing victim)
    {
        DamageInfo newDinfo = new DamageInfo(dinfo);
        newDinfo.SetAmount(0);
        newDinfo.SetIgnoreArmor(true);

        if (victim is not Pawn pawn)
        {
            return base.Apply(newDinfo, victim);
        }

        if (pawn.RaceProps.Humanlike)
        {
            pawn.needs.mood.thoughts.memories.TryGainMemory(TSOAA_DefOf.TSOA_AnimaArrowScream);
        }
        else if (pawn.RaceProps.Animal && pawn.Faction == null)
        {
            pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter);
        }

        return base.Apply(newDinfo, victim);
    }
}
