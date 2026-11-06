using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using LudeonTK;

namespace tsoa.arsenal;

public class DebugTools
{
    [DebugAction("Secret of Anima", "Add virion", true, false, false, true, false, 0, false, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static List<DebugActionNode> AddVirion()
    {
        List<DebugActionNode> list = new();
        IEnumerable<RecipeDef> recipes = DefDatabase<RecipeDef>.AllDefsListForReading.Where(x => x.HasModExtension<VirionExtension>());
        foreach (RecipeDef recipe in recipes)
        {
            RecipeDef recipeInner = recipe;
            list.Add(new DebugActionNode(recipe.LabelCap, DebugActionType.ToolMapForPawns, null, delegate(Pawn p)
            {
                if (p.health.hediffSet.HasHediff(TSOAA_DefOf.TSOA_VirionHediff))
                {
                    Messages.Message($"{p.LabelCap} already has a virion!", MessageTypeDefOf.RejectInput, false);
                    return;
                }
                Hediff_Virion hediff = HediffMaker.MakeHediff(recipe.addsHediff, p, null) as Hediff_Virion;
                hediff.virionRecipeDef = recipe;
                p.health.AddHediff(hediff, null);
                Messages.Message($"Added {hediff.Extension.producedItem.LabelCap} virion to {p.LabelCap}", MessageTypeDefOf.PositiveEvent, false);
            }));
        }
        return list;
    }
}
