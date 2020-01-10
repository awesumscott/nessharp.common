using System;
using System.Collections.Generic;
using System.Text;
using NESSharp.Core;
using static NESSharp.Core.AL;

namespace NESSharp.Common {
	//TODO: figure out if this should either be explicitly named to indicate it's for 4 screen scrolling, or if it should support other options
	public static class Scrolling {
		[Dependencies]
		public static void Variables() {
		}
		public static void Update() {
			NES.PPU.ScrollTo(NES.PPU.LazyScrollX, NES.PPU.LazyScrollY);
		}
		private static void _xAddCorrect() {
			If(() => Carry.IsSet(), () => {
				NES.PPU.LazyControl.Set(z => z.Xor(0b01));
			});
		}
		private static void _xSubtractCorrect() {
			If(() => Carry.IsClear(), () => {
				NES.PPU.LazyControl.Set(z => z.Xor(0b01));
			});
		}
		private static void _yAddCorrect() {
			A.Set(239); //TODO: replace after implementing GreaterThan
			Use(Asm.CMP.Absolute, NES.PPU.LazyScrollY); //Y >= 240?
			If(() => Carry.IsClear(), () => {
				NES.PPU.LazyScrollY.Set(z => z.Add((U8)16));
				NES.PPU.LazyControl.Set(z => z.Xor(0b10));
			});
		}
		private static void _ySubtractCorrect() {
			If(() => Carry.IsClear(), () => {
				NES.PPU.LazyScrollY.Set(z => z.Subtract(16));
				NES.PPU.LazyControl.Set(z => z.Xor(0b10));
			});
		}
		public static void AddX(U8 v) {
			NES.PPU.LazyScrollX.Set(z => z.Add(v));
			_xAddCorrect();
		}
		public static void SubtractX(U8 v) {
			NES.PPU.LazyScrollX.Set(z => z.Subtract(v));
			_xSubtractCorrect();
		}
		public static void AddY(U8 v) {
			NES.PPU.LazyScrollY.Set(z => z.Add(v));
			_yAddCorrect();
		}
		public static void SubtractY(U8 v) {
			NES.PPU.LazyScrollY.Set(z => z.Subtract(v));
			_ySubtractCorrect();
		}
	}
}
