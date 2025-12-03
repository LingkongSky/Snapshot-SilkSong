using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// 位置√ 血量√ 丝线√ 吉欧√ 甲壳√  骨钉/工具/扩容等级√ 能力/剧情(has)√  徽章√ 蓝血/紫血状态 √

namespace Snapshot_SilkSong.PlayerState
{
    public class PlayerState
    {
        // 基础属性
        public Dictionary<string, object> basicProperties = new Dictionary<string, object>();

        // 保留原有的特殊属性
        public Vector3 position;

        // 能力 - 字典自动存储
        public Dictionary<string, bool> abilityStates;

        // 护符
        public ToolCrestsData ToolEquips;
        public FloatingCrestSlotsData ExtraToolEquips;
        public ToolItemsData Tools;
        public ToolItemLiquidsData ToolLiquids;
        public EnemyJournalKillData EnemyJournalKillData;
        public QuestCompletionData QuestCompletionData;
        public QuestRumourData QuestRumourData;
        public CollectableItemsData Collectables;
        public CollectableRelicsData Relics;
        public CollectableMementosData MementosDeposited;
        public MateriumItemsData MateriumCollected;
        public int[] mossBerryValueList;
        public int[] GrubFarmerMimicValueList;

        public PlayerState()
        {
            basicProperties = new Dictionary<string, object>();
            abilityStates = new Dictionary<string, bool>();
            Tools = new ToolItemsData();
            ToolLiquids = new ToolItemLiquidsData();
            EnemyJournalKillData = new EnemyJournalKillData();
            QuestCompletionData = new QuestCompletionData();
            QuestRumourData = new QuestRumourData();
            Collectables = new CollectableItemsData();
            Relics = new CollectableRelicsData();
            MementosDeposited = new CollectableMementosData();
            MateriumCollected = new MateriumItemsData();
            mossBerryValueList = Array.Empty<int>();
            GrubFarmerMimicValueList = Array.Empty<int>();
            ToolEquips = new ToolCrestsData();
            ExtraToolEquips = new FloatingCrestSlotsData();
        }

        public static void SavePlayerState(PlayerState playerState, String path)
        {
            PlayerData playerData = PlayerData.instance;
            HeroController hero = HeroController.instance;

            if (playerData != null)
            {
                // 自动收集PlayerData的所有public基础属性
                Type playerDataType = typeof(PlayerData);
                FieldInfo[] fields = playerDataType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                PropertyInfo[] properties = playerDataType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                playerState.basicProperties.Clear();

                // 收集字段
                foreach (FieldInfo field in fields)
                {
                    if (field.FieldType.IsValueType || field.FieldType == typeof(string))
                    {
                        try
                        {
                            object value = field.GetValue(playerData);
                            playerState.basicProperties[field.Name] = DeepCopier.DeepCopy(value);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"Failed to save field {field.Name}: {e.Message}");
                        }
                    }
                }

                // 收集属性（只收集有getter和setter的）
                foreach (PropertyInfo property in properties)
                {
                    if (property.CanRead && property.CanWrite &&
                        (property.PropertyType.IsValueType || property.PropertyType == typeof(string)))
                    {
                        try
                        {
                            object value = property.GetValue(playerData);
                            playerState.basicProperties[property.Name] = DeepCopier.DeepCopy(value);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"Failed to save property {property.Name}: {e.Message}");
                        }
                    }
                }

                playerState.ToolEquips = DeepCopier.DeepCopy(playerData.ToolEquips);
                playerState.ExtraToolEquips = DeepCopier.DeepCopy(playerData.ExtraToolEquips);
                playerState.Tools = DeepCopier.DeepCopy(playerData.Tools);
                playerState.ToolLiquids = DeepCopier.DeepCopy(playerData.ToolLiquids);
                playerState.EnemyJournalKillData = DeepCopier.DeepCopy(playerData.EnemyJournalKillData);
                playerState.QuestCompletionData = DeepCopier.DeepCopy(playerData.QuestCompletionData);
                playerState.QuestRumourData = DeepCopier.DeepCopy(playerData.QuestRumourData);
                playerState.Collectables = DeepCopier.DeepCopy(playerData.Collectables);
                playerState.Relics = DeepCopier.DeepCopy(playerData.Relics);
                playerState.MementosDeposited = DeepCopier.DeepCopy(playerData.MementosDeposited);
                playerState.MateriumCollected = DeepCopier.DeepCopy(playerData.MateriumCollected);

                // 数组深拷贝
                playerState.mossBerryValueList = DeepCopier.DeepCopy(playerData.mossBerryValueList);
                playerState.GrubFarmerMimicValueList = DeepCopier.DeepCopy(playerData.GrubFarmerMimicValueList);

            }

        }

        public static void LoadPlayerState(PlayerState playerState, String path)
        {
            PlayerData playerData = PlayerData.instance;
            HeroController hero = HeroController.instance;

            if (playerData != null)
            {
                // 自动恢复PlayerData的所有public基础属性
                Type playerDataType = typeof(PlayerData);
                FieldInfo[] fields = playerDataType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                PropertyInfo[] properties = playerDataType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                // 恢复字段
                foreach (FieldInfo field in fields)
                {
                    if (playerState.basicProperties.ContainsKey(field.Name) &&
                        (field.FieldType.IsValueType || field.FieldType == typeof(string)))
                    {
                        try
                        {
                            object value = playerState.basicProperties[field.Name];
                            field.SetValue(playerData, value);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"Failed to load field {field.Name}: {e.Message}");
                        }
                    }
                }

                // 恢复属性（只恢复有getter和setter的）
                foreach (PropertyInfo property in properties)
                {
                    if (playerState.basicProperties.ContainsKey(property.Name) &&
                        property.CanRead && property.CanWrite &&
                        (property.PropertyType.IsValueType || property.PropertyType == typeof(string)))
                    {
                        try
                        {
                            object value = playerState.basicProperties[property.Name];
                            property.SetValue(playerData, value);
                        }
                        catch (Exception e)
                        {
                            Debug.LogWarning($"Failed to load property {property.Name}: {e.Message}");
                        }
                    }
                }

                playerData.ToolEquips = DeepCopier.DeepCopy(playerState.ToolEquips);
                playerData.ExtraToolEquips = DeepCopier.DeepCopy(playerState.ExtraToolEquips);
                playerData.Tools = DeepCopier.DeepCopy(playerState.Tools);
                playerData.ToolLiquids = DeepCopier.DeepCopy(playerState.ToolLiquids);
                playerData.EnemyJournalKillData = DeepCopier.DeepCopy(playerState.EnemyJournalKillData);
                playerData.QuestCompletionData = DeepCopier.DeepCopy(playerState.QuestCompletionData);
                playerData.QuestRumourData = DeepCopier.DeepCopy(playerState.QuestRumourData);
                playerData.Collectables = DeepCopier.DeepCopy(playerState.Collectables);
                playerData.Relics = DeepCopier.DeepCopy(playerState.Relics);
                playerData.MementosDeposited = DeepCopier.DeepCopy(playerState.MementosDeposited);
                playerData.MateriumCollected = DeepCopier.DeepCopy(playerState.MateriumCollected);

                // 数组深拷贝
                playerData.mossBerryValueList = DeepCopier.DeepCopy(playerState.mossBerryValueList);
                playerData.GrubFarmerMimicValueList = DeepCopier.DeepCopy(playerState.GrubFarmerMimicValueList);

                ToolItemManager.SendEquippedChangedEvent(true);

                // 触发UI更新
                HeroController.instance.AddGeo(0);
                HeroController.instance.AddShards(0);
                HeroController.instance.AddSilk(0, false);
                HeroController.instance.AddHealth(0);
            }

        }

    }
}