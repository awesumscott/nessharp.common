using NESSharp.Core;
using System;
using static NESSharp.Core.AL;

namespace NESSharp.Common {
	public class LiveQueue {
		public Array<VByte> Values; //queue for input to lazy-execute slide actions
		private U8 _stopVal = 0;
		public VByte WriteIndex;
		public VByte ReadIndex;
		private bool _isReading = false, _isWriting = false;
		private IndexingRegister _indexReg = null;
		public LiveQueue() {}

		//TODO: this doesn't really enforce length. Implement wrap if length != 0 (0 would indicate 256--full page)
		public static LiveQueue New(RAMRange Zp, RAMRange Ram, RAMRange valuesRam, int length, string name, U8 stopVal) {
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
		}
		public void PushOnce(IndexingRegister indexReg, U8 u8) {
			if (indexReg is RegisterX)		X.Set(WriteIndex);
			else if (indexReg is RegisterY)	Y.Set(WriteIndex);
			Values[indexReg].Set(u8);
			indexReg++;
			Values[indexReg].Set(_stopVal);
			WriteIndex.Set(indexReg);
		}
		public void Push(IOperand u8) {
			if (!_isWriting)
				throw new Exception("Push can only be used within a LiveQueue.Write() block");
			Values[_indexReg].Set(u8);
			_indexReg++;
		}
		public void Push(Address addr) {
			if (!_isWriting)
				throw new Exception("Push can only be used within a LiveQueue.Write() block");
			Values[_indexReg].Set(addr);
			_indexReg++;
		}
		public void Push(RegisterA a) {
			if (!_isWriting)
				throw new Exception("Push can only be used within a LiveQueue.Write() block");
			Values[_indexReg].Set(a);
			_indexReg++;
		}

		//TODO: handle either index in _indexReg
		public void PushRangeOnce(Action action) {
			var len = Length(action);
			if (len > 255) throw new Exception("Block is too big, it must be 255 bytes or less.");
			X.Set(WriteIndex);
			TempPtr0.PointTo(LabelFor(action));
			Loop.AscendWhile(Y.Set(0), () => Y.NotEquals((U8)len), _ => {
				Values[X].Set(TempPtr0[Y]);
				X++;
			});
			Values[X].Set(_stopVal);
			WriteIndex.Set(X);
		}

		public void PushStart(IndexingRegister indexReg) {
			_indexReg = indexReg;
			_isWriting = true;
			if (_indexReg is RegisterX) {
				X.Set(WriteIndex);
			} else if (_indexReg is RegisterY) {
				Y.Set(WriteIndex);
			}
		}
		public void PushDone() {
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
			return Values[_indexReg];
		}
		public VByte Unsafe_Peek(IndexingRegister indexReg) => Values[indexReg];
		public void Pop() {
			if (!_isReading)
				throw new Exception("Pop can only be used within a LiveQueue.Read() block");
			_indexReg++;
		}
		public void Unsafe_Pop(IndexingRegister indexReg) => indexReg++;

		public void Write(IndexingRegister indexReg, Action block) {
			if (_isReading || _isWriting)
				throw new Exception("Queue is already reading or writing");
			_isWriting = true;
			_indexReg = indexReg;
			if (indexReg is RegisterX)		X.Set(WriteIndex);
			else if (indexReg is RegisterY)	Y.Set(WriteIndex);
			block.Invoke();
			Values[indexReg].Set(_stopVal);
			WriteIndex.Set(indexReg);
			_indexReg = null;
			_isWriting = false;
		}

		public void Read(IndexingRegister indexReg, Action block) {
			if (_isReading || _isWriting)
				throw new Exception("Queue is already reading or writing");
			_isReading = true;
			_indexReg = indexReg;
			if (indexReg is RegisterX)		X.Set(ReadIndex);
			else if (indexReg is RegisterY)	Y.Set(ReadIndex);
			block.Invoke();
			ReadIndex.Set(indexReg);
			_indexReg = null;
			_isReading = false;
		}
	}
}
