using NESSharp.Core;
using static NESSharp.Core.AL;

namespace NESSharp.Common {
	public class ByteGrid {
		
		public Array<VByte> Values {get;set;}
		public VByte Width, Height;
		public VByte Max; //Array length of current level (_width * _height)
		public VByte Index; //Used as a cursor for grid cell access via Y

		public static ByteGrid New(RAM ram, U8 length, string name) {
			var grid = new ByteGrid();
			grid.Values			= Array<VByte>.New(length, ram, name + "_grid");
			grid.Width			= VByte.New(ram, name + "_width");
			grid.Height			= VByte.New(ram, name + "_height");
			grid.Max			= VByte.New(ram, name + "_max");
			grid.Index			= VByte.New(ram, name + "_index");
			return grid;
		}
		public void SetDims(U8 w, U8 h) {
		}

		public void Clear(U8 clearVal) {
			Loop.Descend_Pre(X.Set(Values.Length), _ => {
				Values[X].Set(clearVal);
			});
		}

		/// <summary>
		/// Call this after setting Width and Height. It determines the last index in the grid, so iteration is reduced.
		/// </summary>
		public void UpdateMax() {
			Comment("ByteGrid.Max = ByteGrid.Width * ByteGrid.Height");
			Max.Set(0);
			Loop.Descend_Post(X.Set(Height), _ => {
				Max.Set(z => z.Add(Width));
			});
		}
		public RegisterA MoveUp(RegisterA a = null) {
			if (a == null)
				Index.Set(z => z.Subtract(Width));
			else
				Loop.Descend_Post(X.Set(a), _ => {
					Index.Set(z => z.Subtract(Width));
				});
			return A;
		}
		public RegisterA MoveDown(RegisterA a = null) {
			if (a == null)
				Index.Set(z => z.Add(Width));
			else
				Loop.Descend_Post(X.Set(a), _ => {
					Index.Set(z => z.Add(Width));
				});
			return A;
		}
		public RegisterA MoveLeft(RegisterA a = null) {
			if (a == null)
				Index.Set(z => z.Subtract(1));
			else
				Index.Set(z => z.Subtract(a));
			return A;
		}
		public RegisterA MoveRight(RegisterA a = null) {
			if (a == null)
				Index.Set(z => z.Add(1));
			else
				Index.Set(z => z.Add(a));
			return A;
		}
	}
}
