using BepInEx;
using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Snapshot_SilkSong.Utils
{
    public class ConfigManagers : MonoBehaviour
    {
        private static ConfigFile config;
        public Dictionary<string, HotkeySlot> slots = new Dictionary<string, HotkeySlot>();

        // 声音配置
        public bool PlaySoundOnSave { get; private set; }
        public bool PlaySoundOnLoad { get; private set; }

        public class HotkeySlot
        {
            public string SlotName { get; set; }
            public KeyCode SaveKey { get; set; }
            public KeyCode LoadKey { get; set; }
            public bool SaveKeyCtrlModifier { get; set; }
            public bool LoadKeyCtrlModifier { get; set; }
        }

        public ConfigManagers()
        {
            config = new ConfigFile(Utility.CombinePaths(Paths.ConfigPath, "Snapshot.cfg"), true);
            LoadConfig();
        }

        private void LoadConfig()
        {
            // 加载声音配置
            LoadSoundConfig();

            // 加载热键配置
            LoadHotkeyConfig();
        }

        private void LoadSoundConfig()
        {
            var playSoundOnSaveEntry = config.Bind(
                "Sound",
                "PlaySoundOnSave", 
                true,  
                "保存时播放音效"  
            );

            var playSoundOnLoadEntry = config.Bind(
                "Sound",
                "PlaySoundOnLoad",
                true,
                "加载时播放音效"
            );

            PlaySoundOnSave = playSoundOnSaveEntry.Value;
            PlaySoundOnLoad = playSoundOnLoadEntry.Value;
        }

        private void LoadHotkeyConfig()
        {
            slots.Clear();

            string[] slotNames = { "1", "2", "f1", "f2" };

            foreach (var slotName in slotNames)
            {
                var saveKeyEntry = config.Bind(
                    slotName,
                    "SaveKey",
                    slotName,
                    "保存按键");

                var loadKeyEntry = config.Bind(
                    slotName,
                    "LoadKey",
                    "ctrl, " + slotName,
                    "加载按键");

                slots[slotName] = ParseHotkeyConfig(slotName, saveKeyEntry.Value, loadKeyEntry.Value);
            }
        }

        private HotkeySlot ParseHotkeyConfig(string slotName, string saveKeyStr, string loadKeyStr)
        {
            var slot = new HotkeySlot
            {
                SlotName = slotName
            };

            // 解析保存按键
            ParseKeyCombo(saveKeyStr, out var saveKey, out var saveCtrl);
            slot.SaveKey = saveKey;
            slot.SaveKeyCtrlModifier = saveCtrl;

            // 解析加载按键
            ParseKeyCombo(loadKeyStr, out var loadKey, out var loadCtrl);
            slot.LoadKey = loadKey;
            slot.LoadKeyCtrlModifier = loadCtrl;

            return slot;
        }

        private void ParseKeyCombo(string keyCombo, out KeyCode key, out bool ctrlModifier)
        {
            ctrlModifier = false;
            key = KeyCode.None;

            if (string.IsNullOrWhiteSpace(keyCombo)) return;

            var parts = keyCombo.ToLower()
                .Split(new[] { ',', '+', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();

            if (parts.Count == 0) return;

            var ctrlKeywords = new HashSet<string> { "ctrl", "control" };
            var keyKeywords = new Dictionary<string, KeyCode>
            {
                ["1"] = KeyCode.Alpha1,
                ["2"] = KeyCode.Alpha2,
                ["3"] = KeyCode.Alpha3,
                ["4"] = KeyCode.Alpha4,
                ["5"] = KeyCode.Alpha5,
                ["6"] = KeyCode.Alpha6,
                ["7"] = KeyCode.Alpha7,
                ["8"] = KeyCode.Alpha8,
                ["9"] = KeyCode.Alpha9,
                ["0"] = KeyCode.Alpha0,
                ["num0"] = KeyCode.Keypad0,
                ["num1"] = KeyCode.Keypad1,
                ["num2"] = KeyCode.Keypad2,
                ["num3"] = KeyCode.Keypad3,
                ["num4"] = KeyCode.Keypad4,
                ["num5"] = KeyCode.Keypad5,
                ["num6"] = KeyCode.Keypad6,
                ["num7"] = KeyCode.Keypad7,
                ["num8"] = KeyCode.Keypad8,
                ["num9"] = KeyCode.Keypad9,
                ["enter"] = KeyCode.Return,
                ["return"] = KeyCode.Return,
                ["esc"] = KeyCode.Escape,
                ["escape"] = KeyCode.Escape,
                ["space"] = KeyCode.Space,
                ["tab"] = KeyCode.Tab,
                ["backspace"] = KeyCode.Backspace,
                ["delete"] = KeyCode.Delete,
                ["insert"] = KeyCode.Insert,
                ["home"] = KeyCode.Home,
                ["end"] = KeyCode.End,
                ["pageup"] = KeyCode.PageUp,
                ["pagedown"] = KeyCode.PageDown,
                ["up"] = KeyCode.UpArrow,
                ["down"] = KeyCode.DownArrow,
                ["left"] = KeyCode.LeftArrow,
                ["right"] = KeyCode.RightArrow
            };

            ctrlModifier = parts.Any(p => ctrlKeywords.Contains(p));

            var keyStr = parts.Last(p => !ctrlKeywords.Contains(p));

            if (keyKeywords.TryGetValue(keyStr, out key)) return;

            if (Enum.TryParse<KeyCode>(keyStr, true, out key)) return;

            if (keyStr.Length == 1 && char.IsLetterOrDigit(keyStr[0]))
            {
                if (Enum.TryParse<KeyCode>(keyStr.ToUpper(), out key)) return;
            }

            Debug.LogWarning($"Undefined Key: {keyStr}");
        }

        public bool CheckHotkey(KeyCode key, bool requireCtrl)
        {
            if (key == KeyCode.None)
                return false;

            bool ctrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            if (requireCtrl && !ctrlPressed)
                return false;

            if (!requireCtrl && ctrlPressed)
                return false;

            return Input.GetKeyDown(key);
        }
    }
}