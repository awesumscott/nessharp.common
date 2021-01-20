using NESSharp.Core;
using static NESSharp.Core.AL;

namespace NESSharp.Common {
	public class ShiftQueue {
		public Array<VByte> Values; //queue for input to lazy-execute slide actions
		private U8 _clearVal = 0;
		public static ShiftQueue New(RAMRange ram, U8 length, string name, U8 clearValue) {
			var bq = new ShiftQueue();
			bq.Values	= Array<VByte>.New(length, ram, name + "_values");
			bq._clearVal = clearValue;
			return bq;
		}

		public void Clear() {
			Loop.Descend_Pre(X.Set(Values.Length), _ => {
				Values[X].Set(_clearVal);
			});
		}

		public void Push(VByte v) {
			X.Set(0);
			Loop.AscendWhile(X, () => X.NotEquals(Values.Length), loop => {
				If.True(() => Values[X].Equals(_clearVal), () => {
					Comment("Add action to the end of the queue");
					Values[X].Set(v);
					loop.Break();
				});
			});
		}
		
		public RegisterA Peek() {
			X.Set(0);
			A.Set(Values[X]);
			return A;
		}
		public void Pop() {
			Comment("Shift other actions left");
			X.Set(0);
			Loop.AscendWhile(X, () => X.NotEquals((U8)(Values.Length - 1)), _ => {
				//Get next value
				X.State.Unsafe(() => { //indicate X modification needs careful attention
					X.Inc();
					A.Set(Values[X]); //Store in current location
					X.Dec();
				});
				Values[X].Set(A);
			});
			Comment("Last command is empty");
			Values[X.Set(Values.Length - 1)].Set(_clearVal);
		}
	}
}
