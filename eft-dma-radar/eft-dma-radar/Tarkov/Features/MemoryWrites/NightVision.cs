﻿using eft_dma_shared.Common.Misc;
using eft_dma_radar.Tarkov.Features;
using eft_dma_radar.Tarkov.GameWorld;
using eft_dma_shared.Common.DMA.ScatterAPI;
using eft_dma_shared.Common.Features;
using eft_dma_shared.Common.Unity;

namespace eft_dma_radar.Tarkov.Features.MemoryWrites
{
    public sealed class NightVision : MemWriteFeature<NightVision>
    {
        private bool _lastEnabledState;
        private ulong _cachedNightVisionComponent;

        public override bool Enabled
        {
            get => MemWrites.Config.NightVision;
            set => MemWrites.Config.NightVision = value;
        }

        public override void TryApply(ScatterWriteHandle writes)
        {
            try
            {
                if (Memory.Game is not LocalGameWorld game)
                    return;

                if (Enabled != _lastEnabledState)
                {
                    var nightVisionComponent = GetNightVisionComponent(game);
                    if (!nightVisionComponent.IsValidVirtualAddress())
                        return;

                    writes.AddValueEntry(nightVisionComponent + Offsets.NightVision._on, Enabled);

                    writes.Callbacks += () =>
                    {
                        _lastEnabledState = Enabled;
                        LoneLogging.WriteLine($"[NightVision] {(Enabled ? "Enabled" : "Disabled")}");
                    };
                }
            }
            catch (Exception ex)
            {
                LoneLogging.WriteLine($"[NightVision]: {ex}");
                _cachedNightVisionComponent = default;
            }
        }

        private ulong GetNightVisionComponent(LocalGameWorld game)
        {
            if (_cachedNightVisionComponent.IsValidVirtualAddress())
                return _cachedNightVisionComponent;

            var fps = game.CameraManager?.FPSCamera ?? 0x0;
            if (!fps.IsValidVirtualAddress())
                return 0x0;

            var nightVisionComponent = MonoBehaviour.GetComponent(fps, "NightVision");
            if (!nightVisionComponent.IsValidVirtualAddress())
                return 0x0;

            _cachedNightVisionComponent = nightVisionComponent;
            return nightVisionComponent;
        }

        public override void OnRaidStart()
        {
            _lastEnabledState = default;
            _cachedNightVisionComponent = default;
        }
    }
}