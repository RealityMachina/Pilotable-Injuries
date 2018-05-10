using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using BattleTech;
using System.Reflection;

namespace InjuredPiloting
{
    class PilotInjuryCheck
    {
        [HarmonyPatch(typeof(BattleTech.Pilot))]
        [HarmonyPatch("CanPilot", PropertyMethod.Getter)]
        public static class BattleTech_Pilot_CanPilot_Prefix
        {
            static bool Prefix(Pilot __instance, bool __result)
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

                if((medTechs / 3) >= numInjuries) //formula is that for every 3 medtechs, we can ignore an injury. So if we have enough medtechs to equal the number of injuries the pilot has...
                {
                    __result = true;
                }


                return false;
            }
        }

    }
}
