using NESSharp.Core;
using System;
using static NESSharp.Core.AL;

namespace NESSharp.Common {
	public class LiveQueue {
		public Array<VByte> Values; //queue for input to lazy-execute slide actions
		private U8 _stopVal = 0;
		public VByte WriteIndex;
		public VByte ReadIndex;
		//private Var8 _done;
		private bool _isReading = false, _isWriting = false;
		private IndexingRegisterBase _indexReg = null;
		public LiveQueue() {
		}
		public static LiveQueue New(RAM Zp, RAM Ram, RAM valuesRam, U8 length, string name, U8 stopVal) {
			var bq = new LiveQueue();
			bq.Values	= Array<VByte>.New(length, valuesRam, name + "_values");
			bq._stopVal = stopVal;
			bq.WriteIndex = VByte.New(Zp, name + "_write");
			bq.ReadIndex = VByte.New(Zp, name + "_read");
			//bq._done = Var8.New(ram, name + "_done");
			return bq;
		}
		public Condition Empty() =>		ReadIndex.Equals(WriteIndex);
		public Condition NotEmpty() =>	ReadIndex.NotEquals(WriteIndex);

		public void Reset() {
			WriteIndex.Set(0);
			ReadIndex.Set(0);
			Values[0].Set(_stopVal);
			//_done.Set(1);
		}
		public void PushOnce(IndexingRegisterBase indexReg, U8 u8) {
			if (indexReg is RegisterX) {
				X.Set(WriteIndex);
				Values[X].Set(u8);
				X++;
				Values[X].Set(_stopVal);
				WriteIndex.Set(X);
			} else if (indexReg is RegisterY) {
				Y.Set(WriteIndex);
				Values[Y].Set(u8);
				Y++;
				Values[Y].Set(_stopVal);
				WriteIndex.Set(Y);
			}
		}
		public void Push(U8 u8) {
			if (!_isWriting)
				throw new Exception("Push can only be used within a LiveQueue.Write() block");
			if (_indexReg is RegisterX) {
				Values[X].Set(u8);
				X++;
			} else if (_indexReg is RegisterY) {
				Values[Y].Set(u8);
				Y++;
			}

			//if (indexReg is RegisterX) {
			//	X.Set(WriteIndex);
			//	Values[X].Set(u8);
			//	X++;
			//} else if (indexReg is RegisterY) {
			//	Y.Set(WriteIndex);
			//	Values[Y].Set(u8);
			//	Y++;
			//}
		}
		public void Push(IResolvable<U8> v) {
			if (!_isWriting)
				throw new Exception("Push can only be used within a LiveQueue.Write() block");
			if (_indexReg is RegisterX) {
				Values[X].Set(v);
				X++;
			} else if (_indexReg is RegisterY) {
				Values[Y].Set(v);
				Y++;
			}

			//if (indexReg is RegisterX) {
			//	X.Set(WriteIndex);
			//	Values[X].Set(u8);
			//	X++;
			//} else if (indexReg is RegisterY) {
			//	Y.Set(WriteIndex);
			//	Values[Y].Set(u8);
			//	Y++;
			//}
		}
		//public void Unsafe_Push(IndexingRegisterBase indexReg, U8 u8) {
		//	if (indexReg is RegisterX) {
		//		Values[X].Set(u8);
		//		X++;
		//	} else if (indexReg is RegisterY) {
		//		Values[Y].Set(u8);
		//		Y++;
		//	}
		//}
		public void Push(Address addr) {
			if (!_isWriting)
				throw new Exception("Push can only be used within a LiveQueue.Write() block");
			if (_indexReg is RegisterX) {
				Values[X].Set(addr);
				X++;
			} else if (_indexReg is RegisterY) {
				Values[Y].Set(addr);
				Y++;
			}
		}
		//public void Unsafe_Push(IndexingRegisterBase indexReg, Address addr) {
		//	if (indexReg is RegisterX) {
		//		Values[X].Set(addr);
		//		X++;
		//	} else if (indexReg is RegisterY) {
		//		Values[Y].Set(addr);
		//		Y++;
		//	}
		//}
		public void Push(RegisterA a) {
			if (!_isWriting)
				throw new Exception("Push can only be used within a LiveQueue.Write() block");
			if (_indexReg is RegisterX) {
				Values[X].Set(a);
				X++;
			} else if (_indexReg is RegisterY) {
				Values[Y].Set(a);
				Y++;
			}
		}
		//public void Unsafe_Push(IndexingRegisterBase indexReg, RegisterA a) {
		//	if (indexReg is RegisterX) {
		//		Values[X].Set(a);
		//		X++;
		//	} else if (indexReg is RegisterY) {
		//		Values[Y].Set(a);
		//		Y++;
		//	} else throw new Exception();
		//}

		
		public void PushStart(IndexingRegisterBase indexReg) {
			_indexReg = indexReg;
			_isWriting = true;
			if (_indexReg is RegisterX) {
				X.Set(WriteIndex);
			} else if (_indexReg is RegisterY) {
				Y.Set(WriteIndex);
			}
		}
		public void PushDone() {
			//if (_indexReg is RegisterX) {
			//	Values[X].Set(_stopVal);
			//	WriteIndex.Set(X);
			//} else if (_indexReg is RegisterY) {
			//	Values[Y].Set(_stopVal);
			//	WriteIndex.Set(Y);
			//}
			Values[_indexReg].Set(_stopVal);
			WriteIndex.Set(_indexReg);
			_indexReg = null;
			_isWriting = false;
			//Don't increment index, so this gets overwritten on next write
		}
		//public Condition Done => _done.NotEquals(0);
		//public Condition NotDone => _done.Equals(0);
		public VByte Peek() {
			if (!_isReading)
				throw new Exception("Peek can only be used within a LiveQueue.Read() block");
			//if (_indexReg is RegisterX) {
			//	return Values[X];
			//} else if (_indexReg is RegisterY) {
			//	return Values[Y];
			//}
			return Values[_indexReg];
		}
		public VByte Unsafe_Peek(IndexingRegisterBase indexReg) {
			//if (indexReg is RegisterX) {
			//	return Values[X];
			//} else if (indexReg is RegisterY) {
			//	return Values[Y];
			//}
			return Values[indexReg];
		}
		public void Pop() {
			if (!_isReading)
				throw new Exception("Pop can only be used within a LiveQueue.Read() block");
			if (_indexReg is RegisterX) {
				X++;
			} else if (_indexReg is RegisterY) {
				Y++;
			}
		}
		public void Unsafe_Pop(RegisterBase indexReg) {
			if (indexReg is RegisterX) {
				X++;
			} else if (indexReg is RegisterY) {
				Y++;
			}
		}

		public void Write(IndexingRegisterBase indexReg, Action block) {
			if (_isReading || _isWriting)
				throw new Exception("Queue is already reading or writing");
			_isWriting = true;
			_indexReg = indexReg;
			if (indexReg is RegisterX) {
				X.Set(WriteIndex);
				block.Invoke();
				Values[X].Set(_stopVal);
				WriteIndex.Set(X);
			} else if (indexReg is RegisterY) {
				Y.Set(WriteIndex);
				block.Invoke();
				Values[Y].Set(_stopVal);
				WriteIndex.Set(Y);
			}
			_indexReg = null;
			_isWriting = false;
		}

		public void Read(IndexingRegisterBase indexReg, Action block) {
			if (_isReading || _isWriting)
				throw new Exception("Queue is already reading or writing");
			_isReading = true;
			_indexReg = indexReg;
			if (indexReg is RegisterX) {
				X.Set(ReadIndex);
				block.Invoke();
				ReadIndex.Set(X);
			} else if (indexReg is RegisterY) {
				Y.Set(ReadIndex);
				block.Invoke();
				ReadIndex.Set(Y);
			}
			_indexReg = null;
			_isReading = false;
		}
	}
}
