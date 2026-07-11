using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ScintillaNET;

internal static class Helpers
{
	private struct StyleData
	{
		public bool Used;

		public string FontName;

		public int FontIndex;

		public float SizeF;

		public int Weight;

		public int Italic;

		public int Underline;

		public int BackColor;

		public int BackColorIndex;

		public int ForeColor;

		public int ForeColorIndex;

		public int Case;

		public int Visible;
	}

	private static bool registeredFormats;

	private static uint CF_HTML;

	private static uint CF_RTF;

	private static uint CF_LINESELECT;

	private static uint CF_VSLINETAG;

	public static long CopyTo(this Stream source, Stream destination)
	{
		byte[] array = new byte[2048];
		long num = 0L;
		int num2;
		while ((num2 = source.Read(array, 0, array.Length)) > 0)
		{
			destination.Write(array, 0, num2);
			num += num2;
		}
		return num;
	}

	public static byte[] BitmapToArgb(Bitmap image)
	{
		byte[] array = new byte[4 * image.Width * image.Height];
		int num = 0;
		for (int i = 0; i < image.Height; i++)
		{
			for (int j = 0; j < image.Width; j++)
			{
				Color pixel = image.GetPixel(j, i);
				array[num++] = pixel.R;
				array[num++] = pixel.G;
				array[num++] = pixel.B;
				array[num++] = pixel.A;
			}
		}
		return array;
	}

	public unsafe static byte[] ByteToCharStyles(byte* styles, byte* text, int length, Encoding encoding)
	{
		int i = 0;
		int num = 0;
		Decoder decoder = encoding.GetDecoder();
		byte[] array = new byte[encoding.GetCharCount(text, length)];
		for (; i < length; i++)
		{
			if (decoder.GetCharCount(text + i, 1, flush: false) > 0)
			{
				array[num++] = styles[i];
			}
		}
		return array;
	}

	public unsafe static byte[] CharToByteStyles(byte[] styles, byte* text, int length, Encoding encoding)
	{
		int i = 0;
		int num = 0;
		Decoder decoder = encoding.GetDecoder();
		byte[] array = new byte[length];
		for (; i < length; i++)
		{
			if (num >= styles.Length)
			{
				break;
			}
			array[i] = styles[num];
			if (decoder.GetCharCount(text + i, 1, flush: false) > 0)
			{
				num++;
			}
		}
		return array;
	}

	public static int Clamp(int value, int min, int max)
	{
		if (value < min)
		{
			return min;
		}
		if (value > max)
		{
			return max;
		}
		return value;
	}

	public static int ClampMin(int value, int min)
	{
		if (value < min)
		{
			return min;
		}
		return value;
	}

	public static void Copy(Scintilla scintilla, CopyFormat format, bool useSelection, bool allowLine, int startBytePos, int endBytePos)
	{
		if ((format & CopyFormat.Text) > (CopyFormat)0)
		{
			if (useSelection)
			{
				if (allowLine)
				{
					scintilla.DirectMessage(2519);
				}
				else
				{
					scintilla.DirectMessage(2178);
				}
			}
			else
			{
				scintilla.DirectMessage(2419, new IntPtr(startBytePos), new IntPtr(endBytePos));
			}
		}
		if ((format & (CopyFormat.Rtf | CopyFormat.Html)) <= (CopyFormat)0)
		{
			return;
		}
		if (!registeredFormats)
		{
			CF_LINESELECT = NativeMethods.RegisterClipboardFormat("MSDEVLineSelect");
			CF_VSLINETAG = NativeMethods.RegisterClipboardFormat("VisualStudioEditorOperationsLineCutCopyClipboardTag");
			CF_HTML = NativeMethods.RegisterClipboardFormat("HTML Format");
			CF_RTF = NativeMethods.RegisterClipboardFormat("Rich Text Format");
			registeredFormats = true;
		}
		bool flag = false;
		StyleData[] styles = null;
		List<ArraySegment<byte>> list = null;
		if (useSelection)
		{
			if (scintilla.DirectMessage(2650) != IntPtr.Zero)
			{
				if (allowLine)
				{
					list = GetStyledSegments(scintilla, currentSelection: false, currentLine: true, 0, 0, out styles);
					flag = true;
				}
			}
			else
			{
				list = GetStyledSegments(scintilla, currentSelection: true, currentLine: false, 0, 0, out styles);
			}
		}
		else if (startBytePos != endBytePos)
		{
			list = GetStyledSegments(scintilla, currentSelection: false, currentLine: false, startBytePos, endBytePos, out styles);
		}
		if (list == null || list.Count <= 0 || !NativeMethods.OpenClipboard(scintilla.Handle))
		{
			return;
		}
		if ((format & CopyFormat.Text) == 0)
		{
			NativeMethods.EmptyClipboard();
			if (flag)
			{
				NativeMethods.SetClipboardData(CF_LINESELECT, IntPtr.Zero);
				NativeMethods.SetClipboardData(CF_VSLINETAG, IntPtr.Zero);
			}
		}
		if ((format & CopyFormat.Rtf) > (CopyFormat)0)
		{
			CopyRtf(scintilla, styles, list);
		}
		if ((format & CopyFormat.Html) > (CopyFormat)0)
		{
			CopyHtml(scintilla, styles, list);
		}
		NativeMethods.CloseClipboard();
	}

	private static void CopyHtml(Scintilla scintilla, StyleData[] styles, List<ArraySegment<byte>> styledSegments)
	{
		try
		{
			long num = 0L;
			using NativeMemoryStream nativeMemoryStream = new NativeMemoryStream(styledSegments.Sum((ArraySegment<byte> s) => s.Count));
			using StreamWriter streamWriter = new StreamWriter(nativeMemoryStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
			streamWriter.WriteLine("Version:0.9");
			streamWriter.WriteLine("StartHTML:00000000");
			streamWriter.WriteLine("EndHTML:00000000");
			streamWriter.WriteLine("StartFragment:00000000");
			streamWriter.WriteLine("EndFragment:00000000");
			streamWriter.Flush();
			num = nativeMemoryStream.Position;
			nativeMemoryStream.Seek(23L, SeekOrigin.Begin);
			byte[] bytes;
			nativeMemoryStream.Write(bytes = Encoding.ASCII.GetBytes(nativeMemoryStream.Length.ToString("D8")), 0, bytes.Length);
			nativeMemoryStream.Seek(num, SeekOrigin.Begin);
			streamWriter.WriteLine("<html>");
			streamWriter.WriteLine("<head>");
			streamWriter.WriteLine("<meta charset=\"utf-8\" />");
			streamWriter.WriteLine("<title>ScintillaNET v{0}</title>", scintilla.GetType().Assembly.GetName().Version.ToString(3));
			streamWriter.WriteLine("</head>");
			streamWriter.WriteLine("<body>");
			streamWriter.Flush();
			num = nativeMemoryStream.Position;
			nativeMemoryStream.Seek(65L, SeekOrigin.Begin);
			nativeMemoryStream.Write(bytes = Encoding.ASCII.GetBytes(nativeMemoryStream.Length.ToString("D8")), 0, bytes.Length);
			nativeMemoryStream.Seek(num, SeekOrigin.Begin);
			streamWriter.WriteLine("<!--StartFragment -->");
			streamWriter.WriteLine("<style type=\"text/css\" scoped=\"\">");
			streamWriter.Write("div#segments {");
			streamWriter.Write(" float: left;");
			streamWriter.Write(" white-space: pre;");
			streamWriter.Write(" line-height: {0}px;", scintilla.DirectMessage(2279, new IntPtr(0)).ToInt32());
			streamWriter.Write(" background-color: #{0:X2}{1:X2}{2:X2};", styles[32].BackColor & 0xFF, (styles[32].BackColor >> 8) & 0xFF, (styles[32].BackColor >> 16) & 0xFF);
			streamWriter.WriteLine(" }");
			for (int num2 = 0; num2 < styles.Length; num2++)
			{
				if (styles[num2].Used)
				{
					streamWriter.Write("span.s{0} {{", num2);
					streamWriter.Write(" font-family: \"{0}\";", styles[num2].FontName);
					streamWriter.Write(" font-size: {0}pt;", styles[num2].SizeF);
					streamWriter.Write(" font-weight: {0};", styles[num2].Weight);
					if (styles[num2].Italic != 0)
					{
						streamWriter.Write(" font-style: italic;");
					}
					if (styles[num2].Underline != 0)
					{
						streamWriter.Write(" text-decoration: underline;");
					}
					streamWriter.Write(" background-color: #{0:X2}{1:X2}{2:X2};", styles[num2].BackColor & 0xFF, (styles[num2].BackColor >> 8) & 0xFF, (styles[num2].BackColor >> 16) & 0xFF);
					streamWriter.Write(" color: #{0:X2}{1:X2}{2:X2};", styles[num2].ForeColor & 0xFF, (styles[num2].ForeColor >> 8) & 0xFF, (styles[num2].ForeColor >> 16) & 0xFF);
					switch ((StyleCase)styles[num2].Case)
					{
					case StyleCase.Upper:
						streamWriter.Write(" text-transform: uppercase;");
						break;
					case StyleCase.Lower:
						streamWriter.Write(" text-transform: lowercase;");
						break;
					}
					if (styles[num2].Visible == 0)
					{
						streamWriter.Write(" visibility: hidden;");
					}
					streamWriter.WriteLine(" }");
				}
			}
			streamWriter.WriteLine("</style>");
			streamWriter.Write("<div id=\"segments\"><span class=\"s{0}\">", 32);
			streamWriter.Flush();
			int count = scintilla.DirectMessage(2121).ToInt32();
			string value = new string(' ', count);
			streamWriter.AutoFlush = true;
			int num3 = 32;
			bool flag = (scintilla.DirectMessage(2658).ToInt32() & 1) > 0;
			foreach (ArraySegment<byte> styledSegment in styledSegments)
			{
				int num4 = styledSegment.Offset + styledSegment.Count;
				for (int num5 = styledSegment.Offset; num5 < num4; num5 += 2)
				{
					byte b = styledSegment.Array[num5];
					byte b2 = styledSegment.Array[num5 + 1];
					if (num3 != b2)
					{
						streamWriter.Write("</span><span class=\"s{0}\">", b2);
						num3 = b2;
					}
					switch (b)
					{
					case 60:
						streamWriter.Write("&lt;");
						continue;
					case 62:
						streamWriter.Write("&gt;");
						continue;
					case 38:
						streamWriter.Write("&amp;");
						continue;
					case 9:
						streamWriter.Write(value);
						continue;
					case 13:
						if (num5 + 2 < num4 && styledSegment.Array[num5 + 2] == 10)
						{
							num5 += 2;
						}
						goto case 10;
					case 194:
						if (flag && num5 + 2 < num4 && styledSegment.Array[num5 + 2] == 133)
						{
							num5 += 2;
							goto case 10;
						}
						break;
					case 226:
						if (!flag || num5 + 4 >= num4)
						{
							break;
						}
						if (styledSegment.Array[num5 + 2] == 128 && styledSegment.Array[num5 + 4] == 168)
						{
							num5 += 4;
						}
						else
						{
							if (styledSegment.Array[num5 + 2] != 128 || styledSegment.Array[num5 + 4] != 169)
							{
								break;
							}
							num5 += 4;
						}
						goto case 10;
					case 10:
						streamWriter.Write("\r\n");
						continue;
					}
					if (b == 0)
					{
						streamWriter.Write(" ");
					}
					else
					{
						nativeMemoryStream.WriteByte(b);
					}
				}
			}
			streamWriter.AutoFlush = false;
			streamWriter.WriteLine("</span></div>");
			streamWriter.Flush();
			num = nativeMemoryStream.Position;
			nativeMemoryStream.Seek(87L, SeekOrigin.Begin);
			nativeMemoryStream.Write(bytes = Encoding.ASCII.GetBytes(nativeMemoryStream.Length.ToString("D8")), 0, bytes.Length);
			nativeMemoryStream.Seek(num, SeekOrigin.Begin);
			streamWriter.WriteLine("<!--EndFragment-->");
			streamWriter.WriteLine("</body>");
			streamWriter.WriteLine("</html>");
			streamWriter.Flush();
			num = nativeMemoryStream.Position;
			nativeMemoryStream.Seek(41L, SeekOrigin.Begin);
			nativeMemoryStream.Write(bytes = Encoding.ASCII.GetBytes(nativeMemoryStream.Length.ToString("D8")), 0, bytes.Length);
			nativeMemoryStream.Seek(num, SeekOrigin.Begin);
			nativeMemoryStream.WriteByte(0);
			string text = GetString(nativeMemoryStream.Pointer, (int)nativeMemoryStream.Length, Encoding.UTF8);
			if (NativeMethods.SetClipboardData(CF_HTML, nativeMemoryStream.Pointer) != IntPtr.Zero)
			{
				nativeMemoryStream.FreeOnDispose = false;
			}
		}
		catch (Exception)
		{
		}
	}

	private static void CopyRtf(Scintilla scintilla, StyleData[] styles, List<ArraySegment<byte>> styledSegments)
	{
		try
		{
			FontStyle fontStyle = FontStyle.Regular;
			if (styles[32].Weight >= 700)
			{
				fontStyle |= FontStyle.Bold;
			}
			if (styles[32].Italic != 0)
			{
				fontStyle |= FontStyle.Italic;
			}
			if (styles[32].Underline != 0)
			{
				fontStyle |= FontStyle.Underline;
			}
			int num;
			using (Graphics graphics = scintilla.CreateGraphics())
			{
				using Font font = new Font(styles[32].FontName, styles[32].SizeF, fontStyle);
				float width = graphics.MeasureString(" ", font).Width;
				num = (int)(width / graphics.DpiX * 1440f);
			}
			using NativeMemoryStream nativeMemoryStream = new NativeMemoryStream(styledSegments.Sum((ArraySegment<byte> s) => s.Count));
			using StreamWriter streamWriter = new StreamWriter(nativeMemoryStream, Encoding.ASCII);
			int num2 = scintilla.DirectMessage(2121).ToInt32();
			int num3 = num2 * num;
			streamWriter.WriteLine("{{\\rtf1\\ansi\\deff0\\deftab{0}", num3);
			streamWriter.Flush();
			streamWriter.Write("{\\fonttbl");
			streamWriter.Write("{{\\f0 {0};}}", styles[32].FontName);
			int num4 = 1;
			for (int num5 = 0; num5 < styles.Length; num5++)
			{
				if (styles[num5].Used && num5 != 32 && styles[num5].FontName != styles[32].FontName)
				{
					styles[num5].FontIndex = num4++;
					streamWriter.Write("{{\\f{0} {1};}}", styles[num5].FontIndex, styles[num5].FontName);
				}
			}
			streamWriter.WriteLine("}");
			streamWriter.Flush();
			streamWriter.Write("{\\colortbl");
			streamWriter.Write("\\red{0}\\green{1}\\blue{2};", styles[32].ForeColor & 0xFF, (styles[32].ForeColor >> 8) & 0xFF, (styles[32].ForeColor >> 16) & 0xFF);
			streamWriter.Write("\\red{0}\\green{1}\\blue{2};", styles[32].BackColor & 0xFF, (styles[32].BackColor >> 8) & 0xFF, (styles[32].BackColor >> 16) & 0xFF);
			styles[32].ForeColorIndex = 0;
			styles[32].BackColorIndex = 1;
			int num6 = 2;
			for (int num7 = 0; num7 < styles.Length; num7++)
			{
				if (styles[num7].Used && num7 != 32)
				{
					if (styles[num7].ForeColor != styles[32].ForeColor)
					{
						styles[num7].ForeColorIndex = num6++;
						streamWriter.Write("\\red{0}\\green{1}\\blue{2};", styles[num7].ForeColor & 0xFF, (styles[num7].ForeColor >> 8) & 0xFF, (styles[num7].ForeColor >> 16) & 0xFF);
					}
					else
					{
						styles[num7].ForeColorIndex = styles[32].ForeColorIndex;
					}
					if (styles[num7].BackColor != styles[32].BackColor)
					{
						styles[num7].BackColorIndex = num6++;
						streamWriter.Write("\\red{0}\\green{1}\\blue{2};", styles[num7].BackColor & 0xFF, (styles[num7].BackColor >> 8) & 0xFF, (styles[num7].BackColor >> 16) & 0xFF);
					}
					else
					{
						styles[num7].BackColorIndex = styles[32].BackColorIndex;
					}
				}
			}
			streamWriter.WriteLine("}");
			streamWriter.Flush();
			streamWriter.Write("\\f{0}\\fs{1}\\cf{2}\\chshdng0\\chcbpat{3}\\cb{3} ", styles[32].FontIndex, (int)(styles[32].SizeF * 2f), styles[32].ForeColorIndex, styles[32].BackColorIndex);
			if (styles[32].Italic != 0)
			{
				streamWriter.Write("\\i");
			}
			if (styles[32].Underline != 0)
			{
				streamWriter.Write("\\ul");
			}
			if (styles[32].Weight >= 700)
			{
				streamWriter.Write("\\b");
			}
			streamWriter.AutoFlush = true;
			int num8 = 32;
			bool flag = (scintilla.DirectMessage(2658).ToInt32() & 1) > 0;
			foreach (ArraySegment<byte> styledSegment in styledSegments)
			{
				int num9 = styledSegment.Offset + styledSegment.Count;
				for (int num10 = styledSegment.Offset; num10 < num9; num10 += 2)
				{
					byte b = styledSegment.Array[num10];
					byte b2 = styledSegment.Array[num10 + 1];
					if (num8 != b2)
					{
						if (styles[num8].FontIndex != styles[b2].FontIndex)
						{
							streamWriter.Write("\\f{0}", styles[b2].FontIndex);
						}
						if (styles[num8].SizeF != styles[b2].SizeF)
						{
							streamWriter.Write("\\fs{0}", (int)(styles[b2].SizeF * 2f));
						}
						if (styles[num8].ForeColorIndex != styles[b2].ForeColorIndex)
						{
							streamWriter.Write("\\cf{0}", styles[b2].ForeColorIndex);
						}
						if (styles[num8].BackColorIndex != styles[b2].BackColorIndex)
						{
							streamWriter.Write("\\chshdng0\\chcbpat{0}\\cb{0}", styles[b2].BackColorIndex);
						}
						if (styles[num8].Italic != styles[b2].Italic)
						{
							streamWriter.Write("\\i{0}", (styles[b2].Italic != 0) ? "" : "0");
						}
						if (styles[num8].Underline != styles[b2].Underline)
						{
							streamWriter.Write("\\ul{0}", (styles[b2].Underline != 0) ? "" : "0");
						}
						if (styles[num8].Weight != styles[b2].Weight)
						{
							if (styles[b2].Weight >= 700 && styles[num8].Weight < 700)
							{
								streamWriter.Write("\\b");
							}
							else if (styles[b2].Weight < 700 && styles[num8].Weight >= 700)
							{
								streamWriter.Write("\\b0");
							}
						}
						num8 = b2;
						streamWriter.Write(" ");
					}
					switch (b)
					{
					case 123:
						streamWriter.Write("\\{");
						continue;
					case 125:
						streamWriter.Write("\\}");
						continue;
					case 92:
						streamWriter.Write("\\\\");
						continue;
					case 9:
						streamWriter.Write("\\tab ");
						continue;
					case 13:
						if (num10 + 2 < num9 && styledSegment.Array[num10 + 2] == 10)
						{
							num10 += 2;
						}
						goto case 10;
					case 194:
						if (flag && num10 + 2 < num9 && styledSegment.Array[num10 + 2] == 133)
						{
							num10 += 2;
							goto case 10;
						}
						break;
					case 226:
						if (!flag || num10 + 4 >= num9)
						{
							break;
						}
						if (styledSegment.Array[num10 + 2] == 128 && styledSegment.Array[num10 + 4] == 168)
						{
							num10 += 4;
						}
						else
						{
							if (styledSegment.Array[num10 + 2] != 128 || styledSegment.Array[num10 + 4] != 169)
							{
								break;
							}
							num10 += 4;
						}
						goto case 10;
					case 10:
						streamWriter.WriteLine("\\par");
						continue;
					}
					if (b == 0)
					{
						streamWriter.Write(" ");
						continue;
					}
					if (b > 127)
					{
						int num11 = 0;
						if (b < 224 && num10 + 2 < num9)
						{
							num11 |= (0x1F & b) << 6;
							num11 |= 0x3F & styledSegment.Array[num10 + 2];
							streamWriter.Write("\\u{0}?", num11);
							num10 += 2;
							continue;
						}
						if (b < 240 && num10 + 4 < num9)
						{
							num11 |= (0xF & b) << 12;
							num11 |= (0x3F & styledSegment.Array[num10 + 2]) << 6;
							num11 |= 0x3F & styledSegment.Array[num10 + 4];
							streamWriter.Write("\\u{0}?", num11);
							num10 += 4;
							continue;
						}
						if (b < 248 && num10 + 6 < num9)
						{
							num11 |= (7 & b) << 18;
							num11 |= (0x3F & styledSegment.Array[num10 + 2]) << 12;
							num11 |= (0x3F & styledSegment.Array[num10 + 4]) << 6;
							num11 |= 0x3F & styledSegment.Array[num10 + 6];
							streamWriter.Write("\\u{0}?", num11);
							num10 += 6;
							continue;
						}
					}
					nativeMemoryStream.WriteByte(b);
				}
			}
			streamWriter.AutoFlush = false;
			streamWriter.WriteLine("}");
			streamWriter.Flush();
			nativeMemoryStream.WriteByte(0);
			if (NativeMethods.SetClipboardData(CF_RTF, nativeMemoryStream.Pointer) != IntPtr.Zero)
			{
				nativeMemoryStream.FreeOnDispose = false;
			}
		}
		catch (Exception)
		{
		}
	}

	public unsafe static byte[] GetBytes(string text, Encoding encoding, bool zeroTerminated)
	{
		if (string.IsNullOrEmpty(text))
		{
			return zeroTerminated ? new byte[1] : new byte[0];
		}
		int byteCount = encoding.GetByteCount(text);
		byte[] array = new byte[byteCount + (zeroTerminated ? 1 : 0)];
		fixed (byte* bytes = array)
		{
			fixed (char* chars = text)
			{
				encoding.GetBytes(chars, text.Length, bytes, byteCount);
			}
		}
		if (zeroTerminated)
		{
			array[array.Length - 1] = 0;
		}
		return array;
	}

	public unsafe static byte[] GetBytes(char[] text, int length, Encoding encoding, bool zeroTerminated)
	{
		fixed (char* chars = text)
		{
			int byteCount = encoding.GetByteCount(chars, length);
			byte[] array = new byte[byteCount + (zeroTerminated ? 1 : 0)];
			fixed (byte* bytes = array)
			{
				encoding.GetBytes(chars, length, bytes, array.Length);
			}
			if (zeroTerminated)
			{
				array[array.Length - 1] = 0;
			}
			return array;
		}
	}

	public static string GetHtml(Scintilla scintilla, int startBytePos, int endBytePos)
	{
		if (startBytePos == endBytePos)
		{
			return string.Empty;
		}
		StyleData[] styles = null;
		List<ArraySegment<byte>> styledSegments = GetStyledSegments(scintilla, currentSelection: false, currentLine: false, startBytePos, endBytePos, out styles);
		using NativeMemoryStream nativeMemoryStream = new NativeMemoryStream(styledSegments.Sum((ArraySegment<byte> s) => s.Count));
		using StreamWriter streamWriter = new StreamWriter(nativeMemoryStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
		streamWriter.WriteLine("<style type=\"text/css\" scoped=\"\">");
		streamWriter.Write("div#segments {");
		streamWriter.Write(" float: left;");
		streamWriter.Write(" white-space: pre;");
		streamWriter.Write(" line-height: {0}px;", scintilla.DirectMessage(2279, new IntPtr(0)).ToInt32());
		streamWriter.Write(" background-color: #{0:X2}{1:X2}{2:X2};", styles[32].BackColor & 0xFF, (styles[32].BackColor >> 8) & 0xFF, (styles[32].BackColor >> 16) & 0xFF);
		streamWriter.WriteLine(" }");
		for (int num = 0; num < styles.Length; num++)
		{
			if (styles[num].Used)
			{
				streamWriter.Write("span.s{0} {{", num);
				streamWriter.Write(" font-family: \"{0}\";", styles[num].FontName);
				streamWriter.Write(" font-size: {0}pt;", styles[num].SizeF);
				streamWriter.Write(" font-weight: {0};", styles[num].Weight);
				if (styles[num].Italic != 0)
				{
					streamWriter.Write(" font-style: italic;");
				}
				if (styles[num].Underline != 0)
				{
					streamWriter.Write(" text-decoration: underline;");
				}
				streamWriter.Write(" background-color: #{0:X2}{1:X2}{2:X2};", styles[num].BackColor & 0xFF, (styles[num].BackColor >> 8) & 0xFF, (styles[num].BackColor >> 16) & 0xFF);
				streamWriter.Write(" color: #{0:X2}{1:X2}{2:X2};", styles[num].ForeColor & 0xFF, (styles[num].ForeColor >> 8) & 0xFF, (styles[num].ForeColor >> 16) & 0xFF);
				switch ((StyleCase)styles[num].Case)
				{
				case StyleCase.Upper:
					streamWriter.Write(" text-transform: uppercase;");
					break;
				case StyleCase.Lower:
					streamWriter.Write(" text-transform: lowercase;");
					break;
				}
				if (styles[num].Visible == 0)
				{
					streamWriter.Write(" visibility: hidden;");
				}
				streamWriter.WriteLine(" }");
			}
		}
		streamWriter.WriteLine("</style>");
		bool flag = (scintilla.DirectMessage(2658).ToInt32() & 1) > 0;
		int count = scintilla.DirectMessage(2121).ToInt32();
		string value = new string(' ', count);
		int num2 = 32;
		streamWriter.Write("<div id=\"segments\"><span class=\"s{0}\">", 32);
		streamWriter.Flush();
		streamWriter.AutoFlush = true;
		foreach (ArraySegment<byte> item in styledSegments)
		{
			int num3 = item.Offset + item.Count;
			for (int num4 = item.Offset; num4 < num3; num4 += 2)
			{
				byte b = item.Array[num4];
				byte b2 = item.Array[num4 + 1];
				if (num2 != b2)
				{
					streamWriter.Write("</span><span class=\"s{0}\">", b2);
					num2 = b2;
				}
				switch (b)
				{
				case 60:
					streamWriter.Write("&lt;");
					continue;
				case 62:
					streamWriter.Write("&gt;");
					continue;
				case 38:
					streamWriter.Write("&amp;");
					continue;
				case 9:
					streamWriter.Write(value);
					continue;
				case 13:
					if (num4 + 2 < num3 && item.Array[num4 + 2] == 10)
					{
						num4 += 2;
					}
					goto case 10;
				case 194:
					if (flag && num4 + 2 < num3 && item.Array[num4 + 2] == 133)
					{
						num4 += 2;
						goto case 10;
					}
					break;
				case 226:
					if (!flag || num4 + 4 >= num3)
					{
						break;
					}
					if (item.Array[num4 + 2] == 128 && item.Array[num4 + 4] == 168)
					{
						num4 += 4;
					}
					else
					{
						if (item.Array[num4 + 2] != 128 || item.Array[num4 + 4] != 169)
						{
							break;
						}
						num4 += 4;
					}
					goto case 10;
				case 10:
					streamWriter.Write("\r\n");
					continue;
				}
				if (b == 0)
				{
					streamWriter.Write(" ");
				}
				else
				{
					nativeMemoryStream.WriteByte(b);
				}
			}
		}
		streamWriter.AutoFlush = false;
		streamWriter.WriteLine("</span></div>");
		streamWriter.Flush();
		return GetString(nativeMemoryStream.Pointer, (int)nativeMemoryStream.Length, Encoding.UTF8);
	}

	public unsafe static string GetString(IntPtr bytes, int length, Encoding encoding)
	{
		sbyte* value = (sbyte*)(void*)bytes;
		return new string(value, 0, length, encoding);
	}

	private static List<ArraySegment<byte>> GetStyledSegments(Scintilla scintilla, bool currentSelection, bool currentLine, int startBytePos, int endBytePos, out StyleData[] styles)
	{
		List<ArraySegment<byte>> list = new List<ArraySegment<byte>>();
		if (currentSelection)
		{
			List<Tuple<int, int>> list2 = new List<Tuple<int, int>>();
			int num = scintilla.DirectMessage(2570).ToInt32();
			for (int i = 0; i < num; i++)
			{
				int item = scintilla.DirectMessage(2585, new IntPtr(i)).ToInt32();
				int item2 = scintilla.DirectMessage(2587, new IntPtr(i)).ToInt32();
				list2.Add(Tuple.Create(item, item2));
			}
			bool flag = scintilla.DirectMessage(2372) != IntPtr.Zero;
			if (flag)
			{
				list2.OrderBy((Tuple<int, int> r) => r.Item1);
			}
			foreach (Tuple<int, int> item3 in list2)
			{
				ArraySegment<byte> styledText = GetStyledText(scintilla, item3.Item1, item3.Item2, flag);
				list.Add(styledText);
			}
		}
		else if (currentLine)
		{
			int value = scintilla.DirectMessage(2575).ToInt32();
			int value2 = scintilla.DirectMessage(2577, new IntPtr(value)).ToInt32();
			int value3 = scintilla.DirectMessage(2166, new IntPtr(value2)).ToInt32();
			int num2 = scintilla.DirectMessage(2167, new IntPtr(value3)).ToInt32();
			int num3 = scintilla.DirectMessage(2167, new IntPtr(value3)).ToInt32();
			ArraySegment<byte> styledText2 = GetStyledText(scintilla, num2, num2 + num3, addLineBreak: false);
			list.Add(styledText2);
		}
		else
		{
			ArraySegment<byte> styledText3 = GetStyledText(scintilla, startBytePos, endBytePos, addLineBreak: false);
			list.Add(styledText3);
		}
		styles = new StyleData[256];
		styles[32].Used = true;
		styles[32].FontName = scintilla.Styles[32].Font;
		styles[32].SizeF = scintilla.Styles[32].SizeF;
		styles[32].Weight = scintilla.DirectMessage(2064, new IntPtr(32), IntPtr.Zero).ToInt32();
		styles[32].Italic = scintilla.DirectMessage(2484, new IntPtr(32), IntPtr.Zero).ToInt32();
		styles[32].Underline = scintilla.DirectMessage(2488, new IntPtr(32), IntPtr.Zero).ToInt32();
		styles[32].BackColor = scintilla.DirectMessage(2482, new IntPtr(32), IntPtr.Zero).ToInt32();
		styles[32].ForeColor = scintilla.DirectMessage(2481, new IntPtr(32), IntPtr.Zero).ToInt32();
		styles[32].Case = scintilla.DirectMessage(2489, new IntPtr(32), IntPtr.Zero).ToInt32();
		styles[32].Visible = scintilla.DirectMessage(2491, new IntPtr(32), IntPtr.Zero).ToInt32();
		foreach (ArraySegment<byte> item4 in list)
		{
			for (int num4 = 0; num4 < item4.Count; num4 += 2)
			{
				byte b = item4.Array[num4 + 1];
				if (!styles[b].Used)
				{
					styles[b].Used = true;
					styles[b].FontName = scintilla.Styles[b].Font;
					styles[b].SizeF = scintilla.Styles[b].SizeF;
					styles[b].Weight = scintilla.DirectMessage(2064, new IntPtr(b), IntPtr.Zero).ToInt32();
					styles[b].Italic = scintilla.DirectMessage(2484, new IntPtr(b), IntPtr.Zero).ToInt32();
					styles[b].Underline = scintilla.DirectMessage(2488, new IntPtr(b), IntPtr.Zero).ToInt32();
					styles[b].BackColor = scintilla.DirectMessage(2482, new IntPtr(b), IntPtr.Zero).ToInt32();
					styles[b].ForeColor = scintilla.DirectMessage(2481, new IntPtr(b), IntPtr.Zero).ToInt32();
					styles[b].Case = scintilla.DirectMessage(2489, new IntPtr(b), IntPtr.Zero).ToInt32();
					styles[b].Visible = scintilla.DirectMessage(2491, new IntPtr(b), IntPtr.Zero).ToInt32();
				}
			}
		}
		return list;
	}

	private unsafe static ArraySegment<byte> GetStyledText(Scintilla scintilla, int startBytePos, int endBytePos, bool addLineBreak)
	{
		scintilla.DirectMessage(4003, new IntPtr(startBytePos), new IntPtr(endBytePos));
		int num = endBytePos - startBytePos;
		byte[] array = new byte[num * 2 + (addLineBreak ? 4 : 0) + 2];
		fixed (byte* value = array)
		{
			NativeMethods.Sci_TextRange* ptr = stackalloc NativeMethods.Sci_TextRange[1];
			ptr->chrg.cpMin = startBytePos;
			ptr->chrg.cpMax = endBytePos;
			ptr->lpstrText = new IntPtr(value);
			scintilla.DirectMessage(2015, IntPtr.Zero, new IntPtr(ptr));
			num *= 2;
		}
		if (addLineBreak)
		{
			byte b = array[num - 1];
			array[num++] = 13;
			array[num++] = b;
			array[num++] = 10;
			array[num++] = b;
			array[num] = 0;
			array[num + 1] = 0;
		}
		return new ArraySegment<byte>(array, 0, num);
	}

	public static int TranslateKeys(Keys keys)
	{
		return ((keys & Keys.KeyCode) switch
		{
			Keys.Down => 300, 
			Keys.Up => 301, 
			Keys.Left => 302, 
			Keys.Right => 303, 
			Keys.Home => 304, 
			Keys.End => 305, 
			Keys.Prior => 306, 
			Keys.Next => 307, 
			Keys.Delete => 308, 
			Keys.Insert => 309, 
			Keys.Escape => 7, 
			Keys.Back => 8, 
			Keys.Tab => 9, 
			Keys.Return => 13, 
			Keys.Add => 310, 
			Keys.Subtract => 311, 
			Keys.Divide => 312, 
			Keys.LWin => 313, 
			Keys.RWin => 314, 
			Keys.Apps => 315, 
			Keys.OemQuestion => 47, 
			Keys.Oemtilde => 96, 
			Keys.OemOpenBrackets => 91, 
			Keys.OemPipe => 92, 
			Keys.OemCloseBrackets => 93, 
			_ => (int)(keys & Keys.KeyCode), 
		}) | (int)(keys & Keys.Modifiers);
	}
}
