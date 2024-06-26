﻿using System;
using System.Collections.Generic;
using System.Text;
using NESSharp.Core;
using static NESSharp.Core.CPU6502;


namespace NESSharp.Common {
	public class ChrCopy : Module {
		
	}
}

namespace NESSharp.Common.Mapper30 {
	public static class CopyChr_old {
		public static Ptr GraphicsPtr;

		[Dependencies]
		public static void Variables() {
			GraphicsPtr = Ptr.New(NES.zp, "GraphicsPtr");
		}
		public static Address Bank;
		[RegParam(Register.A, "Bank to switch to")]
		[RegParam(Register.X, "16 byte blocks to copy")]
		[RegParam(Register.Y, "Starting offset")]
		[Subroutine]
		public static void ChrCopy() {
			AL.Comment("Copy a chunk to CHR (<=255 bytes)");

			NES.PPU.Address.Write(Y, (U8)0); //Hi then Lo
			Y.Set(0); //starting index into the first page

			//Ptr p = Globals.graphicsPtr;
			//p.Bytes[1]++;

			Loop.Do_old(_ => {
				Loop.Do_old(_ => {
					NES.PPU.Data.Write(GraphicsPtr[Y]);
					Y.Inc();
					//TODO: more here
				}).While(() => Y.NotEquals(0));
				//GraphicsPtr.Address[1]++;
				GraphicsPtr.Hi.Inc();
				X.Dec();
				//TODO: more here
			}).While(() => X.NotEquals(0));
		}
		[Subroutine]
		public static void ChrPageCopy00() {
			//Set GraphicsPtr before calling!
			//Set Y to 0 or 10
			//Set A to the bank containing the CHR
			const byte CHR_ONE_PAGE = 16;

			X.Set(BankSwitching.Bank);
			BankSwitching.SwitchTo(A);
			//A.Set(Bank);
			A.State.Push();		//A is needed later and is backed up as a consequence of backing up X
			Stack.Backup(Register.X);

			//TODO: finish after implementing Pointer
			X.Set(CHR_ONE_PAGE);
			Y.Set(0);
			AL.GoSub(ChrCopy);
			//X.Set(CHR_ONE_PAGE);
			//Y.Set(10);
			//GoSub(ChrCopy);

			Stack.Restore(Register.A);
			
			X.State.Pop();		//get rid of the unneeded X value
			X.State.Alter();	//indicate it's now unknown

			BankSwitching.SwitchTo(A);
		}
		[Subroutine]
		public static void ChrPageCopy10() {
			//Set GraphicsPtr before calling!
			//Set Y to 0 or 10
			//Set A to the bank containing the CHR
			const byte CHR_ONE_PAGE = 16;

			X.Set(BankSwitching.Bank);
			BankSwitching.SwitchTo(A);
			//A.Set(Bank);
			A.State.Push();		//A is needed later and is backed up as a consequence of backing up X
			Stack.Backup(Register.X);

			//TODO: finish after implementing Pointer
			X.Set(CHR_ONE_PAGE);
			Y.Set(0x10);
			AL.GoSub(ChrCopy);
			//X.Set(CHR_ONE_PAGE);
			//Y.Set(10);
			//GoSub(ChrCopy);

			Stack.Restore(Register.A);
			
			X.State.Pop();		//get rid of the unneeded X value
			X.State.Alter();	//indicate it's now unknown

			BankSwitching.SwitchTo(A);
		}
		[Subroutine]
		public static void FullChrCopy() {
			//Set GraphicsPtr before calling!
			//const byte CHR_ONE_PAGE = 16;
			const byte CHR_TWO_PAGES = 32;
			AL.Comment("Backup current bank");
			A.Set(BankSwitching.Bank);
			Stack.Backup(Register.A);

			BankSwitching.SwitchPrgTo(30);

			//TODO: finish after implementing Pointer
			X.Set(CHR_TWO_PAGES);
			Y.Set(0);
			AL.GoSub(ChrCopy);
			//Globals.graphicsPtr.PointTo(LabelFor(Rom.ChrBank1));
			//X.Set(CHR_ONE_PAGE);
			//Y.Set(10);
			//GoSub(ChrCopy);
			
			Stack.Restore(Register.A);
			AL.GoSub(BankSwitching._SwitchBanks);
		}
	}
}
