using NESSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using static NESSharp.Core.AL;

namespace NESSharp.Common {
	public class BorderDefinition {
		public U8 TopLeft;
		public U8 Top;
		public U8 TopRight;
		public U8 Left;
		public U8 Center;
		public U8 Right;
		public U8 BottomLeft;
		public U8 Bottom;
		public U8 BottomRight;

		public BorderDefinition(U8 tl, U8 t, U8 tr, U8 l, U8 c, U8 r, U8 bl, U8 b, U8 br) {
			TopLeft = tl; Top = t; TopRight = tr;
			Left = l; Center = c; Right = r;
			BottomLeft = bl; Bottom = b; BottomRight = br;
		}
	}
	public class Font {
		private readonly U8 _offset;
		private readonly U8 _space;
		private readonly string _chars;
		public Font(U8 offset, string chars, int space = 0) {
			_offset = offset;
			_space = (U8)space;
			_chars = chars;
		}
		public byte[] Encode(string text) => text.ToUpperInvariant().Select(x => (byte)(_chars.Contains(x) ? (U8)(_chars.IndexOf(x) + _offset) : (x == ' ' ? _space : (U8)0))).ToArray();
		public string EncodeString(string text) => string.Join(null, text.ToUpperInvariant().Select(x => (char)(_chars.Contains(x) ? _chars.IndexOf(x) + _offset : (x == ' ' ? (char)_space : x))));
	}
	public static class DrawUtil {
		public static byte[] GenerateTextBox(Font font, BorderDefinition border, params string[] lines) {
			var screenWidth = 32;
			var lineList = new List<string>();
			var maxLen = 0;
			foreach(var line in lines)
				maxLen = System.Math.Max(maxLen, line.Length);
			if (maxLen >= screenWidth - 4) throw new Exception("Line length cannot exceed 28");
			var remainingWidth = screenWidth - maxLen - 4; //screen width in tiles - string length - 2 border tiles with 2 tiles padding
			var leftMargin = (int)MathF.Floor(remainingWidth / 2);
			var rightMargin = remainingWidth - leftMargin;
			lineList.Add($"{(char)(byte)border.TopLeft}{string.Empty.PadRight(maxLen + 2, (char)(byte)border.Top)}{(char)(byte)border.TopRight}"); //upper border
			lineList.Add($"{(char)(byte)border.Left} {string.Empty.PadRight(maxLen, ' ')} {(char)(byte)border.Right}"); //upper border padding
			lineList.AddRange(lines.Select(x => $"{(char)(byte)border.Left} {x.PadRight(maxLen, ' ')} {(char)(byte)border.Right}"));
			lineList.Add($"{(char)(byte)border.Left} {string.Empty.PadRight(maxLen, ' ')} {(char)(byte)border.Right}"); //lower border padding
			lineList.Add($"{(char)(byte)border.BottomLeft}{string.Empty.PadRight(maxLen + 2, (char)(byte)border.Bottom)}{(char)(byte)border.BottomRight}"); //lower border

			lineList = lineList.Select(x => font.EncodeString(x.PadLeft(screenWidth - rightMargin, ' ').PadRight(screenWidth, ' '))).ToList();
			return string.Join(null, lineList).ToArray().Select(x => (byte)x).ToArray();
		}
		public static byte[] GenerateCenteredText(Font font, params string[] lines) {
			var screenWidth = 32;
			var lineList = new List<string>();
			foreach(var line in lines) {
				var len = line.Length;
				if (len >= screenWidth) throw new Exception("Line length cannot exceed 32");
				var remainingWidth = screenWidth - len; //screen width in tiles - string length - 2 border tiles with 2 tiles padding
				var leftMargin = (int)MathF.Floor(remainingWidth / 2);
				var rightMargin = remainingWidth - leftMargin;
				lineList.Add(font.EncodeString(line.PadLeft(screenWidth - rightMargin, ' ').PadRight(screenWidth, ' ')));
			}
			return string.Join(null, lineList).ToArray().Select(x => (byte)x).ToArray();
		}
		[Obsolete("Use a module from NESSharp.Lib.Compression")]
		public static byte[] RLECompress(params byte[] input) {
			byte compressionIndicator = 255;
			byte cur;
			byte? next;
			var len = input.Length;
			var output = new List<byte>();

			void compress(int runLength, byte chr) {
				if (runLength <= 255) {
					output.Add(compressionIndicator);
					output.Add((byte)runLength);
					output.Add(chr);
				} else throw new NotImplementedException(); //TODO: support >255 run lengths
			}

			for (var i = 0; i < len; i++) {
				cur = input[i];
				next = i + 1 >= len ? (byte?)null : input[i + 1];
				if (cur != next && cur != compressionIndicator) {
					output.Add(cur);
					continue;
				}

				//Count total # of the same char
				var runLength = 1;
				for (var j = i + 1; j < len; j++) {
					if (cur == input[j])
						runLength++;
					else
						break;
				}

				//if cur == compressionIndicator, compress regardless of run length
				if (cur == compressionIndicator) {
					compress(runLength, cur);
					i += runLength - 1;
					continue;
				}

				//If run is <= 3, not worth it, add and proceed
				if (runLength <= 3) {
					output.Add(cur);
					continue;
				}
				
				//if >=3, compress and prefix output with compressionIndicator
				compress(runLength, cur);
				i += runLength - 1;
			}
			var count = output.Count;
			if (count > 255) throw new NotImplementedException();
			output.Insert(0, (byte)(count + 1)); //max offset from starting value in this data set

			return output.ToArray();
		}

		/// <summary>
		/// Inline function to use in a decompressing sub
		/// </summary>
		/// <param name="action"></param>
		[Obsolete("Use a module from NESSharp.Lib.Compression")]
		public static void RLEDecompress(/*Action dataAction,*/ VByte temp, Action<RegisterA> block) {
			//var lbl = LabelFor(dataAction);
			//TempPtr0.PointTo(lbl);
			byte compressionIndicator = 255;
			var length = temp.Set(TempPtr0[Y.Set(0)]);
			Loop.AscendWhile(Y.Increment(), () => Y.LessThan(length), _ => {
				If.Block(c => c
					.True(() => A.Set(TempPtr0[Y]).Equals(compressionIndicator), () => {
						Y.State.Unsafe(() => {
							Y.Increment();
							X.Set(A.Set(TempPtr0[Y]));
							Y.Increment();
							Loop.Descend_Post(X, _ => block(A.Set(TempPtr0[Y])));
						});
					})
					.Else(() => block(A.Set(TempPtr0[Y])))
				);
			});
		}
	}
}
