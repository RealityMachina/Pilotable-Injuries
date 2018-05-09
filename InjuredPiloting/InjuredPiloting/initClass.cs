using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Harmony;

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
}
