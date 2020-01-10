﻿using NESSharp.Core;
using static NESSharp.Core.AL;

namespace NESSharp.Common {
	public class ShiftQueue {
		public Array<Var8> Values; //queue for input to lazy-execute slide actions
		private U8 _clearVal = 0;
		public static ShiftQueue New(RAM ram, U8 length, string name, U8 clearValue) {
			var bq = new ShiftQueue();
			bq.Values	= Array<Var8>.New(length, ram, name + "_values");
			bq._clearVal = clearValue;
			return bq;
		}

		public void Clear() {
			Loop.Descend_Pre(X.Set(Values.Length), () => {
				Values[X].Set(_clearVal);
			});
		}

		public void Push(Var8 v) {
			var lblBreak = Label.New();
			X.Set(0);
			Loop.AscendWhile(X, () => X.NotEquals((U8)Values.Length), () => {
				If(() => Values[X].Equals(_clearVal), () => {
					Comment("Add action to the end of the queue");
					Values[X].Set(v);
					GoTo(lblBreak);
				});
			});
			Use(lblBreak);
		}
		
		public RegisterA Peek() {
			X.Set(0);
			A.Set(Values[X]);
			return A;
		}
		public void Pop() {
			Comment("Shift other actions left");
			X.Set(0);
			Loop.AscendWhile(X, () => X.NotEquals((U8)(Values.Length - 1)), () => {
				//Get next value
				X++;
				A.Set(Values[X]);
				//Store in current location
				X--;
				Values[X].Set(A);
			});
			Comment("Last command is empty");
			Values[X.Set((U8)(Values.Length - 1))].Set(_clearVal);
		}
	}
}