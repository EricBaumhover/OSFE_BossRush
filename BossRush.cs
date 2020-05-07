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
        public static bool final_test = false;

        static bool Prefix(WorldBar __instance, int numSteps)
        {
            
            Debug.Log("Boss Rush ? " + BossRush_MainControl.boss_rush);
            if (BossRush_MainControl.boss_rush)
            {
                S.I.muCtrl.StopIntroLoop();
                foreach (Component component in __instance.zoneDotContainer)
                    UnityEngine.Object.Destroy((UnityEngine.Object)component.gameObject);
                __instance.currentZoneDots.Clear();
                __instance.currentZoneSteps.Clear();
                if (__instance.btnCtrl.hideUICounter < 1)
                    __instance.detailPanel.gameObject.SetActive(true);
                //__instance.ResetZoneVars();
                if (__instance.runCtrl.currentWorld.nameString == "Genocide")
                {
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
                __instance.runCtrl.currentRun.lastWorldGenOrigin = __instance.runCtrl.currentRun.currentWorldGen;

                foreach (ZoneDot currentZoneDot in __instance.currentZoneDots)
                {
                    //Debug.Log("Hi");
                    ZoneDot zoneDot = currentZoneDot;
                    zoneDot.CreateLines();
                }
                //Debug.Log("Hello");
                S.I.shopCtrl.baseRefillCost = 20;

                __instance.selectionMarker.transform.position = __instance.currentZoneDots[0].transform.position;
                return false;
            } 
            else
            {
                return true;
            }
        }

        static void CreateBossWorld(WorldBar bar, int numSteps)
        {
            List<ZoneDot> zoneDotList1 = new List<ZoneDot>();
            List<string> stringList = new List<string>((IEnumerable<string>)bar.runCtrl.currentRun.unvisitedWorldNames);
            int num1 = 100;
            if (final_test) stringList.Clear();
            bool beforeFinal = (stringList.Count() == 0);
            bool shop = !(S.I.runCtrl.currentRun.shopkeeperKilled || S.I.runCtrl.currentRun.beingID == "Shopkeeper");
            int num2 =  beforeFinal ? 6 : 4;
            List<ZoneDot> zoneDotList3 = new List<ZoneDot>();
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

                var max = Math.Max(Math.Min(stringList.Count, 3), 1);
                if (index1 < num2)
                {
                    max = 1;
                }
                if (((index1 == 2 && !beforeFinal) || index1 == 5) && shop)
                {
                    max = 2;
                }
                if (final_test && index1==num2)
                {
                    max = 3;
                }
                for (int index2 = 1; index2 <= max; ++index2)
                {
                    //Debug.Log(index1);
                    ZoneDot zoneDot = UnityEngine.Object.Instantiate<ZoneDot>(bar.zoneDotPrefab, bar.transform.position, bar.transform.rotation, rectTransform.transform);
                    zoneDot.stepNum = index1;
                    zoneDotList2.Add(zoneDot);
                    zoneDot.worldBar = bar;
                    zoneDot.idCtrl = bar.idCtrl;
                    zoneDot.btnCtrl = bar.btnCtrl;
                    zoneDot.transform.name = "ZoneDot - Step: " + (object)index1 + " - " + (object)index2;
                    zoneDot.verticalSpacing = bar.defaultVerticalSpacing;
                    //if (index1 == num2)
                    zoneDot.verticalSpacing += 7f;
                    int index3 = bar.runCtrl.NextWorldRand(0, stringList.Count);
                    if (index1 == 1 || (beforeFinal&&index1==4))
                    {
                        zoneDot.SetType(ZoneType.Campsite);
                    }
                    else if (index2 == 1 && shop && ((!beforeFinal && index1 == 2) || (beforeFinal && index1 == 5)))
                    {
                        zoneDot.SetType(ZoneType.Shop);
                    }
                    else if (((index1 == 2 || index1==5) && index2 == 2) || ((!shop || beforeFinal) && (index1==2 || index1==5) && index2 == 1)) 
                    {
                        zoneDot.SetType(ZoneType.Battle);
                    }
                    else if (index1 <= 3)
                    {
                        //Debug.Log("Ey");
                        zoneDot.SetType(ZoneType.Boss);

                        if (stringList.Count != 0)
                        {
                            zoneDot.worldName = stringList[Math.Min(index3, stringList.Count - 1)];
                        }
                        else
                        {
                            if ((!final_test && bar.runCtrl.savedBossKills >= 7) ||  (final_test && index2 == 1))
                            {
                                zoneDot.worldName = "Genocide";
                            }
                            else if ((!final_test && bar.runCtrl.savedBossKills >= 1) || (final_test && index2 == 1))
                            {
                                zoneDot.worldName = "Normal";
                            }
                            else
                            {
                                zoneDot.worldName = "Pacifist";
                            }
                        }

                        zoneDot.world = bar.runCtrl.worlds[zoneDot.worldName];
                        zoneDot.imageName = zoneDot.world.iconName;
                    }
                    else
                    {
                        if (stringList.Count > 0)
                        {
                            //Debug.Log("Yo");
                            zoneDot.worldName = stringList[Math.Min(index3, stringList.Count - 1)];
                            zoneDot.world = bar.runCtrl.worlds[zoneDot.worldName];
                            zoneDot.imageName = zoneDot.world.iconName;
                            stringList.Remove(stringList[Math.Min(index3, stringList.Count - 1)]);
                        }
                        else
                        {
                            //Debug.Log("Dog");
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
                        //Debug.Log("Hey");
                        zoneDot.world = bar.runCtrl.worlds[zoneDot.worldName];
                        zoneDot.SetType(ZoneType.World);
                    }
                    zoneDot.transform.position = vector3 + new Vector3(0.0f, ((float)(max - 1) - (float)(index2 - 1)) * zoneDot.verticalSpacing * bar.rect.localScale.y, 0.0f);
                    zoneDotList3.Add(zoneDot);
                }
                bar.currentZoneSteps.Add(zoneDotList2);
                zoneDotList1.AddRange(zoneDotList3);
                foreach (ZoneDot dot in zoneDotList3)
                {
                    bar.currentZoneDots.Add(dot);
                }
                zoneDotList3.Clear();
            }
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
        static void CreatePacifist(WorldBar bar, int numSteps)
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

        static void Postfix(SpawnCtrl __instance, ZoneType zoneType)
        {
            if (!BossRush_MainControl.boss_rush) return;
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
            else if (zoneType == ZoneType.Battle && S.I.runCtrl.currentRun.worldName != "Pacifist")
            {
                __instance.StartCoroutine(WaitAndSera());
            } 
            else if (zoneType == ZoneType.Campsite)
            {
                var list = S.I.batCtrl.currentPlayer.pactObjs.ToArray();
                foreach (var pactObject in list)
                {
                    if (!pactObject.hellPass) pactObject.FinishPact();
                }
                //S.I.deCtrl.CreatePlayerItems(S.I.batCtrl.currentPlayer);
                
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

    [HarmonyPatch(typeof(SpawnCtrl), nameof(SpawnCtrl.SpawnBattleZone))]
    static class BossRush_SpawnBattleZone
    {
        static bool Prefix(SpawnCtrl __instance)
        {
            return !BossRush_MainControl.boss_rush;
        }
    }

    [HarmonyPatch(typeof(BC), nameof(BC._BattleAssists))]
    static class BossRush_BattleAssists
    {
        static bool Prefix(ref int chance)
        {
            if (BossRush_MainControl.boss_rush) chance = 1;
            return true;
        }
    }

    /*[HarmonyPatch(typeof(Boss), nameof(Boss.Spare))]
    static class BossRush_Spare
    {

        public static void Postfix(Boss __instance, ZoneDot nextZoneDot)
        {
            Debug.Log("Stopping music after sparing boss.");
            S.I.muCtrl.Stop();
            S.I.muCtrl.audioSource.clip = null;
        }
    }*/

    [HarmonyPatch(typeof(MusicCtrl), nameof(MusicCtrl.Play))]
    static class BossRush_Music
    {
        static bool Prefix(MusicCtrl __instance, AudioClip audioClip, bool loop)
        {
            return BossRush_MainControl.boss_rush;
        }
    }

    public class BossRushButton : NavButton
    {
        static Sprite sprite = null;

        protected override void Awake()
        {
            Debug.Log("Awake");
            S.I.mainCtrl.StartCoroutine(LoadSprite());
            base.Awake();
        }

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

        public override void UnFocus()
        {
            gameObject.GetComponent<Image>().sprite = AssetBundleManager.LoadAsset<Sprite>("sprites_ui", "Boss", out string err);
        }
        public override void OnAcceptPress()
        {
            S.I.PlayOnce(this.btnCtrl.pierceSound, false);
            S.I.mainCtrl.OpenPanelSingpleplayer(S.I.heCtrl);
            BossRush_MainControl.boss_rush = true;
        }


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

        public void SetPosition(bool continueExists)
        {
            gameObject.GetComponent<RectTransform>().transform.localPosition = continueExists ? new Vector2(-Screen.width/50, 42*Screen.height/90) : new Vector2(-Screen.width/50, 42*Screen.height/90);
        }

    }

    public static class Util
    {
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
            if (S.I.btnCtrl.focusedButton != button) boss_rush = false;
            return true;
        }

    }

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