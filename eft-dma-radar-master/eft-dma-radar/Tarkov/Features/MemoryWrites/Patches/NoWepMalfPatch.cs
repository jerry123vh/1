﻿using eft_dma_radar.Tarkov.Features;
using eft_dma_shared.Common.Features;
using eft_dma_shared.Common.Unity;

namespace eft_dma_radar.Tarkov.Features.MemoryWrites.Patches
{
    public sealed class NoWepMalfPatch : MemPatchFeature<NoWepMalfPatch>
    {
        public override bool CanRun => base.CanRun && (Memory.Game?.RaidHasStarted ?? false);

        public override bool Enabled
        {
            get => MemWrites.Config.NoWeaponMalfunctions;
            set => MemWrites.Config.NoWeaponMalfunctions = value;
        }

        protected override byte[] Signature { get; } = new byte[]
        {
            0x55,                                    // push rbp
            0x48, 0x8B, 0xEC,                        // mov rbp, rsp
            0x48, 0x81, 0xEC, 0xC0, 0x00, 0x00, 0x00 // sub rsp, 0xC0
        };

        protected override byte[] Patch { get; } = new byte[]
        {
            0xB8, 0x00, 0x00, 0x00, 0x00, // mov eax, 0
            0xC3                          // ret
        };

        protected override Func<ulong> GetPFunc => Lookup_GetMalfunctionState;
        protected override int PFuncSize => 64;

        public override void OnGameStop()
        {
            IsApplied = false;
        }

        private static ulong Lookup_GetMalfunctionState()
        {
            var pFuncGetMalfState = MonoLib.MonoClass.Find("Assembly-CSharp", ClassNames.NoMalfunctions.ClassName, out _).FindJittedMethod(ClassNames.NoMalfunctions.GetMalfunctionState);
            return pFuncGetMalfState;
        }
    }
}
