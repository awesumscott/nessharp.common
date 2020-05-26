using NESSharp.Core;
using System;
using System.Collections.Generic;
using System.Text;
using static NESSharp.Core.AL;

namespace NESSharp.Common.Mapper30 {
	public static class Flashing {
		//These variables need to be located in the Zero Page, due to the need for Indirect addressing.
		private static VByte TargetBank		= VByte.New(GlobalZp, "Flashing_TargetBank");
		private static Ptr TargetAddress	= Ptr.New("Flashing_TargetAddress");
		private static VByte SourceAddress	= VByte.New(GlobalZp, "Flashing_SourceAddress");
		private static VByte ReturnBank		= VByte.New(GlobalZp, "Flashing_ReturnBank");
		private static Address FlashRamPage	= Addr(0x0700);
		private static Address AddrWriteVerify	= Addr(0x0);
		private static Address AddrEraseSector	= Addr(0x0);
		private static Address AddrWriteByte	= Addr(0x0);
		private static Address AddrSoftwareIdentify	= Addr(0x0);

		private static readonly Stream writeAddr0 = new Stream(0xC000);
		private static readonly Stream writeAddr1 = new Stream(0x9555);
		private static readonly Stream writeAddr2 = new Stream(0xAAAA);
		private static U8 _lenTotalRamLength;

		static Flashing() {
			//Determine RAM func lengths
			var lenWriteVerify =		(U8)Length(_WriteVerify);
			var lenEraseSector =		(U8)Length(_EraseSector);
			var lenWriteByte =			(U8)Length(_WriteByte);
			var lenSoftwareIdentify =	(U8)Length(_SoftwareIdentify);
			//Reserve chunks in RAM
			var ramWriteVerify =		GlobalRam.Dim(lenWriteVerify);
			var ramEraseSector =		GlobalRam.Dim(lenEraseSector);
			var ramWriteByte =			GlobalRam.Dim(lenWriteByte);
			var ramSoftwareIdentify =	GlobalRam.Dim(lenSoftwareIdentify);
			//Save starting addresses for invoking them with GoSub
			AddrWriteVerify =			ramWriteVerify[0];
			AddrEraseSector =			ramEraseSector[0];
			AddrWriteByte =				ramWriteByte[0];
			AddrSoftwareIdentify =		ramSoftwareIdentify[0];
			//Total length
			_lenTotalRamLength = (U8)(lenWriteVerify + lenEraseSector + lenWriteByte + lenSoftwareIdentify);
		}
		
		[CodeSection]
		private static void _WriteVerify() {
			Loop.Do(() => {
				A.Set(TargetAddress[Y]).CMP(TargetAddress[Y]);
			}).While(() => A.NotEquals(0));
			Return();
			
			//If(() => A.Set(TargetAddress[Y]).Equals(TargetAddress[Y]), () => {
			//	Return();
			//});
			//Use(Label["EraseSector"]);
		}
		[CodeSection]
		private static void _EraseSector() {
			writeAddr0.Write(0x01);
			writeAddr1.Write(0xAA);
			writeAddr0.Write(0x00);
			writeAddr2.Write(0x55);

			writeAddr0.Write(0x01);
			writeAddr1.Write(0x80);

			writeAddr0.Write(0x01);
			writeAddr1.Write(0xAA);
			writeAddr0.Write(0x00);
			writeAddr2.Write(0x55);
			
			writeAddr0.Write(TargetBank);

			Y.Set(0);
			A.Set(0x30).STA(TargetAddress[Y]);
			//GoSub(FlashRamPage);
			GoSub(AddrWriteVerify);
			writeAddr0.Set(ReturnBank);
			Return();
		}
		[CodeSection]
		private static void _WriteByte() {
			Stack.Preserve(A, () => {
				writeAddr0.Write(0x01);
				writeAddr1.Write(0xAA);
				writeAddr0.Write(0x00);
				writeAddr2.Write(0x55);

				writeAddr0.Write(0x01);
				writeAddr1.Write(0xA0);
			
				writeAddr0.Write(TargetBank);
			});

			A.STA(TargetAddress[Y]);
			//GoSub(FlashRamPage);
			GoSub(AddrWriteVerify);
			writeAddr0.Set(ReturnBank);
			Return();
		}
		[CodeSection]
		private static void _SoftwareIdentify() {
			TargetAddress.Hi.Set(0x80);
			A.Set(1);
			TargetAddress.Lo.Set(A);
			writeAddr0.Write(A);
			writeAddr1.Write(0xAA);
			writeAddr0.Write(0x00);
			writeAddr2.Write(0x55);
			
			writeAddr0.Write(0x01);
			writeAddr1.Write(0x90);
			
			//GoSub(FlashRamPage);
			GoSub(AddrWriteVerify);
			X.Set(Addr(0x8000));
			Y.Set(Addr(0x8001));
			A.Set(0xF0).STA(Addr(0x8000));
			//GoSub(FlashRamPage);
			GoSub(AddrWriteVerify);
			writeAddr0.Set(ReturnBank);
			Return();
		}

		public static void Init() {
			//Copy funcs to those RAM chunks
			Y.Set(0);
			Loop.Do(() => {
				AddrWriteVerify[Y].Set(A.Set(LabelFor(_WriteVerify).Offset(Y)));
				Y++;
				CPU6502.CPY(_lenTotalRamLength);
			}).While(() => Y.NotEquals(0));
		}

		//[Subroutine]
		//private static void _WriteByte() {
		//	//lda #0
		//	//sta TargetAddress
		//	TargetAddress.Lo.Set(0);

		//	//lda #>addr
		//	//sta TargetAddress+1
		//	//lda #bank
		//	//sta TargetBank
		//	//; Restore to same bank
		//	//lda #bank
		//	//sta ReturnBank
		//	//lda #value
		//	//ldy #<addr
		//	GoSub(AddrWriteByte);
		//}
		public static void WriteByte(U8 bank, Address addr, object value) {
			TargetAddress.Lo.Set(0);
			TargetAddress.Hi.Set(addr.Hi);
			TargetBank.Set(bank);
			ReturnBank.Set(bank);
			A.Set(value);
			Y.Set(addr.Lo);
			GoSub(AddrWriteByte);
		}
		public static void WriteByte(U8 bank, OpLabel addr, object value) {
			TargetAddress.Lo.Set(0);
			TargetAddress.Hi.Set(addr.Hi());
			TargetBank.Set(bank);
			ReturnBank.Set(bank);
			A.Set(value);
			Y.Set(addr.Lo());
			GoSub(AddrWriteByte);
		}



		//;Call this routine to copy all the flash related functions into ram.
		//CopyFlashRoutine:
		//	LDY #$00
		//CopyFlashRoutineLoop:
		//	LDA WriteVerify,Y
		//	STA FlashRamPage,Y
		//	INY
		//	CPY #(CopyFlashRoutine-WriteVerify)
		//	BNE CopyFlashRoutineLoop
		//	RTS
	}
}
