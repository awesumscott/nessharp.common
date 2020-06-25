using NESSharp.Core;
using System;
using static NESSharp.Core.AL;

//http://forums.nesdev.com/viewtopic.php?f=2&t=11336
namespace NESSharp.Common {
	public static class Division {
		public static RegisterA Divide(this RegisterA a, U8 v) {
			switch (v) {
				case 2:
					return DivideByTwo();
				case 4:
					return DivideByFour();
				case 8:
					return DivideByEight();
				case 16:
					return DivideBySixteen();
				case 32:
					return DivideByThirtyTwo();
			}
			throw new NotImplementedException();
		}
		public static RegisterA DivideByTwo() {
			//1 byte, 2 cycles
			return A.LogicalShiftRight();
		}
		public static RegisterA DivideByThree() {
			//18 bytes, 30 cycles
			Temp[0].Set(A);
			return A.LogicalShiftRight().ADC(21).LogicalShiftRight()
					.ADC(Temp[0]).ROR().LogicalShiftRight()
					.ADC(Temp[0]).ROR().LogicalShiftRight()
					.ADC(Temp[0]).ROR().LogicalShiftRight();
		}
		public static RegisterA DivideByFour() {
			//2 bytes, 4 cycles
			DivideByTwo();
			return DivideByTwo();
		}
		public static RegisterA DivideByFive() {
			//18 bytes, 30 cycles
			Temp[0].Set(A);
			return A.LogicalShiftRight().ADC(13)
					.ADC(Temp[0]).ROR().LogicalShiftRight().LogicalShiftRight()
					.ADC(Temp[0]).ROR()
					.ADC(Temp[0]).ROR().LogicalShiftRight().LogicalShiftRight();
		}
		public static RegisterA DivideBySix() {
			//17 bytes, 30 cycles
			DivideByTwo();
			return DivideByThree();
		}
		public static RegisterA DivideBySeven() {
			//15 bytes, 27 cycles
			Temp[0].Set(A);
			return A.LogicalShiftRight().LogicalShiftRight().LogicalShiftRight()
					.ADC(Temp[0]).ROR().LogicalShiftRight().LogicalShiftRight()
					.ADC(Temp[0]).ROR().LogicalShiftRight().LogicalShiftRight();
		}
		public static RegisterA DivideByEight() {
			//3 bytes, 6 cycles
			DivideByFour();
			return DivideByTwo();
		}
		public static RegisterA DivideByTen() {
			//17 bytes, 30 cycles
			Temp[0].Set(A.LogicalShiftRight());
			return A.LogicalShiftRight()
					.ADC(Temp[0]).ROR().LogicalShiftRight().LogicalShiftRight()
					.ADC(Temp[0]).ROR()
					.ADC(Temp[0]).ROR().LogicalShiftRight().LogicalShiftRight();
		}
		public static RegisterA DivideBySixteen() {
			DivideByEight();
			return DivideByTwo();
		}
		public static RegisterA DivideByThirtyTwo() {
			DivideBySixteen();
			return DivideByTwo();
		}
	}
}
