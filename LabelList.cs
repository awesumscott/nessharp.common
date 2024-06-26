﻿using NESSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using static NESSharp.Core.CPU6502;

namespace NESSharp.Common;

public class LabelList {
	private Label[] _labels;
	private Label _lo, _hi;
	private Label _stackJumpHelperFunc = null;
	public LabelList(params Action[] methods) {
		_lo = AL.Labels.New();
		_hi = AL.Labels.New();
		_labels = methods.Select(AL.LabelFor).ToArray();
	}
	public LabelList(params Label[] labels) {
		_lo = AL.Labels.New();
		_hi = AL.Labels.New();
		_labels = labels;
	}
	public U8 Length => (U8)_labels.Length;
	public void WriteList() {
		_lo.Write();
		AL.Raw(_labels.Select(x => x.Lo()).ToArray());
		
		_hi.Write();
		AL.Raw(_labels.Select(x => x.Hi()).ToArray());
	}
	public void WriteStackJumpList() {
		_lo.Write();
		AL.Raw(_labels.Select(x => x.Offset(-1).Lo()).ToArray());
		
		_hi.Write();
		AL.Raw(_labels.Select(x => x.Offset(-1).Hi()).ToArray());


		//Write the helper function for stack jumps
		_stackJumpHelperFunc = AL.Labels.New().Write();
		A.Set(_hi[X]);
		Stack.Backup(Register.A);
		A.Set(_lo[X]);
		Stack.Backup(Register.A);
		AL.Return();
	}
	/// <summary>StackJumpV2 doesn't require internal Lo and Hi labels, so it can be accessed by only a pointer to the label, but can only have 127 entries.</summary>
	/// <returns>Label to the stack jump address list to be used with a pointer and an index in A.</returns>
	public Label WriteStackJumpListV2() {
		var newStackJumpHelperFunc = AL.Labels.New().Write();
		var vals = new List<IResolvable<U8>>();
		foreach(var lbl in _labels) {
			var offsetAddr = lbl.Offset(-1);
			vals.Add(offsetAddr.Hi());
			vals.Add(offsetAddr.Lo());
		}
		AL.Raw(vals.ToArray());


		////Write the helper function for stack jumps
		//X.Set(A.Multiply(2));
		//A.Set(_hi[X]);
		//Stack.Backup(Register.A);
		//X.Inc();
		//A.Set(_lo[X]);
		//Stack.Backup(Register.A);
		//Return();
		return newStackJumpHelperFunc;
	}

	public void GoTo(U8 index) => AL.GoTo_Indirect(this[X.Set(index)]);
	public void GoTo(IndexingRegister reg) => AL.GoTo_Indirect(this[reg]);
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
			AL.Temp[0].Set(_lo[reg]);
			AL.Temp[1].Set(_hi[reg]);
			
			//TODO: fix these up: all Ref functions need versions that can accept VByte lists
			return VWord.Ref(AL.Temp[0], 2, AL.Temp[0].Name);
		}
	}
	public Label this[U8 index] => _labels[index];
}
