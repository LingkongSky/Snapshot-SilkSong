using BepInEx;
using HarmonyLib;
using Snapshot_SilkSong.Utils;
using System;


namespace Snapshot
{
    [BepInPlugin("Lingkong.Snapshot.SilkSong", "Silk Sogn Shapshot", "1.0.0.0")]
    [Serializable]
    public class Snapshot : BaseUnityPlugin
    {
        private static Manager manager;
        private KeybindingConfig keybindings;

        void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Snapshot), null);
            Harmony.CreateAndPatchAll(typeof(Manager), null);

            keybindings = new KeybindingConfig();
        }

        void Start()
        {
            manager = new Manager();
            Logger.LogInfo("Snapshot Plugin Loaded.");
        }

        void Update()
        {
            if (keybindings.TryGetSaveSlot(out int saveSlot))
            {
                SaveSnapshot(saveSlot);
            }
            else if (keybindings.TryGetLoadSlot(out int loadSlot))
            {
                LoadSnapshot(loadSlot);
            }
        }

        private void SaveSnapshot(int slot)
        {
            string path = $"snapshot_save/{slot}";
            manager.Save(path);
            Logger.LogInfo($"Saved snapshot to slot {slot}");
        }

        private void LoadSnapshot(int slot)
        {
            string path = $"snapshot_save/{slot}";
            manager.Load(path);
            Logger.LogInfo($"Loaded snapshot from slot {slot}");
        }
    }
}