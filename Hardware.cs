using NESSharp.Core;
using System;
using static NESSharp.Core.AL;

namespace NESSharp.Common {
	public static class Hardware {
		public static void WaitForVBlank() {
			Loop.Do(() => {
				NES.PPU.Status.BitTest();
			}).While(() => Condition.IsPositive);
		}
		public static void LoadPalettes(Action paletteDataSection) {
			Comment("Load palettes");
			WaitForVBlank();
			NES.PPU.SetAddress(NES.MemoryMap.Palette);
			Loop.RepeatX(0, 32, () => {
				NES.PPU.Data.Set(LabelFor(paletteDataSection).Offset(X));
			});
		}

		public static void ClearRAM() {
			Loop.RepeatX(0, 255, () => {
				Addr(0x0000)[X].Set(0);
				Addr(0x0100)[X].Set(0);
				Addr(0x0300)[X].Set(0);
				Addr(0x0400)[X].Set(0);
				Addr(0x0500)[X].Set(0);
				Addr(0x0600)[X].Set(0);
				Addr(0x0700)[X].Set(0);
				Addr(0x0200)[X].Set(0xFE);
			});
		}

		
		public static void GetTVSystem(Var8 nmis) {
			X.Set(0);
			Y.Set(0);
			A.Set(nmis);
			Loop.Do().While(() => A.Equals(nmis));
			A.Set(nmis);
			
			Loop.Do(() => {
				//Each iteration takes 11 cycles.
				//NTSC NES: 29780 cycles or 2707 = $A93 iterations
				//PAL NES:  33247 cycles or 3022 = $BCE iterations
				//Dendy:    35464 cycles or 3224 = $C98 iterations
				//so we can divide by $100 (rounding down), subtract ten,
				//and end up with 0=ntsc, 1=pal, 2=dendy, 3=unknown
				X++;
				If(() => X.Equals(0), () => {
					Y++;
				});
			}).While(() => A.Equals(nmis));

			A.Set(Y);
			Carry.Set();
			A.SBC(10).Equals(3);
			If(() => Carry.IsSet(), () => {
				A.Set(3);
			});
		}
	}
}
