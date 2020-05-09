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
using UnityEngine.Assertions.Must;
using UnityEngine.Networking;
using Steamworks;

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
            
            Debug.Log("Boss Rush ? " + BossRush_MainControl.boss_rush);
            //Value set by button on main menu
            if (BossRush_MainControl.boss_rush)
            {
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
                    //Debug.Log("Hi");
                    ZoneDot zoneDot = currentZoneDot;
                    zoneDot.CreateLines();
                }
                //Debug.Log("Hello");
                S.I.shopCtrl.baseRefillCost = 20;

                //Set selection marker for ui.
                __instance.selectionMarker.transform.position = __instance.currentZoneDots[0].transform.position;
                //Dont call original function.
                return false;
            } 
            else
            {
                //Use original
                return true;
            }
        }

        //Generates world with a normal boss.
        static void CreateBossWorld(WorldBar bar, int numSteps)
        {
            //TODO
            List<ZoneDot> zoneDotList1 = new List<ZoneDot>();
            //List of all remaining normal worlds.
            List<string> stringList = new List<string>((IEnumerable<string>)bar.runCtrl.currentRun.unvisitedWorldNames);
            //From original code
            int num1 = 100;
            //If testing final bosses, make it think there are no more remaining worlds.
            if (final_test) stringList.Clear();
            //Variable true if you are before final bosses.
            bool beforeFinal = (stringList.Count() == 0);
            //Variable true if shops are a thing.
            bool shop = !(S.I.runCtrl.currentRun.shopkeeperKilled || S.I.runCtrl.currentRun.beingID == "Shopkeeper");
            //Number of zone steps to generate. Add two more if before final world.
            int num2 =  beforeFinal ? 6 : 4;
            //Per step list of zone dots.
            List<ZoneDot> zoneDotList3 = new List<ZoneDot>();
            //
            RectTransform rectTransform = null;

            //Loop through each step (collumn)
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

                //How many dots per collumn.
                var max = Math.Max(Math.Min(stringList.Count, 3), 1);
                //If not the last one, default to 1
                if (index1 < num2)
                {
                    max = 1;
                }
                //If shop exists and its a loot spot, there should be 2
                if (((index1 == 2 && !beforeFinal) || index1 == 5) && shop)
                {
                    max = 2;
                }
                //If testing final bosses, have three world options anyway.
                if (final_test && index1==num2)
                {
                    max = 3;
                }
                for (int index2 = 1; index2 <= max; ++index2)
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
                    zoneDot.transform.name = "ZoneDot - Step: " + (object)index1 + " - " + (object)index2;
                    //Use world selection spacing for everything.
                    zoneDot.verticalSpacing = bar.defaultVerticalSpacing;
                    zoneDot.verticalSpacing += 7f;
                    //Decide what row comes next
                    int index3 = bar.runCtrl.NextWorldRand(0, stringList.Count);
                    //Figure out what type to make the zone.
                    if (index1 == 1 || (beforeFinal&&index1==4)) 
                    {
                        zoneDot.SetType(ZoneType.Campsite);
                    }
                    //Checks if it should be a shop.
                    else if (index2 == 1 && shop && ((!beforeFinal && index1 == 2) || (beforeFinal && index1 == 5))) 
                    {
                        zoneDot.SetType(ZoneType.Shop);
                    }
                    //Put battles for loot on the second dot in collumns with shop, or replace shop if shop doesnt exist.
                    else if (((index1 == 2 || index1==5) && index2 == 2) || ((!shop || beforeFinal) && (index1==2 || index1==5) && index2 == 1))
                    {
                        zoneDot.SetType(ZoneType.Battle);
                    }
                    //Boss on 3rd, almost guarenteed 1 and 2 will be taken.
                    else if (index1 <= 3)
                    {
                        zoneDot.SetType(ZoneType.Boss);

                        //Safety (Might be necessary to go to next world)
                        if (stringList.Count != 0)
                        {
                            zoneDot.worldName = stringList[Math.Min(index3, stringList.Count - 1)];
                        }
                        else
                        {
                            //Decide final world, if final test decide based on # in collumn.
                            if ((!final_test && bar.runCtrl.savedBossKills >= 7) ||  (final_test && index2 == 1))
                            {
                                zoneDot.worldName = "Genocide";
                            }
                            else if ((!final_test && bar.runCtrl.savedBossKills >= 1) || (final_test && index2 == 2))
                            {
                                zoneDot.worldName = "Normal";
                            }
                            else
                            {
                                zoneDot.worldName = "Pacifist";
                            }
                        }

                        //Set world stuff.
                        zoneDot.world = bar.runCtrl.worlds[zoneDot.worldName];
                        zoneDot.imageName = zoneDot.world.iconName;
                    }
                    //World selection dots.
                    else
                    {
                        //Use random next boss world.
                        if (stringList.Count > 0)
                        {
                            zoneDot.worldName = stringList[Math.Min(index3, stringList.Count - 1)];
                            zoneDot.world = bar.runCtrl.worlds[zoneDot.worldName];
                            zoneDot.imageName = zoneDot.world.iconName;
                            stringList.Remove(stringList[Math.Min(index3, stringList.Count - 1)]);
                        }
                        //Final world
                        else
                        {
                            //bar.runCtrl.savedBossKills is the correct variable for this type of thing.
                            if ((!final_test && bar.runCtrl.savedBossKills >= 7) || (final_test && index2 == 1))
                            {
                                zoneDot.worldName = "Genocide";
                                zoneDot.imageName = "WorldWasteland";
                            }
                            else if ((!final_test && bar.runCtrl.savedBossKills >= 1) || (final_test && index2 == 2))
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
                    }
                    //Set zone dot position based on indices.
                    zoneDot.transform.position = vector3 + new Vector3(0.0f, ((float)(max - 1) - (float)(index2 - 1)) * zoneDot.verticalSpacing * bar.rect.localScale.y, 0.0f);
                    //Add to temp step list.
                    zoneDotList3.Add(zoneDot);
                }
                //Add step
                bar.currentZoneSteps.Add(zoneDotList2);
                //Add to temp total list of dots.
                zoneDotList1.AddRange(zoneDotList3);
                //Add to WorldBar's list of dots.
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
                for (int j = 0; j < bar.currentZoneSteps[i].Count; j++)
                {
                    if (i == bar.currentZoneSteps.Count() - 1)
                    {
                        foreach (ZoneDot dot in bar.currentZoneSteps[i - 1])
                        {
                            bar.currentZoneSteps[i][j].previousDots.Add(dot);
                        }
                    }
                    else if (i == bar.currentZoneSteps.Count() - 2)
                    {
                        bar.currentZoneSteps[i][j].previousDots.Add(bar.currentZoneSteps[i - 1][0]);
                    } 
                    else if (i == 1 || i==4)
                    {
                        foreach (ZoneDot dot in bar.currentZoneSteps[i-1])
                        {
                            bar.currentZoneSteps[i][j].previousDots.Add(dot);
                        }
                    }
                    else if (i!=0)
                    {
                        bar.currentZoneSteps[i][j].previousDots.Add(bar.currentZoneSteps[i - 1][0]);
                    }
                    if (i + 1 < bar.currentZoneSteps.Count())
                    {
                        foreach (ZoneDot dot in bar.currentZoneSteps[i + 1])
                        {
                            bar.currentZoneSteps[i][j].AddNextDot(dot);
                        }
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
            if (!BossRush_MainControl.boss_rush) return true;
            bool beforeFinal = (S.I.runCtrl.currentRun.unvisitedWorldNames.Count() == 0);
            if (BossRush_ZoningPatches.final_test) beforeFinal = true;
            var num = beforeFinal ? 5 : 3;
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
            //If not boss rush dont do anything.
            if (!BossRush_MainControl.boss_rush) return;
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
            else if (zoneType == ZoneType.Battle && S.I.runCtrl.currentRun.worldName != "Pacifist")
            {
                //Spawn sera piles through couroutine.
                __instance.StartCoroutine(WaitAndSera());
            } 
            else if (zoneType == ZoneType.Campsite)
            {
                var list = S.I.batCtrl.currentPlayer.pactObjs.ToArray();
                //Remove pacts when going to a campfire.
                foreach (var pactObject in list)
                {
                    if (!pactObject.hellPass) pactObject.FinishPact();
                }
                
                //Play correct idle environment music when going to a campfire. Fixing problems with world before final boss.
                var boss_to_audio = new Dictionary<string, string>()
                {
                    {"Gunner","Fire"}, {"Saffron","Fire"}, {"Shiso","Forest"}, {"Hazel","Forest"}, {"Reva","Ruins"}, {"Terra","Ruins"}, {"Selicy","Ice"}, {"Violette","Ice"} 
                };
                var AllAudioClips = S.I.muCtrl.idleEnvironments;
                Debug.Log(S.I.runCtrl.currentRun.worldName);
                Debug.Log(boss_to_audio[S.I.runCtrl.currentRun.worldName]);
                S.I.muCtrl.TransitionTo(AllAudioClips[boss_to_audio[S.I.runCtrl.currentRun.worldName]]);

            }
        }

        static IEnumerator WaitAndMoney()
        {
            yield return new WaitForSeconds(0.5f);
            S.I.shopCtrl.ModifySera(250);
        }

        static IEnumerator WaitAndSera()
        {
            yield return new WaitForSeconds(0.0f);
            S.I.spCtrl.SpawnSerapiles(true);
        }

    }

    //Don't spawn battle zone if boss rush.
    [HarmonyPatch(typeof(SpawnCtrl), nameof(SpawnCtrl.SpawnBattleZone))]
    static class BossRush_SpawnBattleZone
    {
        static bool Prefix(SpawnCtrl __instance)
        {
            return !BossRush_MainControl.boss_rush;
        }
    }

    //Make allies show up 100% of the time.
    [HarmonyPatch(typeof(BC), nameof(BC._BattleAssists))]
    static class BossRush_BattleAssists
    {
        static bool Prefix(ref int chance)
        {
            if (BossRush_MainControl.boss_rush) chance = 1;
            return true;
        }

    }

    //Wait shit TODO
    [HarmonyPatch(typeof(MusicCtrl), nameof(MusicCtrl.Play))]
    static class BossRush_Music
    {
        static bool Prefix(MusicCtrl __instance, AudioClip audioClip, bool loop)
        {
            return BossRush_MainControl.boss_rush;
        }
    }

    //Button to start boss rush.
    public class BossRushButton : NavButton
    {
        static Sprite sprite = null;

        protected override void Awake()
        {
            //Debug.Log("Awake");
            S.I.mainCtrl.StartCoroutine(LoadSprite());
            base.Awake();
        }

        //Highlight Button
        public override void Focus(int playerNum = 0)
        {
            var image = gameObject.GetComponent<Image>();
            if (!sprite) S.I.mainCtrl.StartCoroutine(LoadSprite());
            if (image)
            {
                if (sprite) image.sprite = sprite;
            }
            else
            {
                Debug.LogError("Game Object does not have image needed for Boss Rush Button highlight state.");
            }
            base.Focus(playerNum);
        }

        //Stop highlighting button.
        public override void UnFocus()
        {
            gameObject.GetComponent<Image>().sprite = AssetBundleManager.LoadAsset<Sprite>("sprites_ui", "Boss", out string err);
            base.UnFocus();
        }

        //On button push.
        public override void OnAcceptPress()
        {
            S.I.PlayOnce(this.btnCtrl.pierceSound, false);
            //Open singleplayer menu.
            S.I.mainCtrl.OpenPanelSingpleplayer(S.I.heCtrl);
            BossRush_MainControl.boss_rush = true;
        }

        //Load highlighted sprite.
        public IEnumerator LoadSprite()
        {
            Debug.Log("Loading sprite");
            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string url = Path.Combine(assemblyFolder, "highlighted.png");
            var request = UnityWebRequestTexture.GetTexture(url);
            yield return request.SendWebRequest();
            if (!request.isHttpError && !request.isNetworkError)
            {
                var tex = DownloadHandlerTexture.GetContent(request);
                sprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                if (S.I.btnCtrl.focusedButton == this)
                {
                    var image = gameObject.GetComponent<Image>();
                    if (!sprite) S.I.mainCtrl.StartCoroutine(LoadSprite());
                    if (image)
                    {
                        if (sprite) image.sprite = sprite;
                    }
                    else
                    {
                        Debug.LogError("Game Object does not have image needed for Boss Rush Button highlight state.");
                    }
                }
            }
            else
            {
                Debug.LogErrorFormat("error request [{0}, {1}]", url, request.error);
            }
            request.Dispose();
        }

        //Set to correct position.
        public void SetPosition(bool continueExists)
        {
            gameObject.GetComponent<RectTransform>().transform.localPosition = continueExists ? new Vector2(-Screen.width/50, 42*Screen.height/90) : new Vector2(-Screen.width/50, 42*Screen.height/90);
        }

    }

    public static class Util
    {
        //Create boss rush button.
        public static BossRushButton CreateBossRushUI(ref Canvas canvas)
        {
            if (canvas != null)
                GameObject.Destroy(canvas.gameObject);

            Debug.Log("Creating canvas");
            var canvasGo = new GameObject("Canvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.sortingOrder = int.MaxValue;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<GraphicRaycaster>();
            canvasGo.transform.SetParent(S.I.mainCtrl.mainMenuLeftButtonGrid);

            var buttonGo = new GameObject("button").AddComponent<RectTransform>();
            buttonGo.transform.SetParent(canvasGo.transform);
            buttonGo.transform.localScale = Vector3.one;
            buttonGo.sizeDelta = new Vector2(50, 50);
            buttonGo.gameObject.AddComponent<Image>().sprite = AssetBundleManager.LoadAsset<Sprite>("sprites_ui", "Boss", out string err);
            canvasGo.AddComponent<Animator>();
            var button = buttonGo.gameObject.AddComponent<BossRushButton>();

            return button;
        }
    }

    [HarmonyPatch]
    static class BossRush_MainControl
    {
        public static bool boss_rush = false;
        public static BossRushButton button = null;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainCtrl),nameof(MainCtrl.Open))]
        static void Open(MainCtrl __instance)
        {
            if (!button)
            {
                Canvas canvas = null;
                button = Util.CreateBossRushUI(ref canvas);

                //Set main menu navigation.
                var nav = button;
                nav.up = __instance.quitButton;
                nav.down = __instance.runCtrl.LoadedRunExists() ? __instance.continueButton : __instance.soloButton;
                nav.right = __instance.modsButton;
                __instance.quitButton.down = nav;
                nav.SetPosition(__instance.runCtrl.LoadedRunExists());
                if (__instance.runCtrl.LoadedRunExists())
                {
                    __instance.continueButton.up = nav;
                }
                else
                {
                    __instance.soloButton.up = nav;
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MainCtrl), nameof(MainCtrl.OpenPanel))]
        static bool OpenPanel(MainCtrl __instance)
        {
            //Turn off boss rush.
            if (S.I.btnCtrl.focusedButton != button) boss_rush = false;
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
            if (BossRush_MainControl.boss_rush)
            {
                S.I.runCtrl.DeleteRun();
                var nav = BossRush_MainControl.button;
                S.I.mainCtrl.soloButton.up = nav;
                nav.down = S.I.mainCtrl.soloButton;
            }
            else
            {
                var nav = BossRush_MainControl.button;
                S.I.mainCtrl.continueButton.up = nav;
                nav.down = S.I.mainCtrl.continueButton;
            }
            return !BossRush_MainControl.boss_rush;
        }

        //Disables saving 2: Electric boogaloo.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RunCtrl), nameof(RunCtrl.SaveRun))]
        static bool SaveRun(RunCtrl __instance)
        {
            if (BossRush_MainControl.boss_rush)
            {
                __instance.DeleteRun();
                var nav = BossRush_MainControl.button;
                S.I.mainCtrl.soloButton.up = nav;
                nav.down = S.I.mainCtrl.soloButton;
                nav.SetPosition(false);
            }
            else
            {
                var nav = BossRush_MainControl.button;
                S.I.mainCtrl.continueButton.up = nav;
                S.I.mainCtrl.soloButton.up = S.I.mainCtrl.continueButton;
                nav.down = S.I.mainCtrl.continueButton;
                nav.SetPosition(true);
            }
            return !BossRush_MainControl.boss_rush;
        }

        //Remove continue button stuff.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RunCtrl), nameof(RunCtrl.CreateNewRun))]
        static void CreateNewRun(RunCtrl __instance, int zoneNum, int worldTierNum, bool campaign, string seed)
        {
            var nav = BossRush_MainControl.button;
            S.I.mainCtrl.soloButton.up = nav;
            nav.down = S.I.mainCtrl.soloButton;
            nav.SetPosition(false);
        }
    }
}