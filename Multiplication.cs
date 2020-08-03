using NESSharp.Core;
using System;
using System.Collections.Generic;
using System.Text;
using static NESSharp.Core.AL;

namespace NESSharp.Common {
	public static class Multiplication {
		public static RegisterA Multiply(this VByte v, U8 n) {
			return A.Set(v).Multiply(n);
		}
		public static RegisterA Multiply(this RegisterA a, U8 n) {
			switch ((int)n) {
				case 2:
					Carry.Clear();
					//return A.RotateLeft();
					return A.ASL(); //no need to CLC
				case 4:
					return A.Multiply(2).Multiply(2);
				case 8:
					return A.Multiply(4).Multiply(2);
				case 16:
					return A.Multiply(8).Multiply(2);
				default: throw new NotImplementedException();
			}
		}
	}
}
