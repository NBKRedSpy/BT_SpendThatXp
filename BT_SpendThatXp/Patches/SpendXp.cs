using BattleTech;
using BattleTech.UI;
using BattleTech.UI.TMProWrapper;
using Harmony;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BT_SpendThatXp.Patches
{

    [HarmonyPatch(typeof(SGBarracksRosterSlot), nameof(SGBarracksRosterSlot.Refresh))]
    public static class SpendXp
    {

        public static AccessFields AccessFields { get; set; }


        /// <summary>
        /// The sum of the total amount of XP required to get to a specific level.
        /// </summary>
        public static List<int> XpTotalCost { get; set; }

        public static SimGameState sim;


        private static void InitXpTotalCost()
        {

            if (XpTotalCost != null) return;

            var sim = UnityGameInstance.BattleTechGame.Simulation;

            if(sim == null)
            {
                throw new ApplicationException("Simulation is null");
            }

            XpTotalCost = new List<int>();

            int sum = 0;

            for (int i = 0; i < 10; i++)
            {
                int xpCost = sim.GetLevelCost(i);

                sum += xpCost;

                XpTotalCost.Add(sum);
            }
        }

        public static void Postfix(SGBarracksRosterSlot __instance)
        {
            try
            {
                Pilot pilot = __instance.Pilot;
                if (pilot == null) return;

                InitXpTotalCost();


                if (AccessFields == null)
                {
                    AccessFields = new AccessFields();
                    AccessFields.Init();
                }

                //Check if on hiring screen
                if ((bool)AccessFields.ShowCost.GetValue(__instance) == true) return;

                int unspentXp = pilot.UnspentXP;


                SetSkillColor(__instance, pilot.Gunnery, unspentXp, AccessFields.GunneryFieldInfo);
                SetSkillColor(__instance, pilot.Piloting, unspentXp, AccessFields.PilotingFieldInfo);
                SetSkillColor(__instance, pilot.Guts, unspentXp, AccessFields.GutsFieldInfo);
                SetSkillColor(__instance, pilot.Tactics, unspentXp, AccessFields.TacticsFieldInfo);

            }
            catch (Exception ex)
            {
                Logger.Log(ex);
            }
        }

        private static void SetSkillColor(SGBarracksRosterSlot __instance, int currentSkillLevel, int unspentXp,
            FieldInfo targetSkillLabelInfo)
        {

            int maxLevelIncrease = XpTotalCost.FindLastIndex(x => x <= XpTotalCost[currentSkillLevel - 1] + unspentXp) -
                (currentSkillLevel - 1);

            if (maxLevelIncrease > 0)
            {
                string highlightColor;
                if (maxLevelIncrease == 1)
                {
                    highlightColor = Core.Settings.Level1Color;
                }
                else if (maxLevelIncrease == 2)
                {
                    highlightColor = Core.Settings.Level2Color;
                }
                else
                {
                    highlightColor = Core.Settings.Level3Color;
                }

                var skillLabel = (LocalizableText)targetSkillLabelInfo.GetValue(__instance);
                skillLabel.SetText($"<color={highlightColor}>{skillLabel.text}</color>");
            }
        }
    }

    public class AccessFields
    {

        public FieldInfo PilotingFieldInfo { get; set; }
        public FieldInfo GunneryFieldInfo { get; private set; }
        public FieldInfo GutsFieldInfo { get; private set; }
        public FieldInfo TacticsFieldInfo { get; private set; }
        public FieldInfo ShowCost { get; private set; }

        public void Init()
        {
            PilotingFieldInfo = AccessTools.Field(typeof(SGBarracksRosterSlot), "piloting");
            GunneryFieldInfo = AccessTools.Field(typeof(SGBarracksRosterSlot), "gunnery");
            GutsFieldInfo = AccessTools.Field(typeof(SGBarracksRosterSlot), "guts");
            TacticsFieldInfo = AccessTools.Field(typeof(SGBarracksRosterSlot), "tactics");

            ShowCost = AccessTools.Field(typeof(SGBarracksRosterSlot), "showCost");


        }
    }
}

