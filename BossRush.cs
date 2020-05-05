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
    [HarmonyPatch(typeof(WorldBar), nameof(WorldBar.GenerateWorldBar))]
    static class BossRush_ZoningPatches
    {
        /*[HarmonyPrefix]
        [HarmonyPatch(typeof(RunCtrl), nameof(RunCtrl.GoToNextZone))]
        static bool GoToNextZonePrefix(RunCtrl __instance, ZoneDot zoneDot)
        {
            if(zoneDot.type==ZoneType.World)__instance.currentRun.zoneNum = 1000;
            return true;
        }*/
        static bool Prefix(WorldBar __instance, int numSteps)
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
            var num2 = __instance.runCtrl.currentWorld.nameString == "Eden" ? 2 : 3;
            List<ZoneDot> zoneDotList3 = new List<ZoneDot>();
            RectTransform rectTransform = null;

            for (int index1 = 1; index1 <= num2; ++index1)
            {
                rectTransform = new GameObject("ZoneStep").AddComponent<RectTransform>();
                List<ZoneDot> zoneDotList2 = new List<ZoneDot>();
                Vector3 vector3 = __instance.zoneDotContainer.transform.position - new Vector3((float)((double)__instance.width / 2.0 - (double)__instance.width / (double)(Mathf.Clamp(numSteps, 2, numSteps) - 1) * (double)index1) * __instance.zoneDotContainer.lossyScale.x, 0.0f, 0.0f);
                rectTransform.localScale = __instance.zoneDotContainer.lossyScale;
                rectTransform.SetParent(__instance.zoneDotContainer, true);
                rectTransform.transform.position = vector3;
                rectTransform.sizeDelta = new Vector2(10f, (float)num1);

                __instance.currentZoneSteps.Add(zoneDotList2);
                var max = Math.Max(Math.Min(stringList.Count, 3), 1);
                if (index1 <= 2)
                {
                    max = 1;
                }
                for (int index2 = 1; index2 <= max; ++index2)
                {
                    //Debug.Log(index1);
                    ZoneDot zoneDot = UnityEngine.Object.Instantiate<ZoneDot>(__instance.zoneDotPrefab, __instance.transform.position, __instance.transform.rotation, rectTransform.transform);
                    zoneDot.stepNum = index1;
                    zoneDotList2.Add(zoneDot);
                    zoneDot.worldBar = __instance;
                    zoneDot.idCtrl = __instance.idCtrl;
                    zoneDot.btnCtrl = __instance.btnCtrl;
                    zoneDot.transform.name = "ZoneDot - Step: " + (object)index1 + " - " + (object)index2;
                    zoneDot.verticalSpacing = __instance.defaultVerticalSpacing;
                    int index3 = __instance.runCtrl.NextWorldRand(0, stringList.Count);
                    if (index1 == 1 && num2 != 1) {
                        zoneDot.SetType(ZoneType.Shop);
                    } 
                    else if (index1 <= 2)
                    {
                        //Debug.Log("Ey");
                        zoneDot.SetType(ZoneType.Boss);
                        if (stringList.Count != 0)
                        {
                            zoneDot.worldName = stringList[Math.Min(index3, stringList.Count - 1)];
                        }
                        else
                        {
                            if (__instance.runCtrl.savedBossKills >= 7)
                            {
                                zoneDot.worldName = "Genocide";
                            }
                            else if (__instance.runCtrl.savedBossKills >= 1)
                            {
                                zoneDot.worldName = "Normal";
                            }
                            else
                            {
                                zoneDot.worldName = "Pacifist";
                            }
                        }
                        if (__instance.runCtrl.currentWorld.nameString == "Pacifist")
                        {
                            zoneDot.SetType(ZoneType.Battle);
                        }

                        zoneDot.world = __instance.runCtrl.worlds[zoneDot.worldName];
                        zoneDot.imageName = zoneDot.world.iconName;
                    }
                    else
                    {
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
                            if (__instance.runCtrl.savedBossKills >= 7)
                            {
                                zoneDot.worldName = "Genocide";
                                zoneDot.imageName = "WorldWasteland";
                            }
                            else if (__instance.runCtrl.savedBossKills >= 1)
                            {
                                zoneDot.worldName = "Normal";
                                zoneDot.imageName = "WorldWasteland";
                            }
                            else
                            {
                                zoneDot.worldName = "Pacifist";
                                zoneDot.imageName = "WorldWasteland";
                            }
                            if (__instance.runCtrl.currentRun.worldName == "Pacifist")
                            {
                                zoneDot.imageName = "WorldEden";
                            }

                        }
                        //Debug.Log("Hey");
                        zoneDot.world = __instance.runCtrl.worlds[zoneDot.worldName];
                        zoneDot.SetType(ZoneType.World);
                    }
                    zoneDot.transform.position = vector3 + new Vector3(0.0f, ((float)(max - 1) - (float)(index2 - 1)) * zoneDot.verticalSpacing * __instance.rect.localScale.y, 0.0f);
                    zoneDotList3.Add(zoneDot);
                    zoneDotList1.Add(zoneDot);
                    __instance.currentZoneDots.Add(zoneDot);
                }
            }
            if (num2 > 1)
            {
                zoneDotList1[0].AddNextDot(zoneDotList1[1]);
                zoneDotList1[1].previousDots.Add(zoneDotList1[0]);
                for (int i = 2; i < zoneDotList1.Count(); i++)
                {
                    zoneDotList1[1].AddNextDot(zoneDotList1[i]);
                    zoneDotList1[i].previousDots.Add(zoneDotList1[1]);
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

    [HarmonyPatch]
    [HarmonyPatch(typeof(RunCtrl), nameof(RunCtrl.GoToNextZone))]
    static class BossRush_RunCtrlPatches
    {
        static bool Prefix(RunCtrl __instance, ZoneDot zoneDot)
        {
            if (__instance.worldBar.currentZoneDots.Count() > 1 && __instance.currentZoneDot == __instance.worldBar.currentZoneDots[1])
            {
                __instance.currentRun.zoneNum = 1000;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SpawnCtrl), nameof(SpawnCtrl.SpawnBoss))]
    static class BossRush_SpawnBoss
    {
        /*static bool Prefix(SpawnCtrl __instance, int xPos, int yPos)
        {
            var factor1 = __instance.bossesToSpawn.Count() > 0;
            var factor2 = __instance.bossesToSpawn[0] != "BossGate";
            var factor3 = S.I.runCtrl.currentZoneDot.type == ZoneType.Battle;
            var factor4 = S.I.runCtrl.currentRun.worldName == "Pacifist";
            Debug.Log("Factor1: " + factor1);
            Debug.Log("Factor2: " + factor2);
            Debug.Log("Factor3: " + factor3);
            Debug.Log("Factor4: " + factor4);
            return !(factor1 && factor2 && factor3 && factor4);
        }*/
    }

    [HarmonyPatch(typeof(SpawnCtrl), nameof(SpawnCtrl.SpawnZoneC))]
    static class BossRush_SpawnZone
    {
        /*static bool Prefix(SpawnCtrl __instance, ZoneType zoneType)
        {
            Debug.Log("Prefix");
            if (zoneType == ZoneType.Battle && S.I.runCtrl.currentRun.worldName == "Pacifist" && S.I.runCtrl.savedBossKills > 0)
            {
                S.I.dontSpawnAnything = true;
            }
            return true;
        }*/

        static void Postfix(SpawnCtrl __instance, ZoneType zoneType)
        {
            if (zoneType == ZoneType.Boss)
            {
                S.I.batCtrl.experienceGained = 600;
                __instance.StartCoroutine(WaitAndMoney());
                S.I.batCtrl.noHitMoneyBonus = 150;
                if (S.I.runCtrl.currentRun.worldName == "Normal")
                {
                    __instance.bossesToSpawn.Insert(0, "BossGate");
                }
            }
        }

        static IEnumerator WaitAndMoney()
        {
            yield return new WaitForSeconds(0.5f);
            S.I.shopCtrl.ModifySera(250);
        }
    }
}