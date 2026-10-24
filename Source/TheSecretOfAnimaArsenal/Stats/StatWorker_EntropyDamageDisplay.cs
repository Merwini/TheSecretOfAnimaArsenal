using System;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace tsoa.arsenal;

public class StatWorker_EntropyDamageDisplay : StatWorker
{
    public override bool ShouldShowFor(StatRequest req)
    {
        ThingDef def = null;
        if (req.HasThing)
            def = req.Thing.def;
        else if (req.Def is ThingDef)
            def = req.Def as ThingDef;

        if (def == null) return false;

        EntropyDamageExtension extension = def.GetModExtension<EntropyDamageExtension>();

        return extension != null;
    }

    public override float GetValueUnfinalized(StatRequest req, bool applyPostProcess = true)
    {
        ThingDef def = null;
        if (req.HasThing)
            def = req.Thing.def;
        else if (req.Def is ThingDef)
            def = req.Def as ThingDef;

        if (def == null) return 0f;

        if (def.GetModExtension<EntropyDamageExtension>() is EntropyDamageExtension extension)
                return extension.entropyCounted;

        return 0f;
    }

    public override string GetExplanationUnfinalized(StatRequest req, ToStringNumberSense numberSense)
    {
        ThingDef def = null;
        if (req.HasThing)
            def = req.Thing.def;
        else if (req.Def is ThingDef)
            def = req.Def as ThingDef;

        if (def == null) return "[TSOA] something has gone wrong, this should not be visible.";

        EntropyDamageExtension extension = def.GetModExtension<EntropyDamageExtension>();
        if (extension == null)
        {
            Log.Error($"[TSOA] {def.defName} has stat TSOA_EntropyDamage but no EntropyDamageExtension"); // maybe ErrorOnce?
            extension = new();
        }

        return "TSOA_EntropyDamageWorkerExplanation".Translate(extension.entropyCounted.ToStringPercent(), extension.entropySpent.ToStringPercent(), extension.damagePerEntropy);
    }

    public override string GetExplanationFinalizePart(StatRequest req, ToStringNumberSense numberSense, float finalVal)
    {
        return null;
    }
}
