using NESSharp.Core;
using System;
using static NESSharp.Core.AL;

namespace NESSharp.Common {
	public static class Hardware {
		public static void WaitForVBlank() {
			Loop.Do_old(_ => {
				CPU6502.BIT(NES.PPU.Status);
			}).While(() => Condition.IsPositive);
		}
		public static void LoadPalettes(Action paletteDataSection, bool waitForVBlank = true) {
			Comment("Load palettes");
			if (waitForVBlank)
				WaitForVBlank();
			NES.PPU.SetAddress(NES.MemoryMap.Palette);
			Loop.Repeat(X.Set(0), 32, _ => {
				NES.PPU.Data.Write(LabelFor(paletteDataSection)[X]);
			});
		}

		public static void ClearRAM() {
			Loop.Repeat(X.Set(0), 256, _ => {
				A.Set(0);
				CPU6502.STA(Addr(0x0000)[X]);
				CPU6502.STA(Addr(0x0100)[X]);
				CPU6502.STA(Addr(0x0300)[X]);
				CPU6502.STA(Addr(0x0400)[X]);
				CPU6502.STA(Addr(0x0500)[X]);
				CPU6502.STA(Addr(0x0600)[X]);
				CPU6502.STA(Addr(0x0700)[X]);
				A.Set(0xFE);
				CPU6502.STA(Addr(0x0200)[X]);
			});
		}

		
		public static RegisterA GetTVSystem(VByte nmis) {
			X.Set(0);
			Y.Set(0);
			A.Set(nmis);
			Loop.Do_old().While(() => A.Equals(nmis));
			A.Set(nmis);
			
			Loop.Do_old(_ => {
				//Each iteration takes 11 cycles.
				//NTSC NES:	29780 cycles or 2707 = $A93 iterations
				//PAL NES:	33247 cycles or 3022 = $BCE iterations
				//Dendy:	35464 cycles or 3224 = $C98 iterations
				//so we can divide by $100 (rounding down), subtract ten,
				//and end up with 0=ntsc, 1=pal, 2=dendy, 3=unknown
				X.Inc();
				If.True(() => X.Equals(0), () => Y.Increment());
			}).While(() => A.Equals(nmis));

			If.True(() => A.Set(Y).Subtract(10).GreaterThanOrEqualTo(3), () => A.Set(3));
			return A;
		}

		public static class TV {
			public static readonly U8 NTSC		= 0;
			public static readonly U8 PAL		= 1;
			public static readonly U8 Dendy		= 2;
			public static readonly U8 Unknown	= 3;
		}
	}
}
