using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using BattleTech;
using System.Reflection;
using UnityEngine;
using System.Reflection.Emit;

namespace InjuredPiloting
{
    class PilotInjuryCheck
    {
        [HarmonyPatch(typeof(BattleTech.Pilot))]
        [HarmonyPatch("CanPilot", PropertyMethod.Getter)]
        public static class BattleTech_Pilot_CanPilot_Prefix
        {
            static bool Prefix(Pilot __instance, ref bool __result)
            {
                GameInstance battletechGame = UnityGameInstance.BattleTechGame;

                if(battletechGame == null || battletechGame.Simulation == null)
                {
                    return true; //ignore the prefix check because the game doesn't exist, or the simulation doesn't
                }

                //we know we exist now, so...

                int medTechs = battletechGame.Simulation.MedTechSkill;

                if(medTechs < 3)
                {
                    return true; //not enough medtechs to override 
                }

                int numInjuries = __instance.Injuries;

                if(numInjuries <= 0)
                {
                    return true; //non-applicable
                }

                if((medTechs / 3) >= numInjuries) //formula is that for every 3 medtechs, we can ignore an injury. So if we have enough medtechs to equal the number of injuries the pilot has...
                {
                    __result = true;
                }


                return false;
            }
        }

        [HarmonyPatch(typeof(BattleTech.Pilot))]
        [HarmonyPatch("InjurePilot")]
        public static class BattleTech_Pilot_InjurePilot_Prefix
        {
            static void Prefix(Pilot __instance, string sourceID, int stackItemUID, int dmg, string source)
            {
                if (__instance.ParentActor != null && __instance.ParentActor.StatCollection.GetValue<bool>("IgnorePilotInjuries"))
                {
                    return;
                }
                int bonusHealth = __instance.BonusHealth;
                if (bonusHealth > 0)
                {
                    int num = Mathf.Min(dmg, bonusHealth);
                    dmg -= num;

                }

                if(dmg <= 0)
                {
                    return;
                }
                //we're getting injured, so...
                Holder.newlyInjured.Add(__instance);
            }
        }


        [HarmonyPatch(typeof(BattleTech.GameInstance), "LaunchContract", new Type[] { typeof(Contract), typeof(string) })]
        public static class BattleTech_GameInstance_LaunchContract_Patch
        {
            static void Postfix(GameInstance __instance, Contract contract)
            {
                // reset on new contracts
                Holder.Reset();

                foreach (Pilot pilot in __instance.Simulation.PilotRoster)
                {
                    if(pilot.Injuries > 0)
                    {
                        Holder.injPilots.Add(pilot);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(BattleTech.SimGameState))]
        [HarmonyPatch("RefreshInjuries")]
        public static class BattleTech_RefreshInjuries_Patch
        {
            static void Postfix(SimGameState __instance)
            {
                foreach (Pilot pilot in Holder.injPilots)
                {
                    if(Holder.newlyInjured.Contains(pilot))
                    {
                        //an already injured pilot has been injured, so...
                        WorkOrderEntry_MedBayHeal workOrderEntry_MedBayHeal;
                        if (!__instance.MedBayQueue.SubEntryContainsID(pilot.Description.Id))
                        {
                            return;
                        }
                        //we now know they have an injury queue, so...
                        workOrderEntry_MedBayHeal = (WorkOrderEntry_MedBayHeal)__instance.MedBayQueue.GetSubEntry(pilot.Description.Id);

                        workOrderEntry_MedBayHeal.SetCost(GetInjuryCost(pilot, __instance));
                    }
                }
            }

            private static int GetInjuryCost(Pilot p, SimGameState __instance)
            {
                int num = 0;
                if (p.LethalInjuries)
                {
                    num += __instance.Constants.Pilot.LethalDamageCost;
                }
                if (p.IsIncapacitated)
                {
                    num += __instance.Constants.Pilot.IncapacitatedDamageCost;
                }
                int num2 = Mathf.Min(p.Injuries, p.Health);
                int num3 = __instance.Constants.Pilot.BaseInjuryDamageCost / p.Health;
                for (int i = 0; i < num2; i++)
                {
                    num += num3;
                }
                return num + p.pilotDef.TimeoutRemaining * __instance.GetDailyHealValue();
            }
        }
    }
}
