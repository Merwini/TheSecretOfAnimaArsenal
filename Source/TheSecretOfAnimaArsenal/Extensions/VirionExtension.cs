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
        1, // days until poor
        3, // days until normal
        5, // days until good
        7, // days until excellent
        10, // days until masterwork
        15, // days until legendary
    };

    public float requiredTendQualityPerStage = 0.15f;

    public FloatRange virionActivityDaysRange = new FloatRange(3f, 7f);

    public FloatRange virionDamageDaysRange = new FloatRange(0.041f, 0.125f);

    public List<List<VirionSpawnEntry>> entitySpawnByStage;

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

    public static List<List<VirionSpawnEntry>> metalhorrorDefaults = new List<List<VirionSpawnEntry>>()
    { 
        new List<VirionSpawnEntry>() // Gestating - 1 larva
        {
            new VirionSpawnEntry()
            {
                kind = PawnKindDefOf.Metalhorror,
                forcedLifeStageIndex = 0,
                count = 1,
            }
        },

        new List<VirionSpawnEntry>() // Awful -  1 juvinile
        {
            new VirionSpawnEntry()
            {
                kind = PawnKindDefOf.Metalhorror,
                forcedLifeStageIndex = 1,
                count = 1,
            }
        },

        new List<VirionSpawnEntry>() // Poor -  1 juvinile
        {
            new VirionSpawnEntry()
            {
                kind = PawnKindDefOf.Metalhorror,
                forcedLifeStageIndex = 1,
                count = 1,
            }
        },

        new List<VirionSpawnEntry>() // Normal -  1 adult
        {
            new VirionSpawnEntry()
            {
                kind = PawnKindDefOf.Metalhorror,
                forcedLifeStageIndex = 2,
                count = 1,
            }
        },

        new List<VirionSpawnEntry>() // Good - 1 adult, 1 larva
        {
            new VirionSpawnEntry()
            {
                kind = PawnKindDefOf.Metalhorror,
                forcedLifeStageIndex = 2,
                count = 1,
            }
        },

        new List<VirionSpawnEntry>() // Excellent - 1 adult, 1 juvenile
        {
            new VirionSpawnEntry()
            {
                kind = PawnKindDefOf.Metalhorror,
                forcedLifeStageIndex = 2,
                count = 1,
            },
            new VirionSpawnEntry()
            {
                kind = PawnKindDefOf.Metalhorror,
                forcedLifeStageIndex = 0,
                count = 1,
            }
        },

        new List<VirionSpawnEntry>() // Masterwork - 2 adults
        {
            new VirionSpawnEntry()
            {
                kind = PawnKindDefOf.Metalhorror,
                forcedLifeStageIndex = 2,
                count = 1,
            },
            new VirionSpawnEntry()
            {
                kind = PawnKindDefOf.Metalhorror,
                forcedLifeStageIndex = 1,
                count = 1,
            }
        },

        new List<VirionSpawnEntry>() // Legendary - 3 adults
        {
            new VirionSpawnEntry()
            {
                kind = PawnKindDefOf.Metalhorror,
                forcedLifeStageIndex = 2,
                count = 2,
            }
        }
    };
}

public class VirionSpawnEntry
{
    public PawnKindDef kind;
    public int forcedLifeStageIndex = -1;
    public int count = 1;

}
