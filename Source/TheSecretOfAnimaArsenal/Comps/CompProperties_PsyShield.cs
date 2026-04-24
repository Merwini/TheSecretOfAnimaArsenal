using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace tsoa.arsenal;

public class CompProperties_PsyShield : CompProperties
{
    public float heatPerDamage = 0.5f;

    public float resetHeatPercent = 0.8f;

    public int resetDelayTicks = 120;

    public float minDrawSize = 1.2f;

    public float maxDrawSize = 1.55f;

    public Dictionary<QualityCategory, float> qualityHeatMultipliers = new Dictionary<QualityCategory, float>
    {
        { QualityCategory.Awful, 1f },
        { QualityCategory.Poor, 1f },
        { QualityCategory.Normal, 0.9f },
        { QualityCategory.Good, 0.8f },
        { QualityCategory.Excellent, 0.7f },
        { QualityCategory.Masterwork, 0.6f },
        { QualityCategory.Legendary, 0.5f }
    };

    public CompProperties_PsyShield()
    {
        this.compClass = typeof(CompPsyShield);
    }
}
