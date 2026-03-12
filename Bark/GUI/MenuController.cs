using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Bark.Gestures;
using Bark.Interaction;
using Bark.Modules;
using Bark.Modules.Misc;
using Bark.Modules.Movement;
using Bark.Modules.Multiplayer;
using Bark.Modules.Physics;
using Bark.Modules.Teleportation;
using Bark.Tools;
using BepInEx.Configuration;
using Bark.Extensions;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.XR;
using Player = GorillaLocomotion.GTPlayer;

namespace Bark.GUI;

public class MenuController : BarkGrabbable
{
    public static MenuController? Instance;
    private static InputTracker? _summonTracker;
    private static ConfigEntry<string>? _summonInput;
    private static ConfigEntry<string>? _summonInputHand;
    private static ConfigEntry<string>? _theme;
    public static Material[]? ShinyRocks;

    public static bool Debugger = true;

    public Vector3
        initialMenuOffset = new(0, .035f, .65f),
        btnDimensions = new(.3f, .05f, .05f);

    public Rigidbody? rigidbody;
    public List<Transform>? modPages;
    public List<ButtonController>? buttons;
    public List<BarkModule> modules = [];
    public GameObject? modPage, settingsPage;
    public Text? helpText;

    public Renderer? renderer;
    public Material[]? grateMat, barkMat, hollopurpMat, monkeMat, oldMat;

    private int debugButtons;

    private bool docked;

    private int pageIndex;
    public bool Built { get; private set; }

    protected override void Awake()
    {
        Instance = this;
        try
        {
            base.Awake();
            throwOnDetach = true;
            gameObject.AddComponent<PositionValidator>();
            if (Plugin.ConfigFile != null) Plugin.ConfigFile.SettingChanged += SettingsChanged;
            var tooAddmodules = new List<BarkModule>
            {
                // Locomotion
                gameObject.AddComponent<Airplane>(),
                gameObject.AddComponent<Helicopter>(),
                gameObject.AddComponent<Bubble>(),
                gameObject.AddComponent<Fly>(),
                gameObject.AddComponent<HandFly>(),
                gameObject.AddComponent<GrapplingHooks>(),
                gameObject.AddComponent<Climb>(),
                gameObject.AddComponent<DoubleJump>(),
                gameObject.AddComponent<Platforms>(),
                gameObject.AddComponent<Frozone>(),
                gameObject.AddComponent<NailGun>(),
                gameObject.AddComponent<Rockets>(),
                gameObject.AddComponent<SpeedBoost>(),
                gameObject.AddComponent<Swim>(),
                gameObject.AddComponent<Wallrun>(),
                gameObject.AddComponent<Zipline>(),

                //// Physics
                gameObject.AddComponent<LowGravity>(),
                gameObject.AddComponent<NoClip>(),
                gameObject.AddComponent<NoSlip>(),
                gameObject.AddComponent<Potions>(),
                gameObject.AddComponent<SlipperyHands>(),
                gameObject.AddComponent<DisableWind>(),
                gameObject.AddComponent<UpsideDown>(),

                //// Teleportation
                gameObject.AddComponent<Checkpoint>(),
                gameObject.AddComponent<Portal>(),
                gameObject.AddComponent<Pearl>(),
                gameObject.AddComponent<Teleport>(),

                //// Multiplayer
                gameObject.AddComponent<Boxing>(),
                gameObject.AddComponent<Piggyback>(),
                gameObject.AddComponent<Telekinesis>(),
                gameObject.AddComponent<Grab>(),
                gameObject.AddComponent<Fireflies>(),
                gameObject.AddComponent<ESP>(),
                gameObject.AddComponent<RatSword>(),
                gameObject.AddComponent<Kamehameha>()

                //// Misc
                //gameObject.AddComponent<ReturnToVS>(),
                //gameObject.AddComponent<Lobby>(),
            };
            modules.AddRange(tooAddmodules);
            ReloadConfiguration();
        }
        catch (Exception e)
        {
            Logging.Exception(e);
        }
    }

    private void Start() // sigma sigma sigma s
    {
        Summon();
        transform.SetParent(null);
        transform.position = Vector3.zero;
        if (rigidbody != null)
        {
            rigidbody.isKinematic = false;
            rigidbody.useGravity = true;
        }

        transform.SetParent(null);
        AddBlockerToAllButtons(ButtonController.Blocker.MENU_FALLING);
        docked = false;
    }

    private void FixedUpdate()
    {
        // The potions tutorial needs to be updated frequently to keep the current size
        // up-to-date, even when the mod is disabled
        if (BarkModule.LastEnabled && BarkModule.LastEnabled == Potions.Instance)
            helpText.text = Potions.Instance.Tutorial();
    }

    private void Update()
    {
        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            if (!docked)
            {
                Summon();
            }
            else
            {
                if (rigidbody != null)
                {
                    rigidbody.isKinematic = false;
                    rigidbody.useGravity = true;
                }

                transform.SetParent(null);
                AddBlockerToAllButtons(ButtonController.Blocker.MENU_FALLING);
                docked = false;
            }
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Plugin.ConfigFile!.SettingChanged -= SettingsChanged;
    }

    private void ThemeChanged()
    {
        if (Plugin.AssetBundle != null)
        {
            if (!renderer)
                renderer = GetComponent<MeshRenderer>();
            if (Plugin.AssetBundle != null)
            {
                if (grateMat == null)
                {
                    var zipline = Plugin.AssetBundle.LoadAsset<Material>("Zipline Rope Material");
                    var metal = Plugin.AssetBundle.LoadAsset<Material>("Metal Material");
                    if (zipline && metal) grateMat = [zipline, metal];
                }

                if (barkMat == null)
                {
                    var outer = Plugin.AssetBundle.LoadAsset<Material>("m_Menu Outer");
                    var inner = Plugin.AssetBundle.LoadAsset<Material>("m_Menu Inner");
                    if (outer && inner) barkMat = [outer, inner];
                }

                if (hollopurpMat == null)
                {
                    var sparkleMat = Plugin.AssetBundle.LoadAsset<Material>("m_TK Sparkles");
                    if (sparkleMat) hollopurpMat = [sparkleMat, sparkleMat];
                }

                if (monkeMat == null || oldMat == null)
                {
                    var baseMat = Plugin.AssetBundle.LoadAsset<Material>("Gorilla Material");
                    if (baseMat)
                    {
                        monkeMat ??= [baseMat, baseMat];

                        oldMat ??=
                        [
                            new Material(baseMat)
                            {
                                mainTexture = null,
                                color = new Color(0.17f, 0.17f, 0.17f)
                            },
                            new Material(baseMat)
                            {
                                mainTexture = null,
                                color = new Color(0.2f, 0.2f, 0.2f)
                            }
                        ];
                    }
                }
            }

            var themeName = _theme.Value.ToLower();

            switch (themeName)
            {
                case "grate":
                    if (grateMat != null) renderer.materials = grateMat;
                    break;

                case "bark":
                    renderer.materials = barkMat;
                    break;

                case "holowpurple":
                    renderer.materials = hollopurpMat;
                    break;

                case "oldgrate":
                    renderer.materials = oldMat;
                    break;

                case "player" when VRRig.LocalRig.CurrentCosmeticSkin != null:
                    var skinMat = VRRig.LocalRig.CurrentCosmeticSkin.scoreboardMaterial;
                    renderer.materials = [skinMat, skinMat];
                    break;

                case "player":
                    if (monkeMat != null)
                    {
                        renderer.materials = monkeMat;
                        var playerColor = VRRig.LocalRig.playerColor;
                        monkeMat[0].color = playerColor;
                        monkeMat[1].color = playerColor;
                    }

                    break;
            }
        }
    }

    private void ReloadConfiguration()
    {
        if (_summonTracker != null)
            _summonTracker.OnPressed -= Summon;
        GestureTracker.Instance.OnMeatBeat -= Summon;

        var hand = _summonInputHand.Value == "left"
            ? XRNode.LeftHand
            : XRNode.RightHand;

        if (_summonInput.Value == "gesture")
        {
            GestureTracker.Instance.OnMeatBeat += Summon;
        }
        else
        {
            _summonTracker = GestureTracker.Instance.GetInputTracker(
                _summonInput.Value, hand
            );
            if (_summonTracker != null)
                _summonTracker.OnPressed += Summon;
        }
    }

    private void SettingsChanged(object sender, SettingChangedEventArgs e)
    {
        if (e.ChangedSetting == _summonInput || e.ChangedSetting == _summonInputHand) ReloadConfiguration();
        if (e.ChangedSetting == _theme) ThemeChanged();
    }

    private void Summon(InputTracker _)
    {
        Summon();
    }

    public void Summon()
    {
        if (!Built)
            BuildMenu();
        else
            ResetPosition();
    }

    private void ResetPosition()
    {
        rigidbody.isKinematic = true;
        rigidbody.velocity = Vector3.zero;
        transform.SetParent(Player.Instance.bodyCollider.transform);
        transform.localPosition = initialMenuOffset;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        foreach (var button in buttons) button.RemoveBlocker(ButtonController.Blocker.MENU_FALLING);
        docked = true;
    }

    private void BuildMenu()
    {
        Logging.Debug("Building menu...");
        try
        {
            helpText = gameObject.transform.Find("Help Canvas").GetComponentInChildren<Text>();
            helpText.transform.parent.localPosition = new Vector3(0, -0.0731f, -0.1f);
            helpText.transform.parent.localRotation = Quaternion.Euler(0, 180f, 0);
            helpText.text = "Enable a module to see its tutorial.";
            var collider = gameObject.GetOrAddComponent<BoxCollider>();
            collider.isTrigger = true;
            rigidbody = gameObject.GetComponent<Rigidbody>();
            rigidbody.isKinematic = true;

            SetupInteraction();
            SetupModPages();
            SetupSettingsPage();

            transform.SetParent(Player.Instance.bodyCollider.transform);
            ResetPosition();
            Logging.Debug("Build successful.");
            ReloadConfiguration();
            ThemeChanged();
        }
        catch (Exception ex)
        {
            Logging.Warning(ex.Message);
            Logging.Warning(ex.StackTrace);
            return;
        }

        Built = true;
    }

    private void SetupSettingsPage()
    {
        var button = gameObject.transform.Find("Settings Button").gameObject;
        var btnController = button.AddComponent<ButtonController>();
        buttons.Add(btnController);
        btnController.OnPressed += (obj, pressed) =>
        {
            settingsPage.SetActive(pressed);
            if (pressed)
                settingsPage.GetComponent<SettingsPage>().UpdateText();
            modPage.SetActive(!pressed);
        };

        settingsPage = transform.Find("Settings Page").gameObject;
        settingsPage.AddComponent<SettingsPage>();
        settingsPage.SetActive(false);
    }

    public void SetupModPages()
    {
        var modPageTemplate = gameObject.transform.Find("Mod Page");
        var buttonsPerPage = modPageTemplate.childCount - 2; // Excludes the prev/next page btns
        var numPages = (modules.Count - 1) / buttonsPerPage + 1;
        if (Plugin.DebugMode)
            numPages++;

        modPages = new List<Transform> { modPageTemplate };
        for (var i = 0; i < numPages - 1; i++)
            modPages.Add(Instantiate(modPageTemplate, gameObject.transform));

        buttons = new List<ButtonController>();
        for (var i = 0; i < modules.Count; i++)
        {
            var module = modules[i];

            var page = modPages[i / buttonsPerPage];
            var button = page.Find($"Button {i % buttonsPerPage}").gameObject;

            var btnController = button.AddComponent<ButtonController>();
            buttons.Add(btnController);
            btnController.OnPressed += (obj, pressed) =>
            {
                module.enabled = pressed;
                if (pressed)
                    helpText.text = module.GetDisplayName().ToUpper() +
                                    "\n\n" + module.Tutorial().ToUpper();
            };
            module.button = btnController;
            btnController.SetText(module.GetDisplayName().ToUpper());
        }

        AddDebugButtons();

        foreach (var modPage in modPages)
        {
            foreach (Transform button in modPage)
                if (button.name == "Button Left" && modPage != modPages[0])
                {
                    var btnController = button.gameObject.AddComponent<ButtonController>();
                    btnController.OnPressed += PreviousPage;
                    btnController.SetText("Prev Page");
                    buttons.Add(btnController);
                }
                else if (button.name == "Button Right" && modPage != modPages[modPages.Count - 1])
                {
                    var btnController = button.gameObject.AddComponent<ButtonController>();
                    btnController.OnPressed += NextPage;
                    btnController.SetText("Next Page");
                    buttons.Add(btnController);
                }
                else if (!button.GetComponent<ButtonController>())
                {
                    button.gameObject.SetActive(false);
                }

            modPage.gameObject.SetActive(false);
        }

        modPageTemplate.gameObject.SetActive(true);
        modPage = modPageTemplate.gameObject;
    }

    private void AddDebugButtons()
    {
        AddDebugButton("Debug Log", (btn, isPressed) =>
        {
            Debugger = isPressed;
            Logging.Debug("Debugger", Debugger ? "active" : "inactive");
            Plugin.DebugText.text = "";
        });

        AddDebugButton("Close game", (btn, isPressed) =>
        {
            Debugger = isPressed;
            if (btn.text.text == "You sure?")
                Application.Quit();
            else
                btn.text.text = "You sure?";
        });

        AddDebugButton("Show Colliders", (btn, isPressed) =>
        {
            if (isPressed)
                foreach (var c in FindObjectsOfType<Collider>())
                    c.gameObject.AddComponent<ColliderRenderer>();
            else
                foreach (var c in FindObjectsOfType<ColliderRenderer>())
                    c.Obliterate();
        });
    }

    private void AddDebugButton(string title, Action<ButtonController, bool> onPress)
    {
        if (!Plugin.DebugMode) return;
        var page = modPages.Last();
        var button = page.Find($"Button {debugButtons}").gameObject;
        var btnController = button.gameObject.AddComponent<ButtonController>();
        btnController.OnPressed += onPress;
        btnController.SetText(title);
        buttons.Add(btnController);
        debugButtons++;
    }

    public void PreviousPage(ButtonController button, bool isPressed)
    {
        button.IsPressed = false;
        pageIndex--;
        for (var i = 0; i < modPages.Count; i++) modPages[i].gameObject.SetActive(i == pageIndex);
        modPage = modPages[pageIndex].gameObject;
    }

    public void NextPage(ButtonController button, bool isPressed)
    {
        button.IsPressed = false;
        pageIndex++;
        for (var i = 0; i < modPages.Count; i++) modPages[i].gameObject.SetActive(i == pageIndex);
        modPage = modPages[pageIndex].gameObject;
    }

    public void SetupInteraction()
    {
        throwOnDetach = true;
        priority = 100;
        OnSelectExit += (_, __) =>
        {
            AddBlockerToAllButtons(ButtonController.Blocker.MENU_FALLING);
            docked = false;
        };
        OnSelectEnter += (_, __) => { RemoveBlockerFromAllButtons(ButtonController.Blocker.MENU_FALLING); };
    }

    public Material GetMaterial(string name)
    {
        foreach (var renderer in FindObjectsOfType<Renderer>())
        {
            var _name = renderer.material.name.ToLower();
            if (_name.Contains(name)) return renderer.material;
        }

        return null;
    }

    public void AddBlockerToAllButtons(ButtonController.Blocker blocker)
    {
        foreach (var button in buttons) button.AddBlocker(blocker);
    }

    public void RemoveBlockerFromAllButtons(ButtonController.Blocker blocker)
    {
        foreach (var button in buttons) button.RemoveBlocker(blocker);
    }

    public static void BindConfigEntries()
    {
        try
        {
            var inputDesc = new ConfigDescription(
                "Which button you press to open the menu",
                new AcceptableValueList<string>("gesture", "stick", "a/x", "b/y")
            );
            _summonInput = Plugin.ConfigFile.Bind("General",
                "open menu",
                "gesture",
                inputDesc
            );

            var handDesc = new ConfigDescription(
                "Which hand can open the menu",
                new AcceptableValueList<string>("left", "right")
            );
            _summonInputHand = Plugin.ConfigFile.Bind("General",
                "open hand",
                "right",
                handDesc
            );

            var ThemeDesc = new ConfigDescription(
                "Which Theme Should Bark Use?",
                new AcceptableValueList<string>("grate", "OldGrate", "bark", "holowpurple", "Player")
            );
            _theme = Plugin.ConfigFile.Bind("General",
                "theme",
                "Bark",
                ThemeDesc
            );
        }
        catch (Exception e)
        {
            Logging.Exception(e);
        }
    }
}