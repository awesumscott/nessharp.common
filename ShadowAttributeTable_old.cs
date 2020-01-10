using NESSharp.Core;
using NESSharp.Common;
using System;
using System.Collections.Generic;
using System.Text;
using static NESSharp.Core.AL;

namespace NESSharp.Common {
	/*
		Starting addresses
		NT 0	23C0
		NT 1	27C0
		NT 2	2BC0
		NT 3	2FC0

		Drawing only a vertical col of Attr bytes with VRamQueue:
		Vertical increments
		SetAddress 23C0
		Write val for 23C0 then 23E0
		SetAddress 23C8
		Write val for 23C8 then 23E8
		SetAddress 23D0
		Write val for 23D0 then 23F0
		SetAddress 23D8
		Write val for 23D8 then 23F8
		16 PPU writes total (1/4 of writes for a frame)
	*/


	public class ShadowAttributeTable {
		[Flags]
		public enum NameTable {
			NT0	=	0b0001,
			NT1	=	0b0010,
			NT2	=	0b0100,
			NT3	=	0b1000
		}

		public static Var16 PPUAddress;
		public static Array<Var8> Table;
		private static Var8 _temp;
		private bool _twoHigh = false, _twoWide = false;
		private bool _xAsc, _yAsc;
		public ShadowAttributeTable(RAM r, NameTable nts) {
			var ntCount = 0;
			var nt0 = nts.HasFlag(NameTable.NT0);
			var nt1 = nts.HasFlag(NameTable.NT1);
			var nt2 = nts.HasFlag(NameTable.NT2);
			var nt3 = nts.HasFlag(NameTable.NT3);
			if (nt0) ntCount++;
			if (nt1) ntCount++;
			if (nt2) ntCount++;
			if (nt3) ntCount++;
			_twoWide = nts.HasFlag(NameTable.NT0 | NameTable.NT1) || nts.HasFlag(NameTable.NT2 | NameTable.NT3);
			_twoHigh = nts.HasFlag(NameTable.NT0 | NameTable.NT2) || nts.HasFlag(NameTable.NT1 | NameTable.NT3);

			PPUAddress = Var16.New(ram, "ShadowAttributeTable_PPUAddress");
			Table	= Array<Var8>.New((U8)(64 * ntCount), r, "ShadowAttributeTable_attributeTable");
			_temp = Var8.New(ram, "ShadowAttributeTable_temp");
		}
		public void Horiz(bool ascending = true) {
			_xAsc = ascending;
		}
		public void Clear(NameTable nts) {
			//TODO: single or multiple, in the correct index range
			Comment("Clear attribute table");
			Loop.Descend_Pre(X.Set(Table.Length), () => {
				Table[X].Set(0);
			});
		}
	}


	public static class ShadowAttributeTable_old {
		public static Var16 PPUAddress;
		public static Array<Var8> Table;
		private static Var8 _temp;
		static ShadowAttributeTable_old() {
			PPUAddress = Var16.New(ram, "ShadowAttributeTable_PPUAddress");
			Table	= Array<Var8>.New(64, ram, "ShadowAttributeTable_attributeTable");
			_temp = Var8.New(ram, "ShadowAttributeTable_temp");
		}

		public static void Clear() {
			Comment("Clear attribute table");
			Loop.Descend_Pre(X.Set(Table.Length), () => {
				Table[X].Set(0);
			});
		}

		[Subroutine]
		public static void UpdateAttributeTableImmediately() {
			NES.PPU.SetHorizontalWrite();
			NES.PPU.SetAddress(0x23C0);
			//Something is wrong with this partially unrolled version
			//AscendWhile(X.Set(0), () => X.NotEquals((U8)(Table.Length / 4)), () => {
			//	NES.PPU.Data.Set(Table[X]);
			//	X++;
			//	NES.PPU.Data.Set(Table[X]);
			//	X++;
			//	NES.PPU.Data.Set(Table[X]);
			//	X++;
			//	NES.PPU.Data.Set(Table[X]);
			//});
			Loop.AscendWhile(X.Set(0), () => X.NotEquals((U8)Table.Length), () => {
				NES.PPU.Data.Set(Table[X]);
			});
		}

		[Subroutine]
		public static void UpdateAttr() {
			//PPUAddress = addr of bg tile to update
			
			//Stack.Backup(Register.A);
			
			_temp.Set(0xC0); //starting lo of attr address
			If(() => PPUAddress.Lo.ToA().IsNegative(), () => {
				//_temp.SetAdd(8);
				_temp.Set(z => z.Add(8));
			});
			_temp.Set(z => z.Add(PPUAddress.Lo.ToA().And(0x1F).Divide(4)));
			_temp.Set(z => z.Add(PPUAddress.Hi.ToA().And(0x0F).Multiply(4)));

			//NES.PPU.SetAddress()
			//Stack.Restore(Register.A);
		}
	}
}
