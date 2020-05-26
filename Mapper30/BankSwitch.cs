using NESSharp.Core;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using static NESSharp.Core.AL;

namespace NESSharp.Common.Mapper30 {
	public class BankedSubroutine {
		public object Key;
		public U8 Bank;
		public Action Method;
		public BankedSubroutine(object key, U8 bank, Action method) {
			Key = key;
			Bank = bank;
			Method = method;
		}
	}
	public class BankSwitchTable {//: List<BankedSubroutine> {//<object, BankedSubroutine> {
		//public new void Add(object name, BankedSubroutine bs) {
		//	//TODO: add var names to ASM output here
		//	base.Add(name, bs);
		//}
		public static OpLabel BankLabel;
		public static OpLabel SubLoLabel;
		public static OpLabel SubHiLabel;
		private List<BankedSubroutine> _list = new List<BankedSubroutine>();

		static BankSwitchTable() {
			BankLabel = Label["BankCallTable_Bank"];
			SubLoLabel = Label["BankCallTable_SubLo"];
			SubHiLabel = Label["BankCallTable_SubHi"];
		}
		public void Add(BankedSubroutine bs) {
			_list.Add(bs);
		}
		public void Write() {
			Use(BankLabel);
			//Raw(this.Select(x => (byte)x.Value.Bank).ToArray()); //Dictionary<object, BankedSubroutine> version
			//Raw(this.Cast<System.Collections.DictionaryEntry>().Select(x => (byte)((BankedSubroutine)x.Value).Bank).ToArray()); //OrderedDictionary version
			Raw(_list.Select(x => (byte)x.Bank).ToArray()); //List<BankedSubroutine> version
			Use(SubLoLabel);
			Raw(_list.Select(x => LabelFor(x.Method).Lo(-1)).ToArray());
			Use(SubHiLabel);
			Raw(_list.Select(x => LabelFor(x.Method).Hi(-1)).ToArray());
		}
		public U8 IndexOf(object name) => (U8)_list.IndexOf(_list.Where(x => x.Key.ToString() == name.ToString()).First());
		[Subroutine]
		private static void _BankCall() {
			//X = bankcallfunc ID
			A.Set(BankLabel.Offset(X));
			BankSwitching.SwitchPrgTo(A);
			A.Set(SubHiLabel.Offset(X));
			Stack.Backup(A);
			A.Set(SubLoLabel.Offset(X));
			Stack.Backup(A);
		}
		public void Call(OpLabel lbl) {
			X.Set(lbl.Reference());
			GoSub(_BankCall);
		}
		public void Call(OpLabelIndexed oli) {
			X.Set(A.Set(oli));
			GoSub(_BankCall);
		}
		public void Call(object name) {
			X.Set(IndexOf(name));
			GoSub(_BankCall);
		}
		public void BSCAR(object name) {
			X.Set(IndexOf(name));
			GoSub(_BankSwitchCallAndRestore);
		}


		[Subroutine]
		private static void _BankSwitchCallAndRestore() {
			A.Set(BankSwitching.Bank);
			Stack.Preserve(A, () => {
				GoSub(_BankCall); //Using JSR here instead of the usual JMP so it comes back here instead, because BC tables are all addr-1
			});
			//GoSub(BankSwitching._SwitchBanks);
			BankSwitching.SwitchPrgTo(A);
		}
	}
	public static class BankSwitching {
		public static VByte Bank;

		static BankSwitching() {
			Bank = VByte.New(GlobalZp, "bank_current");
		}

		[Dependencies]
		public static void _Variables() {
			//BSCARLo = new Var8();
			//BSCARLo.Name = "BSCAR_Lo";
			//BSCARLo.Address[0] = Addr(0x07F9);
			//BSCARHi = new Var8();
			//BSCARHi.Name = "BSCAR_Hi";
			//BSCARHi.Address[0] = Addr(0x07FA);
		}

		//[Subroutine]
		//public static void Init() {
		//	Comment("Load BSCAR into RAM");
		//	RepeatX(0, 15, () => {
		//		_BSCARSub[X].Set(LabelFor(_BankSwitchCallAndRestore).Offset(X));
		//	});
		//}

		[Subroutine]
		[RegParam(Register.A, "Bank to switch to")]
		public static void _SwitchBanks() {
			//If(() => A.Equals(Bank), () => {
			//	Comment("already there, so jump back");
			//	Return();
			//});
			Y.Set(A);
			Bank.Set(Y);
			Use(Label["BankSwitchNoSave"]);
			LabelFor(_BankTable).Offset(Y).Set(LabelFor(_BankTable).Offset(Y));
		}

		[DataSection]
		private static void _BankTable() {
			//bits 6-5 used for CHR-RAM bankswitching
			var bankIds = new byte[128]; //32-1 to skip the fixed bank
			for (byte i = 0; i < 128; i++)
				bankIds[i] = i;
			Raw(bankIds);
		}

		public static void SwitchPrgTo(U8 bankNum) {
			if (bankNum > 31) throw new Exception("PRG bank range is 0-31");
			//Carry.Clear();
			Bank.And(0b11100000).Or(bankNum);
			GoSub(_SwitchBanks);
		}
		public static void SwitchPrgTo(RegisterA a) {
			//Carry.Clear();
			Temp[0].Set(A);
			Bank.And(0b11100000).Or(Temp[0]);
			GoSub(_SwitchBanks);
		}
		public static void SwitchChrTo(U8 bankNum) {
			if (bankNum > 31) throw new Exception("PRG bank range is 0-31");
			Carry.Clear();
			A.Set(bankNum).ROL().ROL().ROL().ROL().ROL();
			Temp[0].Set(A);
			Bank.And(0b00011111).ADC(Temp[0]);
			GoSub(_SwitchBanks);
		}
		public static void SwitchChrTo(RegisterA a) {
			Carry.Clear();
			A.ROL().ROL().ROL().ROL().ROL();
			Temp[0].Set(A);
			Bank.And(0b00011111).ADC(Temp[0]);
			GoSub(_SwitchBanks);
		}
		//TODO: replace this with enum flags for PRG and CHR banks
		public static void SwitchTo(U8 bankNum) {
			if (bankNum > 31) throw new Exception("PRG bank range is 0-31");
			A.Set(bankNum);
			GoSub(_SwitchBanks);
		}
		public static void SwitchTo(RegisterA a) {
			GoSub(_SwitchBanks);
		}

	}
}
