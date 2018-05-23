using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Harmony;
using BattleTech;

namespace InjuredPiloting
{
    public class InitClass
    {
        public static void Init()
        {
            var harmony = HarmonyInstance.Create("Battletech.realitymachina.InjuredPiloting");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    public class Holder
    {
        public static HashSet<Pilot> injPilots; //use this to track already injured pilots on a mission
        public static HashSet<Pilot> newlyInjured; //use this to track pilots who already gotten hit
        public static void Reset()
        {
            injPilots = new HashSet<Pilot>();
            newlyInjured = new HashSet<Pilot>();
        }
    }

}
