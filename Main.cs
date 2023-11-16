global using static KitchenLib.Utils.GDOUtils;
global using static KitchenLib.Utils.KitchenPropertiesUtils;
global using static KitchenLib.Utils.LocalisationUtils;
global using static KitchenRenovation.Main;
using KitchenData;
using KitchenLib;
using KitchenLib.Customs;
using KitchenLib.Event;
using KitchenLib.Utils;
using KitchenLib.Views;
using KitchenMods;
using KitchenRenovation.Utility;
using KitchenRenovation.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;

namespace KitchenRenovation
{
    public class Main : BaseMod
    {
        public const string NAME = "Renovation";
        public const string GUID = "nova.renovation";
        public const string VERSION = "1.0.4";

        public Main() : base(GUID, NAME, "Zoey Davis", VERSION, ">=1.0.0", Assembly.GetExecutingAssembly()) { }

        private static AssetBundle Bundle;

        // References
        public static SoundEvent DestroySoundEvent;
        public static SoundEvent ExplosionSoundEvent;
        public static CustomViewType PurchaseView;
        public static CustomViewType RenovationView;
        public static CustomViewType ParticleEventView;
        public static ShoppingTags MiscShoppingTag = (ShoppingTags)1048576;
        public static ShoppingTags RemovalShoppingTag = (ShoppingTags)524288;


        private void PostActivate()
        {
            AddIcons();

            PurchaseView = AddViewType("PurchaseView", SetupPurchaseView);
            RenovationView = AddViewType("RenovationView", SetupRenovationView);
            ParticleEventView = AddViewType("ParticleEvent", SetupParticleEventView);
        }

        private void BuildGameData(GameData gameData)
        {
            Bundle.LoadAllAssets<AudioClip>();
            SetupDestroySoundEvents(gameData);
            SetupExplosionSoundEvents(gameData);
            PreferenceOverrides(gameData);
        }

        private void SetupDestroySoundEvents(GameData gameData)
        {
            DestroySoundEvent = (SoundEvent)VariousUtils.GetID(GUID + "-DESTROY");

            if (!gameData.ReferableObjects.Clips.ContainsKey(DestroySoundEvent))
                gameData.ReferableObjects.Clips.Add(DestroySoundEvent, new AudioAssetRandom());

            var d1 = GetAsset<AudioClip>("Destroy_1"); d1.LoadAudioData();
            var d2 = GetAsset<AudioClip>("Destroy_2"); d2.LoadAudioData();
            var d3 = GetAsset<AudioClip>("Destroy_3"); d3.LoadAudioData();

            typeof(AudioAssetRandom)
                .GetField("Clips", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(gameData.ReferableObjects.Clips[DestroySoundEvent], new List<AudioClip>() { d1, d2, d3 });
        }

        private void SetupExplosionSoundEvents(GameData gameData)
        {
            ExplosionSoundEvent = (SoundEvent)VariousUtils.GetID(GUID + "-EXPLODE");

            if (!gameData.ReferableObjects.Clips.ContainsKey(ExplosionSoundEvent))
                gameData.ReferableObjects.Clips.Add(ExplosionSoundEvent, new AudioAsset());

            var audio = GetAsset<AudioClip>("Explosion"); audio.LoadAudioData();

            typeof(AudioAsset)
                .GetField("Clip", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(gameData.ReferableObjects.Clips[ExplosionSoundEvent], audio);
        }

        private GameObject SetupPurchaseView()
        {
            var prefab = GetPrefab("Purchase Indicator");
            prefab.TryAddComponent<CostIndicatorView>().Text =
                prefab.CreateLabel("Coins", Vector3.zero, Quaternion.identity, MaterialUtils.GetExistingMaterial("Alphakind Atlas Material"), FontUtils.GetExistingTMPFont("Large Text"),
                0, 1.75f, "<color=#ff1111>999 <sprite name=\"coin\" color=#FF9800>").GetComponent<TextMeshPro>();
            return prefab;
        }

        private GameObject SetupRenovationView()
        {
            var prefab = GetPrefab("Renovation View");
            var view = prefab.AddComponent<RenovationView>();

            var liner = GetPrefab("Destroyed Wall");
            liner.ApplyMaterialToChild("Wall", "Metal");
            view.LinerPrefab = liner;

            var doorstop = GetPrefab("Door Attachment");
            doorstop.ApplyMaterialToChild("Stop", "Metal Black");
            view.DoorstopPrefab = doorstop;

            var hatch = GetPrefab("Created Hatch");
            hatch.ApplyMaterialToChild("Surface", "Wood - Default");
            hatch.ApplyMaterialToChild("Supports", "Wood - Corkboard");
            view.HatchPrefab = hatch;

            return prefab;
        }

        private GameObject SetupParticleEventView()
        {
            var prefab = GetPrefab("Particle Event");
            var view = prefab.TryAddComponent<ParticleView>();
            return prefab;
        }

        internal void AddIcons()
        {
            Bundle.LoadAllAssets<Texture2D>();
            Bundle.LoadAllAssets<Sprite>();

            var icons = Bundle.LoadAsset<TMP_SpriteAsset>("Icon Asset");
            icons.material = UnityEngine.Object.Instantiate(TMP_Settings.defaultSpriteAsset.material);
            icons.material.mainTexture = Bundle.LoadAsset<Texture2D>("Icon Texture");
            TMP_Settings.defaultSpriteAsset.fallbackSpriteAssets.Add(icons);

            Log("Registered icons");
        }

        private void PreferenceOverrides(GameData gameData)
        {
            foreach (var gdoPair in gameData.Objects)
            {
                if (!(gdoPair.Value is Appliance appliance) || !appliance.HasUpgrades)
                    continue;

                appliance.Upgrades.RemoveAll(p => ShouldBlock(p.ID));
            }
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

                AddGameDataObject(type);
                counter++;
            }
            Log($"Registered {counter} GameDataObjects.");
        }

        public void AddGameDataObject(Type type)
        {
            if (type.IsAbstract || !type.IsClass || !type.IsSubclassOf(typeof(CustomGameDataObject)))
                return;

            CustomGameDataObject gdo = (CustomGameDataObject)Activator.CreateInstance(type);
            gdo.ModID = ModID;
            gdo.ModName = ModName;

            gdo = CustomGDO.RegisterGameDataObject(gdo);

            if (typeof(IRequirePreference).IsAssignableFrom(type))
                GDOPreferences.Add(gdo.ID, ((IRequirePreference)gdo).PreferenceName());

            if (typeof(IBlockDesks).IsAssignableFrom(type))
                DeskBlockingGDOs.Add(gdo.ID);
        }

        public interface IWontRegister { }

        public static bool ShouldBlock(int id) => false; // GDOPreferences.TryGetValue(id, out var pref) && !PrefManager.Get<bool>(pref)
        internal static Dictionary<int, string> GDOPreferences = new();
        public interface IRequirePreference
        {
            public string PreferenceName();
        }

        public static bool BlocksDesk(int id) => DeskBlockingGDOs.Contains(id);
        internal static List<int> DeskBlockingGDOs = new();
        public interface IBlockDesks { }
        #endregion

        #region Utility
        public static T GetGDO<T>(int id) where T : GameDataObject => GDOUtils.GetExistingGDO(id) as T;

        public static GameObject GetPrefab(string name) => Bundle.LoadAsset<GameObject>(name);
        public static T GetAsset<T>(string name) where T : UnityEngine.Object => Bundle.LoadAsset<T>(name);
        #endregion
    }
}
