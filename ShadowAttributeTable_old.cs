using NESSharp.Core;
using System;
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


	public class ShadowAttributeTable_v3_WIP : Module {
		[Flags]
		public enum NameTable {
			NT0	=	0b0001,
			NT1	=	0b0010,
			NT2	=	0b0100,
			NT3	=	0b1000
		}

		public VWord PPUAddress;
		public Array<VByte> Table;
		private VByte _temp;
		private bool _twoHigh = false, _twoWide = false;
		private bool _xAsc, _yAsc;
		public ShadowAttributeTable_v3_WIP(RAM r, NameTable nts) {
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

			PPUAddress = VWord.New(GlobalRam, "ShadowAttributeTable_PPUAddress");
			Table	= Array<VByte>.New((U8)(64 * ntCount), r, "ShadowAttributeTable_attributeTable");
			_temp = VByte.New(GlobalRam, "ShadowAttributeTable_temp");
		}
		public void Horiz(bool ascending = true) {
			_xAsc = ascending;
		}
		public void Clear(NameTable nts) {
			//TODO: single or multiple, in the correct index range
			Comment("Clear attribute table");
			Loop.Descend_Pre(X.Set(Table.Length), _ => {
				Table[X].Set(0);
			});
		}
	}
	public class ShadowAttributeTable_v2 : Module {
		public VWord PPUAddress;
		public Array<VByte> Table;
		private VByte _temp;
		private VByte _attrMaskIdHoriz, _attrMaskIdVert; //current mask ID values used while iterating
		private VByte _attrOffsetLoStart, _attrOffsetLo, _attrOffsetLoRowStart; //starting and iterating lo address bytes of attribute table offsets (offset = 0x23C0 - [_bgOrigin's attribute address])
		private VByte _lastAttrMask;		//cached mask from last attribute grid vacancy color
		private VByte _currentAttrMask;	//current mask from last attribute grid vacancy color

		private static class AttributeMaskID {
			public static U8 Vert			= 0b00000010;	//used with XOR as toggle, or with Or after using VertClear
			public static U8 VertMask		= 0b11111101;
			public static U8 Horiz			= 0b00000001;
			public static U8 HorizMask		= 0b11111110;

			public static U8 TopIndex		= 0b00;
			public static U8 BottomIndex	= 0b01;
			public static U8 LeftIndex		= 0b00;
			public static U8 RightIndex		= 0b10;

			//Indexes to be used to access tables containing the bitmask values below
			public static U8 TopLeftIndex		= (U8)(TopIndex & LeftIndex);
			public static U8 TopRightIndex		= (U8)(TopIndex & RightIndex);
			public static U8 BottomLeftIndex	= (U8)(BottomIndex & LeftIndex);
			public static U8 BottomRightIndex	= (U8)(BottomIndex & RightIndex);
			
			//Bit and mask values to be used in a table (in this order), for setting/unsetting on an attribute table byte
			public static U8 TopLeft			= 0b00000011;
			public static U8 TopRight			= 0b00001100;
			public static U8 BottomLeft			= 0b00110000;
			public static U8 BottomRight		= 0b11000000;

			public static U8 TopLeftMask		= 0b11111100;
			public static U8 TopRightMask		= 0b11110011;
			public static U8 BottomLeftMask		= 0b11001111;
			public static U8 BottomRightMask	= 0b00111111;

			//Palette IDs to be "OR"d into an attribute after "AND"ing a mask
			public static U8 Palette0		= 0b00000000;
			public static U8 Palette1		= 0b01010101;
			public static U8 Palette2		= 0b10101010;
			public static U8 Palette3		= 0b11111111;
		}


		[Dependencies]
		private void Dependencies() {
			PPUAddress = VWord.New(Ram, $"{nameof(ShadowAttributeTable_v2)}_{nameof(PPUAddress)}");
			Table = Array<VByte>.New(64, Ram, $"{nameof(ShadowAttributeTable_v2)}_{nameof(Table)}");
			_temp = VByte.New(Ram, $"{nameof(ShadowAttributeTable_v2)}{nameof(_temp)}");
			_attrMaskIdHoriz		= VByte.New(Ram, $"{nameof(ShadowAttributeTable_v2)}{nameof(_attrMaskIdHoriz)}");
			_attrMaskIdVert			= VByte.New(Ram, $"{nameof(ShadowAttributeTable_v2)}{nameof(_attrMaskIdVert)}");
			_attrOffsetLoStart		= VByte.New(Ram, $"{nameof(ShadowAttributeTable_v2)}{nameof(_attrOffsetLoStart)}");
			_attrOffsetLo			= VByte.New(Ram, $"{nameof(ShadowAttributeTable_v2)}{nameof(_attrOffsetLo)}");
			_attrOffsetLoRowStart	= VByte.New(Ram, $"{nameof(ShadowAttributeTable_v2)}{nameof(_attrOffsetLoRowStart)}");
			_lastAttrMask			= VByte.New(Ram, $"{nameof(ShadowAttributeTable_v2)}{nameof(_lastAttrMask)}");
			_currentAttrMask		= VByte.New(Ram, $"{nameof(ShadowAttributeTable_v2)}{nameof(_currentAttrMask)}");
		}

		public void Clear() {
			Comment("Clear attribute table");
			Loop.Descend_Pre(X.Set(Table.Length), _ => {
				Table[X].Set(0);
			});
		}

		[Subroutine]
		public void UpdateAttributeTableImmediately() {
			NES.PPU.SetHorizontalWrite();
			NES.PPU.SetAddress(0x23C0);
			Loop.AscendWhile(X.Set(0), () => X.NotEquals((U8)Table.Length), _ => {
				NES.PPU.Data.Set(Table[X]);
			});
		}

		[DataSection]
		public void AttributeBitMask() {
			Raw(
				AttributeMaskID.TopLeftMask,
				AttributeMaskID.TopRightMask,
				AttributeMaskID.BottomLeftMask,
				AttributeMaskID.BottomRightMask
			);
		}
		[DataSection]
		public void AttributeBit() {
			Raw(
				AttributeMaskID.TopLeft,
				AttributeMaskID.TopRight,
				AttributeMaskID.BottomLeft,
				AttributeMaskID.BottomRight
			);
		}
	}

	public class ShadowAttributeTable_v1 : Module {
		public VWord PPUAddress;
		public Array<VByte> Table;
		private VByte _temp;

		[Dependencies]
		private void Dependencies() {
			PPUAddress = VWord.New(Ram, $"{nameof(ShadowAttributeTable_v1)}_{nameof(PPUAddress)}");
			Table = Array<VByte>.New(64, Ram, $"{nameof(ShadowAttributeTable_v1)}_{nameof(Table)}");
			_temp = VByte.New(Ram, $"{nameof(ShadowAttributeTable_v1)}{nameof(_temp)}");
		}

		public void Clear() {
			Comment("Clear attribute table");
			Loop.Descend_Pre(X.Set(Table.Length), _ => {
				Table[X].Set(0);
			});
		}

		[Subroutine]
		public void UpdateAttributeTableImmediately() {
			NES.PPU.SetHorizontalWrite();
			NES.PPU.SetAddress(0x23C0);
			Loop.AscendWhile(X.Set(0), () => X.NotEquals((U8)Table.Length), _ => {
				NES.PPU.Data.Set(Table[X]);
			});
		}
	}
}

//public static class ShadowAttributeTable_old {
//	public static VWord PPUAddress;
//	public static Array<VByte> Table;
//	private static VByte _temp;
//	static ShadowAttributeTable_old() {
//		PPUAddress = VWord.New(GlobalRam, "ShadowAttributeTable_PPUAddress");
//		Table	= Array<VByte>.New(64, GlobalRam, "ShadowAttributeTable_attributeTable");
//		_temp = VByte.New(GlobalRam, "ShadowAttributeTable_temp");
//	}

//	public static void Clear() {
//		Comment("Clear attribute table");
//		Loop.Descend_Pre(X.Set(Table.Length), () => {
//			Table[X].Set(0);
//		});
//	}

//	[Subroutine]
//	public static void UpdateAttributeTableImmediately() {
//		NES.PPU.SetHorizontalWrite();
//		NES.PPU.SetAddress(0x23C0);
//		//Something is wrong with this partially unrolled version
//		//AscendWhile(X.Set(0), () => X.NotEquals((U8)(Table.Length / 4)), () => {
//		//	NES.PPU.Data.Set(Table[X]);
//		//	X++;
//		//	NES.PPU.Data.Set(Table[X]);
//		//	X++;
//		//	NES.PPU.Data.Set(Table[X]);
//		//	X++;
//		//	NES.PPU.Data.Set(Table[X]);
//		//});
//		Loop.AscendWhile(X.Set(0), () => X.NotEquals((U8)Table.Length), () => {
//			NES.PPU.Data.Set(Table[X]);
//		});
//	}

//	[Subroutine]
//	public static void UpdateAttr() {
//		//PPUAddress = addr of bg tile to update
			
//		//Stack.Backup(Register.A);
			
//		_temp.Set(0xC0); //starting lo of attr address
//		If(() => A.Set(PPUAddress.Lo).IsNegative(), () => {
//			//_temp.SetAdd(8);
//			_temp.Set(z => z.Add(8));
//		});
//		_temp.Set(z => z.Add(PPUAddress.Lo.And(0x1F).Divide(4)));
//		_temp.Set(z => z.Add(PPUAddress.Hi.And(0x0F).Multiply(4)));

//		//NES.PPU.SetAddress()
//		//Stack.Restore(Register.A);
//	}
//}