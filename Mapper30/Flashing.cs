using NESSharp.Core;
using static NESSharp.Core.CPU6502;

namespace NESSharp.Common.Mapper30;

public static class Flashing {
	//These variables need to be located in the Zero Page, due to the need for Indirect addressing.
	private static VByte TargetBank		= VByte.New(NES.zp, "Flashing_TargetBank");
	private static Ptr TargetAddress	= Ptr.New(NES.zp, "Flashing_TargetAddress");
	private static VByte SourceAddress	= VByte.New(NES.zp, "Flashing_SourceAddress");
	private static VByte ReturnBank		= VByte.New(NES.zp, "Flashing_ReturnBank");
	private static Address FlashRamPage		= 0x0700;
	private static Address AddrWriteVerify	= 0x0;
	private static Address AddrEraseSector	= 0x0;
	private static Address AddrWriteByte	= 0x0;
	private static Address AddrSoftwareIdentify	= 0x0;

	private static readonly Bus writeAddr0 = new Bus(0xC000);
	private static readonly Bus writeAddr1 = new Bus(0x9555);
	private static readonly Bus writeAddr2 = new Bus(0xAAAA);
	private static U8 _lenTotalRamLength;

	static Flashing() {
		//Determine RAM func lengths
		var lenWriteVerify =		AL.Length(_WriteVerify);
		var lenEraseSector =		AL.Length(_EraseSector);
		var lenWriteByte =			AL.Length(_WriteByte);
		var lenSoftwareIdentify =	AL.Length(_SoftwareIdentify);
		//Reserve chunks in RAM
		var ramWriteVerify =		NES.ram.Dim(lenWriteVerify);
		var ramEraseSector =		NES.ram.Dim(lenEraseSector);
		var ramWriteByte =			NES.ram.Dim(lenWriteByte);
		var ramSoftwareIdentify =	NES.ram.Dim(lenSoftwareIdentify);
		//Save starting addresses for invoking them with GoSub
		AddrWriteVerify =			ramWriteVerify[0];
		AddrEraseSector =			ramEraseSector[0];
		AddrWriteByte =				ramWriteByte[0];
		AddrSoftwareIdentify =		ramSoftwareIdentify[0];
		//Total length
		_lenTotalRamLength = lenWriteVerify + lenEraseSector + lenWriteByte + lenSoftwareIdentify;
	}
	
	[CodeSection]
	private static void _WriteVerify() {
		Loop.Do_old(_ => {
			A.Set(TargetAddress[Y]).CMP(TargetAddress[Y]);
			//TODO: verify, there should probably be some Y inc/dec in here
		}).While(() => A.NotEquals(0));
		AL.Return();
		
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
		AL.GoSub(AddrWriteVerify);
		writeAddr0.Set(ReturnBank);
		AL.Return();
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
		AL.GoSub(AddrWriteVerify);
		writeAddr0.Set(ReturnBank);
		AL.Return();
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
		AL.GoSub(AddrWriteVerify);
		X.Set(AL.Addr(0x8000));
		Y.Set(AL.Addr(0x8001));
		A.Set(0xF0).STA(AL.Addr(0x8000));
		//GoSub(FlashRamPage);
		AL.GoSub(AddrWriteVerify);
		writeAddr0.Set(ReturnBank);
		AL.Return();
	}

	public static void Init() {
		//Copy funcs to those RAM chunks
		Y.Set(0);
		Loop.Do_old(_ => {
			AddrWriteVerify[Y].Set(A.Set(AL.LabelFor(_WriteVerify)[Y]));
			Y.Inc();
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
	public static void WriteByte(U8 bank, Address addr, IOperand value) {
		TargetAddress.Lo.Set(0);
		TargetAddress.Hi.Set(addr.Hi);
		TargetBank.Set(bank);
		ReturnBank.Set(bank);
		A.Set(value);
		Y.Set(addr.Lo);
		AL.GoSub(AddrWriteByte);
	}
	public static void WriteByte(U8 bank, Label addr, IOperand value) {
		TargetAddress.Lo.Set(0);
		TargetAddress.Hi.Set(addr.Hi());
		TargetBank.Set(bank);
		ReturnBank.Set(bank);
		A.Set(value);
		Y.Set(addr.Lo());
		AL.GoSub(AddrWriteByte);
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
