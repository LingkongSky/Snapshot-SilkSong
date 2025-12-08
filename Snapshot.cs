using BepInEx;
using HarmonyLib;
using Snapshot_SilkSong.Utils;
using Snapshot_SilkSong.Utils.Snapshot_SilkSong.Patches;
using System;
using UnityEngine;
using static SteelSoulQuestSpot;

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
            Harmony.CreateAndPatchAll(typeof(StatePatches), null);

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
            /*
            if (Input.GetKey(KeyCode.Alpha1))
            {
                Logger.LogInfo("close");
                StateController.IsFsmEnabled = false;
            }

            if (Input.GetKey(KeyCode.Alpha2))
            {
                Logger.LogInfo("open");
                StateController.IsFsmEnabled = true;
            }
            */
        }

        private void SaveSnapshot(int slot)
        {
            string path = $"{slot}";
            manager.Save(path);
            Logger.LogInfo($"Saved snapshot to slot {slot}");
        }

        private void LoadSnapshot(int slot)
        {
            string path = $"{slot}";
            manager.Load(path);
            Logger.LogInfo($"Loaded snapshot from slot {slot}");
        }
    }
}