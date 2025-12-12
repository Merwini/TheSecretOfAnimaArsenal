using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace nuff.tsoa.core
{
    class StatWorker_PsyScalingDisplay : StatWorker
    {
        public override bool ShouldShowFor(StatRequest req)
        {
            return req.HasThing && req.Thing.def.statBases.StatListContains(ArsenalDefOf.TSOA_PsyScaling);
        }

        public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
        {
            return req.Thing.def.statBases.GetStatValueFromList(ArsenalDefOf.TSOA_PsyScaling, 0);
        }

        public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
        {
            float scale = req.Thing.def.statBases.GetStatValueFromList(ArsenalDefOf.TSOA_PsyScaling, 0);

            return "TSOA_PsyScalingWorkerExplanation".Translate(scale);
        }
    }

    //class StatWorker_PsyDamageMultiplier : StatWorker_PsyScalingDisplay
    //{
    //    public override bool ShouldShowFor(StatRequest req)
    //    {
    //        if (!req.HasThing)
    //            return false;

    //        ThingDef def = req.Thing.def;
    //        return def.IsWeapon && def.GetModExtension<PsyScalingExtension>() != null;
    //    }
    //}

    //class StatWorker_PsyArmorMultiplier : StatWorker_PsyScalingDisplay
    //{
    //    public override bool ShouldShowFor(StatRequest req)
    //    {
    //        if (!req.HasThing)
    //            return false;

    //        ThingDef def = req.Thing.def;
    //        return def.IsApparel && def.GetModExtension<PsyScalingExtension>() != null;
    //    }
    //}
}
