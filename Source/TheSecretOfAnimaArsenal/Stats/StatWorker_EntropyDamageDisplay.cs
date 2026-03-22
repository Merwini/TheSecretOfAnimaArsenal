using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace tsoa.arsenal;

public class StatWorker_EntropyDamageDisplay : StatWorker
{
    public override bool ShouldShowFor(StatRequest req)
    {
        if (req.HasThing)
            return req.Thing.def.statBases.StatListContains(TSOAA_DefOf.TSOA_EntropyDamage);

        if (req.Def is ThingDef thingDef)
            return thingDef.statBases != null && thingDef.statBases.StatListContains(TSOAA_DefOf.TSOA_EntropyDamage);

        return false;
    }

    public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
    {
        if (req.HasThing)
            return req.Thing.def.statBases.GetStatValueFromList(TSOAA_DefOf.TSOA_EntropyDamage, 0f);

        if (req.Def is ThingDef thingDef)
            return thingDef.statBases?.GetStatValueFromList(TSOAA_DefOf.TSOA_EntropyDamage, 0f) ?? 0f;

        return 0f;
    }

    public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
    {
        ThingDef def;
        if (req.HasThing)
            def = req.Thing.def;
        else
            def = req.Def as ThingDef;

        float percent = def.statBases.GetStatValueFromList(TSOAA_DefOf.TSOA_EntropyDamage, 0);
        float damage = def.tools
            .Where(t => !t.extraMeleeDamages.NullOrEmpty())
            .SelectMany(t => t.extraMeleeDamages)
            .FirstOrDefault(d => d.def == TSOAA_DefOf.TSOA_EntropyExtraDamage)
            ?.amount ?? 0f;

        return "TSOA_EntropyDamageWorkerExplanation".Translate(percent.ToStringPercent(), damage);
    }

    public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
    {
        return null;
    }
}
