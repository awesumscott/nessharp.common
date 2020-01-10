using NESSharp.Core;
using static NESSharp.Core.AL;

namespace NESSharp.Common.Input {
	public class Gamepad : Struct {
		public Var8 State { get; set; }
		public Var8 StatePrev { get; set; }
		public Var8 JustPressed { get; set; }
		public Var8 JustReleased { get; set; }

		public RegisterA Held(U8 buttons) => State.And(buttons);
		public RegisterA Pressed(U8 buttons) => JustPressed.And(buttons);
		public RegisterA Released(U8 buttons) => JustReleased.And(buttons);
	}
	public static class Gamepads {
		public static int NumPlayers = 4;
		public static StructOfArrays<Gamepad> Player;
		public static Var8 GamepadIndex = Var8.New(zp, "gamepad_index");

		static Gamepads() {
			Player = StructOfArrays<Gamepad>.New("gamepadData", 4).Dim(zp);
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
			Y.Set(GamepadIndex);
			Player[Y].StatePrev.Set(Player[Y].State);
			X.Set(8);
			//TODO: make more loop options to avoid manual locking
			//X.Lock();
			Loop.Do(() => {
				Stack.Preserve(X, () => {
					A.Set(NES.Controller.One).LogicalShiftRight();
					Player[Y].State.SetRotateLeft();
				});
				//X.Unlock();
				X--;
				//X.Lock();
			}).While(() => X.NotEquals(0));
			//X.Unlock();

			Player[Y].JustReleased.Set(Player[Y].State.Xor(0xFF).And(Player[Y].StatePrev));
			Player[Y].JustPressed.Set(Player[Y].StatePrev.Xor(0xFF).And(Player[Y].State));
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
