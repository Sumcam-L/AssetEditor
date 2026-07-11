using System;
using System.Drawing;
using System.Text;

namespace ScintillaNET;

public class Style
{
	public static class Ada
	{
		public const int Default = 0;

		public const int CommentLine = 10;

		public const int Number = 3;

		public const int Word = 1;

		public const int String = 7;

		public const int Character = 5;

		public const int Delimiter = 4;

		public const int Label = 9;

		public const int Identifier = 2;

		public const int StringEol = 8;

		public const int CharacterEol = 6;

		public const int Illegal = 11;
	}

	public static class Asm
	{
		public const int Default = 0;

		public const int Comment = 1;

		public const int CommentBlock = 11;

		public const int Number = 2;

		public const int MathInstruction = 7;

		public const int String = 3;

		public const int Character = 12;

		public const int CpuInstruction = 6;

		public const int Register = 8;

		public const int Operator = 4;

		public const int Identifier = 5;

		public const int StringEol = 13;

		public const int Directive = 9;

		public const int DirectiveOperand = 10;

		public const int ExtInstruction = 14;

		public const int CommentDirective = 15;
	}

	public static class BlitzBasic
	{
		public const int Default = 0;

		public const int Comment = 1;

		public const int Number = 2;

		public const int Keyword = 3;

		public const int String = 4;

		public const int Preprocessor = 5;

		public const int Operator = 6;

		public const int Identifier = 7;

		public const int Date = 8;

		public const int StringEol = 9;

		public const int Keyword2 = 10;

		public const int Keyword3 = 11;

		public const int Keyword4 = 12;

		public const int Constant = 13;

		public const int Asm = 14;

		public const int Label = 15;

		public const int Error = 16;

		public const int HexNumber = 17;

		public const int BinNumber = 18;

		public const int CommentBlock = 19;

		public const int DocLine = 20;

		public const int DocBlock = 21;

		public const int DocKeyword = 22;
	}

	public static class Batch
	{
		public const int Default = 0;

		public const int Comment = 1;

		public const int Word = 2;

		public const int Label = 3;

		public const int Hide = 4;

		public const int Command = 5;

		public const int Identifier = 6;

		public const int Operator = 7;
	}

	public static class Cpp
	{
		public const int Default = 0;

		public const int Comment = 1;

		public const int CommentLine = 2;

		public const int CommentDoc = 3;

		public const int Number = 4;

		public const int Word = 5;

		public const int String = 6;

		public const int Character = 7;

		public const int Uuid = 8;

		public const int Preprocessor = 9;

		public const int Operator = 10;

		public const int Identifier = 11;

		public const int StringEol = 12;

		public const int Verbatim = 13;

		public const int Regex = 14;

		public const int CommentLineDoc = 15;

		public const int Word2 = 16;

		public const int CommentDocKeyword = 17;

		public const int CommentDocKeywordError = 18;

		public const int GlobalClass = 19;

		public const int StringRaw = 20;

		public const int TripleVerbatim = 21;

		public const int HashQuotedString = 22;

		public const int PreprocessorComment = 23;

		public const int PreprocessorCommentDoc = 24;

		public const int UserLiteral = 25;

		public const int TaskMarker = 26;

		public const int EscapeSequence = 27;
	}

	public static class Css
	{
		public const int Default = 0;

		public const int Tag = 1;

		public const int Class = 2;

		public const int PseudoClass = 3;

		public const int UnknownPseudoClass = 4;

		public const int Operator = 5;

		public const int Identifier = 6;

		public const int UnknownIdentifier = 7;

		public const int Value = 8;

		public const int Comment = 9;

		public const int Id = 10;

		public const int Important = 11;

		public const int Directive = 12;

		public const int DoubleString = 13;

		public const int SingleString = 14;

		public const int Identifier2 = 15;

		public const int Attribute = 16;

		public const int Identifier3 = 17;

		public const int PseudoElement = 18;

		public const int ExtendedIdentifier = 19;

		public const int ExtendedPseudoClass = 20;

		public const int ExtendedPseudoElement = 21;

		public const int Media = 22;

		public const int Variable = 23;
	}

	public static class Fortran
	{
		public const int Default = 0;

		public const int Comment = 1;

		public const int Number = 2;

		public const int String1 = 3;

		public const int String2 = 4;

		public const int StringEol = 5;

		public const int Operator = 6;

		public const int Identifier = 7;

		public const int Word = 8;

		public const int Word2 = 9;

		public const int Word3 = 10;

		public const int Preprocessor = 11;

		public const int Operator2 = 12;

		public const int Label = 13;

		public const int Continuation = 14;
	}

	public static class FreeBasic
	{
		public const int Default = 0;

		public const int Comment = 1;

		public const int Number = 2;

		public const int Keyword = 3;

		public const int String = 4;

		public const int Preprocessor = 5;

		public const int Operator = 6;

		public const int Identifier = 7;

		public const int Date = 8;

		public const int StringEol = 9;

		public const int Keyword2 = 10;

		public const int Keyword3 = 11;

		public const int Keyword4 = 12;

		public const int Constant = 13;

		public const int Asm = 14;

		public const int Label = 15;

		public const int Error = 16;

		public const int HexNumber = 17;

		public const int BinNumber = 18;

		public const int CommentBlock = 19;

		public const int DocLine = 20;

		public const int DocBlock = 21;

		public const int DocKeyword = 22;
	}

	public static class Html
	{
		public const int Default = 0;

		public const int Tag = 1;

		public const int TagUnknown = 2;

		public const int Attribute = 3;

		public const int AttributeUnknown = 4;

		public const int Number = 5;

		public const int DoubleString = 6;

		public const int SingleString = 7;

		public const int Other = 8;

		public const int Comment = 9;

		public const int Entity = 10;

		public const int TagEnd = 11;

		public const int XmlStart = 12;

		public const int XmlEnd = 13;

		public const int Script = 14;

		public const int Asp = 15;

		public const int AspAt = 16;

		public const int CData = 17;

		public const int Question = 18;

		public const int Value = 19;

		public const int XcComment = 20;
	}

	public static class Json
	{
		public const int Default = 0;

		public const int Number = 1;

		public const int String = 2;

		public const int StringEol = 3;

		public const int PropertyName = 4;

		public const int EscapeSequence = 5;

		public const int LineComment = 6;

		public const int BlockComment = 7;

		public const int Operator = 8;

		public const int Uri = 9;

		public const int CompactIRI = 10;

		public const int Keyword = 11;

		public const int LdKeyword = 12;

		public const int Error = 13;
	}

	public static class Lisp
	{
		public const int Default = 0;

		public const int Comment = 1;

		public const int Number = 2;

		public const int Keyword = 3;

		public const int KeywordKw = 4;

		public const int Symbol = 5;

		public const int String = 6;

		public const int StringEol = 8;

		public const int Identifier = 9;

		public const int Operator = 10;

		public const int Special = 11;

		public const int MultiComment = 12;
	}

	public static class Lua
	{
		public const int Default = 0;

		public const int Comment = 1;

		public const int CommentLine = 2;

		public const int CommentDoc = 3;

		public const int Number = 4;

		public const int Word = 5;

		public const int String = 6;

		public const int Character = 7;

		public const int LiteralString = 8;

		public const int Preprocessor = 9;

		public const int Operator = 10;

		public const int Identifier = 11;

		public const int StringEol = 12;

		public const int Word2 = 13;

		public const int Word3 = 14;

		public const int Word4 = 15;

		public const int Word5 = 16;

		public const int Word6 = 17;

		public const int Word7 = 18;

		public const int Word8 = 19;

		public const int Label = 20;
	}

	public static class Pascal
	{
		public const int Default = 0;

		public const int Identifier = 1;

		public const int Comment = 2;

		public const int Comment2 = 3;

		public const int CommentLine = 4;

		public const int Preprocessor = 5;

		public const int Preprocessor2 = 6;

		public const int Number = 7;

		public const int HexNumber = 8;

		public const int Word = 9;

		public const int String = 10;

		public const int StringEol = 11;

		public const int Character = 12;

		public const int Operator = 13;

		public const int Asm = 14;
	}

	public static class Perl
	{
		public const int Default = 0;

		public const int Error = 1;

		public const int CommentLine = 2;

		public const int Pod = 3;

		public const int Number = 4;

		public const int Word = 5;

		public const int String = 6;

		public const int Character = 7;

		public const int Punctuation = 8;

		public const int Preprocessor = 9;

		public const int Operator = 10;

		public const int Identifier = 11;

		public const int Scalar = 12;

		public const int Array = 13;

		public const int Hash = 14;

		public const int SymbolTable = 15;

		public const int VariableIndexer = 16;

		public const int Regex = 17;

		public const int RegSubst = 18;

		public const int BackTicks = 20;

		public const int DataSection = 21;

		public const int HereDelim = 22;

		public const int HereQ = 23;

		public const int HereQq = 24;

		public const int HereQx = 25;

		public const int StringQ = 26;

		public const int StringQq = 27;

		public const int StringQx = 28;

		public const int StringQr = 29;

		public const int StringQw = 30;

		public const int PodVerb = 31;

		public const int SubPrototype = 40;

		public const int FormatIdent = 41;

		public const int Format = 42;

		public const int StringVar = 43;

		public const int XLat = 44;

		public const int RegexVar = 54;

		public const int RegSubstVar = 55;

		public const int BackticksVar = 57;

		public const int HereQqVar = 61;

		public const int HereQxVar = 62;

		public const int StringQqVar = 64;

		public const int StringQxVar = 65;

		public const int StringQrVar = 66;
	}

	public static class PhpScript
	{
		public const int ComplexVariable = 104;

		public const int Default = 118;

		public const int HString = 119;

		public const int SimpleString = 120;

		public const int Word = 121;

		public const int Number = 122;

		public const int Variable = 123;

		public const int Comment = 124;

		public const int CommentLine = 125;

		public const int HStringVariable = 126;

		public const int Operator = 127;
	}

	public static class PowerShell
	{
		public const int Default = 0;

		public const int Comment = 1;

		public const int String = 2;

		public const int Character = 3;

		public const int Number = 4;

		public const int Variable = 5;

		public const int Operator = 6;

		public const int Identifier = 7;

		public const int Keyword = 8;

		public const int Cmdlet = 9;

		public const int Alias = 10;

		public const int Function = 11;

		public const int User1 = 12;

		public const int CommentStream = 13;

		public const int HereString = 14;

		public const int HereCharacter = 15;

		public const int CommentDocKeyword = 16;
	}

	public static class Properties
	{
		public const int Default = 0;

		public const int Comment = 1;

		public const int Section = 2;

		public const int Assignment = 3;

		public const int DefVal = 4;

		public const int Key = 5;
	}

	public static class PureBasic
	{
		public const int Default = 0;

		public const int Comment = 1;

		public const int Number = 2;

		public const int Keyword = 3;

		public const int String = 4;

		public const int Preprocessor = 5;

		public const int Operator = 6;

		public const int Identifier = 7;

		public const int Date = 8;

		public const int StringEol = 9;

		public const int Keyword2 = 10;

		public const int Keyword3 = 11;

		public const int Keyword4 = 12;

		public const int Constant = 13;

		public const int Asm = 14;

		public const int Label = 15;

		public const int Error = 16;

		public const int HexNumber = 17;

		public const int BinNumber = 18;

		public const int CommentBlock = 19;

		public const int DocLine = 20;

		public const int DocBlock = 21;

		public const int DocKeyword = 22;
	}

	public static class Python
	{
		public const int Default = 0;

		public const int CommentLine = 1;

		public const int Number = 2;

		public const int String = 3;

		public const int Character = 4;

		public const int Word = 5;

		public const int Triple = 6;

		public const int TripleDouble = 7;

		public const int ClassName = 8;

		public const int DefName = 9;

		public const int Operator = 10;

		public const int Identifier = 11;

		public const int CommentBlock = 12;

		public const int StringEol = 13;

		public const int Word2 = 14;

		public const int Decorator = 15;
	}

	public static class Ruby
	{
		public const int Default = 0;

		public const int Error = 1;

		public const int CommentLine = 2;

		public const int Pod = 3;

		public const int Number = 4;

		public const int Word = 5;

		public const int String = 6;

		public const int Character = 7;

		public const int ClassName = 8;

		public const int DefName = 9;

		public const int Operator = 10;

		public const int Identifier = 11;

		public const int Regex = 12;

		public const int Global = 13;

		public const int Symbol = 14;

		public const int ModuleName = 15;

		public const int InstanceVar = 16;

		public const int ClassVar = 17;

		public const int BackTicks = 18;

		public const int DataSection = 19;

		public const int HereDelim = 20;

		public const int HereQ = 21;

		public const int HereQq = 22;

		public const int HereQx = 23;

		public const int StringQ = 24;

		public const int StringQq = 25;

		public const int StringQx = 26;

		public const int StringQr = 27;

		public const int StringQw = 28;

		public const int WordDemoted = 29;

		public const int StdIn = 30;

		public const int StdOut = 31;

		public const int StdErr = 40;
	}

	public static class Smalltalk
	{
		public const int Default = 0;

		public const int String = 1;

		public const int Number = 2;

		public const int Comment = 3;

		public const int Symbol = 4;

		public const int Binary = 5;

		public const int Bool = 6;

		public const int Self = 7;

		public const int Super = 8;

		public const int Nil = 9;

		public const int Global = 10;

		public const int Return = 11;

		public const int Special = 12;

		public const int KwsEnd = 13;

		public const int Assign = 14;

		public const int Character = 15;

		public const int SpecSel = 16;
	}

	public static class Sql
	{
		public const int Default = 0;

		public const int Comment = 1;

		public const int CommentLine = 2;

		public const int CommentDoc = 3;

		public const int Number = 4;

		public const int Word = 5;

		public const int String = 6;

		public const int Character = 7;

		public const int SqlPlus = 8;

		public const int SqlPlusPrompt = 9;

		public const int Operator = 10;

		public const int Identifier = 11;

		public const int SqlPlusComment = 13;

		public const int CommentLineDoc = 15;

		public const int Word2 = 16;

		public const int CommentDocKeyword = 17;

		public const int CommentDocKeywordError = 18;

		public const int User1 = 19;

		public const int User2 = 20;

		public const int User3 = 21;

		public const int User4 = 22;

		public const int QuotedIdentifier = 23;

		public const int QOperator = 24;
	}

	public static class Markdown
	{
		public const int Default = 0;

		public const int LineBegin = 1;

		public const int Strong1 = 2;

		public const int Strong2 = 3;

		public const int Em1 = 4;

		public const int Em2 = 5;

		public const int Header1 = 6;

		public const int Header2 = 7;

		public const int Header3 = 8;

		public const int Header4 = 9;

		public const int Header5 = 10;

		public const int Header6 = 11;

		public const int PreChar = 12;

		public const int UListItem = 13;

		public const int OListItem = 14;

		public const int BlockQuote = 15;

		public const int Strikeout = 16;

		public const int HRule = 17;

		public const int Link = 18;

		public const int Code = 19;

		public const int Code2 = 20;

		public const int CodeBk = 21;
	}

	public static class R
	{
		public const int Default = 0;

		public const int Comment = 1;

		public const int KWord = 2;

		public const int BaseKWord = 3;

		public const int OtherKWord = 4;

		public const int Number = 5;

		public const int String = 6;

		public const int String2 = 7;

		public const int Operator = 8;

		public const int Identifier = 9;

		public const int Infix = 10;

		public const int InfixEol = 11;
	}

	public static class Vb
	{
		public const int Default = 0;

		public const int Comment = 1;

		public const int Number = 2;

		public const int Keyword = 3;

		public const int String = 4;

		public const int Preprocessor = 5;

		public const int Operator = 6;

		public const int Identifier = 7;

		public const int Date = 8;

		public const int StringEol = 9;

		public const int Keyword2 = 10;

		public const int Keyword3 = 11;

		public const int Keyword4 = 12;

		public const int Constant = 13;

		public const int Asm = 14;

		public const int Label = 15;

		public const int Error = 16;

		public const int HexNumber = 17;

		public const int BinNumber = 18;

		public const int CommentBlock = 19;

		public const int DocLine = 20;

		public const int DocBlock = 21;

		public const int DocKeyword = 22;
	}

	public static class VbScript
	{
		public const int Default = 0;

		public const int Comment = 1;

		public const int Number = 2;

		public const int Keyword = 3;

		public const int String = 4;

		public const int Preprocessor = 5;

		public const int Operator = 6;

		public const int Identifier = 7;

		public const int Date = 8;

		public const int StringEol = 9;

		public const int Keyword2 = 10;

		public const int Keyword3 = 11;

		public const int Keyword4 = 12;

		public const int Constant = 13;

		public const int Asm = 14;

		public const int Label = 15;

		public const int Error = 16;

		public const int HexNumber = 17;

		public const int BinNumber = 18;

		public const int CommentBlock = 19;

		public const int DocLine = 20;

		public const int DocBlock = 21;

		public const int DocKeyword = 22;
	}

	public static class Verilog
	{
		public const int Default = 0;

		public const int Comment = 1;

		public const int CommentLine = 2;

		public const int CommentLineBang = 3;

		public const int Number = 4;

		public const int Word = 5;

		public const int String = 6;

		public const int Word2 = 7;

		public const int Word3 = 8;

		public const int Preprocessor = 9;

		public const int Operator = 10;

		public const int Identifier = 11;

		public const int StringEol = 12;

		public const int User = 19;

		public const int CommentWord = 20;

		public const int Input = 21;

		public const int Output = 22;

		public const int InOut = 23;

		public const int PortConnect = 24;
	}

	public static class Xml
	{
		public const int Default = 0;

		public const int Tag = 1;

		public const int TagUnknown = 2;

		public const int Attribute = 3;

		public const int AttributeUnknown = 4;

		public const int Number = 5;

		public const int DoubleString = 6;

		public const int SingleString = 7;

		public const int Other = 8;

		public const int Comment = 9;

		public const int Entity = 10;

		public const int TagEnd = 11;

		public const int XmlStart = 12;

		public const int XmlEnd = 13;

		public const int Script = 14;

		public const int Asp = 15;

		public const int AspAt = 16;

		public const int CData = 17;

		public const int Question = 18;

		public const int Value = 19;

		public const int XcComment = 20;
	}

	public const int Default = 32;

	public const int LineNumber = 33;

	public const int CallTip = 38;

	public const int IndentGuide = 37;

	public const int BraceLight = 34;

	public const int BraceBad = 35;

	public const int FoldDisplayText = 39;

	private readonly Scintilla scintilla;

	public Color BackColor
	{
		get
		{
			int win32Color = scintilla.DirectMessage(2482, new IntPtr(Index), IntPtr.Zero).ToInt32();
			return ColorTranslator.FromWin32(win32Color);
		}
		set
		{
			if (value.IsEmpty)
			{
				value = Color.White;
			}
			int value2 = ColorTranslator.ToWin32(value);
			scintilla.DirectMessage(2052, new IntPtr(Index), new IntPtr(value2));
		}
	}

	public bool Bold
	{
		get
		{
			return scintilla.DirectMessage(2483, new IntPtr(Index), IntPtr.Zero) != IntPtr.Zero;
		}
		set
		{
			IntPtr lParam = (value ? new IntPtr(1) : IntPtr.Zero);
			scintilla.DirectMessage(2053, new IntPtr(Index), lParam);
		}
	}

	public StyleCase Case
	{
		get
		{
			return (StyleCase)scintilla.DirectMessage(2489, new IntPtr(Index), IntPtr.Zero).ToInt32();
		}
		set
		{
			scintilla.DirectMessage(2060, new IntPtr(Index), new IntPtr((int)value));
		}
	}

	public bool FillLine
	{
		get
		{
			return scintilla.DirectMessage(2487, new IntPtr(Index), IntPtr.Zero) != IntPtr.Zero;
		}
		set
		{
			IntPtr lParam = (value ? new IntPtr(1) : IntPtr.Zero);
			scintilla.DirectMessage(2057, new IntPtr(Index), lParam);
		}
	}

	public unsafe string Font
	{
		get
		{
			int num = scintilla.DirectMessage(2486, new IntPtr(Index), IntPtr.Zero).ToInt32();
			byte[] array = new byte[num];
			fixed (byte* value = array)
			{
				scintilla.DirectMessage(2486, new IntPtr(Index), new IntPtr(value));
			}
			return Encoding.UTF8.GetString(array, 0, num);
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				value = "Verdana";
			}
			fixed (byte* bytes = Helpers.GetBytes(value, Encoding.UTF8, zeroTerminated: true))
			{
				scintilla.DirectMessage(2056, new IntPtr(Index), new IntPtr(bytes));
			}
		}
	}

	public Color ForeColor
	{
		get
		{
			int win32Color = scintilla.DirectMessage(2481, new IntPtr(Index), IntPtr.Zero).ToInt32();
			return ColorTranslator.FromWin32(win32Color);
		}
		set
		{
			if (value.IsEmpty)
			{
				value = Color.Black;
			}
			int value2 = ColorTranslator.ToWin32(value);
			scintilla.DirectMessage(2051, new IntPtr(Index), new IntPtr(value2));
		}
	}

	public bool Hotspot
	{
		get
		{
			return scintilla.DirectMessage(2493, new IntPtr(Index), IntPtr.Zero) != IntPtr.Zero;
		}
		set
		{
			IntPtr lParam = (value ? new IntPtr(1) : IntPtr.Zero);
			scintilla.DirectMessage(2409, new IntPtr(Index), lParam);
		}
	}

	public int Index { get; private set; }

	public bool Italic
	{
		get
		{
			return scintilla.DirectMessage(2484, new IntPtr(Index), IntPtr.Zero) != IntPtr.Zero;
		}
		set
		{
			IntPtr lParam = (value ? new IntPtr(1) : IntPtr.Zero);
			scintilla.DirectMessage(2054, new IntPtr(Index), lParam);
		}
	}

	public int Size
	{
		get
		{
			return scintilla.DirectMessage(2485, new IntPtr(Index), IntPtr.Zero).ToInt32();
		}
		set
		{
			scintilla.DirectMessage(2055, new IntPtr(Index), new IntPtr(value));
		}
	}

	public float SizeF
	{
		get
		{
			int num = scintilla.DirectMessage(2062, new IntPtr(Index), IntPtr.Zero).ToInt32();
			return (float)num / 100f;
		}
		set
		{
			int value2 = (int)(value * 100f);
			scintilla.DirectMessage(2061, new IntPtr(Index), new IntPtr(value2));
		}
	}

	public bool Underline
	{
		get
		{
			return scintilla.DirectMessage(2488, new IntPtr(Index), IntPtr.Zero) != IntPtr.Zero;
		}
		set
		{
			IntPtr lParam = (value ? new IntPtr(1) : IntPtr.Zero);
			scintilla.DirectMessage(2059, new IntPtr(Index), lParam);
		}
	}

	public bool Visible
	{
		get
		{
			return scintilla.DirectMessage(2491, new IntPtr(Index), IntPtr.Zero) != IntPtr.Zero;
		}
		set
		{
			IntPtr lParam = (value ? new IntPtr(1) : IntPtr.Zero);
			scintilla.DirectMessage(2074, new IntPtr(Index), lParam);
		}
	}

	public int Weight
	{
		get
		{
			return scintilla.DirectMessage(2064, new IntPtr(Index), IntPtr.Zero).ToInt32();
		}
		set
		{
			scintilla.DirectMessage(2063, new IntPtr(Index), new IntPtr(value));
		}
	}

	public void CopyTo(Style destination)
	{
		if (destination != null)
		{
			destination.BackColor = BackColor;
			destination.Case = Case;
			destination.FillLine = FillLine;
			destination.Font = Font;
			destination.ForeColor = ForeColor;
			destination.Hotspot = Hotspot;
			destination.Italic = Italic;
			destination.Size = Size;
			destination.SizeF = SizeF;
			destination.Underline = Underline;
			destination.Visible = Visible;
			destination.Weight = Weight;
		}
	}

	public Style(Scintilla scintilla, int index)
	{
		this.scintilla = scintilla;
		Index = index;
	}
}
