using NESSharp.Core;
using static NESSharp.Core.AL;

namespace NESSharp.Common {
	public class ByteGrid {
		
		public Array<Var8> Values {get;set;}
		public Var8 Width, Height;
		public Var8 Max; //Array length of current level (_width * _height)
		public Var8 Index; //Used as a cursor for grid cell access via Y

		public static ByteGrid New(RAM ram, U8 length, string name) {
			var grid = new ByteGrid();
			grid.Values			= Array<Var8>.New(length, ram, name + "_grid");
			grid.Width			= Var8.New(ram, name + "_width");
			grid.Height			= Var8.New(ram, name + "_height");
			grid.Max			= Var8.New(ram, name + "_max");
			grid.Index			= Var8.New(ram, name + "_index");
			return grid;
		}
		public void SetDims(U8 w, U8 h) {
		}

		public void Clear(U8 clearVal) {
			Loop.Descend_Pre(X.Set(Values.Length), () => {
				Values[X].Set(clearVal);
			});
		}

		/// <summary>
		/// Call this after setting Width and Height. It determines the last index in the grid, so iteration is reduced.
		/// </summary>
		public void UpdateMax() {
			Comment("ByteGrid.Max = ByteGrid.Width * ByteGrid.Height");
			Max.Set(0);
			Loop.Descend(X.Set(Height), () => {
				Max.Set(z => z.Add(Width));
			});
		}
		public RegisterA MoveUp(RegisterA a = null) {
			if (a == null)
				Index.Set(z => z.Subtract(Width));
			else
				Loop.Descend(X.Set(a), () => {
					Index.Set(z => z.Subtract(Width));
				});
			return A;
		}
		public RegisterA MoveDown(RegisterA a = null) {
			if (a == null)
				Index.Set(z => z.Add(Width));
			else
				Loop.Descend(X.Set(a), () => {
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
