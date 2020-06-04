using System;
using System.Collections.Generic;
using System.Text;
using static NESSharp.Core.AL;
using NESSharp.Core;
using System.Linq;

namespace NESSharp.Common {
	public class LabelList {
		private OpLabel[] _labels;
		private OpLabel _lo, _hi;
		private OpLabel _stackJumpHelperFunc = null;
		public LabelList(params Action[] methods) {
			_lo = Label.New();
			_hi = Label.New();
			_labels = methods.Select(LabelFor).ToArray();
		}
		public LabelList(params OpLabel[] labels) {
			_lo = Label.New();
			_hi = Label.New();
			_labels = labels;
		}
		public void WriteList() {
			Use(_lo);
			Raw(_labels.Select(x => x.Lo()).ToArray());
			
			Use(_hi);
			Raw(_labels.Select(x => x.Hi()).ToArray());
		}
		public void WriteStackJumpList() {
			Use(_lo);
			Raw(_labels.Select(x => x.Lo(-1)).ToArray());
			
			Use(_hi);
			Raw(_labels.Select(x => x.Hi(-1)).ToArray());


			//Write the helper function for stack jumps
			_stackJumpHelperFunc = Label.New();
			Use(_stackJumpHelperFunc);
			A.Set(_hi[X]);
			Stack.Backup(Register.A);
			A.Set(_lo[X]);
			Stack.Backup(Register.A);
			Return();
		}
		
		public void GoTo(U8 index) => GoTo_Indirect(this[X.Set(index)]);
		public void GoTo(IndexingRegisterBase reg) => GoTo_Indirect(this[reg]);
		//public void GoTo(RegisterBase r) => GoTo_Indirect(this[r]);
		
		public void GoSub(RegisterY y) {
			if (_stackJumpHelperFunc == null) throw new Exception("Stack jump list not written for this label list");
			X.Set(y);
			AL.GoSub(_stackJumpHelperFunc);
			//GoTo_Indirect(this[X.Set(index)]);
		}
		public void GoSub(U8 index) {
			if (_stackJumpHelperFunc == null) throw new Exception("Stack jump list not written for this label list");
			X.Set(index);
			AL.GoSub(_stackJumpHelperFunc);
			//GoTo_Indirect(this[X.Set(index)]);
		}
		public void GoSub(VByte index) {
			if (_stackJumpHelperFunc == null) throw new Exception("Stack jump list not written for this label list");
			X.Set(index);
			AL.GoSub(_stackJumpHelperFunc);
			//GoTo_Indirect(this[X.Set(index)]);
		}

		public VWord this[IndexingRegisterBase reg] {
			get {
				Temp[0].Set(_lo[reg]);
				Temp[1].Set(_hi[reg]);
				
				//TODO: fix these up: VWord needs a Ref() func, and all need versions that can accept VByte lists
				return VWord.Ref(Temp[0], 2);
			}
		}
	}
}
