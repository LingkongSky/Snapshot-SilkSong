using BepInEx;
using HarmonyLib;
using Snapshot_SilkSong.Utils;
using Snapshot_SilkSong.Utils.Snapshot_SilkSong.Patches;
using System;
namespace Snapshot
{
    [BepInPlugin("Lingkong.Snapshot.SilkSong", "Silk Sogn Shapshot", "1.2.0.0")]
    [Serializable]
    public class Snapshot : BaseUnityPlugin
    {
        private static Manager manager;
        private ConfigManagers configManager;

        void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(Snapshot), null);
            Harmony.CreateAndPatchAll(typeof(Manager), null);
            Harmony.CreateAndPatchAll(typeof(StatePatches), null);

            configManager = new ConfigManagers();
        }

        void Start()
        {
            manager = new Manager();
            Logger.LogInfo("Snapshot Plugin Loaded.");
        }

        void Update()
        {

            foreach (var slot in configManager.slots.Values)
            {
                // 检查保存按键
                if (configManager.CheckHotkey(slot.SaveKey, slot.SaveKeyCtrlModifier))
                {
                    SaveSnapshot(slot.SlotName);
                }

                // 检查加载按键
                if (configManager.CheckHotkey(slot.LoadKey, slot.LoadKeyCtrlModifier))
                {
                    LoadSnapshot(slot.SlotName);
                }
            }

        }

        private void SaveSnapshot(string slot)
        {
            manager.Save(slot);
            Logger.LogInfo($"Saved snapshot to slot {slot}");
            if(configManager.PlaySoundOnSave)
                System.Media.SystemSounds.Beep.Play();
        }

        private void LoadSnapshot(string slot)
        {
            manager.Load(slot);
            Logger.LogInfo($"Loaded snapshot from slot {slot}");
            if (configManager.PlaySoundOnLoad)
                System.Media.SystemSounds.Beep.Play();

        }
    }
}