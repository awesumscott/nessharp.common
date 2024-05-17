using NESSharp.Core;
using static NESSharp.Core.CPU6502;

namespace NESSharp.Common;

public static class Math {
	public static void Clamp(VByte v, U8 low, U8 high) {
		If.Block(c => c
			.True(() => A.Set(v).LessThan(low),	() => v.Set(low))
			.True(() => A.GreaterThan(high),		() => v.Set(high))
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
		If.True(A.IsNegative, () => A.Xor(0xFF).Add(1));
		return A;
	}
	public static RegisterA Abs(IOperand o) {
		If.True(A.Set(o).IsNegative, () => A.Xor(0xFF).Add(1));
		return A;
	}
}
