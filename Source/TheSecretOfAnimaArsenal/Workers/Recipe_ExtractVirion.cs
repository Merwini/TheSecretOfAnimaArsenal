using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;

namespace tsoa.arsenal;

public class Recipe_ExtractVirion : Recipe_Surgery
{
    public override bool AvailableOnNow(Thing thing, BodyPartRecord part = null)
    {
        Log.Message("checking if recipe_extractvirior is available on " + thing.LabelShort);
        if (!base.AvailableOnNow(thing, part))
        {
            Log.Message("base not available now");
            return false;
        }

        if (thing is not Pawn pawn)
        {
            Log.Message("not a pawn");
            return false;

        }

        bool available = pawn.health?.hediffSet?.GetFirstHediffOfDef(TSOAA_DefOf.TSOA_VirionHediff) != null;
        Log.Message("found Virion Hediff: " + available);
        return available;
    }

    public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
    {
        if (billDoer != null)
        {
            if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                return;

            TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
        }

        Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(TSOAA_DefOf.TSOA_VirionHediff);
        // Quick safety check in case the hediff somehow gets removed between when the surgery starts and finishes
        if (hediff is not Hediff_Virion virion)
        {
            return;
        }

        // Hediff will handle all the results
        virion.Notify_VirionExtracted();
    }
}
