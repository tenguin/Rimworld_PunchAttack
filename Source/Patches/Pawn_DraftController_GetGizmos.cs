using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using Verse.AI;

namespace PunchAttack
{
    [HarmonyPatch(typeof(PawnAttackGizmoUtility), "GetAttackGizmos")]
    internal static class PawnAttackGizmoUtility_GetAttackGizmos
    {
        //Called once per frame per selected colonist
        [HarmonyPostfix]
        private static void GetGizmos(Pawn pawn, ref IEnumerable<Gizmo> __result)
        {
            if (pawn.Drafted)
            {
                __result = GenerateGizmos(pawn, __result);
            }
        }

        //Modified version of PawnAttackGizmoUtility.GetMeleeAttackGizmo
        private static IEnumerable<Gizmo> GenerateGizmos(Pawn pawn, IEnumerable<Gizmo> result)
        {
            Command_Target command_Target = new Command_Target();
            command_Target.icon = Assets.PunchAttack; //Icon, label, desc, and hotkey modified
            command_Target.defaultLabel = Assets.DefaultLabel;
            command_Target.defaultDesc = Assets.DefaultDesc;
            command_Target.hotKey = KeyBindingDefOf.Misc8;
            command_Target.targetingParams = TargetingParameters.ForAttackAny();
            if (GetPunchAttackAction(pawn, LocalTargetInfo.Invalid, out var failStr) == null) //Use modified punch attack
            {
                command_Target.Disable(failStr.CapitalizeFirst() + ".");
            }
            command_Target.action = delegate (LocalTargetInfo target)
            {
                foreach (Pawn item in Find.Selector.SelectedObjects.Where(delegate (object x)
                {
                    Pawn pawn2 = x as Pawn;
                    return pawn2 != null && pawn2.IsColonistPlayerControlled && pawn2.Drafted;
                }).Cast<Pawn>())
                {
                    string failStr2;
                    Action meleeAttackAction = GetPunchAttackAction(item, target, out failStr2); //Use modified punch attack
                    if (meleeAttackAction != null)
                    {
                        meleeAttackAction();
                    }
                    else if (!failStr2.NullOrEmpty())
                    {
                        Messages.Message(failStr2, target.Thing, MessageTypeDefOf.RejectInput, historical: false);
                    }
                }
            };
            yield return command_Target;

            foreach (Gizmo g in result)
            {
                yield return g;
            }
        }

        //Modified version of FloatMenuUtility.GetMeleeAttackAction
        private static Action GetPunchAttackAction(Pawn pawn, LocalTargetInfo target, out string failStr)
        {
            failStr = "";
            Pawn target2;
            if (!pawn.Drafted)
            {
                failStr = "IsNotDraftedLower".Translate(pawn.LabelShort, pawn);
            }
            else if (!pawn.IsColonistPlayerControlled)
            {
                failStr = "CannotOrderNonControlledLower".Translate();
            }
            else if (target.IsValid && !pawn.CanReach(target, PathEndMode.Touch, Danger.Deadly))
            {
                failStr = "NoPath".Translate();
            }
            else if (pawn.WorkTagIsDisabled(WorkTags.Violent))
            {
                failStr = "IsIncapableOfViolenceLower".Translate(pawn.LabelShort, pawn);
            }
            else if (!InteractionUtility.TryGetRandomVerbForSocialFight(pawn, out var verb)) //Use social fight verb instead of melee verb
            {
                failStr = "Incapable".Translate();
            }
            else if (pawn == target.Thing)
            {
                failStr = "CannotAttackSelf".Translate();
            }
            else if ((target2 = target.Thing as Pawn) != null && (pawn.InSameExtraFaction(target2, ExtraFactionType.HomeFaction) || pawn.InSameExtraFaction(target2, ExtraFactionType.MiniFaction)))
            {
                failStr = "CannotAttackSameFactionMember".Translate();
            }
            else
            {
                Pawn pawn2;
                if ((pawn2 = target.Thing as Pawn) == null || !pawn2.RaceProps.Animal || !HistoryEventUtility.IsKillingInnocentAnimal(pawn, pawn2) || new HistoryEvent(HistoryEventDefOf.KilledInnocentAnimal, pawn.Named(HistoryEventArgsNames.Doer)).DoerWillingToDo())
                {
                    return delegate
                    {
                        Job job = JobMaker.MakeJob(PunchAttackDefOf.Fuu_PunchAttack, target); //Use custom job that hides the weapon
                        job.verbToUse = verb; //Use social fight verb
                        Pawn pawn3 = target.Thing as Pawn;
                        if (pawn3 != null)
                        {
                            job.killIncappedTarget = pawn3.Downed;
                        }
                        pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    };
                }
                failStr = "IdeoligionForbids".Translate();
            }
            failStr = failStr.CapitalizeFirst();
            return null;
        }
    }
}
