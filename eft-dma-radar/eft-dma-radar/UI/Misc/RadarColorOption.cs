﻿using eft_dma_radar.UI.ESP;
using eft_dma_radar.UI.Misc;
using eft_dma_radar.UI.SKWidgetControl;
using System.Windows.Markup.Localizer;

namespace eft_dma_radar.UI
{
    public enum RadarColorOption
    {
        LocalPlayer,
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
        DeathMarker,
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
        DoorShut,
        QuestKillZone,
        GroupLines
    }

    internal static class RadarColorOptions
    {
        #region Static Interfaces

        /// <summary>
        /// Load all ESP Color Config. Run once at start of application.
        /// </summary>
        internal static void LoadColors(Config config)
        {
            config.Colors ??= new Dictionary<RadarColorOption, string>();

            foreach (var defaultColor in GetDefaultColors())
                config.Colors.TryAdd(defaultColor.Key, defaultColor.Value);

            SetColors(config.Colors);
        }

        /// <summary>
        /// Returns all default color combinations for Radar.
        /// </summary>
        internal static Dictionary<RadarColorOption, string> GetDefaultColors() =>
            new()
            {
                [RadarColorOption.LocalPlayer] = SKColors.White.ToString(),
                [RadarColorOption.Friendly] = SKColors.LimeGreen.ToString(),
                [RadarColorOption.USEC] = SKColors.Red.ToString(),
                [RadarColorOption.BEAR] = SKColors.Blue.ToString(),
                [RadarColorOption.Focused] = SKColors.Coral.ToString(),
                [RadarColorOption.Streamer] = SKColors.MediumPurple.ToString(),
                [RadarColorOption.AimbotTarget] = SKColors.Blue.ToString(),
                [RadarColorOption.Special] = SKColors.MediumPurple.ToString(),
                [RadarColorOption.PlayerScav] = SKColors.Orange.ToString(),
                [RadarColorOption.Scav] = SKColors.Yellow.ToString(),
                [RadarColorOption.Raider] = SKColor.Parse("ffc70f").ToString(),
                [RadarColorOption.Boss] = SKColors.Fuchsia.ToString(),
                [RadarColorOption.DeathMarker] = SKColors.Black.ToString(),
                [RadarColorOption.RegularLoot] = SKColors.WhiteSmoke.ToString(),
                [RadarColorOption.ValuableLoot] = SKColors.Turquoise.ToString(),
                [RadarColorOption.WishlistLoot] = SKColors.Red.ToString(),
                [RadarColorOption.ContainerLoot] = SKColor.Parse("FFFFCC").ToString(),
                [RadarColorOption.QuestLoot] = SKColors.YellowGreen.ToString(),
                [RadarColorOption.StaticQuestItemsAndZones] = SKColors.DeepPink.ToString(),
                [RadarColorOption.QuestKillZone] = SKColors.DeepPink.ToString(),
                [RadarColorOption.Corpse] = SKColors.Silver.ToString(),
                [RadarColorOption.MedsFilterLoot] = SKColors.LightSalmon.ToString(),
                [RadarColorOption.FoodFilterLoot] = SKColors.CornflowerBlue.ToString(),
                [RadarColorOption.BackpackFilterLoot] = SKColor.Parse("00b02c").ToString(),
                [RadarColorOption.Explosives] = SKColors.OrangeRed.ToString(),
                [RadarColorOption.Switches] = SKColors.Orange.ToString(),
                [RadarColorOption.ExfilOpen] = SKColors.MediumSeaGreen.ToString(),
                [RadarColorOption.ExfilPending] = SKColors.Yellow.ToString(),
                [RadarColorOption.ExfilClosed] = SKColors.Red.ToString(),
                [RadarColorOption.ExfilInactive] = SKColors.Gray.ToString(),
                [RadarColorOption.ExfilTransit] = SKColors.Orange.ToString(),
                [RadarColorOption.DoorOpen] = SKColors.Green.ToString(),
                [RadarColorOption.DoorLocked] = SKColors.Red.ToString(),
                [RadarColorOption.DoorShut] = SKColors.Orange.ToString(),
                [RadarColorOption.GroupLines] = SKColors.LimeGreen.ToString(),
            };

        /// <summary>
        /// Save all ESP Color Changes.
        /// </summary>
        internal static void SetColors(IReadOnlyDictionary<RadarColorOption, string> colors)
        {
            try
            {
                foreach (var color in colors)
                {
                    if (!SKColor.TryParse(color.Value, out var skColor))
                        throw new Exception($"Invalid Color Value for {color.Key}!");

                    switch (color.Key)
                    {
                        case RadarColorOption.LocalPlayer:
                            SKPaints.PaintLocalPlayer.Color = skColor;
                            SKPaints.TextLocalPlayer.Color = skColor;
                            EspWidget.PaintESPWidgetLocalPlayer.Color = skColor;
                            break;
                        case RadarColorOption.Friendly:
                            SKPaints.PaintTeammate.Color = skColor;
                            SKPaints.TextTeammate.Color = skColor;
                            EspWidget.PaintESPWidgetTeammate.Color = skColor;
                            break;
                        case RadarColorOption.USEC:
                            SKPaints.PaintUSEC.Color = skColor;
                            SKPaints.TextUSEC.Color = skColor;
                            EspWidget.PaintESPWidgetUSEC.Color = skColor;
                            break;
                        case RadarColorOption.BEAR:
                            SKPaints.PaintBEAR.Color = skColor;
                            SKPaints.TextBEAR.Color = skColor;
                            EspWidget.PaintESPWidgetBEAR.Color = skColor;
                            break;
                        case RadarColorOption.Focused:
                            SKPaints.PaintFocused.Color = skColor;
                            SKPaints.TextFocused.Color = skColor;
                            EspWidget.PaintESPWidgetFocused.Color = skColor;
                            break;
                        case RadarColorOption.Streamer:
                            SKPaints.PaintStreamer.Color = skColor;
                            SKPaints.TextStreamer.Color = skColor;
                            EspWidget.PaintESPWidgetStreamer.Color = skColor;
                            break;
                        case RadarColorOption.AimbotTarget:
                            SKPaints.PaintAimbotLocked.Color = skColor;
                            SKPaints.TextAimbotLocked.Color = skColor;
                            EspWidget.PaintESPWidgetAimbotLocked.Color = skColor;
                            break;
                        case RadarColorOption.Special:
                            SKPaints.PaintSpecial.Color = skColor;
                            SKPaints.TextSpecial.Color = skColor;
                            EspWidget.PaintESPWidgetSpecial.Color = skColor;
                            break;
                        case RadarColorOption.PlayerScav:
                            SKPaints.PaintPScav.Color = skColor;
                            SKPaints.TextPScav.Color = skColor;
                            EspWidget.PaintESPWidgetPScav.Color = skColor;
                            break;
                        case RadarColorOption.Scav:
                            SKPaints.PaintScav.Color = skColor;
                            SKPaints.TextScav.Color = skColor;
                            EspWidget.PaintESPWidgetScav.Color = skColor;
                            break;
                        case RadarColorOption.Raider:
                            SKPaints.PaintRaider.Color = skColor;
                            SKPaints.TextRaider.Color = skColor;
                            EspWidget.PaintESPWidgetRaider.Color = skColor;
                            break;
                        case RadarColorOption.Boss:
                            SKPaints.PaintBoss.Color = skColor;
                            SKPaints.TextBoss.Color = skColor;
                            EspWidget.PaintESPWidgetBoss.Color = skColor;
                            break;
                        case RadarColorOption.DeathMarker:
                            SKPaints.PaintDeathMarker.Color = skColor;
                            break;
                        case RadarColorOption.RegularLoot:
                            SKPaints.PaintLoot.Color = skColor;
                            SKPaints.TextLoot.Color = skColor;
                            EspWidget.PaintESPWidgetLoot.Color = skColor;
                            EspWidget.TextESPWidgetLoot.Color = skColor;
                            break;
                        case RadarColorOption.ValuableLoot:
                            SKPaints.PaintImportantLoot.Color = skColor;
                            SKPaints.TextImportantLoot.Color = skColor;
                            break;
                        case RadarColorOption.WishlistLoot:
                            SKPaints.PaintWishlistItem.Color = skColor;
                            SKPaints.TextWishlistItem.Color = skColor;
                            break;
                        case RadarColorOption.QuestLoot:
                            SKPaints.PaintQuestItem.Color = skColor;
                            SKPaints.TextQuestItem.Color = skColor;
                            break;
                        case RadarColorOption.StaticQuestItemsAndZones:
                            SKPaints.QuestHelperPaint.Color = skColor;
                            SKPaints.QuestHelperText.Color = skColor;
                            break;
                        case RadarColorOption.QuestKillZone:
                            SKPaints.QuestHelperOutline.Color = skColor;
                            break;
                        case RadarColorOption.Corpse:
                            SKPaints.PaintCorpse.Color = skColor;
                            SKPaints.TextCorpse.Color = skColor;
                            break;
                        case RadarColorOption.MedsFilterLoot:
                            SKPaints.PaintMeds.Color = skColor;
                            SKPaints.TextMeds.Color = skColor;
                            break;
                        case RadarColorOption.FoodFilterLoot:
                            SKPaints.PaintFood.Color = skColor;
                            SKPaints.TextFood.Color = skColor;
                            break;
                        case RadarColorOption.BackpackFilterLoot:
                            SKPaints.PaintBackpacks.Color = skColor;
                            SKPaints.TextBackpacks.Color = skColor;
                            break;
                        case RadarColorOption.ContainerLoot:
                            SKPaints.PaintContainerLoot.Color = skColor;
                            SKPaints.TextContainer.Color = skColor;
                            break;
                        case RadarColorOption.Explosives:
                            SKPaints.PaintExplosives.Color = skColor;
                            SKPaints.TextExplosives.Color = skColor;
                            break;
                        case RadarColorOption.Switches:
                            SKPaints.PaintSwitch.Color = skColor;
                            SKPaints.TextSwitch.Color = skColor;
                            break;
                        case RadarColorOption.DoorOpen:
                            SKPaints.TextDoorOpen.Color = skColor;
                            SKPaints.PaintDoorOpen.Color = skColor;
                            break;
                        case RadarColorOption.DoorLocked:
                            SKPaints.TextDoorLocked.Color = skColor;
                            SKPaints.PaintDoorLocked.Color = skColor;
                            break;
                        case RadarColorOption.DoorShut:
                            SKPaints.TextDoorShut.Color = skColor;
                            SKPaints.PaintDoorShut.Color = skColor;
                            break;
                        case RadarColorOption.ExfilOpen:
                            SKPaints.PaintExfilOpen.Color = skColor;
                            SKPaints.TextExfilOpen.Color = skColor;
                            break;
                        case RadarColorOption.ExfilPending:
                            SKPaints.PaintExfilPending.Color = skColor;
                            SKPaints.TextExfilPending.Color = skColor;
                            break;
                        case RadarColorOption.ExfilClosed:
                            SKPaints.PaintExfilClosed.Color = skColor;
                            SKPaints.TextExfilClosed.Color = skColor;
                            break;
                        case RadarColorOption.ExfilInactive:
                            SKPaints.PaintExfilInactive.Color = skColor;
                            SKPaints.TextExfilInactive.Color = skColor;
                            break;
                        case RadarColorOption.ExfilTransit:
                            SKPaints.PaintExfilTransit.Color = skColor;
                            SKPaints.TextExfilTransit.Color = skColor;
                            break;
                        case RadarColorOption.GroupLines:
                            SKPaints.PaintConnectorGroup.Color = skColor;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ERROR Setting Radar Colors", ex);
            }
        }

        #endregion
    }
}