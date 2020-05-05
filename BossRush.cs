using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.Xml;
using System.Globalization;
using System.Collections;
using System.IO;
using UnityEngine;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.CompilerServices;
using AssetBundles;
using UnityEngine.UI;

namespace BossRush
{
    [HarmonyPatch]
    static class BossRush_ZoningPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RunCtrl), nameof(RunCtrl.GoToNextZone))]
        static bool GoToNextZonePrefix(RunCtrl __instance, ZoneDot zoneDot)
        {
            __instance.currentRun.zoneNum = 1000;
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(WorldBar), nameof(WorldBar.GenerateWorldBar))]
        static bool GenerateWorldBarPreFix(WorldBar __instance, int numSteps)
        {
            foreach (Component component in __instance.zoneDotContainer)
                UnityEngine.Object.Destroy((UnityEngine.Object)component.gameObject);
            __instance.currentZoneDots.Clear();
            __instance.currentZoneSteps.Clear();
            if (__instance.btnCtrl.hideUICounter < 1)
                __instance.detailPanel.gameObject.SetActive(true);
            List<ZoneDot> zoneDotList1 = new List<ZoneDot>();
            List<string> stringList = new List<string>((IEnumerable<string>)__instance.runCtrl.currentRun.unvisitedWorldNames);
            int num1 = 100;
            //__instance.ResetZoneVars();
            __instance.runCtrl.currentRun.lastWorldGenOrigin = __instance.runCtrl.currentRun.currentWorldGen;
            bool flag1 = false;
            var num2 = 2;
            for (int index1 = 1; index1 <= num2; ++index1)
            {
                RectTransform rectTransform = new GameObject("ZoneStep").AddComponent<RectTransform>();
                List<ZoneDot> zoneDotList2 = new List<ZoneDot>();
                Vector3 vector3 = __instance.zoneDotContainer.transform.position - new Vector3((float)((double)__instance.width / 2.0 - (double)__instance.width / (double)(Mathf.Clamp(numSteps, 2, numSteps) - 1) * (double)index1) * __instance.zoneDotContainer.lossyScale.x, 0.0f, 0.0f);
                rectTransform.localScale = __instance.zoneDotContainer.lossyScale;
                rectTransform.SetParent(__instance.zoneDotContainer, true);
                rectTransform.transform.position = vector3;
                rectTransform.sizeDelta = new Vector2(10f, (float)num1);

                __instance.currentZoneSteps.Add(zoneDotList2);
                ZoneDot zoneDot = UnityEngine.Object.Instantiate<ZoneDot>(__instance.zoneDotPrefab, __instance.transform.position, __instance.transform.rotation, rectTransform.transform);
                zoneDotList2.Add(zoneDot);
                zoneDotList1.Add(zoneDot);
                zoneDot.stepNum = index1;
                zoneDot.worldBar = __instance;
                zoneDot.idCtrl = __instance.idCtrl;
                zoneDot.btnCtrl = __instance.btnCtrl;
                zoneDot.transform.name = "ZoneDot - Step: " + (object)index1;
                zoneDot.verticalSpacing = __instance.defaultVerticalSpacing;
                var max = 0;
                int count = __instance.runCtrl.currentRun.unvisitedWorldNames.Count;
                int index3 = __instance.runCtrl.NextWorldRand(0, stringList.Count);
                if (index1 == 1)
                {
                    //Debug.Log("Ey");
                    zoneDot.type = ZoneType.Boss;
                    max = 1;
                    if (stringList.Count != 0)
                    {
                        zoneDot.worldName = stringList[Math.Min(index3, stringList.Count - 1)];
                    }
                    else
                    {
                        zoneDot.worldName = "Pacifist";
                    }
                    if (S.I.runCtrl.currentWorld.nameString == "Pacifist")
                    {
                        zoneDot.type = ZoneType.Battle;
                    }

                    zoneDot.world = __instance.runCtrl.worlds[zoneDot.worldName];
                    zoneDot.imageName = zoneDot.world.iconName;
                }
                else
                {
                    max = 1;

                    if (stringList.Count > 0)
                    {
                        //Debug.Log("Yo");
                        zoneDot.worldName = stringList[index3];
                        zoneDot.world = __instance.runCtrl.worlds[zoneDot.worldName];
                        zoneDot.imageName = zoneDot.world.iconName;
                        stringList.Remove(stringList[index3]);
                    }
                    else
                    {
                        //Debug.Log("Dog");
                        zoneDot.worldName = "Pacifist";
                        zoneDot.imageName = "WorldWasteland";
                    }
                    //Debug.Log("Hey");
                    zoneDot.world = __instance.runCtrl.worlds[zoneDot.worldName];
                    zoneDot.world.numZones = 1;
                    zoneDot.type = ZoneType.World;
                }
                zoneDot.transform.position = vector3 + new Vector3(0.0f, ((float)(max - 1) / 2f - 0 * zoneDot.verticalSpacing) * __instance.rect.localScale.y, 0.0f);
                __instance.currentZoneDots.Add(zoneDot);
            }
            for (int i = 0; i < zoneDotList1.Count(); i++)
            {
                if (i < zoneDotList1.Count() - 1)
                {
                    zoneDotList1[i].AddNextDot(zoneDotList1[i + 1]);
                }
                if (i > 0)
                {
                    zoneDotList1[i].previousDots.Add(zoneDotList1[i - 1]);
                }
            }
            foreach (ZoneDot currentZoneDot in __instance.currentZoneDots)
            {
                //Debug.Log("Hi");
                ZoneDot zoneDot = currentZoneDot;
                zoneDot.CreateLines();
            }
            //Debug.Log("Hello");
            __instance.selectionMarker.transform.position = __instance.currentZoneDots[0].transform.position;
            return false;
        }
    }
}