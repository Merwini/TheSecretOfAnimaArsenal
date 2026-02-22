using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace tsoa.arsenal;

class StatWorker_PsyScalingDisplay : StatWorker
{
    public override bool ShouldShowFor(StatRequest req)
    {
        return req.HasThing && req.Thing.def.statBases.StatListContains(TSOAA_DefOf.TSOA_PsyScaling);
    }

    public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
    {
        return req.Thing.def.statBases.GetStatValueFromList(TSOAA_DefOf.TSOA_PsyScaling, 0);
    }

    public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
    {
        float scale = req.Thing.def.statBases.GetStatValueFromList(TSOAA_DefOf.TSOA_PsyScaling, 0);

        return "TSOA_PsyScalingWorkerExplanation".Translate(scale);
    }
}
