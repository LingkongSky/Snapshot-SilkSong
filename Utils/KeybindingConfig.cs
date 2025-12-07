using BepInEx.Configuration;
using UnityEngine;

namespace Snapshot_SilkSong.Utils
{
    public class KeybindingConfig
    {
        public readonly KeyboardShortcut[] SaveShortcuts;
        public readonly KeyboardShortcut[] LoadShortcuts;

        public KeybindingConfig()
        {
            SaveShortcuts = new KeyboardShortcut[8];
            LoadShortcuts = new KeyboardShortcut[8];

            for (int i = 0; i < 8; i++)
            {
                SaveShortcuts[i] = new KeyboardShortcut(KeyCode.F1 + i);
                LoadShortcuts[i] = new KeyboardShortcut(KeyCode.F1 + i,
                    KeyCode.LeftControl);
            }
        }

        public bool TryGetSaveSlot(out int slot)
        {
            return TryGetSlot(SaveShortcuts, out slot);
        }

        public bool TryGetLoadSlot(out int slot)
        {
            return TryGetSlot(LoadShortcuts, out slot);
        }

        private bool TryGetSlot(KeyboardShortcut[] shortcuts, out int slot)
        {
            for (int i = 0; i < shortcuts.Length; i++)
            {
                if (shortcuts[i].IsDown())
                {
                    slot = i + 1;
                    return true;
                }
            }

            slot = -1;
            return false;
        }
    }
}
