using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using PieceManager;
using ServerSync;
using UnityEngine;

namespace ItemDrawers_Remake
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class ItemDrawersMod : BaseUnityPlugin
    {
        internal const string ModName = "ItemDrawersMod";
        internal const string ModVersion = "1.0";
        private const string ModGUID = "com.zarboz.drawers";
        private static Harmony harmony = null!;

        #region ConfigSync

        ConfigSync configSync = new(ModGUID) 
            { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion};
        internal static ConfigEntry<bool> ServerConfigLocked = null!;
        ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
        {
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }
        ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true) => config(group, name, value, new ConfigDescription(description), synchronizedSetting);


        #endregion
        
        private static ConfigEntry<bool> _retreiveEnabled;
        private static ConfigEntry<float> _retreiveRadius;
        private static ConfigEntry<bool> _enabled;
        private static ConfigEntry<int> _maxItems;
        internal static ConfigEntry<KeyCode> _configKeyDepositAll;
        internal static ConfigEntry<KeyCode> _configKeyWithdrawOne;
        internal static ConfigEntry<KeyCode> _configKeyClear;
        internal static ConfigEntry<Color> _enabledColorOpacity;
        internal static ConfigEntry<Color> _disabledColorOpacity;
        internal static ConfigEntry<bool> _rotateAtPlayer;
        public BuildPiece itemdrawerJude { get; set; }  
        public void Awake()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            itemdrawerJude = new BuildPiece("item_drawer", "piece_judeDrawer", "assets");
            itemdrawerJude.Name.English("Drawer Stack");
            itemdrawerJude.Description.English("A Stack of drawers for storing things");
            itemdrawerJude.RequiredItems.Add("FineWood", 10, true);
            itemdrawerJude.Category.Add(BuildPieceCategory.Furniture);
            itemdrawerJude.Crafting.Set(CraftingTable.Workbench);
            LoadConfig();
            ApplyConfig(itemdrawerJude.Prefab);
            
            harmony = new(ModGUID);
            harmony.PatchAll(assembly);
        }
        
        public void OnDestroy() => harmony.UnpatchSelf();

        private void LoadConfig()
        {
            ServerConfigLocked = config("1 - General", "Lock Configuration", true, "If on, the configuration is locked and can be changed by server admins only.");
            _enabled = config<bool>("1 - General", "Enabled", true, "Enable creation of Item Drawers");
            _maxItems = config<int>("General", "MaxItems", 9999, new ConfigDescription("The maximum number of items that can be stored in a drawer", (AcceptableValueBase) new AcceptableValueRange<int>(0, 9999), Array.Empty<object>()));
            _retreiveEnabled = config<bool>("Item Retreival", "Enabled", true, "Drawers will retrieve dropped items matching their item");
            _retreiveRadius = config<float>("Item Retreival", "Radius", 5f, new ConfigDescription("The distance drawers will check for dropped items", (AcceptableValueBase) new AcceptableValueRange<float>(0.0f, 30f), new object[1]));
            _configKeyDepositAll = config<KeyCode>("Hotkeys", "Deposit All", KeyCode.LeftShift, "Hold while interacting to deposit all", false);
            _configKeyWithdrawOne = config<KeyCode>("Hotkeys", "Withdraw One", KeyCode.LeftAlt, "Hold while interacting to withdraw one", false);
            _configKeyClear = config<KeyCode>("Hotkeys", "DrawerClear", KeyCode.LeftAlt, "Hold while interacting to clear contents (only if 0 quantity)", false);
            _enabledColorOpacity = config<Color>("1 - General", "Icon Opacity Enabled", Color.white, new ConfigDescription("This is the default opacity for the icon when it is enabled", (AcceptableValueBase) null, Array.Empty<object>()));
            _disabledColorOpacity = config<Color>("1 - General", "Icon Opacity Disabled", Color.clear, new ConfigDescription("This is the default opacity for the icon when it is disabled", (AcceptableValueBase) null, Array.Empty<object>()));
            _rotateAtPlayer = config<bool>("1 - General", "Should Icon on alt drawers rotate", true, "When set to true the icons on alt drawers will rotate towards the camera");
            configSync.AddLockingConfigEntry<bool>(ServerConfigLocked);
        }

        internal static void ApplyConfig(GameObject gameObject)
        {
            DrawerContainer component = gameObject.GetComponent<DrawerContainer>();
            component.MaxItems = _maxItems.Value;
            component.RetreiveEnabled = _retreiveEnabled.Value;
            component.RetrieveRadius = (int) _retreiveRadius.Value;
            component._text.SetText(_maxItems.Value.ToString());
        }

    }
}