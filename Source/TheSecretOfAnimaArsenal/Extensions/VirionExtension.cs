using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace tsoa.arsenal;

public class VirionExtension : DefModExtension
{
    public ThingDef producedItem;

    public int producedCount = 1;

    public float gestationDays;

    public List<PawnKindDef> spawnedEntities;

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (string error in base.ConfigErrors())
        {
            yield return error;
        }

        if (producedItem == null)
        {
            producedItem = ThingDefOf.WoodLog;
            yield return "BioferriteVirionExtension missing producedItem. Defaulting to one wooden log (this is funny to me).";
        }

        if (gestationDays <= 0)
        {
            gestationDays = 1;
            yield return "BioferriteVirionExtension gestationDays must be greater than 0. Defaulting to 1.";
        }
    }
}
