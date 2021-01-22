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

		public Func<RegisterA> IsHeld(U8 buttons) => () => State.And(buttons);
		public Func<RegisterA> WasPressed(U8 buttons) => () => JustPressed.And(buttons);
		public Func<RegisterA> WasReleased(U8 buttons) => () => JustReleased.And(buttons);
	}

	public class Gamepads : Module {
		private StructOfArrays<Gamepad> _gamepads;
		public VByte _index;
		private U8 _numPlayers;

		public Gamepads Setup(U8 players) {
			_numPlayers = players;
			return this;
		}

		public void Update() => GoSub(Read);

		[Dependencies]
		private void Dependencies() {
			_index = VByte.New(Ram, "gamepad_index");
			_gamepads = StructOfArrays<Gamepad>.New("gamepadData", _numPlayers).Dim(Ram);
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
			_gamepads[X].StatePrev.Set(_gamepads[X].State);
			Loop.Descend_Post(Y.Set(8), _ => {
				//If(	Option(() => _index.LessThan(2), () => {
				//		A.Set(NES.Controller.One).LogicalShiftRight();
				//	}),
				//	Default(() => {
				//		A.Set(NES.Controller.One).LogicalShiftRight();
				//	})
				//)
				
				A.Set(NES.Controller.One).LSR();
				_gamepads[X].State.SetROL();
			});

			_gamepads[X].JustReleased.Set(_gamepads[X].State.Xor(0xFF).And(_gamepads[X].StatePrev));
			_gamepads[X].JustPressed.Set(_gamepads[X].StatePrev.Xor(0xFF).And(_gamepads[X].State));
		}
		[Subroutine]
		[Clobbers(Register.A, Register.X, Register.Y)]
		private void UpdateController2() {
			X.Set(_index);
			_gamepads[X].StatePrev.Set(_gamepads[X].State);
			Loop.Descend_Post(Y.Set(8), _ => {
				A.Set(NES.Controller.Two).LSR();
				_gamepads[X].State.SetROL();
			});

			_gamepads[X].JustReleased.Set(_gamepads[X].State.Xor(0xFF).And(_gamepads[X].StatePrev));
			_gamepads[X].JustPressed.Set(_gamepads[X].StatePrev.Xor(0xFF).And(_gamepads[X].State));
		}
		public Gamepad this[IndexingRegister offset] => _gamepads[offset];
		public Gamepad this[int offset] => _gamepads[offset];
	}
}