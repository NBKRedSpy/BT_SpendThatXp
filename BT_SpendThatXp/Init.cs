using Harmony;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BT_SpendThatXp
{
    public static class HarmonyInit
    {
        public static void Init(string directory, string settingsJSON)
        {
            var harmony = HarmonyInstance.Create("io.github.nbk_redspy.BT_SpendThatXp");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
