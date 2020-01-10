using System;
using System.Collections.Generic;
using System.Text;
using static NESSharp.Core.AL;
using NESSharp.Core;

namespace NESSharp.Common {
	public static class Math {
		public static void Clamp(Var8 v, U8 low, U8 high) {
			If(	Option(() => A.Set(v).LessThan(low), () => {
					v.Set(low);
				}),
				Option(() => A.GreaterThan(high), () => {
					v.Set(high);
				})
			);
		}
	}
}
