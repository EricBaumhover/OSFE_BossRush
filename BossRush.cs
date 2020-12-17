using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace BossRush
{
    [HarmonyPatch]
    [HarmonyPatch(typeof(WorldBar), nameof(WorldBar.GenerateWorldBar))]
    static class BossRush_ZoningPatches
    {
        //Developer variable to test final bosses.
        public static bool final_test = false;

        //Generates custom world bar if boss rush is selected.
        static bool Prefix(WorldBar __instance, int numSteps)
        {
            
            //Value set by button on main menu
            //IDK, Might help with music erros.
            S.I.muCtrl.StopIntroLoop();
            //Delete all previous zone dots.
            foreach (Component component in __instance.zoneDotContainer)
                UnityEngine.Object.Destroy((UnityEngine.Object)component.gameObject);
            //Clear deleted zone dots from lists/
            __instance.currentZoneDots.Clear();
            __instance.currentZoneSteps.Clear();

            //From original code.
            if (__instance.btnCtrl.hideUICounter < 1)
                __instance.detailPanel.gameObject.SetActive(true);

            //__instance.ResetZoneVars(); //This line is part of original code, but not necessary often.

            //If else to decide which generation function to use.
            if (__instance.runCtrl.currentWorld.nameString == "Genocide") 
            {
                //These music pauses might not do anything.
                S.I.muCtrl.PauseIntroLoop(true);
                CreateGenocide(__instance, numSteps);
            }
            else if (__instance.runCtrl.currentWorld.nameString == "Pacifist")
            {
                S.I.muCtrl.PauseIntroLoop(true);
                CreatePacifist(__instance, numSteps);
            }
            else if (__instance.runCtrl.currentWorld.nameString == "Normal")
            {
                S.I.muCtrl.PauseIntroLoop(true);
                CreateNormal(__instance, numSteps);
            }
            else
            {
                S.I.muCtrl.PauseIntroLoop(true);
                CreateBossWorld(__instance, numSteps);
            }
            //IDK what this does but i guess its important
            __instance.runCtrl.currentRun.lastWorldGenOrigin = __instance.runCtrl.currentRun.currentWorldGen;

            //Draw lines in ui
            foreach (ZoneDot currentZoneDot in __instance.currentZoneDots)
            {
                Debug.Log("connect the dots!");
                ZoneDot zoneDot = currentZoneDot;
                zoneDot.CreateLines();
            }
            //Debug.Log("Hello");

            //Set selection marker for ui.
            __instance.selectionMarker.transform.position = __instance.currentZoneDots[0].transform.position;
            //Dont call original function.
            return false;
        }

        //Generates world with a normal boss.
        static void CreateBossWorld(WorldBar bar, int numSteps)
        {
            //List of all remaining normal worlds.
            List<string> stringList = new List<string>((IEnumerable<string>)bar.runCtrl.currentRun.unvisitedWorldNames);
            //From original code
            int num1 = 100;
            //If testing final bosses, make it think there are no more remaining worlds.
            if (final_test) stringList.Clear();
            //Number of zone steps to generate. Add two more if before final world.
            int num2 = 3;
            //Per step list of zone dots.
            List<ZoneDot> zoneDotList3 = new List<ZoneDot>();
            //
            RectTransform rectTransform = null;
            for (int index1 = 1; index1 <= num2; ++index1)
            {
                //Bunch of stuff which makes it look right. From original function.
                rectTransform = new GameObject("ZoneStep").AddComponent<RectTransform>();
                List<ZoneDot> zoneDotList2 = new List<ZoneDot>();
                Vector3 vector3 = bar.zoneDotContainer.transform.position - new Vector3((float)((double)bar.width / 2.0 - (double)bar.width / (double)(Mathf.Clamp(numSteps, 2, numSteps) - 1) * (double)index1) * bar.zoneDotContainer.lossyScale.x, 0.0f, 0.0f);
                rectTransform.localScale = bar.zoneDotContainer.lossyScale;
                rectTransform.SetParent(bar.zoneDotContainer, true);
                rectTransform.transform.position = vector3;
                rectTransform.sizeDelta = new Vector2(10f, (float)num1);

                
                
                var max = 0;
                int count = bar.runCtrl.currentRun.unvisitedWorldNames.Count;
                int index3 = final_test ? 3 : stringList.Count;
                if (index1 == num2-1)
                {
                    //Create zone dot from prefab.
                    ZoneDot zoneDot = UnityEngine.Object.Instantiate<ZoneDot>(bar.zoneDotPrefab, bar.transform.position, bar.transform.rotation, rectTransform.transform);
                    //Specify step of dot.
                    zoneDot.stepNum = index1;
                    //List2 is the step list.
                    zoneDotList2.Add(zoneDot);
                    //Zone dot needs to know its world bar.
                    zoneDot.worldBar = bar;
                    //Set ctrls
                    zoneDot.idCtrl = bar.idCtrl;
                    zoneDot.btnCtrl = bar.btnCtrl;
                    //Set name of dot.
                    zoneDot.transform.name = "ZoneDot - Step: " + (object)index1;
                    //Use world selection spacing for everything.
                    zoneDot.verticalSpacing = bar.defaultVerticalSpacing;
                    //zoneDot.verticalSpacing += 7f;
                    //Decide what row comes next
                    zoneDot.SetType(ZoneType.Boss);
                    zoneDot.transform.position = vector3 + new Vector3(0.0f, ((float)(max - 1) / 2f - 0 * zoneDot.verticalSpacing) * bar.rect.localScale.y, 0.0f);
                    zoneDotList3.Add(zoneDot);
                }
                else if (index1 == 1)
                {
                    //Create zone dot from prefab.
                    ZoneDot zoneDot = UnityEngine.Object.Instantiate<ZoneDot>(bar.zoneDotPrefab, bar.transform.position, bar.transform.rotation, rectTransform.transform);
                    //Specify step of dot.
                    zoneDot.stepNum = index1;
                    //List2 is the step list.
                    zoneDotList2.Add(zoneDot);
                    //Zone dot needs to know its world bar.
                    zoneDot.worldBar = bar;
                    //Set ctrls
                    zoneDot.idCtrl = bar.idCtrl;
                    zoneDot.btnCtrl = bar.btnCtrl;
                    //Set name of dot.
                    zoneDot.transform.name = "ZoneDot - Step: " + (object)index1;
                    //Use world selection spacing for everything.
                    zoneDot.verticalSpacing = bar.defaultVerticalSpacing;
                    //zoneDot.verticalSpacing += 7f;
                    //Decide what row comes next
                    zoneDot.SetType(ZoneType.Campsite);
                    zoneDot.transform.position = vector3 + new Vector3(0.0f, ((float)(max - 1) / 2f - 0 * zoneDot.verticalSpacing) * bar.rect.localScale.y, 0.0f);
                    zoneDotList3.Add(zoneDot);
                }
                else for (int loop_index = 0; loop_index < index3 || loop_index < 1; loop_index++)
                {
                    //Create zone dot from prefab.
                    ZoneDot zoneDot = UnityEngine.Object.Instantiate<ZoneDot>(bar.zoneDotPrefab, bar.transform.position, bar.transform.rotation, rectTransform.transform);
                    //Specify step of dot.
                    zoneDot.stepNum = index1;
                    //List2 is the step list.
                    zoneDotList2.Add(zoneDot);
                    //Zone dot needs to know its world bar.
                    zoneDot.worldBar = bar;
                    //Set ctrls
                    zoneDot.idCtrl = bar.idCtrl;
                    zoneDot.btnCtrl = bar.btnCtrl;
                    //Set name of dot.
                    zoneDot.transform.name = "ZoneDot - Step: " + (object)index1 + " - " + (object)loop_index;
                    //Use world selection spacing for everything.
                    zoneDot.verticalSpacing = bar.defaultVerticalSpacing;
                    //Decide what row comes next
                    //Use random next boss world.
                    if (stringList.Count > 0 && !final_test)
                    {
                        int world_index = bar.runCtrl.NextWorldRand(0, stringList.Count);
                        zoneDot.worldName = stringList[Math.Min(world_index, stringList.Count - 1)];
                        zoneDot.world = bar.runCtrl.worlds[zoneDot.worldName];
                        zoneDot.imageName = zoneDot.world.iconName;
                        stringList.Remove(stringList[Math.Min(world_index, stringList.Count - 1)]);
                    }
                    //Final world
                    else
                    {
                        //bar.runCtrl.savedBossKills is the correct variable for this type of thing.
                        if ((!final_test && bar.runCtrl.savedBossKills >= 7) || (final_test && loop_index == 1))
                        {
                            zoneDot.worldName = "Genocide";
                            zoneDot.imageName = "WorldWasteland";
                        }
                        else if ((!final_test && bar.runCtrl.savedBossKills >= 1) || (final_test && loop_index == 2))
                        {
                            zoneDot.worldName = "Normal";
                            zoneDot.imageName = "WorldWasteland";
                        }
                        else
                        {
                            zoneDot.worldName = "Pacifist";
                            zoneDot.imageName = "WorldWasteland";
                        }
                            

                    }
                    //Set world stuff.
                    zoneDot.world = bar.runCtrl.worlds[zoneDot.worldName];
                    zoneDot.SetType(ZoneType.World);
                    zoneDot.transform.position = vector3 + new Vector3(0.0f, ((float)(max - 1) - (float)(loop_index - 1)) * zoneDot.verticalSpacing * bar.rect.localScale.y, 0.0f);
                    zoneDotList3.Add(zoneDot);
                }
                bar.currentZoneSteps.Add(zoneDotList2);
                foreach (ZoneDot dot in zoneDotList3)
                {
                    bar.currentZoneDots.Add(dot);
                }
                //Clear temp list.
                zoneDotList3.Clear();

            }
            //Decide connections.
            for (int i = 0; i < bar.currentZoneSteps.Count(); i++)
            {
                if (i == num2-2)
                {
                    Debug.Log("First Step!");
                    for (int j = 0; j < bar.currentZoneSteps[i+1].Count(); j++)
                    {
                        Debug.Log("Next Dot!");
                        bar.currentZoneSteps[i][0].AddNextDot(bar.currentZoneSteps[i+1][j]);
                    }
                } 
                else if (i == 0)
                {
                    bar.currentZoneSteps[0][0].AddNextDot(bar.currentZoneSteps[1][0]);
                }
                else
                {
                    Debug.Log("Other Step!");
                    for (int j = 0; j < bar.currentZoneSteps[i].Count(); j++)
                    {
                        Debug.Log("Previous Connection!");
                        bar.currentZoneSteps[i][j].previousDots.Add(bar.currentZoneSteps[i-1][0]);
                    }
                }
            }
        }

        //Generates world with only Serif boss.
        static void CreateGenocide(WorldBar bar, int numSteps)
        {
            List<ZoneDot> zoneDotList1 = new List<ZoneDot>();
            int num1 = 100;
            RectTransform rectTransform = null;
 
            rectTransform = new GameObject("ZoneStep").AddComponent<RectTransform>();
               
            List<ZoneDot> zoneDotList2 = new List<ZoneDot>();
            
            Vector3 vector3 = bar.zoneDotContainer.transform.position - new Vector3((float)((double)bar.width / 2.0 - (double)bar.width / (double)(Mathf.Clamp(numSteps, 2, numSteps) - 1) * 1) * bar.zoneDotContainer.lossyScale.x, 0.0f, 0.0f);
            
            rectTransform.localScale = bar.zoneDotContainer.lossyScale;
            
            rectTransform.SetParent(bar.zoneDotContainer, true);
            
            rectTransform.transform.position = vector3;
            
            rectTransform.sizeDelta = new Vector2(10f, (float)num1);
            
            ZoneDot zoneDot = UnityEngine.Object.Instantiate<ZoneDot>(bar.zoneDotPrefab, bar.transform.position, bar.transform.rotation, rectTransform.transform);
            
            
            
            zoneDot.stepNum = 1;
            
            
            
            zoneDotList2.Add(zoneDot);
                 
               
            zoneDot.worldBar = bar;
                
                
            zoneDot.idCtrl = bar.idCtrl;
                
                
            zoneDot.btnCtrl = bar.btnCtrl;


            zoneDot.SetType(ZoneType.Boss);

            bar.currentZoneDots.Add(zoneDot);

            bar.currentZoneSteps.Add(zoneDotList2);
 
            zoneDot.transform.position = vector3 + new Vector3(0.0f, 0.0f, 0.0f);
        }

        //Generates world with Battle with Terrable at the start, and the world dot for eden.
        static void CreatePacifist(WorldBar bar, int numSteps)
        {
            //Lot of stuff the same as boss world generator, but with only 1 zone dot per step.
            List<ZoneDot> zoneDotList1 = new List<ZoneDot>();
            int num1 = 100;
            var num2 = 2;
            RectTransform rectTransform = null;

            for (int index1 = 1; index1 <= num2; ++index1)
            {
                rectTransform = new GameObject("ZoneStep").AddComponent<RectTransform>();
                List<ZoneDot> zoneDotList2 = new List<ZoneDot>();
                Vector3 vector3 = bar.zoneDotContainer.transform.position - new Vector3((float)((double)bar.width / 2.0 - (double)bar.width / (double)(Mathf.Clamp(numSteps, 2, numSteps) - 1) * (double)index1) * bar.zoneDotContainer.lossyScale.x, 0.0f, 0.0f);
                rectTransform.localScale = bar.zoneDotContainer.lossyScale;
                rectTransform.SetParent(bar.zoneDotContainer, true);
                rectTransform.transform.position = vector3;
                rectTransform.sizeDelta = new Vector2(10f, (float)num1);
                ZoneDot zoneDot = UnityEngine.Object.Instantiate<ZoneDot>(bar.zoneDotPrefab, bar.transform.position, bar.transform.rotation, rectTransform.transform);

                zoneDot.stepNum = index1;

                zoneDotList2.Add(zoneDot);

                zoneDot.worldBar = bar;

                zoneDot.idCtrl = bar.idCtrl;

                zoneDot.btnCtrl = bar.btnCtrl;

                zoneDot.transform.name = "ZoneDot - Step: " + (object)index1;


                if (index1 == 1)
                {
                    //Terrable does not spawn in Boss zones, only battle ones (idk why)
                    zoneDot.SetType(ZoneType.Battle);
                }
                else if (index1 == 2)
                {
                    zoneDot.worldName = "Genocide";
                    zoneDot.imageName = "WorldEden";
                    zoneDot.SetType(ZoneType.World);
                }


                zoneDot.verticalSpacing = bar.defaultVerticalSpacing;

                zoneDot.transform.position = vector3 + new Vector3(0.0f, 0.0f, 0.0f);

                bar.currentZoneSteps.Add(zoneDotList2);
                zoneDotList1.Add(zoneDot);
                bar.currentZoneDots.Add(zoneDot);
            }
            bar.currentZoneDots[0].AddNextDot(bar.currentZoneDots[1]);
                
            bar.currentZoneDots[1].previousDots.Add(bar.currentZoneDots[0]);
            
        }

        //Basically the same as pacifist.
        static void CreateNormal(WorldBar bar, int numSteps)
        {
            List<ZoneDot> zoneDotList1 = new List<ZoneDot>();
            int num1 = 100;
            var num2 = 2;
            RectTransform rectTransform = null;

            for (int index1 = 1; index1 <= num2; ++index1)
            {
                rectTransform = new GameObject("ZoneStep").AddComponent<RectTransform>();
                List<ZoneDot> zoneDotList2 = new List<ZoneDot>();
                Vector3 vector3 = bar.zoneDotContainer.transform.position - new Vector3((float)((double)bar.width / 2.0 - (double)bar.width / (double)(Mathf.Clamp(numSteps, 2, numSteps) - 1) * (double)index1) * bar.zoneDotContainer.lossyScale.x, 0.0f, 0.0f);
                rectTransform.localScale = bar.zoneDotContainer.lossyScale;
                rectTransform.SetParent(bar.zoneDotContainer, true);
                rectTransform.transform.position = vector3;
                rectTransform.sizeDelta = new Vector2(10f, (float)num1);
                ZoneDot zoneDot = UnityEngine.Object.Instantiate<ZoneDot>(bar.zoneDotPrefab, bar.transform.position, bar.transform.rotation, rectTransform.transform);

                zoneDot.stepNum = index1;

                zoneDotList2.Add(zoneDot);

                zoneDot.worldBar = bar;

                zoneDot.idCtrl = bar.idCtrl;

                zoneDot.btnCtrl = bar.btnCtrl;

                zoneDot.transform.name = "ZoneDot - Step: " + (object)index1;


                if (index1 == 1)
                {
                    //Wall shows up on Boss zones unlike Terrable.
                    zoneDot.SetType(ZoneType.Boss);
                }
                else if (index1 == 2)
                {
                    zoneDot.worldName = "Genocide";
                    zoneDot.imageName = "WorldWasteland";
                    zoneDot.SetType(ZoneType.World);
                }


                zoneDot.verticalSpacing = bar.defaultVerticalSpacing;

                zoneDot.transform.position = vector3 + new Vector3(0.0f, 0.0f, 0.0f);

                bar.currentZoneSteps.Add(zoneDotList2);
                zoneDotList1.Add(zoneDot);
                bar.currentZoneDots.Add(zoneDot);
            }
            bar.currentZoneDots[0].AddNextDot(bar.currentZoneDots[1]);

            bar.currentZoneDots[1].previousDots.Add(bar.currentZoneDots[0]);

        }

    }

    //Patches going to next zone to make the zone before the world selection go to a world on selecting the worlds.
    [HarmonyPatch]
    [HarmonyPatch(typeof(RunCtrl), nameof(RunCtrl.GoToNextZone))]
    static class BossRush_RunCtrlPatches
    {
        static bool Prefix(RunCtrl __instance, ZoneDot zoneDot)
        {
            var num = 2;
            if (__instance.currentZoneDot.stepNum == num)
            {
                __instance.currentRun.zoneNum = 1000;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(SpawnCtrl), nameof(SpawnCtrl.SpawnZoneC))]
    static class BossRush_SpawnZone
    {

        static void Postfix(SpawnCtrl __instance, ZoneType zoneType)
        {
            //If boss set rewards.
            if (zoneType == ZoneType.Boss)
            {
                S.I.batCtrl.experienceGained = 600;
                //Run couroutine to add money right after boss starts.
                __instance.StartCoroutine(WaitAndMoney());
                S.I.batCtrl.noHitMoneyBonus = 150;
                if (S.I.runCtrl.currentRun.worldName == "Normal")
                {
                    //IDK what this does but im too scared to remove it.
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

    //Don't spawn battle zone if boss rush.
    [HarmonyPatch(typeof(SpawnCtrl), nameof(SpawnCtrl.SpawnBattleZone))]
    static class BossRush_SpawnBattleZone
    {
        static bool Prefix(SpawnCtrl __instance)
        {
            return false;
        }
    }

    //Make allies show up 100% of the time.
    [HarmonyPatch(typeof(BC), nameof(BC._BattleAssists))]
    static class BossRush_BattleAssists
    {
        static bool Prefix(ref int chance)
        {
            chance = 1;
            return true;
        }

    }

    
    //Disables saving.
    [HarmonyPatch]
    static class BossRush_DisableSaving
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Run), nameof(Run.Save))]
        static bool Save(Run __instance)
        {

            S.I.runCtrl.DeleteRun();
            return false;
        }

        //Disables saving 2: Electric boogaloo.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RunCtrl), nameof(RunCtrl.SaveRun))]
        static bool SaveRun(RunCtrl __instance)
        {
            return false;
        }
    }

    public static class BossRush_Misc
    {

        public static IEnumerator WaitAndShop()
        {
            yield return new WaitForSeconds(5.0f);
            S.I.shopCtrl.selfMode = true;
        }
    }

    //Perma Enable Self Shop.
    [HarmonyPatch]
    public static class BossRush_ShopPatch
    {

        [HarmonyPrefix]
        [HarmonyPatch(typeof(BC), nameof(BC.EndBattle))]
        public static bool BattleEndKill(BC __instance)
        {
            Debug.Log("Resetting shop after boss.");
            S.I.StartCoroutine(S.I.shopCtrl.CreateShopOptions());
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Boss), nameof(Boss.Spare))]
        public static bool BattleEndSpare(BC __instance)
        {
            Debug.Log("Resetting shop after boss.");
            S.I.StartCoroutine(S.I.shopCtrl.CreateShopOptions());
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Boss), nameof(Boss._Mercy))]
        public static bool BattleEndMercye(BC __instance)
        {
            Debug.Log("Resetting shop after boss.");
            S.I.StartCoroutine(S.I.shopCtrl.CreateShopOptions());
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RunCtrl), nameof(RunCtrl.StartCampaign))]
        public static void StartPost(RunCtrl __instance, bool loadRun, string seed = "")
        {
            S.I.StartCoroutine(BossRush_Misc.WaitAndShop());
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(ShopCtrl), nameof(ShopCtrl.RefreshButtonMapping))]
        public static void ForceShopPrices(ShopCtrl __instance)
        {
            S.I.shopCtrl.refillCost = 40;
            S.I.shopCtrl.refillInterval = 20;
            __instance.refillCostText.text = (__instance.refillCost + __instance.refillAdd).ToString();
            foreach (var card in __instance.currentShopOptions)
            {
                
                var itemObj = card.itemObj;
                if (itemObj.pactObj == null) {
                
                    if ((UnityEngine.Object)card.cardInner.voteDisplay != (UnityEngine.Object)null)
                        card.cardInner.voteDisplay.gameObject.SetActive(false);
                    int num = Mathf.RoundToInt((float)(__instance.rarityCostbase * 2 + itemObj.rarity * __instance.rarityCostMultiplier));
                    if (itemObj.type == ItemType.Spell)
                        num = Mathf.RoundToInt((float)__instance.rarityCostbase + (float)(itemObj.rarity * __instance.rarityCostMultiplier) * 0.75f);
                    if (__instance.shopZoneType == ZoneType.DarkShop)
                        num = Mathf.RoundToInt((float)(num * 2));
                    if (S.I.runCtrl.currentRun.hellPasses.Contains(12))
                        num = Mathf.RoundToInt((float)num * 1.15f);
                    card.cardInner.priceText.amount.text = string.Format("{0}", (object)num);
                    card.cardInner.priceText.Set(__instance.shopZoneType);
                    card.price = num;
                }
                else card.price = 0;
            }

            
        }
    }

}