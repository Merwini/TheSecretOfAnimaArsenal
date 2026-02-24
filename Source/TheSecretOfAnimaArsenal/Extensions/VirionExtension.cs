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

    public float initialGestationDays;

    public List<float> qualityGestationDaysList = new List<float>()
    {
        // starts at awful
        1, // poor
        3, // normal
        5, // good
        7, // excellent
        10, // masterwork
        15, // legendary
    };

    public float requiredTendQualityPerStage = 0.15f;

    public FloatRange virionAcvtivityDaysRange = new FloatRange(3f, 7f);

    public float virionDamageDaysRange = new FloatRange(0.041f, 0.125f);

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

        if (initialGestationDays <= 0)
        {
            initialGestationDays = 1;
            yield return "BioferriteVirionExtension gestationDays must be greater than 0. Defaulting to 1.";
        }
    }
}
