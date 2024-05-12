﻿using ArchipelagoULTRAKILL.Structures;
using ArchipelagoULTRAKILL.Components;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using TMPro;
using System.Security.Policy;
using System.Diagnostics;

namespace ArchipelagoULTRAKILL
{
    public class UIManager : MonoBehaviour
    {
        public static AssetBundle bundle = AssetBundle.LoadFromMemory(Properties.Resources.trpg_archipelago);

        public static TMP_FontAsset font;
        public static TextMeshProUGUI log;
        public static TextMeshProUGUI logBlack;
        public static int lines = 5;

        public static GameObject canvas;
        public static GameObject chapterSelect;
        public static TextMeshProUGUI actStats;
        public static Dictionary<string, GameObject> chapters = new Dictionary<string, GameObject>();
        public static Dictionary<string, GameObject> layers = new Dictionary<string, GameObject>();
        public static Dictionary<string, GameObject> levels = new Dictionary<string, GameObject>();
        public static Dictionary<string, GameObject> secrets = new Dictionary<string, GameObject>();

        public static TextMeshProUGUI menuText;
        public static GameObject menuIcon;
        public static TextMeshProUGUI goalCount;

        public static bool createdSkullIcons = false;
        public static bool createdSwitchIcons = false;

        public static GameObject hud;
        public static GameObject popupCanvas;
        public static TextMeshProUGUI popupText;
        public static GameObject popupImage;
        public static bool displayingMessage = false;

        public DeathLinkMessage deathLinkMessage = null;

        public void Update()
        {
            if (LocationManager.itemQueue.Count > 0)
            {
                QueuedItem qItem = LocationManager.itemQueue[0];
                LocationManager.GetUKItem(qItem.item, qItem.sendingPlayer, qItem.silent);
                LocationManager.itemQueue.RemoveAt(0);
            }
        }

        public void CreateLogObject()
        {
            StartCoroutine(VersionChecker.CheckVersion());

            GameConsole.Console.Instance.RegisterCommand(new Commands.Connect());
            GameConsole.Console.Instance.RegisterCommand(new Commands.Disconnect());
            GameConsole.Console.Instance.RegisterCommand(new Commands.Say());

            GameObject go = new GameObject();
            go.name = "Log";
            go.transform.parent = Core.obj.transform;
            go.transform.localPosition = new Vector3(0, 0, 0);

            Core.obj.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            Core.obj.GetComponent<Canvas>().sortingOrder = 256;

            log = go.AddComponent<TextMeshProUGUI>();
            log.font = font;
            log.fontSize = ConfigManager.logFontSize.value;
            log.alignment = TextAlignmentOptions.BottomGeoAligned;
            log.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width - 10, Screen.height - 10);

            logBlack = GameObject.Instantiate(log.gameObject, log.transform.parent).GetComponent<TextMeshProUGUI>();
            logBlack.gameObject.name = "Shadow";
            logBlack.color = new Color(0, 0, 0, 0.7f);
            logBlack.overrideColorTags = true;
            logBlack.transform.localPosition = new Vector3(0, -1.5f, 0);
            logBlack.transform.SetAsFirstSibling();
        }

        public static void SetLogText(string text)
        {
            if (log != null) log.text = text;
            if (logBlack != null) logBlack.text = text;
        }

        public static void AdjustLogBounds()
        {
            if (PrefsManager.Instance.GetInt("hudType") >= 2 && Core.IsInLevel)
            {
                log.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width - 10, (float)Math.Round(Screen.height * 0.77f));
                logBlack.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width - 10, (float)Math.Round(Screen.height * 0.77f));
            }
            else
            {
                log.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width - 10, Screen.height - 10);
                logBlack.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width - 10, Screen.height - 10);
            }
        }

        public static void FindMenuObjects()
        {
            canvas = GameObject.Find("/Canvas");

            foreach (ChapterSelectButton component in canvas.GetComponentsInChildren<ChapterSelectButton>(true))
            {
                //logger.LogInfo(component.gameObject.name);
                switch (component.gameObject.name)
                {
                    case "Prelude":
                        chapters["prelude"] = component.gameObject;
                        break;
                    case "Act I":
                        chapters["act1"] = component.gameObject;
                        break;
                    case "Act II":
                        chapters["act2"] = component.gameObject;
                        break;
                    case "Act III":
                        chapters["act3"] = component.gameObject;
                        break;
                    case "Prime":
                        chapters["prime"] = component.gameObject;
                        break;
                    default:
                        break;
                }
            }

            chapterSelect = chapters["prelude"].gameObject.transform.parent.gameObject;
            chapterSelect.AddComponent<ChapterSelectState>();

            actStats = GameObject.Instantiate(chapterSelect.transform.Find("Prelude").Find("Name"), chapterSelect.transform).GetComponent<TextMeshProUGUI>();
            Vector3 rankPos = chapterSelect.transform.Find("Prelude").Find("RankPanel").transform.position;
            actStats.transform.position = new Vector3(rankPos.x + 77, rankPos.y, rankPos.z);
            actStats.overflowMode = TextOverflowModes.Overflow;
            actStats.alignment = TextAlignmentOptions.TopLeft;
            actStats.lineSpacing = 1.2f;
            actStats.gameObject.SetActive(false);

            chapterSelect.transform.Find("Prelude").gameObject.AddComponent<ActStats>().Init(1, 5);
            chapterSelect.transform.Find("Act I").gameObject.AddComponent<ActStats>().Init(6, 15);
            chapterSelect.transform.Find("Act II").gameObject.AddComponent<ActStats>().Init(16, 25);
            chapterSelect.transform.Find("Act III").gameObject.AddComponent<ActStats>().Init(26, 29);
            chapterSelect.transform.Find("Prime").gameObject.AddComponent<ActStats>().Init(666, 667, true);

            foreach (LayerSelect component in canvas.GetComponentsInChildren<LayerSelect>(true))
            {
                switch (component.gameObject.name)
                {
                    case "Overture":
                        layers["layer0"] = component.gameObject;
                        break;
                    case "Layer 1 Limbo":
                        if (component.gameObject.transform.parent.transform.parent.transform.parent.name == "Level Select (Prelude)") break;
                        else
                        {
                            layers["layer1"] = component.gameObject;
                            break;
                        }
                    case "Layer 2 Lust":
                        layers["layer2"] = component.gameObject;
                        break;
                    case "Layer 3 Gluttony":
                        layers["layer3"] = component.gameObject;
                        break;
                    case "Layer 4 Greed":
                        layers["layer4"] = component.gameObject;
                        break;
                    case "Layer 5 Wrath":
                        layers["layer5"] = component.gameObject;
                        break;
                    case "Layer 6 Heresy":
                        layers["layer6"] = component.gameObject;
                        break;
                    case "Layer 7 Violence":
                        layers["layer7"] = component.gameObject;
                        break;
                    case "Prime Sanctums":
                        layers["layerP"] = component.gameObject;
                        break;
                    default:
                        break;
                }
            }

            foreach (LevelSelectPanel component in canvas.GetComponentsInChildren<LevelSelectPanel>(true))
            {
                //logger.LogInfo(component.gameObject.transform.parent.name + ", " + component.gameObject.name);
                switch (component.gameObject.name)
                {
                    case "0-1 Panel":
                        levels["0-1"] = component.gameObject;
                        break;
                    case "0-2 Panel":
                        levels["0-2"] = component.gameObject;
                        break;
                    case "0-3 Panel":
                        levels["0-3"] = component.gameObject;
                        break;
                    case "0-4 Panel":
                        levels["0-4"] = component.gameObject;
                        break;
                    case "0-5 Panel":
                        levels["0-5"] = component.gameObject;
                        break;
                    case "1-1 Panel":
                        switch (component.gameObject.transform.parent.parent.name)
                        {
                            case "Layer 1 Limbo":
                                if (!levels.ContainsKey("1-1")) levels["1-1"] = component.gameObject;
                                break;
                            case "Layer 6 Heresy":
                                levels["6-1"] = component.gameObject;
                                break;
                            default:
                                break;
                        }
                        break;
                    case "1-2 Panel":
                        switch (component.gameObject.transform.parent.parent.name)
                        {
                            case "Layer 1 Limbo":
                                if (!levels.ContainsKey("1-2")) levels["1-2"] = component.gameObject;
                                break;
                            case "Layer 6 Heresy":
                                levels["6-2"] = component.gameObject;
                                break;
                            default:
                                break;
                        }
                        break;
                    case "1-3 Panel":
                        levels["1-3"] = component.gameObject;
                        break;
                    case "1-4 Panel":
                        levels["1-4"] = component.gameObject;
                        break;
                    case "2-1 Panel":
                        levels["2-1"] = component.gameObject;
                        break;
                    case "2-2 Panel":
                        levels["2-2"] = component.gameObject;
                        break;
                    case "2-3 Panel":
                        levels["2-3"] = component.gameObject;
                        break;
                    case "2-4 Panel":
                        levels["2-4"] = component.gameObject;
                        break;
                    case "3-1 Panel":
                        levels["3-1"] = component.gameObject;
                        break;
                    case "3-2 Panel":
                        levels["3-2"] = component.gameObject;
                        break;
                    case "4-1 Panel":
                        levels["4-1"] = component.gameObject;
                        break;
                    case "4-2 Panel":
                        levels["4-2"] = component.gameObject;
                        break;
                    case "4-3 Panel":
                        levels["4-3"] = component.gameObject;
                        break;
                    case "4-4 Panel":
                        levels["4-4"] = component.gameObject;
                        break;
                    case "5-1 Panel":
                        levels["5-1"] = component.gameObject;
                        break;
                    case "5-2 Panel":
                        levels["5-2"] = component.gameObject;
                        break;
                    case "5-3 Panel":
                        levels["5-3"] = component.gameObject;
                        break;
                    case "5-4 Panel":
                        levels["5-4"] = component.gameObject;
                        break;
                    case "7-1 Panel":
                        levels["7-1"] = component.gameObject;
                        break;
                    case "7-2 Panel":
                        levels["7-2"] = component.gameObject;
                        break;
                    case "7-3 Panel":
                        levels["7-3"] = component.gameObject;
                        break;
                    case "7-4 Panel":
                        levels["7-4"] = component.gameObject;
                        break;
                    case "P-1 Panel":
                        levels["P-1"] = component.gameObject;
                        break;
                    case "P-2 Panel":
                        levels["P-2"] = component.gameObject;
                        break;
                    case "P-3 Panel":
                        levels["P-3"] = component.gameObject;
                        break;
                    default:
                        break;
                }
            }

            foreach (SecretMissionPanel component in canvas.GetComponentsInChildren<SecretMissionPanel>(true))
            {
                //logger.LogInfo(component.gameObject.transform.parent.name + ", " + component.gameObject.name);
                switch (component.gameObject.transform.parent.parent.name)
                {
                    case "Overture":
                        secrets["0-S"] = component.gameObject;
                        break;
                    case "Layer 1 Limbo":
                        if (!secrets.ContainsKey("1-S")) secrets["1-S"] = component.gameObject;
                        break;
                    case "Layer 2 Lust":
                        secrets["2-S"] = component.gameObject;
                        break;
                    case "Layer 4 Greed":
                        secrets["4-S"] = component.gameObject;
                        break;
                    case "Layer 5 Wrath":
                        secrets["5-S"] = component.gameObject;
                        break;
                    case "Layer 7 Violence":
                        secrets["7-S"] = component.gameObject;
                        break;
                    default:
                        break;
                }
            }

            CreateMenuUI();
        }
        
        public static void CreateMenuUI()
        {
            foreach (TextMeshProUGUI tmp in canvas.GetComponentsInChildren<TextMeshProUGUI>())
            {
                if (tmp.text == "-- EARLY ACCESS --")
                {
                    menuText = Instantiate(tmp.gameObject, tmp.gameObject.transform.parent).GetComponent<TextMeshProUGUI>();
                    break;
                }
            }
            menuText.gameObject.name = "Archipelago Text";
            menuText.transform.localPosition = new Vector3(-6, -148, 0);
            menuText.alignment = TextAlignmentOptions.TopRight;
            menuText.fontSize = 24;
            string totalLocations = (LocationManager.locations.Count == 0) ? "?" : LocationManager.locations.Count.ToString();
            if (Core.DataExists()) menuText.text = "Archipelago\n" + Core.PluginVersion + "\nSlot " + (GameProgressSaver.currentSlot + 1) + "\n" + Core.data.@checked.Count + "/" + totalLocations;
            else if (Multiworld.HintMode) menuText.text = "Archipelago\n" + Core.PluginVersion + "\nSlot " + (GameProgressSaver.currentSlot + 1) + "\n<color=yellow>Hint Mode</color>";
            else menuText.text = "Archipelago\n" + Core.PluginVersion + "\nSlot " + (GameProgressSaver.currentSlot + 1) + "\nNo data.";
            font = menuText.font;
            menuIcon = new GameObject();
            menuIcon.gameObject.name = "Archipelago Logo";
            menuIcon.transform.SetParent(menuText.transform.parent.gameObject.transform);
            menuIcon.AddComponent<Image>().sprite = bundle.LoadAsset<Sprite>("assets/archipelago.png");
            menuIcon.transform.localPosition = new Vector3(600, 40, 0);
            menuIcon.transform.localScale = new Vector3(0.7f, 0.7f, 1);

            if (Core.DataExists() && Core.data.completedLevels.Count < Core.data.goalRequirement) CreateGoalCounter();
        }

        public static void CreateGoalCounter()
        {
            GameObject go = new GameObject();
            go.transform.SetParent(levels[Core.data.goal].transform);
            if (Core.data.goal.Contains("P")) go.transform.localPosition = new Vector3(0, 20, 0);
            else if (PrefsManager.Instance.GetBool("levelLeaderboards", true)) go.transform.localPosition = new Vector3(0, 90, 0);
            else go.transform.localPosition = new Vector3(0, 22.5f, 0);
            go.layer = 5;
            goalCount = go.AddComponent<TextMeshProUGUI>();
            goalCount.text = (Core.data.goalRequirement - Core.data.completedLevels.Count).ToString();
            goalCount.font = font;
            goalCount.fontSize = 100;
            goalCount.alignment = TextAlignmentOptions.Center;
            goalCount.overflowMode = TextOverflowModes.Overflow;

            levels[Core.data.goal].transform.GetChild(2).gameObject.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f);
            if (!Multiworld.Authenticated) goalCount.gameObject.SetActive(false);
        }

        public static void UpdateLevels()
        {
            Sprite locked = Addressables.LoadAssetAsync<Sprite>("Assets/Textures/UI/Level Thumbnails/Locked.png").WaitForCompletion();
            foreach (string level in Core.AllLevels)
            {
                if ((!Core.data.unlockedLevels.Contains(level) && level != Core.data.goal) || (level == Core.data.goal && Core.data.completedLevels.Count < Core.data.goalRequirement))
                {
                    foreach (Image component in levels[level].GetComponentsInChildren<Image>())
                    {
                        if (component.gameObject.name == "Image") component.sprite = locked;
                    }
                    levels[level].GetComponent<Button>().interactable = false;
                }
                else if (Core.data.unlockedLevels.Contains(level))
                {
                    foreach (Image component in levels[level].GetComponentsInChildren<Image>())
                    {
                        if (component.gameObject.name == "Image" && component.sprite.Equals(locked)) component.sprite = Traverse.Create(component.transform.parent.GetComponent<LevelSelectPanel>()).Field<Sprite>("origSprite").Value;
                    }
                    levels[level].GetComponent<Button>().interactable = true;
                }
            }
        }

        public static void CreateMenuSkullIcons()
        {
            Sprite sprite = bundle.LoadAsset<Sprite>("assets/skull.png");

            foreach (LevelInfo info in Core.levelInfos)
            {
                int xPos = 74;
                int xOffset = 33;
                if (info.Skulls == SkullsType.Normal)
                {
                    if (info.SkullsList == null) throw new Exception($"Skull list is null for level {info.Name}.");

                    foreach (string skull in info.SkullsList)
                    {
                        GameObject go = new GameObject();
                        go.name = skull;
                        go.transform.SetParent(levels[info.Name].transform);
                        go.transform.localScale = new Vector3(0.35f, 0.35f, 1);
                        go.layer = 5;
                        go.AddComponent<Image>().sprite = sprite;
                        go.AddComponent<Shadow>().effectDistance = new Vector2(2, -2);
                        go.transform.localPosition = new Vector3(xPos - (xOffset * info.SkullsList.FindIndex(a => a == skull)), 65, 0);
                        go.AddComponent<SkullIcon>().SetId(skull);
                    }
                }
                else if (info.Name == "1-4")
                {
                    for (int i = 0; i < 4; i++)
                    {
                        string skull = "9_b" + (i + 1);
                        GameObject go = new GameObject();
                        go.name = skull;
                        go.transform.SetParent(levels[info.Name].transform);
                        go.transform.localScale = new Vector3(0.35f, 0.35f, 1);
                        go.layer = 5;
                        go.AddComponent<Image>().sprite = sprite;
                        go.AddComponent<Shadow>().effectDistance = new Vector2(2, -2);
                        go.transform.localPosition = new Vector3(xPos - (xOffset * i), 65, 0);
                        go.AddComponent<SkullIcon>().SetId(skull);
                    }
                }
                else if (info.Name == "5-1")
                {
                    for (int i = 0; i < 3; i++)
                    {
                        string skull = "20_b" + (i + 1);
                        GameObject go = new GameObject();
                        go.name = skull;
                        go.transform.SetParent(levels[info.Name].transform);
                        go.transform.localScale = new Vector3(0.35f, 0.35f, 1);
                        go.layer = 5;
                        go.AddComponent<Image>().sprite = sprite;
                        go.AddComponent<Shadow>().effectDistance = new Vector2(2, -2);
                        go.transform.localPosition = new Vector3(xPos - (xOffset * i), 65, 0);
                        go.AddComponent<SkullIcon>().SetId(skull);
                    }
                }
            }

            foreach (LevelInfo info in Core.secretMissionInfos)
            {
                if (info.Skulls == SkullsType.Normal)
                {
                    if (info.SkullsList == null) throw new Exception($"Skull list is null for level {info.Name}.");

                    int xPos = 275;
                    if (info.Name == "0-S") xPos = 290;
                    int xOffset = 38;
                    int yPos = 10;

                    GameObject rankPanel = secrets[info.Name].transform.parent.Find("RankPanel").gameObject;
                    Vector3 rankPos = rankPanel.transform.localPosition;
                    rankPos.x = xPos - (34 * info.SkullsList.Count);
                    rankPanel.transform.localPosition = rankPos;

                    foreach (string skull in info.SkullsList)
                    {
                        GameObject go = new GameObject();
                        go.name = skull;
                        go.transform.SetParent(secrets[info.Name].transform.parent);
                        go.transform.localScale = new Vector3(0.4f, 0.4f, 1);
                        go.layer = 5;
                        go.AddComponent<Image>().sprite = sprite;
                        go.AddComponent<Shadow>().effectDistance = new Vector2(2, -2);
                        go.transform.localPosition = new Vector3(xPos - (xOffset * info.SkullsList.FindIndex(a => a == skull)), yPos, 0);
                        go.AddComponent<SkullIcon>().SetId(skull, false);
                    }
                }
            }
            createdSkullIcons = true;
        }

        public static void CreateMenuSwitchIcons()
        {
            int xPos = 70;
            int xOffset = 40;
            if (Core.data.l1switch)
            {
                for (int i = 4; i > 0; i--)
                {
                    Sprite sprite = bundle.LoadAsset<Sprite>($"assets/switch{i}.png");
                    GameObject go = new GameObject();
                    go.name = i.ToString();
                    go.transform.SetParent(levels["1-4"].transform);
                    go.transform.localScale = new Vector3(0.35f, 0.35f, 1);
                    go.layer = 5;
                    go.AddComponent<Image>().sprite = sprite;
                    go.AddComponent<Shadow>().effectDistance = new Vector2(2, -2);
                    go.transform.localPosition = new Vector3(xPos - (xOffset * (4 - i)), -32, 0);
                    go.AddComponent<SwitchIcon>().SetId(i - 1, true, true);
                }
            }
            if (Core.data.l7switch)
            {
                for (int i = 3; i > 0; i--)
                {
                    Sprite sprite = bundle.LoadAsset<Sprite>($"assets/switch{i}.png");
                    GameObject go = new GameObject();
                    go.name = i.ToString();
                    go.transform.SetParent(levels["7-2"].transform);
                    go.transform.localScale = new Vector3(0.35f, 0.35f, 1);
                    go.layer = 5;
                    go.AddComponent<Image>().sprite = sprite;
                    go.AddComponent<Shadow>().effectDistance = new Vector2(2, -2);
                    go.transform.localPosition = new Vector3(xPos - (xOffset * (3 - i)), -32, 0);
                    go.AddComponent<SwitchIcon>().SetId(i - 1, false, true);
                }
            }
            createdSwitchIcons = true;
        }

        public static void CreatePauseSkullIcons(GameObject pauseMenu, bool createdSwitches)
        {

            if (Core.CurrentLevelHasInfo)
            {
                Sprite sprite = bundle.LoadAsset<Sprite>("assets/skull.png");
                LevelInfo info = Core.CurrentLevelInfo;

                int xPos = 275;
                int xOffset = 33;
                int yPos = -175;
                if (createdSwitches) yPos = -135;
                if (info.Skulls == SkullsType.Normal)
                {
                    if (info.SkullsList == null) throw new Exception($"Skull list is null for level {info.Name}");
                    foreach (string skull in info.SkullsList)
                    {
                        if (pauseMenu.transform.Find(skull)) continue;
                        GameObject go = new GameObject();
                        go.name = skull;
                        go.transform.SetParent(pauseMenu.transform);
                        go.transform.localScale = new Vector3(0.35f, 0.35f, 1);
                        go.layer = 5;
                        go.AddComponent<Image>().sprite = sprite;
                        go.AddComponent<Shadow>().effectDistance = new Vector2(2, -2);
                        go.transform.localPosition = new Vector3(xPos - (xOffset * info.SkullsList.FindIndex(a => a == skull)), yPos, 0);
                        go.AddComponent<SkullIcon>().SetId(skull, false);
                    }
                }
                else if (info.Name == "1-4")
                {
                    for (int i = 0; i < 4; i++)
                    {
                        string skull = "9_b" + (i + 1);
                        if (pauseMenu.transform.Find(skull)) continue;
                        GameObject go = new GameObject();
                        go.name = skull;
                        go.transform.SetParent(pauseMenu.transform);
                        go.transform.localScale = new Vector3(0.35f, 0.35f, 1);
                        go.layer = 5;
                        go.AddComponent<Image>().sprite = sprite;
                        go.AddComponent<Shadow>().effectDistance = new Vector2(2, -2);
                        go.transform.localPosition = new Vector3(xPos - (xOffset * i), yPos, 0);
                        go.AddComponent<SkullIcon>().SetId(skull, false);
                    }
                }
                else if (info.Name == "5-1")
                {
                    for (int i = 0; i < 3; i++)
                    {
                        string skull = "20_b" + (i + 1);
                        if (pauseMenu.transform.Find(skull)) continue;
                        GameObject go = new GameObject();
                        go.name = skull;
                        go.transform.SetParent(pauseMenu.transform);
                        go.transform.localScale = new Vector3(0.35f, 0.35f, 1);
                        go.layer = 5;
                        go.AddComponent<Image>().sprite = sprite;
                        go.AddComponent<Shadow>().effectDistance = new Vector2(2, -2);
                        go.transform.localPosition = new Vector3(xPos - (xOffset * i), yPos, 0);
                        go.AddComponent<SkullIcon>().SetId(skull, false);
                    }
                }
            }
        }

        public static void CreatePauseSwitchIcons(GameObject pauseMenu, ref bool created)
        {
            if (Core.CurrentLevelHasInfo)
            {
                if (!(Core.CurrentLevelInfo.Id == 9 || Core.CurrentLevelInfo.Id == 27)) return;

                int xPos = 275;
                int xOffset = 40;
                int yPos = -175;

                int switches = 0;
                if (Core.CurrentLevelInfo.Id == 9) switches = 4;
                if (Core.CurrentLevelInfo.Id == 27) switches = 3;

                bool limbo = true;
                if (Core.CurrentLevelInfo.Id == 27) limbo = false;

                for (int i = switches; i > 0; i--)
                {
                    Sprite sprite = bundle.LoadAsset<Sprite>($"assets/switch{i}.png");
                    if (pauseMenu.transform.Find(i.ToString())) continue;
                    GameObject go = new GameObject();
                    go.name = i.ToString();
                    go.transform.SetParent(pauseMenu.transform);
                    go.transform.localScale = new Vector3(0.35f, 0.35f, 1);
                    go.layer = 5;
                    go.AddComponent<Image>().sprite = sprite;
                    go.AddComponent<Shadow>().effectDistance = new Vector2(2, -2);
                    go.transform.localPosition = new Vector3(xPos - (xOffset * (switches - i)), yPos, 0);
                    go.AddComponent<SwitchIcon>().SetId(i-1, limbo);
                }
                created = true;
            }
        }

        public static void CreateMessageUI()
        {
            GameObject player = GameObject.Find("/Player");
            foreach (HudController component in player.GetComponentsInChildren<HudController>())
            {
                //Core.logger.LogInfo(component.gameObject.name);
                hud = component.gameObject;
            }

            popupCanvas = Instantiate(hud.transform.GetChild(0).gameObject, hud.transform);
            popupCanvas.name = "APCanvas";
            Traverse hudT = Traverse.Create(popupCanvas.GetComponent<HUDPos>());
            hudT.Field<Vector3>("defaultPos").Value = new Vector3(1.125f, -0.53f, 1);
            hudT.Field<Vector3>("defaultRot").Value = new Vector3(0, 30, 0);
            popupCanvas.GetComponent<HUDPos>().reversePos = new Vector3(-1.06f, -0.53f, 1);
            popupCanvas.GetComponent<HUDPos>().reverseRot = new Vector3(0, 330, 0);
            popupCanvas.GetComponent<HUDPos>().active = false;
            popupCanvas.GetComponent<HUDPos>().active = true;
            Destroy(popupCanvas.transform.Find("SpeedometerPanel").gameObject);
            Destroy(popupCanvas.transform.Find("GunPanel").gameObject);
            Destroy(popupCanvas.transform.Find("StatsPanel").Find("RailcannonChargePanel").gameObject);
            Destroy(popupCanvas.transform.Find("StatsPanel").Find("Filler").Find("Panel (2)").gameObject);
            Destroy(popupCanvas.transform.Find("StatsPanel").Find("Filler").Find("Panel (3)").gameObject);
            Destroy(popupCanvas.transform.Find("StatsPanel").Find("Filler").Find("Panel").gameObject);
            Destroy(popupCanvas.transform.Find("StatsPanel").Find("Filler").Find("AltRailcannonPanel").gameObject);
            try
            {
                popupImage = popupCanvas.transform.Find("StatsPanel").Find("Filler").Find("FistPanel").Find("Panel").gameObject;
            }
            catch (NullReferenceException)
            {
                foreach (Image image in popupCanvas.transform.Find("StatsPanel").Find("Filler").GetComponentsInChildren<Image>())
                {
                    if (image.name == "Panel" && image.transform.parent.name == "FistPanel") popupImage = image.gameObject;
                }
            }
            popupCanvas.transform.Find("StatsPanel").gameObject.SetActive(true);
            popupCanvas.SetActive(false);
            popupImage.GetComponent<Image>().sprite = bundle.LoadAsset<Sprite>("assets/layer4.png");
            popupImage.GetComponent<Image>().color = Colors.Layer4;
            GameObject go = new GameObject();
            go.name = "APText";
            go.transform.SetParent(popupCanvas.transform.Find("StatsPanel"));
            go.layer = 13;
            popupText = go.AddComponent<TextMeshProUGUI>();
            popupText.font = font;
            popupText.alignment = TextAlignmentOptions.Right;
            popupText.enableAutoSizing = true;
            popupText.fontSizeMax = 120;
            popupText.lineSpacing = 1.1f;
            popupText.material = HUDOptions.Instance.hudMaterial;
            popupText.text = "UNLOCKED: <color=#ffe800ff>4-2 GOD DAMN THE SUN</color> (<color=#fafad2ff>Trev</color>)";
            popupText.GetComponent<RectTransform>().sizeDelta = new Vector2(750, 150);
            popupText.transform.localPosition = new Vector3(100.6437f, 23.9347f, 0);
            popupText.transform.localScale = new Vector3(0.25f, 0.25f, 1);
            popupText.transform.localRotation = new Quaternion();
        }

        public IEnumerator DisplayMessage()
        {
            displayingMessage = true;
            Message message = LocationManager.messages[0];
            LocationManager.messages.RemoveAt(0);

            popupText.text = message.message;
            popupImage.GetComponent<Image>().color = message.color;

            if (bundle.Contains("assets/" + message.image + ".png")) popupImage.GetComponent<Image>().sprite = bundle.LoadAsset<Sprite>("assets/" + message.image + ".png");
            else popupImage.GetComponent<Image>().sprite = bundle.LoadAsset<Sprite>("assets/archipelago.png");

            popupCanvas.SetActive(true);
            popupText.isOverlay = true;
            yield return new WaitForSeconds(3f);
            popupCanvas.SetActive(false);
            popupText.isOverlay = false;
            if (LocationManager.messages.Count > 0 && Core.IsPlaying) StartCoroutine(DisplayMessage());
            else displayingMessage = false;
        }

        public void CreateDeathLinkMessage()
        {
            GameObject go = new GameObject()
            {
                name = "APDeathMessage",
                layer = 5,
            };
            GameObject go2 = new GameObject()
            {
                name = "Text",
                layer = 5
            };
            go.transform.parent = NewMovement.Instance.blackScreen.gameObject.transform;
            go.AddComponent<Canvas>();
            go2.transform.parent = go.transform;
            go2.AddComponent<Text>();
            deathLinkMessage = go2.AddComponent<DeathLinkMessage>();
            deathLinkMessage.Initialize();
        }
    }
}