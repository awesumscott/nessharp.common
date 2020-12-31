﻿using System;
using System.Collections.Generic;
using System.Text;
using static NESSharp.Core.AL;
using NESSharp.Core;

namespace NESSharp.Common {
	public static class Math {
		public static void Clamp(VByte v, U8 low, U8 high) {
			If(	Option(() => A.Set(v).LessThan(low), () => {
					v.Set(low);
				}),
				Option(() => A.GreaterThan(high), () => {
					v.Set(high);
				})
			);
		}
		public static RegisterA Negate(RegisterA _) {
			A.Xor(0xFF);
			Carry.Set();
			return A.ADC(0);
		}
		public static RegisterA Negate(VByte v) {
			A.Set(v).Xor(0xFF);
			Carry.Set();
			return A.ADC(0);
		}

		public static RegisterA Abs(this RegisterA _) {
			If(A.IsNegative(), () => {
				A.Xor(0xFF).Add(1);
			});
			return A;
		}
		public static RegisterA Abs(IOperand o) {
			If(A.Set(o).IsNegative(), () => {
				A.Xor(0xFF).Add(1);
			});
			return A;
		}
	}
}
