using System;
using System.Collections.Generic;
using System.Text;
using static NESSharp.Core.AL;
using NESSharp.Core;
using System.Linq;

namespace NESSharp.Common {
	public class LabelList {
		private Label[] _labels;
		private Label _lo, _hi;
		private Label _stackJumpHelperFunc = null;
		public LabelList(params Action[] methods) {
			_lo = Labels.New();
			_hi = Labels.New();
			_labels = methods.Select(LabelFor).ToArray();
		}
		public LabelList(params Label[] labels) {
			_lo = Labels.New();
			_hi = Labels.New();
			_labels = labels;
		}
		public U8 Length => (U8)_labels.Length;
		public void WriteList() {
			Use(_lo);
			Raw(_labels.Select(x => x.Lo()).ToArray());
			
			Use(_hi);
			Raw(_labels.Select(x => x.Hi()).ToArray());
		}
		public void WriteStackJumpList() {
			Use(_lo);
			Raw(_labels.Select(x => x.Offset(-1).Lo()).ToArray());
			
			Use(_hi);
			Raw(_labels.Select(x => x.Offset(-1).Hi()).ToArray());


			//Write the helper function for stack jumps
			_stackJumpHelperFunc = Labels.New();
			Use(_stackJumpHelperFunc);
			A.Set(_hi[X]);
			Stack.Backup(Register.A);
			A.Set(_lo[X]);
			Stack.Backup(Register.A);
			Return();
		}
		
		public void GoTo(U8 index) => GoTo_Indirect(this[X.Set(index)]);
		public void GoTo(IndexingRegister reg) => GoTo_Indirect(this[reg]);
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

		public VWord this[IndexingRegister reg] {
			get {
				Temp[0].Set(_lo[reg]);
				Temp[1].Set(_hi[reg]);
				
				//TODO: fix these up: VWord needs a Ref() func, and all need versions that can accept VByte lists
				return VWord.Ref(Temp[0], 2);
			}
		}
		public Label this[U8 index] {
			get {
				return _labels[index];
			}
		}
	}
}
