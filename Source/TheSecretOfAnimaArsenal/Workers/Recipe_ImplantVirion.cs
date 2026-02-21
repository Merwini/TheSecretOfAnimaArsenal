using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace tsoa.arsenal;

public class Recipe_ImplantVirion : Recipe_InstallImplant
{
    public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
    {
        if (billDoer != null)
        {
            if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
            {
                return;
            }
            TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
        }

        Thing virionThing = null;
        if (ingredients != null)
        {
            for (int i = 0; i < ingredients.Count; i++)
            {
                Thing t = ingredients[i];
                if (t != null)
                {
                    virionThing = t;
                    break;
                }
            }
        }

        Hediff_Virion hediff = HediffMaker.MakeHediff(recipe.addsHediff, pawn, part) as Hediff_Virion;

        hediff.virionDef = virionThing.def;

        pawn.health.AddHediff(hediff, part);
    }
}
