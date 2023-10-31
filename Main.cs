global using static KitchenRenovation.Main;
global using static KitchenLib.Utils.GDOUtils;
global using static KitchenLib.Utils.LocalisationUtils;
global using static KitchenLib.Utils.KitchenPropertiesUtils;
using KitchenData;
using KitchenLib;
using KitchenLib.Customs;
using KitchenLib.Event;
using KitchenLib.Utils;
using KitchenMods;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using KitchenLib.Views;
using KitchenRenovation.Utility;
using KitchenRenovation.Views;

namespace KitchenRenovation
{
    public class Main : BaseMod
    {
        public const string NAME = "Renovation";
        public const string GUID = "nova.renovation";
        public const string VERSION = "1.0.0";

        public Main() : base(GUID, NAME, "Zoey Davis", VERSION, ">=1.0.0", Assembly.GetExecutingAssembly()) { }

        private static AssetBundle Bundle;

        // References
        public static CustomViewType PurchaseView;

        private void PostActivate()
        {
            //AddIcons();

            PurchaseView = AddViewType("PurchaseView", SetupPurchaseView);
        }

        private void BuildGameData(GameData gameData)
        {

        }

        private GameObject SetupPurchaseView()
        {
            var prefab = GetPrefab("Purchase Indicator");
            prefab.TryAddComponent<CostIndicatorView>().Text =
                prefab.CreateLabel("Coins", Vector3.zero, Quaternion.identity, MaterialUtils.GetExistingMaterial("Alphakind Atlas Material"), FontUtils.GetExistingTMPFont("Large Text"),
                0, 2f, "<color=#ff1111>999 <sprite name=\"coin\" color=#FF9800>").GetComponent<TextMeshPro>();
            return prefab;
        }

        internal void AddIcons()
        {
            Bundle.LoadAllAssets<Texture2D>();
            Bundle.LoadAllAssets<Sprite>();

            var icons = Bundle.LoadAsset<TMP_SpriteAsset>("Icon Asset");
            icons.material = Object.Instantiate(TMP_Settings.defaultSpriteAsset.material);
            icons.material.mainTexture = Bundle.LoadAsset<Texture2D>("Icon Texture");
            TMP_Settings.defaultSpriteAsset.fallbackSpriteAssets.Add(icons);

            Log("Registered icons");
        }

        #region Logging
        internal static void LogInfo(string msg) { Debug.Log($"[{NAME}] " + msg); }
        internal static void LogWarning(string msg) { Debug.LogWarning($"[{NAME}] " + msg); }
        internal static void LogError(string msg) { Debug.LogError($"[{NAME}] " + msg); }
        internal static void LogInfo(object msg) { LogInfo(msg.ToString()); }
        internal static void LogWarning(object msg) { LogWarning(msg.ToString()); }
        internal static void LogError(object msg) { LogError(msg.ToString()); }
        #endregion

        #region Registry
        protected override void OnPostActivate(Mod mod)
        {
            Bundle = mod.GetPacks<AssetBundleModPack>().SelectMany(e => e.AssetBundles).First();

            PostActivate();

            AddGameData();

            Events.BuildGameDataEvent += (s, args) => BuildGameData(args.gamedata);
        }

        internal void AddGameData()
        {
            MethodInfo AddGDOMethod = typeof(BaseMod).GetMethod(nameof(BaseMod.AddGameDataObject));
            int counter = 0;
            Log("Registering GameDataObjects.");
            foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsAbstract || typeof(IWontRegister).IsAssignableFrom(type))
                    continue;

                if (!typeof(CustomGameDataObject).IsAssignableFrom(type))
                    continue;

                MethodInfo generic = AddGDOMethod.MakeGenericMethod(type);
                generic.Invoke(this, null);
                counter++;
            }
            Log($"Registered {counter} GameDataObjects.");
        }

        public interface IWontRegister { }
        #endregion

        #region Utility
        public static T GetGDO<T>(int id) where T : GameDataObject => GDOUtils.GetExistingGDO(id) as T;

        public static GameObject GetPrefab(string name) => Bundle.LoadAsset<GameObject>(name);
        public static T GetAsset<T>(string name) where T : Object => Bundle.LoadAsset<T>(name);
        #endregion
    }
}
