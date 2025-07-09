﻿using eft_dma_radar.UI.Misc;

namespace eft_dma_radar.UI
{
    public enum EspColorOption
    {
        Friendly,
        USEC,
        BEAR,
        Focused,
        Streamer,
        AimbotTarget,
        Special,
        PlayerScav,
        Scav,
        Raider,
        Boss,
        RegularLoot,
        ValuableLoot,
        WishlistLoot,
        ContainerLoot,
        MedsFilterLoot,
        FoodFilterLoot,
        BackpackFilterLoot,
        QuestLoot,
        StaticQuestItemsAndZones,
        Corpse,
        Explosives,
        ExfilOpen,
        ExfilPending,
        ExfilClosed,
        ExfilInactive,
        ExfilTransit,
        Switches,
        DoorOpen,
        DoorLocked,
        DoorShut
    }

    internal static class EspColorOptions
    {
        #region Static Interfaces

        /// <summary>
        /// Load all ESP Color Config. Run once at start of application.
        /// </summary>
        internal static void LoadColors(Config config)
        {
            config.ESP.Colors ??= new Dictionary<EspColorOption, string>();
            foreach (var defaultColor in GetDefaultColors())
                config.ESP.Colors.TryAdd(defaultColor.Key, defaultColor.Value);
            SetColors(config.ESP.Colors);
        }

        /// <summary>
        /// Returns all default color combinations for ESP.
        /// </summary>
        internal static Dictionary<EspColorOption, string> GetDefaultColors() =>
            new()
            {
                [EspColorOption.Friendly] = SKColors.LimeGreen.ToString(),
                [EspColorOption.USEC] = SKColors.Red.ToString(),
                [EspColorOption.BEAR] = SKColors.Blue.ToString(),
                [EspColorOption.Focused] = SKColors.Coral.ToString(),
                [EspColorOption.Streamer] = SKColors.MediumPurple.ToString(),
                [EspColorOption.AimbotTarget] = SKColor.Parse("4654ff").ToString(), // Blue doesnt work on some fusers
                [EspColorOption.Special] = SKColors.MediumPurple.ToString(),
                [EspColorOption.PlayerScav] = SKColors.Orange.ToString(),
                [EspColorOption.Scav] = SKColors.Yellow.ToString(),
                [EspColorOption.Raider] = SKColor.Parse("ffc70f").ToString(),
                [EspColorOption.Boss] = SKColors.Fuchsia.ToString(),
                [EspColorOption.RegularLoot] = SKColors.WhiteSmoke.ToString(),
                [EspColorOption.ValuableLoot] = SKColors.Turquoise.ToString(),
                [EspColorOption.WishlistLoot] = SKColors.Red.ToString(),
                [EspColorOption.ContainerLoot] = SKColor.Parse("FFFFCC").ToString(),
                [EspColorOption.QuestLoot] = SKColors.YellowGreen.ToString(),
                [EspColorOption.StaticQuestItemsAndZones] = SKColors.DeepPink.ToString(),
                [EspColorOption.Corpse] = SKColors.Silver.ToString(),
                [EspColorOption.MedsFilterLoot] = SKColors.LightSalmon.ToString(),
                [EspColorOption.FoodFilterLoot] = SKColors.CornflowerBlue.ToString(),
                [EspColorOption.BackpackFilterLoot] = SKColor.Parse("00b02c").ToString(),
                [EspColorOption.Explosives] = SKColors.OrangeRed.ToString(),
                [EspColorOption.Switches] = SKColors.Orange.ToString(),
                [EspColorOption.DoorOpen] = SKColors.Green.ToString(),
                [EspColorOption.DoorLocked] = SKColors.Red.ToString(),
                [EspColorOption.DoorShut] = SKColors.Orange.ToString(),
                [EspColorOption.ExfilOpen] = SKColors.MediumSeaGreen.ToString(),
                [EspColorOption.ExfilPending] = SKColors.Yellow.ToString(),
                [EspColorOption.ExfilClosed] = SKColors.Red.ToString(),
                [EspColorOption.ExfilInactive] = SKColors.Gray.ToString(),
                [EspColorOption.ExfilTransit] = SKColors.Orange.ToString()
            };

        /// <summary>
        /// Save all ESP Color Changes.
        /// </summary>
        internal static void SetColors(IReadOnlyDictionary<EspColorOption, string> colors)
        {
            try
            {
                foreach (var color in colors)
                {
                    if (!SKColor.TryParse(color.Value, out var skColor))
                        throw new Exception($"Invalid Color Value for {color.Key}!");
                    switch (color.Key)
                    {
                        case EspColorOption.Friendly:
                            SKPaints.PaintFriendlyESP.Color = skColor;
                            SKPaints.TextFriendlyESP.Color = skColor;
                            break;
                        case EspColorOption.USEC:
                            SKPaints.PaintUSECESP.Color = skColor;
                            SKPaints.TextUSECESP.Color = skColor;
                            break;
                        case EspColorOption.BEAR:
                            SKPaints.PaintBEARESP.Color = skColor;
                            SKPaints.TextBEARESP.Color = skColor;
                            break;
                        case EspColorOption.Focused:
                            SKPaints.PaintFocusedESP.Color = skColor;
                            SKPaints.TextFocusedESP.Color = skColor;
                            break;
                        case EspColorOption.Streamer:
                            SKPaints.PaintStreamerESP.Color = skColor;
                            SKPaints.TextStreamerESP.Color = skColor;
                            break;
                        case EspColorOption.AimbotTarget:
                            SKPaints.PaintAimbotLockedESP.Color = skColor;
                            SKPaints.TextAimbotLockedESP.Color = skColor;
                            break;
                        case EspColorOption.Special:
                            SKPaints.PaintSpecialESP.Color = skColor;
                            SKPaints.TextSpecialESP.Color = skColor;
                            break;
                        case EspColorOption.PlayerScav:
                            SKPaints.PaintPlayerScavESP.Color = skColor;
                            SKPaints.TextPlayerScavESP.Color = skColor;
                            break;
                        case EspColorOption.Scav:
                            SKPaints.PaintScavESP.Color = skColor;
                            SKPaints.TextScavESP.Color = skColor;
                            break;
                        case EspColorOption.Raider:
                            SKPaints.PaintRaiderESP.Color = skColor;
                            SKPaints.TextRaiderESP.Color = skColor;
                            break;
                        case EspColorOption.Boss:
                            SKPaints.PaintBossESP.Color = skColor;
                            SKPaints.TextBossESP.Color = skColor;
                            break;
                        case EspColorOption.RegularLoot:
                            SKPaints.PaintLootESP.Color = skColor;
                            SKPaints.TextLootESP.Color = skColor;
                            break;
                        case EspColorOption.ValuableLoot:
                            SKPaints.PaintImpLootESP.Color = skColor;
                            SKPaints.TextImpLootESP.Color = skColor;
                            break;
                        case EspColorOption.WishlistLoot:
                            SKPaints.PaintWishlistItemESP.Color = skColor;
                            SKPaints.TextWishlistItemESP.Color = skColor;
                            break;
                        case EspColorOption.QuestLoot:
                            SKPaints.PaintQuestItemESP.Color = skColor;
                            SKPaints.TextQuestItemESP.Color = skColor;
                            break;
                        case EspColorOption.StaticQuestItemsAndZones:
                            SKPaints.PaintQuestHelperESP.Color = skColor;
                            SKPaints.TextQuestHelperESP.Color = skColor;
                            break;
                        case EspColorOption.Corpse:
                            SKPaints.PaintCorpseESP.Color = skColor;
                            SKPaints.TextCorpseESP.Color = skColor;
                            break;
                        case EspColorOption.MedsFilterLoot:
                            SKPaints.PaintMedsESP.Color = skColor;
                            SKPaints.TextMedsESP.Color = skColor;
                            break;
                        case EspColorOption.FoodFilterLoot:
                            SKPaints.PaintFoodESP.Color = skColor;
                            SKPaints.TextFoodESP.Color = skColor;
                            break;
                        case EspColorOption.BackpackFilterLoot:
                            SKPaints.PaintBackpackESP.Color = skColor;
                            SKPaints.TextBackpackESP.Color = skColor;
                            break;
                        case EspColorOption.ContainerLoot:
                            SKPaints.PaintContainerLootESP.Color = skColor;
                            SKPaints.TextContainerLootESP.Color = skColor;
                            break;
                        case EspColorOption.Explosives:
                            SKPaints.PaintExplosiveESP.Color = skColor;
                            SKPaints.PaintExplosiveRadiusESP.Color = skColor;
                            SKPaints.TextExplosiveESP.Color = skColor;
                            break;
                        case EspColorOption.Switches:
                            SKPaints.TextSwitchesESP.Color = skColor;
                            SKPaints.PaintSwitchESP.Color = skColor;
                            break;
                        case EspColorOption.DoorOpen:
                            SKPaints.TextDoorOpenESP.Color = skColor;
                            SKPaints.PaintDoorOpenESP.Color = skColor;
                            break;
                        case EspColorOption.DoorShut:
                            SKPaints.TextDoorShutESP.Color = skColor;
                            SKPaints.PaintDoorShutESP.Color = skColor;
                            break;
                        case EspColorOption.DoorLocked:
                            SKPaints.TextDoorLockedESP.Color = skColor;
                            SKPaints.PaintDoorLockedESP.Color = skColor;
                            break;
                        case EspColorOption.ExfilOpen:
                            SKPaints.TextExfilOpenESP.Color = skColor;
                            SKPaints.PaintExfilOpenESP.Color = skColor;
                            break;
                        case EspColorOption.ExfilPending:
                            SKPaints.TextExfilPendingESP.Color = skColor;
                            SKPaints.PaintExfilPendingESP.Color = skColor;
                            break;
                        case EspColorOption.ExfilClosed:
                            SKPaints.TextExfilClosedESP.Color = skColor;
                            SKPaints.PaintExfilClosedESP.Color = skColor;
                            break;
                        case EspColorOption.ExfilTransit:
                            SKPaints.TextExfilTransitESP.Color = skColor;
                            SKPaints.PaintExfilTransitESP.Color = skColor;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ERROR Setting ESP Colors", ex);
            }
        }

        #endregion
    }
}