/**
 *      _   _           _         __  __           _ 
 *     | \ | | __ _ ___| |_ _   _|  \/  | ___   __| |
 *     |  \| |/ _` / __| __| | | | |\/| |/ _ \ / _` |
 *     | |\  | (_| \__ \ |_| |_| | |  | | (_) | (_| |
 *     |_| \_|\__,_|___/\__|\__, |_|  |_|\___/ \__,_|
 *                          |___/                    
 *     
 *     a MelonLoader mod for the game Schedule I
 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using NastyMod;

using UnityEngine;

using MelonLoader;
using HarmonyLib;

using Il2CppScheduleOne;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.PlayerScripts;
using static Il2CppScheduleOne.PlayerScripts.PlayerCrimeData;
using Il2CppScheduleOne.Employees;
using Il2CppScheduleOne.NPCs;
using Il2CppScheduleOne.NPCs.Relation;
using Il2CppScheduleOne.Property;
using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.UI.Phone.Delivery;
using Il2CppScheduleOne.Storage;
using Il2CppScheduleOne.Delivery;
using Il2CppScheduleOne.PlayerScripts.Health;
using Il2CppScheduleOne.ObjectScripts;

public class NastyModClass : MelonMod
{
    #region Fields
    public static NastyModClass Instance { get; private set; }

    private HarmonyLib.Harmony harmony;

    // Menu variables
    private bool menu_is_open = false;
    private int menu_width = 800;
    private int menu_height = 500;
    private int menu_spacing = 15;
    private int menu_tab = 0;
    private readonly List<string> menu_tabs = new List<string> { "Player", "World", "Spawner", "Misc", "Teleport", "Employees", "Credits" };

    // Button variables
    private int button_width = 90;
    private int button_height = 30;
    private int button_spacing = 10;

    private Dictionary<string, Dictionary<string, string>> items;
    private Dictionary<string, Dictionary<string, string>> teleports;

    // TAB - Player variables
    private bool _player_god_mode = false;
    private bool _player_infinite_energy = false;
    private bool _player_infinite_stamina = false;
    private bool _player_never_wanted = false;
    private float _player_move_speed_multiplier = 1f;
    private float _player_crouch_speed_multiplier = 0.6f;
    private float _player_jump_multiplier = 1f;
    private int _player_exp_amount = 1000;
    private int _player_cash_amount = 1000;
    private int _player_balance_amount = 1000;

    // TAB - World variables
    private bool _world_npc_esp = false;
    private bool _world_player_esp = false;
    private float _world_esp_range = 100f;
    private float _world_grow_speed_multiplier = 1;

    // TAB - Spawner variables
    private string _spawner_selected_category;
    private Vector2 _spawner_category_scoll_pos;
    private int _spawner_item_amount = 1;

    // TAB - Misc variables
    private Vector2 _misc_scroll_pos;
    private int _misc_stack_size = 20;
    private bool _misc_use_deal_success_chance = false;
    private float _misc_deal_sucess_chance = 0.75f;
    private Dictionary<string, string> _misc_packaging_types = new Dictionary<string, string> { { "brick", "Brick" }, { "jar", "Jar" }, { "baggie", "Baggie" } };

    // TAB - Employees variables
    private string _employees_selected_property;
    private Vector2 _employees_category_scroll_pos;
    private Vector2 _employees_scroll_pos;
    private Dictionary<string, Dictionary<string, float>> _employees_employee_speed = new Dictionary<string, Dictionary<string, float>> ();

    // TAB - Teleport variables
    private string _teleport_selected_category;
    private Vector2 _teleport_category_scroll_pos;

    private GUIStyle _titleStyle;
    private GUIStyle _headerStyle;
    private GUIStyle _subHeaderStyle;
    private GUIStyle _buttonStyle;
    private GUIStyle _uniform_label;
    private GUIStyle _uniform_small_label;
    #endregion

    #region MelonLoader
    public override void OnInitializeMelon()
    {
        MelonLogger.Msg("Initializing...");

        Instance = this;

        // load json files
        items = LoadJSON("NastyMod.Resources.Items.json");
        teleports = LoadJSON("NastyMod.Resources.Teleports.json");

        // player preferences
        this._player_god_mode = NastyMod.Properties.Settings.Default.playerGodMode;
        this._player_infinite_energy = NastyMod.Properties.Settings.Default.playerInfiniteEnergy;
        this._player_infinite_stamina = NastyMod.Properties.Settings.Default.playerInfiniteStamina;
        this._player_never_wanted = NastyMod.Properties.Settings.Default.playerNeverWanted;
        this._player_crouch_speed_multiplier = NastyMod.Properties.Settings.Default.playerCrouchSpeedMultiplier;
        this._player_move_speed_multiplier = NastyMod.Properties.Settings.Default.playerMoveSpeedMultiplier;
        this._player_jump_multiplier = NastyMod.Properties.Settings.Default.playerJumpMultiplier;

        // world preferences
        this._world_npc_esp = NastyMod.Properties.Settings.Default.worldNpcEsp;
        this._world_player_esp = NastyMod.Properties.Settings.Default.worldPlayerEsp;
        this._world_esp_range = NastyMod.Properties.Settings.Default.worldEspRange;
        this._world_grow_speed_multiplier = NastyMod.Properties.Settings.Default.worldGrowSpeedMultiplier;

        // spawner preferences
        if (NastyMod.Properties.Settings.Default.spawnerSelectedCategory == "")
        {
            NastyMod.Properties.Settings.Default.spawnerSelectedCategory = items.Keys.First();
            NastyMod.Properties.Settings.Default.Save();
        }
        this._spawner_selected_category = NastyMod.Properties.Settings.Default.spawnerSelectedCategory;
        this._spawner_item_amount = NastyMod.Properties.Settings.Default.spawnerItemAmount;

        // misc preferences
        this._misc_stack_size = NastyMod.Properties.Settings.Default.stackSize;
        this._misc_use_deal_success_chance = NastyMod.Properties.Settings.Default.useDealSuccessChange;
        this._misc_deal_sucess_chance = NastyMod.Properties.Settings.Default.dealSuccessChance;

        // teleport preferences
        if (NastyMod.Properties.Settings.Default.teleportSelectedCategory == "")
        {
            NastyMod.Properties.Settings.Default.teleportSelectedCategory = teleports.Keys.First();
            NastyMod.Properties.Settings.Default.Save();
        }
        this._teleport_selected_category = NastyMod.Properties.Settings.Default.teleportSelectedCategory;

        // employees preferences
        if (NastyMod.Properties.Settings.Default.employeesSelectedProperty == "")
        {
            NastyMod.Properties.Settings.Default.employeesSelectedProperty = teleports[_teleport_selected_category].Keys.First();
            NastyMod.Properties.Settings.Default.Save();
        }
        this._employees_selected_property = NastyMod.Properties.Settings.Default.employeesSelectedProperty;

        // setup harmony
        harmony = new HarmonyLib.Harmony("com.nastymod.schedulei");
        harmony.PatchAll();

        UnlockDebugMode();
    }

    public override void OnUpdate()
    {
        CheckPlayerMods();

        if (Input.GetKeyDown(KeyCode.F11) || Input.GetKeyDown(KeyCode.RightAlt))
            menu_is_open = !menu_is_open;
    }

    public override void OnGUI()
    {
        GUIFunctions();

        if (!menu_is_open) return;

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
        };
        _headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
        };
        _subHeaderStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
        };
        _buttonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 14,
            fixedWidth = 80,
            fixedHeight = 24
        };
        _uniform_label = new GUIStyle(GUI.skin.label)
        {
            fontSize = 13,
            fixedWidth = 260,
        };
        _uniform_small_label = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11
        };

        GUILayout.BeginArea(new Rect(50, 50, menu_width, menu_height), GUI.skin.box);

        // ** Title
        GUILayout.BeginArea(new Rect(15, 15, menu_width - (menu_spacing * 2), 40));
        GUILayout.Label("NastyMod v1", _titleStyle);
        GUILayout.EndArea();

        // ** Tab Buttons
        GUILayout.BeginArea(new Rect(15, 55, menu_width - (menu_spacing * 2), 40));
        GUILayout.BeginHorizontal();
        for (int i = 0; i < menu_tabs.Count; i++)
        {
            if (
                menu_tabs[i] == "Player" && !PlayerMovement.InstanceExists ||
                menu_tabs[i] == "World" && !PlayerMovement.InstanceExists ||
                menu_tabs[i] == "Spawner" && !PlayerMovement.InstanceExists ||
                menu_tabs[i] == "Misc" && !PlayerMovement.InstanceExists ||
                menu_tabs[i] == "Teleport" && !PlayerMovement.InstanceExists ||
                menu_tabs[i] == "Employees" && !PlayerMovement.InstanceExists
                )
            {
                continue;
            }

            if (GUILayout.Button(menu_tabs[i], GUILayout.Width(button_width), GUILayout.Height(button_height)))
                menu_tab = i;
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        // ** Tabs
        GUILayout.BeginArea(new Rect(15, 95, menu_width - (menu_spacing * 2), menu_height - 110), GUI.skin.box);
        try
        {
            switch (menu_tab)
            {
                case 0:
                    if (PlayerMovement.InstanceExists)
                    {
                        RenderPlayerTab();
                        break;
                    }
                    else
                    {
                        menu_tab = 6;
                        break;
                    }
                case 1:
                    if (PlayerMovement.InstanceExists)
                    {
                        RenderWorldTab();
                        break;
                    }
                    else
                    {
                        menu_tab = 6;
                        break;
                    }
                case 2:
                    if (PlayerMovement.InstanceExists)
                    {
                        RenderSpawnerTab();
                        break;
                    }
                    else
                    {
                        menu_tab = 6;
                        break;
                    }
                case 3:
                    if (PlayerMovement.InstanceExists)
                    {
                        RenderMiscTab();
                        break;
                    }
                    else
                    {
                        menu_tab = 6;
                        break;
                    }
                case 4:
                    if (PlayerMovement.InstanceExists)
                    {
                        RenderTeleportTab();
                        break;
                    }
                    else
                    {
                        menu_tab = 6;
                        break;
                    }
                case 5:
                    if (PlayerMovement.InstanceExists)
                    {
                        RenderEmployeesTab();
                        break;
                    }
                    else
                    {
                        menu_tab = 6;
                        break;
                    }
                case 6:
                    RenderCreditsTab();
                    break;
            }
        }
        catch (Exception ex)
        {
            MelonLogger.Error($"Failed to render Tab: {ex.Message} !! {ex.StackTrace} !! {ex.TargetSite}");
        }
        GUILayout.EndArea();

        GUILayout.EndArea();
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (sceneName == "Main")
        {
            MelonCoroutines.Start(WaitForCamera());
        }
    }
    #endregion

    #region IEnumerators
    private IEnumerator WaitForCamera()
    {
        yield return new WaitUntil(new System.Func<bool>(IsCameraReady));

        MelonCoroutines.Start(PatchStackLimit());

        yield break;
    }

    private IEnumerator PatchStackLimit()
    {
        yield return new WaitForSeconds(1f);

        ItemDefinition[] array = Resources.FindObjectsOfTypeAll<ItemDefinition>();
        foreach (ItemDefinition itemDefinition in array)
        {
            if (itemDefinition != null && itemDefinition.StackLimit < 255)
            {
                itemDefinition.StackLimit = GetStackSize();
            }
        }

        yield break;
    }

    private IEnumerator SetEmployeeSpeeds(Il2CppSystem.Guid guid, float speed)
    {
        yield return new WaitForSeconds(1f);

        foreach (EEmployeeType employeeType in System.Enum.GetValues(typeof(EEmployeeType)))
        {
            Botanist[] botanistEmployees = Resources.FindObjectsOfTypeAll<Botanist>();
            foreach (Botanist botanistEmployee in botanistEmployees)
            {
                if (botanistEmployee != null && botanistEmployee.GUID == guid)
                {
                    var additive_pour_time = 10f;
                    botanistEmployee.ADDITIVE_POUR_TIME = speed == 1 ? additive_pour_time : additive_pour_time / (speed * 1.5f);
                    var harvest_time = 15f;
                    botanistEmployee.HARVEST_TIME = speed == 1 ? harvest_time : harvest_time / (speed * 1.5f);
                    var seed_sow_time = 15f;
                    botanistEmployee.SEED_SOW_TIME = speed == 1 ? seed_sow_time : seed_sow_time / (speed * 1.5f);
                    var soil_pour_time = 10f;
                    botanistEmployee.SOIL_POUR_TIME = speed == 1 ? soil_pour_time : soil_pour_time / (speed * 1.5f);
                    var water_pour_time = 10f;
                    botanistEmployee.WATER_POUR_TIME = speed == 1 ? water_pour_time : water_pour_time / (speed * 1.5f);
                }
            }
        }

        yield break;
    }

    // Check dealers
    #endregion

    #region Tabs
    private void RenderPlayerTab()
    {
        GUILayout.BeginArea(new Rect(15, 15, menu_width - (menu_spacing * 4), menu_height - 110 - (menu_spacing * 2)));

        // Tab Title
        GUILayout.Label("Player", _headerStyle);

        // Spacer
        GUILayout.Space(10);

        // ** God Mode toggle
        GUILayout.BeginHorizontal();
        GUILayout.Label("God Mode");
        string status_godMode = _player_god_mode ? "On" : "Off";
        if (GUILayout.Button(status_godMode, _buttonStyle))
        {
            _player_god_mode = !_player_god_mode;
            NastyMod.Properties.Settings.Default.playerGodMode = _player_god_mode;
            NastyMod.Properties.Settings.Default.Save();
            MelonLogger.Msg("God Mode toggled!");
        }
        GUILayout.EndHorizontal();

        // ** Infinite Energy toggle
        GUILayout.BeginHorizontal();
        GUILayout.Label("Infinite Energy");
        string status_infiniteEnergy = _player_infinite_energy ? "On" : "Off";
        if (GUILayout.Button(status_infiniteEnergy, _buttonStyle))
        {
            _player_infinite_energy = !_player_infinite_energy;
            NastyMod.Properties.Settings.Default.playerInfiniteEnergy = _player_infinite_energy;
            NastyMod.Properties.Settings.Default.Save();
            MelonLogger.Msg("Infinite Energy toggled!");
        }
        GUILayout.EndHorizontal();

        // ** Infinite Stamina toggle
        GUILayout.BeginHorizontal();
        GUILayout.Label("Infinite Stamina");
        string status_infiniteStamina = _player_infinite_stamina ? "On" : "Off";
        if (GUILayout.Button(status_infiniteStamina, _buttonStyle))
        {
            _player_infinite_stamina = !_player_infinite_stamina;
            NastyMod.Properties.Settings.Default.playerInfiniteStamina = _player_infinite_stamina;
            NastyMod.Properties.Settings.Default.Save();
            MelonLogger.Msg("Infinite Stamina toggled!");
        }
        GUILayout.EndHorizontal();

        // ** Never wanted toggle
        GUILayout.BeginHorizontal();
        GUILayout.Label("Never Wanted");
        string status_neverWanted = _player_never_wanted ? "On" : "Off";
        if (GUILayout.Button(status_neverWanted, _buttonStyle))
        {
            _player_never_wanted = !_player_never_wanted;
            NastyMod.Properties.Settings.Default.playerNeverWanted = _player_never_wanted;
            NastyMod.Properties.Settings.Default.Save();
            MelonLogger.Msg("Never Wanted toggled!");
        }
        GUILayout.EndHorizontal();

        // Spacer
        GUILayout.Space(10);

        // ** Move speed slider
        GUILayout.BeginHorizontal();
        GUILayout.Label("Move speed multiplier: " + _player_move_speed_multiplier);
        _player_move_speed_multiplier = (int)GUILayout.HorizontalSlider(_player_move_speed_multiplier, 1f, 10f, GUILayout.Width(180), GUILayout.Height(10));
        if (NastyMod.Properties.Settings.Default.playerMoveSpeedMultiplier != _player_move_speed_multiplier)
        {
            NastyMod.Properties.Settings.Default.playerMoveSpeedMultiplier = (float)_player_move_speed_multiplier;
            NastyMod.Properties.Settings.Default.Save();
        }
        if (PlayerMovement.Instance.MoveSpeedMultiplier != _player_move_speed_multiplier)
        {
            PlayerMovement.Instance.MoveSpeedMultiplier = _player_move_speed_multiplier;
        }
        GUILayout.EndHorizontal();

        // ** Crouch speed slider
        GUILayout.BeginHorizontal();
        GUILayout.Label("Crouch speed multiplier: " + _player_crouch_speed_multiplier);
        _player_crouch_speed_multiplier = GUILayout.HorizontalSlider(_player_crouch_speed_multiplier, 0.6f, 10f, GUILayout.Width(180), GUILayout.Height(10));
        if (NastyMod.Properties.Settings.Default.playerCrouchSpeedMultiplier !=  _player_crouch_speed_multiplier)
        {
            NastyMod.Properties.Settings.Default.playerCrouchSpeedMultiplier = _player_crouch_speed_multiplier;
            NastyMod.Properties.Settings.Default.Save();
        }
        if (PlayerMovement.Instance.crouchSpeedMultipler != _player_crouch_speed_multiplier)
        { 
            PlayerMovement.Instance.crouchSpeedMultipler = _player_crouch_speed_multiplier;
        }
        GUILayout.EndHorizontal();

        // ** Jump Multiplier slider
        GUILayout.BeginHorizontal();
        GUILayout.Label("Jump Multiplier: " + _player_jump_multiplier);
        _player_jump_multiplier = (int)GUILayout.HorizontalSlider(_player_jump_multiplier, 1f, 10f, GUILayout.Width(180), GUILayout.Height(10));
        if (NastyMod.Properties.Settings.Default.playerJumpMultiplier != _player_jump_multiplier)
        {
            NastyMod.Properties.Settings.Default.playerJumpMultiplier = (float)_player_jump_multiplier;
            NastyMod.Properties.Settings.Default.Save();
        }
        if (PlayerMovement.JumpMultiplier != _player_jump_multiplier)
        { 
            PlayerMovement.JumpMultiplier = _player_jump_multiplier;
        }
        GUILayout.EndHorizontal();

        // Spacer
        GUILayout.Space(10);

        // ** EXP Modifier
        GUILayout.BeginHorizontal();
        GUILayout.Label("EXP: " + _player_exp_amount, _uniform_label);
        _player_exp_amount = (int)GUILayout.HorizontalSlider(_player_exp_amount, 0, 100000, GUILayout.Width((menu_width - (30 * 2)) - 350), GUILayout.Height(20));
        if (GUILayout.Button("Add", _buttonStyle))
        {
            Il2CppScheduleOne.Console.GiveXP command = new Il2CppScheduleOne.Console.GiveXP();
            Il2CppSystem.Collections.Generic.List<string> args = new Il2CppSystem.Collections.Generic.List<string>();

            args.Add(_player_exp_amount.ToString());

            command.Execute(args);
        }
        GUILayout.EndHorizontal();

        // ** Cash Modifier
        GUILayout.BeginHorizontal();
        GUILayout.Label("Cash: " + _player_cash_amount, _uniform_label);
        _player_cash_amount = (int)GUILayout.HorizontalSlider(_player_cash_amount, 0, 100000, GUILayout.Width((menu_width - (30 * 2)) - 435), GUILayout.Height(20));
        if (GUILayout.Button("Add", _buttonStyle))
        {
            Il2CppScheduleOne.Console.ChangeCashCommand command = new Il2CppScheduleOne.Console.ChangeCashCommand();
            Il2CppSystem.Collections.Generic.List<string> args = new Il2CppSystem.Collections.Generic.List<string>();

            args.Add(_player_cash_amount.ToString());

            command.Execute(args);
        }
        if (GUILayout.Button("Remove", _buttonStyle))
        {
            Il2CppScheduleOne.Console.ChangeCashCommand command = new Il2CppScheduleOne.Console.ChangeCashCommand();
            Il2CppSystem.Collections.Generic.List<string> args = new Il2CppSystem.Collections.Generic.List<string>();

            args.Add($"-{_player_cash_amount}");

            command.Execute(args);
        }
        GUILayout.EndHorizontal();

        // ** Balance Modifier
        GUILayout.BeginHorizontal();
        GUILayout.Label("Balance: " + _player_balance_amount, _uniform_label);
        _player_balance_amount = (int)GUILayout.HorizontalSlider(_player_balance_amount, 0, 100000, GUILayout.Width((menu_width - (30 * 2)) - 435), GUILayout.Height(20));
        if (GUILayout.Button("Add", _buttonStyle))
        {
            Il2CppScheduleOne.Console.ChangeOnlineBalanceCommand command = new Il2CppScheduleOne.Console.ChangeOnlineBalanceCommand();
            Il2CppSystem.Collections.Generic.List<string> args = new Il2CppSystem.Collections.Generic.List<string>();

            args.Add(_player_balance_amount.ToString());

            command.Execute(args);
        }
        if (GUILayout.Button("Remove", _buttonStyle))
        {
            Il2CppScheduleOne.Console.ChangeOnlineBalanceCommand command = new Il2CppScheduleOne.Console.ChangeOnlineBalanceCommand();
            Il2CppSystem.Collections.Generic.List<string> args = new Il2CppSystem.Collections.Generic.List<string>();

            args.Add($"-{_player_balance_amount.ToString()}");

            command.Execute(args);
        }
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }
    private void CheckPlayerMods()
    {
        if (!PlayerMovement.InstanceExists)
        {
            return;
        }

        if (_player_god_mode)
        {
            if (Player.Local.Health.CurrentHealth < PlayerHealth.MAX_HEALTH)
            {
                Player.Local.Health.RecoverHealth(PlayerHealth.MAX_HEALTH - Player.Local.Health.CurrentHealth);
            }
        }

        if (_player_infinite_energy)
        {
            if (Player.Local.Energy.CurrentEnergy < PlayerEnergy.MAX_ENERGY)
            {
                Player.Local.Energy.RestoreEnergy();
            }
        }

        if (_player_infinite_stamina)
        {
            // @todo only update when below max stamina
            PlayerMovement.Instance.ChangeStamina(100);
        }

        if (_player_never_wanted)
        {
            if (Player.Local.CrimeData.CurrentPursuitLevel != EPursuitLevel.None)
            {
                Player.Local.CrimeData.SetPursuitLevel(EPursuitLevel.None);
            }
        }
    }

    private void RenderWorldTab()
    {
        GUILayout.BeginArea(new Rect(15, 15, menu_width - (menu_spacing * 4), menu_height - 110 - (menu_spacing * 2)));

        // Tab Title
        GUILayout.Label("World", _headerStyle);

        // Spacer
        GUILayout.Space(10);

        // ** NPC Esp
        GUILayout.BeginHorizontal();
        GUILayout.Label("NPC Box ESP");
        string status_NPC_ESP = _world_npc_esp ? "On" : "Off";
        if (GUILayout.Button(status_NPC_ESP, _buttonStyle))
        {
            _world_npc_esp = !_world_npc_esp;
            NastyMod.Properties.Settings.Default.worldNpcEsp = _world_npc_esp;
            NastyMod.Properties.Settings.Default.Save();
            MelonLogger.Msg("NPC Box ESP toggled!");
        }
        GUILayout.EndHorizontal();

        // ** Player ESP
        GUILayout.BeginHorizontal();
        GUILayout.Label("Player Box ESP");
        string status_Player_ESP = _world_player_esp ? "On" : "Off";
        if (GUILayout.Button(status_Player_ESP, _buttonStyle))
        {
            _world_player_esp = !_world_player_esp;
            NastyMod.Properties.Settings.Default.worldPlayerEsp = _world_player_esp;
            NastyMod.Properties.Settings.Default.Save();
            MelonLogger.Msg("Player Box ESP toggled!");
        }
        GUILayout.EndHorizontal();

        // Spacer
        GUILayout.Space(10);

        // ** ESP Range slider
        GUILayout.BeginHorizontal();
        GUILayout.Label("ESP Range: " + _world_esp_range);
        _world_esp_range = (int)GUILayout.HorizontalSlider(_world_esp_range, 1f, 100f, GUILayout.Width(180), GUILayout.Height(10));
        if (NastyMod.Properties.Settings.Default.worldEspRange != _world_esp_range)
        {
            NastyMod.Properties.Settings.Default.worldEspRange = (float)_world_esp_range;
            NastyMod.Properties.Settings.Default.Save();
        }
        GUILayout.EndHorizontal();

        // Spacer
        GUILayout.Space(10);

        // ** Growing Speed
        GUILayout.BeginHorizontal();
        GUILayout.Label("Growth Speed multiplier: " + _world_grow_speed_multiplier);
        _world_grow_speed_multiplier = (int)GUILayout.HorizontalSlider(_world_grow_speed_multiplier, 1f, 100f, GUILayout.Width(180), GUILayout.Height(10));
        if (NastyMod.Properties.Settings.Default.worldGrowSpeedMultiplier != _world_grow_speed_multiplier)
        {
            NastyMod.Properties.Settings.Default.worldGrowSpeedMultiplier = (float)_world_grow_speed_multiplier;
            NastyMod.Properties.Settings.Default.Save();
        }
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }

    private void RenderSpawnerTab()
    {
        GUILayout.BeginArea(new Rect(15, 15, menu_width - (menu_spacing * 4), menu_height - 110 - (menu_spacing * 2)));

        // Tab Title
        GUILayout.Label("Spawner", _headerStyle);

        #region Left Side
        GUILayout.BeginArea(new Rect(0, 40, (menu_width - (menu_spacing * 4) - menu_spacing) / 4, menu_height - 110 - (menu_spacing * 2) - 40));

        // ** Item Category dropdown
        _spawner_category_scoll_pos = GUILayout.BeginScrollView(_spawner_category_scoll_pos, GUILayout.Width((menu_width - (menu_spacing * 4) - menu_spacing) / 4), GUILayout.Height(menu_height - 110 - (menu_spacing * 2) - 80 - menu_spacing));
        foreach (var category in items)
        {
            if (GUILayout.Button(category.Key))
            {
                _spawner_selected_category = category.Key;
                if (NastyMod.Properties.Settings.Default.spawnerSelectedCategory != _spawner_selected_category)
                {
                    NastyMod.Properties.Settings.Default.spawnerSelectedCategory = _spawner_selected_category;
                    NastyMod.Properties.Settings.Default.Save();
                }
            }
        }
        GUILayout.EndScrollView();

        // Spacer
        GUILayout.Space(10);

        // ** Item Amount slider
        GUILayout.Label($"Item Amount: {_spawner_item_amount}");
        _spawner_item_amount = (int)GUILayout.HorizontalSlider(_spawner_item_amount, 1, _misc_stack_size);
        if (NastyMod.Properties.Settings.Default.spawnerItemAmount != _spawner_item_amount)
        {
            NastyMod.Properties.Settings.Default.spawnerItemAmount = _spawner_item_amount;
            NastyMod.Properties.Settings.Default.Save();
        }

        GUILayout.EndArea();
        #endregion

        #region Right side
        GUILayout.BeginArea(new Rect(((menu_width - (menu_spacing * 4) - menu_spacing) / 4) + menu_spacing, 40, (((menu_width - (menu_spacing * 4)) / 4) * 3) - menu_spacing, menu_height - 110 - (menu_spacing * 2) - 40));

        // ** Item buttons
        int columns = 3;
        int currentColumn = 0;
        GUILayout.BeginHorizontal();
        foreach (var category in items)
        {
            if (category.Key == _spawner_selected_category)
            {
                foreach (var item in category.Value)
                {
                    if (currentColumn == columns)
                    {
                        currentColumn = 0;

                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                    }

                    int availableWidth = ((menu_width - (menu_spacing * 4) - menu_spacing) / 4) * 3;
                    if (GUILayout.Button(item.Value, GUILayout.Width((availableWidth - (5 * columns - 1)) / columns)))
                    {
                        Il2CppScheduleOne.Console.AddItemToInventoryCommand command = new Il2CppScheduleOne.Console.AddItemToInventoryCommand();
                        Il2CppSystem.Collections.Generic.List<string> args = new Il2CppSystem.Collections.Generic.List<string>();
                        args.Add(item.Key);
                        args.Add(_spawner_item_amount.ToString());
                        command.Execute(args);
                    }

                    currentColumn++;
                }
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
        #endregion

        GUILayout.EndArea();
    }

    private void RenderMiscTab()
    {
        GUILayout.BeginArea(new Rect(15, 15, menu_width - (menu_spacing * 4), menu_height - 110 - (menu_spacing * 2)));

        // Tab Title
        GUILayout.Label("Misc", _headerStyle);

        // Spacer
        GUILayout.Space(10);

        GUILayout.BeginArea(new Rect(0, 40, menu_width - (menu_spacing * 4), menu_height - 110 - (menu_spacing * 2) - 40));
        _misc_scroll_pos = GUILayout.BeginScrollView(_misc_scroll_pos, GUILayout.Width(menu_width - (menu_spacing * 4)), GUILayout.Height(menu_height - 110 - (menu_spacing * 2) - 40));

        // ** Default Stack Limit slider
        GUILayout.BeginHorizontal();
        GUILayout.Label("Default Stack Limit: " + _misc_stack_size);
        _misc_stack_size = (int)GUILayout.HorizontalSlider(_misc_stack_size, 10, 1000, GUILayout.Width(390), GUILayout.Height(10));
        if (NastyMod.Properties.Settings.Default.stackSize != _misc_stack_size)
        {
            NastyMod.Properties.Settings.Default.stackSize = _misc_stack_size;
            NastyMod.Properties.Settings.Default.Save();

            MelonCoroutines.Start(PatchStackLimit());
        }
        GUILayout.EndHorizontal();

        // Spacer
        GUILayout.Space(10);

        // ** Custom Deal Success Chance
        GUILayout.BeginHorizontal();
        GUILayout.Label("Custom Deal Success Chance");
        string status_use_custom_deal_success_chance = _misc_use_deal_success_chance ? "On" : "Off";
        if (GUILayout.Button(status_use_custom_deal_success_chance, _buttonStyle))
        {
            _misc_use_deal_success_chance = !_misc_use_deal_success_chance;
            NastyMod.Properties.Settings.Default.useDealSuccessChange = _misc_use_deal_success_chance;
            NastyMod.Properties.Settings.Default.Save();
            MelonLogger.Msg("Custom Deal Success Chance toggled!");
        }
        GUILayout.EndHorizontal();

        // ** Default Stack Limit slider
        GUILayout.BeginHorizontal();
        GUILayout.Label("Deal Success Chance: " + _misc_deal_sucess_chance);
        _misc_deal_sucess_chance = (int)GUILayout.HorizontalSlider(_misc_deal_sucess_chance, 0, 100, GUILayout.Width(390), GUILayout.Height(10));
        if (NastyMod.Properties.Settings.Default.dealSuccessChance != _misc_deal_sucess_chance)
        {
            NastyMod.Properties.Settings.Default.dealSuccessChance = (float)_misc_deal_sucess_chance;
            NastyMod.Properties.Settings.Default.Save();
        }
        GUILayout.EndHorizontal();

        // ** TrashGrabber Capacity
        /* GUILayout.BeginHorizontal();
        GUILayout.Label("Trash Grabber Capacity: " + _misc_trash_grabber_capacity);
        _misc_trash_grabber_capacity = (int)GUILayout.HorizontalSlider(_misc_trash_grabber_capacity, 10, 2500, GUILayout.Width(240), GUILayout.Height(10));
        GUILayout.EndHorizontal(); */

        // Spacer
        GUILayout.Space(10);

        // Note
        GUILayout.Label("Change product quality");
        GUILayout.Label("YOU NEED TO EQUIP THE ITEM FOR THIS FEATURE", _uniform_small_label);

        // ** Quality type buttons
        GUILayout.BeginHorizontal();
        foreach (var qualityType in Enum.GetValues(typeof(EQuality)))
        {
            if (GUILayout.Button(qualityType.ToString(), _buttonStyle))
            {
                Il2CppScheduleOne.Console.SetQuality command = new Il2CppScheduleOne.Console.SetQuality();
                Il2CppSystem.Collections.Generic.List<string> args = new Il2CppSystem.Collections.Generic.List<string>();
                args.Add(qualityType.ToString());
                command.Execute(args);
            }
        }
        GUILayout.EndHorizontal();

        // Spacer
        GUILayout.Space(10);

        // Note
        GUILayout.Label("Package product");
        GUILayout.Label("YOU NEED TO EQUIP THE ITEM FOR THIS FEATURE", _uniform_small_label);

        // ** Packaging type buttons
        GUILayout.BeginHorizontal();
        foreach (var packagingType in _misc_packaging_types)
        {
            if (GUILayout.Button(packagingType.Value, _buttonStyle))
            {
                Il2CppScheduleOne.Console.PackageProduct command = new Il2CppScheduleOne.Console.PackageProduct();
                Il2CppSystem.Collections.Generic.List<string> args = new Il2CppSystem.Collections.Generic.List<string>();
                args.Add(packagingType.Key);
                command.Execute(args);
                MelonLogger.Msg($"Packaging product as {packagingType.Value}");
            }
        }
        GUILayout.EndHorizontal();

        // Spacer
        GUILayout.Space(20);

        // ** Unlock all NPCs
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Unlock all NPCs", GUILayout.Width(menu_width - (menu_spacing * 4) - menu_spacing - 10), GUILayout.Height(24)))
        {
            foreach (NPC npc in NPCManager.NPCRegistry)
            {
                try
                {
                    NPCRelationData relation = npc.RelationData;
                    if (relation != null)
                    {
                        relation.Unlock(NPCRelationData.EUnlockType.Recommendation, true);
                        MelonLogger.Msg($"Unlocked NPC: {npc.FirstName ?? npc.ID} {npc.LastName ?? ""}");
                    }
                } catch {
                    MelonLogger.Error($"Failed to unlock NPC: {npc.FirstName ?? npc.ID} {npc.LastName ?? ""}");
                }
            }

            MelonLogger.Msg("Successfully unlocked all NPCs");
        }
        GUILayout.EndHorizontal();

        // ** Unlock all Properties
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Unlock all Properties", GUILayout.Width(menu_width - (menu_spacing * 4) - menu_spacing - 10), GUILayout.Height(24)))
        {
            foreach (var property in PropertyManager.FindObjectsOfType<Property>())
            {
                try
                {
                    property.SetOwned();
                    MelonLogger.Msg($"Unlocked Property: {property.propertyName ?? property.propertyCode}");
                } catch
                {
                    MelonLogger.Error($"Failed to unlock Property: {property.propertyName ?? property.propertyCode}");
                }
            }

            MelonLogger.Msg("Successfully unlocked all Properties");
        }
        GUILayout.EndHorizontal();

        // ** Unlock all Achievements
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Unlock all Achievements", GUILayout.Width(menu_width - (menu_spacing * 4) - menu_spacing - 10), GUILayout.Height(24)))
        {
            foreach (var achievement in Enum.GetValues(typeof(AchievementManager.EAchievement)))
            {
                AchievementManager.Instance.UnlockAchievement((AchievementManager.EAchievement)achievement);
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.EndScrollView();
        GUILayout.EndArea();

        GUILayout.EndArea();
    }

    private void RenderEmployeesTab()
    {
        GUILayout.BeginArea(new Rect(15, 15, menu_width - (menu_spacing * 4), menu_height - 110 - (menu_spacing * 2)));

        // Tab Title
        GUILayout.Label($"Employees on Property \"{teleports["Properties"][_employees_selected_property]}\"", _headerStyle);

        // Spacer
        GUILayout.Space(10);

        #region Left Side
        GUILayout.BeginArea(new Rect(0, 40, (menu_width - (menu_spacing * 4) - menu_spacing) / 4, menu_height - 110 - (menu_spacing * 2) - 40));
        // ** Property buttons
        _employees_category_scroll_pos = GUILayout.BeginScrollView(_employees_category_scroll_pos, GUILayout.Width((menu_width - (menu_spacing * 4) - menu_spacing) / 4), GUILayout.Height(menu_height - 110 - (menu_spacing * 2) - 80 - menu_spacing));
        foreach (var property in teleports["Properties"])
        {
            if (GUILayout.Button(property.Value))
            {
                _employees_selected_property = property.Key;
                if (NastyMod.Properties.Settings.Default.employeesSelectedProperty != _employees_selected_property)
                {
                    NastyMod.Properties.Settings.Default.employeesSelectedProperty = _employees_selected_property;
                    NastyMod.Properties.Settings.Default.Save();
                }
            }
        }
        GUILayout.EndScrollView();
        GUILayout.EndArea();
        #endregion

        #region Right side
        GUILayout.BeginArea(new Rect(((menu_width - (menu_spacing * 4) - menu_spacing) / 4) + menu_spacing, 40, (((menu_width - (menu_spacing * 4)) / 4) * 3) - menu_spacing, menu_height - 110 - (menu_spacing * 2) - 40));

        _employees_scroll_pos = GUILayout.BeginScrollView(_employees_scroll_pos, GUILayout.Width((((menu_width - (menu_spacing * 4) - menu_spacing) / 4) * 3) - menu_spacing), GUILayout.Height(menu_height - 110 - (menu_spacing * 2) - 80 - menu_spacing));
        // ** Employee buttons
        Dictionary<string, Dictionary<string, List<Employee>>> employees = new Dictionary<string, Dictionary<string, List<Employee>>>();
        employees = GetAlLEmployees();

        if (!employees.ContainsKey(_employees_selected_property))
        {
            GUILayout.Label($"No Employees on Property \"{_employees_selected_property}\"");
        } else
        {
            foreach (var employeeType in employees[_employees_selected_property])
            {
                GUILayout.Label(employeeType.Key, _subHeaderStyle);

                foreach (var employee in employeeType.Value)
                {
                    if (!_employees_employee_speed.ContainsKey(employeeType.Key))
                    {
                        _employees_employee_speed.Add(employeeType.Key, new Dictionary<string, float> { { employee.ID, employee.Movement.MoveSpeedMultiplier } });
                    }
                    if (!_employees_employee_speed[employeeType.Key].ContainsKey(employee.ID))
                    {
                        _employees_employee_speed[employeeType.Key].Add(employee.ID, employee.Movement.MoveSpeedMultiplier);
                    }

                    string currentEmployeeType = employee.EmployeeType.ToString();
                    string currentEmployeeName = employee.FirstName;

                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"{currentEmployeeName}");

                    if (GUILayout.Button("TP", GUILayout.Width(60)))
                    {
                        employee.gameObject.transform.position = Player.Local.transform.position;
                    }
                    if (GUILayout.Button("Fire", GUILayout.Width(60)))
                    {
                        employee.SendFire();
                        employee.Movement.MoveSpeedMultiplier = 2.5f;
                    }

                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("> Speed: " + _employees_employee_speed[employeeType.Key][employee.ID], _uniform_small_label);
                    _employees_employee_speed[employeeType.Key][employee.ID] = (int)GUILayout.HorizontalSlider(_employees_employee_speed[employeeType.Key][employee.ID], 0, 10, GUILayout.Width(140), GUILayout.Height(10));
                    if (employee.Movement.MoveSpeedMultiplier.ToString("F2") != _employees_employee_speed[employeeType.Key][employee.ID].ToString())
                    {
                        employee.Movement.MoveSpeedMultiplier = (float)_employees_employee_speed[employeeType.Key][employee.ID];
                        SetEmployeeSpeeds(employee.GUID, _employees_employee_speed[employeeType.Key][employee.ID]);
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.Space(10);
            }
        }
        GUILayout.EndScrollView();

        // Spacer
        GUILayout.Space(10);

        // ** Add Employee buttons
        GUILayout.BeginHorizontal();
        foreach (var employeeType in Enum.GetValues(typeof(EEmployeeType)))
        {
            if (GUILayout.Button($"Add {employeeType.ToString()}"))
            {
                Il2CppScheduleOne.Console.AddEmployeeCommand command = new Il2CppScheduleOne.Console.AddEmployeeCommand();
                Il2CppSystem.Collections.Generic.List<string> args = new Il2CppSystem.Collections.Generic.List<string>();

                args.Add(employeeType.ToString());
                args.Add(_employees_selected_property);

                command.Execute(args);
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
        #endregion

        GUILayout.EndArea();
    }

    private void RenderTeleportTab()
    {
        GUILayout.BeginArea(new Rect(15, 15, menu_width - (menu_spacing * 4), menu_height - 110 - (menu_spacing * 2)));

        // Tab Title
        GUILayout.Label("Teleport", _headerStyle);

        #region Left Side
        GUILayout.BeginArea(new Rect(0, 40, (menu_width - (menu_spacing * 4) - menu_spacing) / 4, menu_height - 110 - (menu_spacing * 2) - 40));

        // ** Item Category dropdown
        _teleport_category_scroll_pos = GUILayout.BeginScrollView(_teleport_category_scroll_pos, GUILayout.Width((menu_width - (menu_spacing * 4) - menu_spacing) / 4), GUILayout.Height(menu_height - 110 - (menu_spacing * 2) - 80 - menu_spacing));
        foreach (var category in teleports)
        {
            if (GUILayout.Button(category.Key))
            {
                _teleport_selected_category = category.Key;
                if (NastyMod.Properties.Settings.Default.teleportSelectedCategory != _teleport_selected_category)
                {
                    NastyMod.Properties.Settings.Default.teleportSelectedCategory = _teleport_selected_category;
                    NastyMod.Properties.Settings.Default.Save();
                }
            }
        }
        GUILayout.EndScrollView();

        GUILayout.EndArea();
        #endregion

        #region Right side
        GUILayout.BeginArea(new Rect(((menu_width - (menu_spacing * 4) - menu_spacing) / 4) + menu_spacing, 40, ((menu_width - (menu_spacing * 4) - menu_spacing) / 4) * 3, menu_height - 110 - (menu_spacing * 2) - 40));

        // ** Teleport buttons
        int columns = 3;
        int currentColumn = 0;
        GUILayout.BeginHorizontal();
        foreach (var category in teleports)
        {
            if (category.Key == _teleport_selected_category)
            {
                foreach (var item in category.Value)
                {
                    if (currentColumn == columns)
                    {
                        currentColumn = 0;

                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                    }

                    int availableWidth = ((menu_width - (menu_spacing * 4) - menu_spacing) / 4) * 3;
                    if (GUILayout.Button(item.Value, GUILayout.Width((availableWidth - (5 * columns - 1)) / columns)))
                    {
                        if (category.Key == "Custom")
                        {
                            Vector3 pos = new Vector3(float.Parse(item.Key.Split(' ')[0]), float.Parse(item.Key.Split(' ')[1]), float.Parse(item.Key.Split(' ')[2]));
                            Player.Local.transform.position = pos;
                        }
                        else
                        {
                            Il2CppScheduleOne.Console.Teleport command = new Il2CppScheduleOne.Console.Teleport();
                            Il2CppSystem.Collections.Generic.List<string> args = new Il2CppSystem.Collections.Generic.List<string>();
                            args.Add(item.Key);
                            command.Execute(args);
                        }
                    }

                    currentColumn++;
                }
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
        #endregion

        GUILayout.EndArea();
    }

    private void RenderCreditsTab()
    {
        GUILayout.BeginArea(new Rect(15, 15, menu_width - (menu_spacing * 4), menu_height - 110 - (menu_spacing * 2)));

        // Tab Title
        GUILayout.Label("Credits", _headerStyle);

        // Spacer
        GUILayout.Space(10);

        // ** Creator
        GUILayout.Label("This mod was created by nasty.codes");
        GUILayout.Label("Discord: nasty.codes");

        // Spacer
        GUILayout.Space(20);

        // ** Contributors
        GUILayout.Label("Jumpman - Initial Box ESP feature in 1.0.2 (reworked by nasty.codes)");

        // Spacer
        GUILayout.Space(20);

        // **Thanks**
        GUILayout.Label("Thanks to");
        GUILayout.Label("GitHub Copilot");
        GUILayout.Label("MelonLoader");

        GUILayout.EndArea();
    }
    #endregion

    #region Custom Functions
    public void SendLoggerMsg(string msg)
    {
        MelonLogger.Msg(msg);
    }

    public float GetDealSuccessChance()
    {
        return _misc_deal_sucess_chance/100;
    }

    public int GetStackSize()
    {
        return _misc_stack_size;
    }

    public void GUIFunctions()
    {
        if (!PlayerMovement.InstanceExists)
        {
            return;
        }

        // ** ESP
        if (_world_player_esp)
        {
            foreach (Player player in Player.PlayerList)
            {
                Vector3 pivotPos = player.gameObject.transform.position;

                // Use the foot position as is, and calculate the head position based on the NPC's height
                Vector3 w2s_footpos = Camera.main.WorldToScreenPoint(pivotPos);

                // Get NPC height from mesh bounds
                MeshRenderer meshRenderer = player.GetComponent<MeshRenderer>();
                float npcHeight = meshRenderer != null ? meshRenderer.bounds.size.y : 1.9f; // Static default height if no MeshRenderer found

                // Set head position based on NPC height
                Vector3 playerHeadPos = pivotPos;
                playerHeadPos.y += npcHeight;

                Vector3 w2s_headpos = Camera.main.WorldToScreenPoint(playerHeadPos);

                if (w2s_footpos.x < 0 || w2s_footpos.x > Screen.width ||
                    w2s_footpos.y < 0 || w2s_footpos.y > Screen.height ||
                    w2s_headpos.x < 0 || w2s_headpos.x > Screen.width ||
                    w2s_headpos.y < 0 || w2s_headpos.y > Screen.height)
                {
                    continue; // Nicht rendern, wenn die Box außerhalb des Bildschirms ist
                }

                if (w2s_footpos.z > 0f && w2s_footpos.z < _world_esp_range)
                {
                    DrawSkewedESP(w2s_footpos, w2s_headpos, Color.blue);
                }
            }
        }

        // ** ESP
        if (_world_npc_esp)
        {
            foreach (NPC npc in NPCManager.NPCRegistry)
            {
                if (npc.FirstName == String.Empty) continue;

                Vector3 pivotPos = npc.gameObject.transform.position;

                // Use the foot position as is, and calculate the head position based on the NPC's height
                Vector3 w2s_footpos = Camera.main.WorldToScreenPoint(pivotPos);

                // Get NPC height from mesh bounds
                MeshRenderer meshRenderer = npc.GetComponent<MeshRenderer>();
                float npcHeight = meshRenderer != null ? meshRenderer.bounds.size.y : 1.9f; // Static default height if no MeshRenderer found

                // Set head position based on NPC height
                Vector3 playerHeadPos = pivotPos;
                playerHeadPos.y += npcHeight;

                Vector3 w2s_headpos = Camera.main.WorldToScreenPoint(playerHeadPos);

                if (w2s_footpos.x < 0 || w2s_footpos.x > Screen.width ||
                    w2s_footpos.y < 0 || w2s_footpos.y > Screen.height ||
                    w2s_headpos.x < 0 || w2s_headpos.x > Screen.width ||
                    w2s_headpos.y < 0 || w2s_headpos.y > Screen.height)
                {
                    continue; // Nicht rendern, wenn die Box außerhalb des Bildschirms ist
                }

                if (w2s_footpos.z > 0f && w2s_footpos.z < _world_esp_range)
                {
                    DrawSkewedESP(w2s_footpos, w2s_headpos, Color.red);
                }
            }
        }
    }

    private static void UnlockDebugMode()
    {
        try
        {
            MelonLogger.Msg("Attempting to unlock Debug Mode...");
            HarmonyLib.Harmony.CreateAndPatchAll(typeof(DebugPatch), (string)null);
            MelonLogger.Msg("Debug Mode unlocked!");
        }
        catch (Exception value)
        {
            MelonLogger.Error($"Error unlocking Debug Mode: {value}");
        }
    }

    private Dictionary<string, Dictionary<string, string>> LoadJSON(string resource)
    {
        Dictionary<string, Dictionary<string, string>> returnObject = new Dictionary<string, Dictionary<string, string>>();

        using (System.IO.Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
        {
            if (stream == null)
            {
                MelonLogger.Error($"Failed loading {resource}. Please set it as embedded resource");
            }

            using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
            {
                string json = reader.ReadToEnd();

                try
                {
                    var categories = Newtonsoft.Json.JsonConvert.DeserializeObject<Root>(json);
                    if (categories != null)
                    {
                        foreach (var category in categories.Categories)
                        {
                            returnObject[category.Category] = category.Items;
                        }
                    }

                    MelonLogger.Msg($"{resource} loaded successfully.");
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Failed to parse {resource}: {ex.Message}");
                }
            }
        }

        return returnObject;
    }

    public Dictionary<string, Dictionary<string, List<Employee>>> GetAlLEmployees()
    {
        Dictionary<string, Dictionary<string, List<Employee>>> employees = new Dictionary<string, Dictionary<string, List<Employee>>>();

        foreach (var employee in EmployeeManager.Instance.AllEmployees.ToArray())
        {
            // MelonLogger.Msg("Employee: " + employee.FirstName + " " + employee.LastName + " " + employee.EmployeeType + " " + employee.AssignedProperty.propertyCode);

            if (employees.ContainsKey(employee.AssignedProperty.propertyCode))
            {
                if (employees[employee.AssignedProperty.propertyCode].ContainsKey(employee.EmployeeType.ToString()))
                {
                    employees[employee.AssignedProperty.propertyCode][employee.EmployeeType.ToString()].Add(employee);
                }
                else
                {
                    employees[employee.AssignedProperty.propertyCode].Add(employee.EmployeeType.ToString(), new List<Employee> { employee });
                }
            }
            else
            {
                employees.Add(employee.AssignedProperty.propertyCode, new Dictionary<string, List<Employee>> { { employee.EmployeeType.ToString(), new List<Employee> { employee } } });
            }
        }
        return employees;
    }

    public void DrawSkewedESP(Vector3 footpos, Vector3 headpos, Color color)
    {
        float npcHeight = headpos.y - footpos.y;
        float npcWidth = npcHeight / 2.7f;

        Vector3 screen_topLeft = new Vector3(headpos.x - npcWidth / 2, headpos.y, headpos.z);
        Vector3 screen_topRight = new Vector3(headpos.x + npcWidth / 2, headpos.y, headpos.z);
        Vector3 screen_bottomLeft = new Vector3(footpos.x - npcWidth / 2, footpos.y, footpos.z);
        Vector3 screen_bottomRight = new Vector3(footpos.x + npcWidth / 2, footpos.y, footpos.z);

        Render.DrawLine(new Vector2(screen_topLeft.x, Screen.height - screen_topLeft.y),
                        new Vector2(screen_topRight.x, Screen.height - screen_topRight.y), color, 2f);

        Render.DrawLine(new Vector2(screen_topRight.x, Screen.height - screen_topRight.y),
                        new Vector2(screen_bottomRight.x, Screen.height - screen_bottomRight.y), color, 2f);

        Render.DrawLine(new Vector2(screen_bottomRight.x, Screen.height - screen_bottomRight.y),
                        new Vector2(screen_bottomLeft.x, Screen.height - screen_bottomLeft.y), color, 2f);

        Render.DrawLine(new Vector2(screen_bottomLeft.x, Screen.height - screen_bottomLeft.y),
                        new Vector2(screen_topLeft.x, Screen.height - screen_topLeft.y), color, 2f);
    }
    #endregion

    #region Unity Functions
    private bool IsCameraReady()
    {
        return Camera.main != null;
    }

    private GameObject FindGameObjectByPath(string path)
    {
        Transform transform = null;
        string[] array = path.Split('/');
        foreach (string text in array)
        {
            if (transform == null)
            {
                GameObject gameObject = GameObject.Find(text);
                if (gameObject == null)
                {
                    return null;
                }

                transform = gameObject.transform;
            }
            else
            {
                transform = transform.Find(text);
                if (transform == null)
                {
                    return null;
                }
            }
        }

        return transform?.gameObject;
    }
    
    private void ToggleGameObject(GameObject gameObject, bool state)
    {
        gameObject.SetActive(state);
    }
    #endregion

    #region JSON structure
    class Root
    {
        public List<CategoryEntry> Categories { get; set; }
    }

    class CategoryEntry
    {
        public string Category { get; set; }
        public Dictionary<string, string> Items { get; set; }
    }
    #endregion

    #region Harmony Patches
    [HarmonyPrefix]
    [HarmonyPatch("Escalate")]
    private static bool Escalate_Prefix(PlayerCrimeData __instance)
    {
        if (NastyModClass.Instance != null)
        {
            return !NastyModClass.Instance._player_never_wanted;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch("RpcLogic___set_CurrentPursuitLevel_2979171596")]
    private static bool RpcSetPursuitLevel_Prefix(PlayerCrimeData __instance, ref EPursuitLevel value)
    {
        if (NastyModClass.Instance != null)
        {
            if (NastyModClass.Instance._player_never_wanted && value != EPursuitLevel.None)
            {
                value = EPursuitLevel.None;
            }

            return true;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch("SetPursuitLevel")]
    private static bool SetPursuitLevel_Prefix(PlayerCrimeData __instance, ref EPursuitLevel level)
    {
        if (NastyModClass.Instance != null)
        {
            if (NastyModClass.Instance._player_never_wanted && level != EPursuitLevel.None)
            {
                level = EPursuitLevel.None;
            }

            return true;
        }

        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch("Update")]
    private static bool Update_Prefix(PlayerCrimeData __instance)
    {
        if (NastyModClass.Instance != null)
        {
            if (NastyModClass.Instance._player_never_wanted)
            {
                __instance.Player.VisualState.RemoveState("Wanted", 0f);
                __instance.Player.VisualState.RemoveState("SearchedFor", 0f);
                return false;
            }


            return true;
        }

        return true;
    }

    [HarmonyPatch]
    public class OfferSuccessChancePatch
    {
        [HarmonyPatch(typeof(Customer), "GetOfferSuccessChance")]
        [HarmonyPostfix]
        public static void Postfix(ref float __result, List<ItemInstance> items, float askingPrice)
        {
            if (NastyModClass.Instance != null && NastyModClass.Instance._misc_use_deal_success_chance)
            {
                __result = NastyModClass.Instance.GetDealSuccessChance();
            }
        }
    }

    [HarmonyPatch]
    public class CreateDeliveryDisplayPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(DeliveryApp), "CreateDeliveryStatusDisplay")]
        public static void Postfix(DeliveryInstance instance)
        {
            if (NastyModClass.Instance != null)
            {
                MelonLogger.Msg($"A Delivery Status Display was created.");
                if (instance.Items != null) {
                    foreach (var item in instance.Items)
                    {
                        MelonLogger.Msg($"- item: {item.String} x{item.Int}");
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(Pot), "GetAdditiveGrowthMultiplier")]
    public class PlantGetAdditiveGrowthMultiplierPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ref float __result)
        {
            if (NastyModClass.Instance != null)
            {
                __result *= NastyModClass.Instance._world_grow_speed_multiplier;
            }
        }
    }

    [HarmonyPatch]
    public class SetItemSlotQuantityPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(StorageEntity), "SetItemSlotQuantity")]
        public static void SetItemSlotQuantity(int itemSlotIndex, int quantity)
        {
            if (NastyModClass.Instance != null)
            {
                MelonLogger.Msg($"StorageEntity.SetItemSlotQuantity (index: {itemSlotIndex}, quantity: {quantity})");
            }
        }
    }

    [HarmonyPatch]
    public class RPCSetItemSlotQuantityPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(StorageEntity), "RpcLogic___SetItemSlotQuantity_1692629761")]
        public static void RpcSetItemSlotQuantity(int itemSlotIndex, int quantity)
        {
            if (NastyModClass.Instance != null)
            {
                MelonLogger.Msg($"And then the RPC Item Slot Quantity is set (index: {itemSlotIndex}, quantity: {quantity}.");
            }
        }
    }

    [HarmonyPatch]
    public static class DebugPatch
    {
        [HarmonyPatch(typeof(Debug), "get_isDebugBuild")]
        [HarmonyPostfix]
        public static void Postfix(ref bool __result)
        {
            __result = true;
        }
    }
    #endregion
}
