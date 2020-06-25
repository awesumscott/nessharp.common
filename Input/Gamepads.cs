using NESSharp.Core;
using System;
using static NESSharp.Core.AL;

namespace NESSharp.Common.Input {
	public class Gamepad : Struct {
		public VByte State { get; set; }
		public VByte StatePrev { get; set; }
		public VByte JustPressed { get; set; }
		public VByte JustReleased { get; set; }

		public RegisterA Held(U8 buttons) => State.And(buttons);
		public RegisterA Pressed(U8 buttons) => JustPressed.And(buttons);
		public RegisterA Released(U8 buttons) => JustReleased.And(buttons);
	}

	public class Gamepads : Module {
		private StructOfArrays<Gamepad> _instances;
		public VByte _index;
		private U8 _numPlayers;

		public Gamepads Setup(U8 players) {
			_numPlayers = players;
			return this;
		}

		public void Update() {
			GoSub(Read);
		}

		[Dependencies]
		private void Dependencies() {
			_index = VByte.New(Ram, "gamepad_index");
			_instances = StructOfArrays<Gamepad>.New("gamepadData", _numPlayers).Dim(Ram);
		}

		[Subroutine]
		private void Read() {
			NES.Controller.Latch();
			_index.Set(0);
			GoSub(UpdateController);
			if (_numPlayers >= 3) {
				_index.Set(2);
				GoSub(UpdateController);
			}
			if (_numPlayers >= 2) {
				_index.Set(1);
				GoSub(UpdateController2);
			}
			if (_numPlayers >= 4) {
				_index.Set(3);
				GoSub(UpdateController2);
			}
		}
		[Subroutine]
		[Clobbers(Register.A, Register.X, Register.Y)]
		private void UpdateController() {
			X.Set(_index);
			_instances[X].StatePrev.Set(_instances[X].State);
			Loop.Descend(Y.Set(8), () => {
				//If(	Option(() => _index.LessThan(2), () => {
				//		A.Set(NES.Controller.One).LogicalShiftRight();
				//	}),
				//	Default(() => {
				//		A.Set(NES.Controller.One).LogicalShiftRight();
				//	})
				//)
				
				A.Set(NES.Controller.One).LogicalShiftRight();
				_instances[X].State.SetROL();
			});

			_instances[X].JustReleased.Set(_instances[X].State.Xor(0xFF).And(_instances[X].StatePrev));
			_instances[X].JustPressed.Set(_instances[X].StatePrev.Xor(0xFF).And(_instances[X].State));
		}
		[Subroutine]
		[Clobbers(Register.A, Register.X, Register.Y)]
		private void UpdateController2() {
			X.Set(_index);
			_instances[X].StatePrev.Set(_instances[X].State);
			Loop.Descend(Y.Set(8), () => {
				A.Set(NES.Controller.Two).LogicalShiftRight();
				_instances[X].State.SetROL();
			});

			_instances[X].JustReleased.Set(_instances[X].State.Xor(0xFF).And(_instances[X].StatePrev));
			_instances[X].JustPressed.Set(_instances[X].StatePrev.Xor(0xFF).And(_instances[X].State));
		}
		public Gamepad this[IndexingRegisterBase offset] => _instances[offset];
		public Gamepad this[int offset] => _instances[offset];
	}

	[Obsolete]
	public static class Gamepads_old {
		public static int NumPlayers = 4;
		public static StructOfArrays<Gamepad> Player;
		public static VByte GamepadIndex = VByte.New(GlobalZp, "gamepad_index");

		static Gamepads_old() {
			Player = StructOfArrays<Gamepad>.New("gamepadData", 4).Dim(GlobalZp);
		}

		//[Declarations]
		//public static void Variables() {
			
		//}
		//[RegParam(Register.A, "Bank to switch to")]

		[Subroutine]
		public static void Read() {
			NES.Controller.Latch();

			//TODO: implement the "watching" flags for skipping unused input reads
			//with If(lambda : Input.Watching.And(Input.CONTROLLER_0) != 0):
			//	Input.controllerIndex.set(0)
			//	context.functions.FuncControllerUpdate.Call()
			//	with If(lambda : Input.Watching.And(Input.CONTROLLER_2) != 0):
			//		Input.controllerIndex.set(2)
			//		context.functions.FuncControllerUpdate.Call()
			//	EndIf()
			//EndIf()

			//with If(lambda : Input.Watching.And(Input.CONTROLLER_1) != 0):
			//	Input.controllerIndex.set(1)
			//	context.functions.FuncControllerUpdate.Call()
			//	with If(lambda : Input.Watching.And(Input.CONTROLLER_3) != 0):
			//		Input.controllerIndex.set(3)
			//		context.functions.FuncControllerUpdate.Call()
			//	EndIf()
			//EndIf()
			GamepadIndex.Set(0);
			GoSub(UpdateController);
			//GamepadIndex.Set(2);
			//GoSub(UpdateController);
			GamepadIndex.Set(1);
			GoSub(UpdateController);
			//GamepadIndex.Set(3);
			//GoSub(UpdateController);
		}

		[Subroutine]
		public static void UpdateController() {
			X.Set(GamepadIndex);
			Player[X].StatePrev.Set(Player[X].State);
			Loop.Descend(Y.Set(8), () => {
				A.Set(NES.Controller.One).LogicalShiftRight();
				Player[X].State.SetROL();
			});

			Player[X].JustReleased.Set(Player[X].State.Xor(0xFF).And(Player[X].StatePrev));
			Player[X].JustPressed.Set(Player[X].StatePrev.Xor(0xFF).And(Player[X].State));
		}
	}
	public static class GamepadsOnePlayer {

		[Dependencies]
		public static void Variables() {
		}
	}
	public static class GamepadsTwoPlayers {

		[Dependencies]
		public static void Variables() {
		}
	}
	public static class GamepadsThreePlayers {

		[Dependencies]
		public static void Variables() {
		}
	}
	public static class GamepadsFourPlayers {

		[Dependencies]
		public static void Variables() {
		}
	}
}
/*

in the ROM definition:
	ROMManager.Write(typeof(GamepadsOnePlayer));

Gamepads.Player[0].buttonsJustPressed
Gamepads.Read()


 */
