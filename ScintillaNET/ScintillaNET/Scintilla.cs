using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using FlagsEnumTypeConverter;

namespace ScintillaNET;

[Docking(DockingBehavior.Ask)]
public class Scintilla : Control
{
	private static bool? reparentAll;

	private bool reparent;

	private static string modulePath;

	private static IntPtr moduleHandle;

	private static NativeMethods.Scintilla_DirectFunction directFunction;

	private static readonly object scNotificationEventKey = new object();

	private static readonly object insertCheckEventKey = new object();

	private static readonly object beforeInsertEventKey = new object();

	private static readonly object beforeDeleteEventKey = new object();

	private static readonly object insertEventKey = new object();

	private static readonly object deleteEventKey = new object();

	private static readonly object updateUIEventKey = new object();

	private static readonly object modifyAttemptEventKey = new object();

	private static readonly object styleNeededEventKey = new object();

	private static readonly object savePointReachedEventKey = new object();

	private static readonly object savePointLeftEventKey = new object();

	private static readonly object changeAnnotationEventKey = new object();

	private static readonly object marginClickEventKey = new object();

	private static readonly object marginRightClickEventKey = new object();

	private static readonly object charAddedEventKey = new object();

	private static readonly object autoCSelectionEventKey = new object();

	private static readonly object autoCCompletedEventKey = new object();

	private static readonly object autoCCancelledEventKey = new object();

	private static readonly object autoCCharDeletedEventKey = new object();

	private static readonly object dwellStartEventKey = new object();

	private static readonly object dwellEndEventKey = new object();

	private static readonly object borderStyleChangedEventKey = new object();

	private static readonly object doubleClickEventKey = new object();

	private static readonly object paintedEventKey = new object();

	private static readonly object needShownEventKey = new object();

	private static readonly object hotspotClickEventKey = new object();

	private static readonly object hotspotDoubleClickEventKey = new object();

	private static readonly object hotspotReleaseClickEventKey = new object();

	private static readonly object indicatorClickEventKey = new object();

	private static readonly object indicatorReleaseEventKey = new object();

	private static readonly object zoomChangedEventKey = new object();

	private IntPtr sciPtr;

	private BorderStyle borderStyle;

	private int stylingPosition;

	private int stylingBytePosition;

	private int? cachedPosition = null;

	private string cachedText = null;

	private bool doubleClick;

	private IntPtr fillUpChars;

	private string lastCallTip = string.Empty;

	public const int TimeForever = 10000000;

	public const int InvalidPosition = -1;

	[Category("Multiple Selection")]
	[Description("The additional caret foreground color.")]
	public Color AdditionalCaretForeColor
	{
		get
		{
			int win32Color = DirectMessage(2605).ToInt32();
			return ColorTranslator.FromWin32(win32Color);
		}
		set
		{
			int value2 = ColorTranslator.ToWin32(value);
			DirectMessage(2604, new IntPtr(value2));
		}
	}

	[DefaultValue(true)]
	[Category("Multiple Selection")]
	[Description("Whether the carets in additional selections should blink.")]
	public bool AdditionalCaretsBlink
	{
		get
		{
			return DirectMessage(2568) != IntPtr.Zero;
		}
		set
		{
			IntPtr wParam = (value ? new IntPtr(1) : IntPtr.Zero);
			DirectMessage(2567, wParam);
		}
	}

	[DefaultValue(true)]
	[Category("Multiple Selection")]
	[Description("Whether the carets in additional selections are visible.")]
	public bool AdditionalCaretsVisible
	{
		get
		{
			return DirectMessage(2609) != IntPtr.Zero;
		}
		set
		{
			IntPtr wParam = (value ? new IntPtr(1) : IntPtr.Zero);
			DirectMessage(2608, wParam);
		}
	}

	[DefaultValue(256)]
	[Category("Multiple Selection")]
	[Description("The transparency of additional selections.")]
	public int AdditionalSelAlpha
	{
		get
		{
			return DirectMessage(2603).ToInt32();
		}
		set
		{
			value = Helpers.Clamp(value, 0, 256);
			DirectMessage(2602, new IntPtr(value));
		}
	}

	[DefaultValue(false)]
	[Category("Multiple Selection")]
	[Description("Whether typing, backspace, or delete works with multiple selection simultaneously.")]
	public bool AdditionalSelectionTyping
	{
		get
		{
			return DirectMessage(2566) != IntPtr.Zero;
		}
		set
		{
			IntPtr wParam = (value ? new IntPtr(1) : IntPtr.Zero);
			DirectMessage(2565, wParam);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int AnchorPosition
	{
		get
		{
			int pos = DirectMessage(2009).ToInt32();
			return Lines.ByteToCharPosition(pos);
		}
		set
		{
			value = Helpers.Clamp(value, 0, TextLength);
			int value2 = Lines.CharToBytePosition(value);
			DirectMessage(2026, new IntPtr(value2));
		}
	}

	[DefaultValue(Annotation.Hidden)]
	[Category("Appearance")]
	[Description("Display and location of annotations.")]
	public Annotation AnnotationVisible
	{
		get
		{
			return (Annotation)DirectMessage(2549).ToInt32();
		}
		set
		{
			DirectMessage(2548, new IntPtr((int)value));
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool AutoCActive => DirectMessage(2102) != IntPtr.Zero;

	[DefaultValue(true)]
	[Category("Autocompletion")]
	[Description("Whether to automatically cancel autocompletion when no match is possible.")]
	public bool AutoCAutoHide
	{
		get
		{
			return DirectMessage(2119) != IntPtr.Zero;
		}
		set
		{
			IntPtr wParam = (value ? new IntPtr(1) : IntPtr.Zero);
			DirectMessage(2118, wParam);
		}
	}

	[DefaultValue(true)]
	[Category("Autocompletion")]
	[Description("Whether to cancel an autocompletion if the caret moves from its initial location, or is allowed to move to the word start.")]
	public bool AutoCCancelAtStart
	{
		get
		{
			return DirectMessage(2111) != IntPtr.Zero;
		}
		set
		{
			IntPtr wParam = (value ? new IntPtr(1) : IntPtr.Zero);
			DirectMessage(2110, wParam);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int AutoCCurrent => DirectMessage(2445).ToInt32();

	[DefaultValue(false)]
	[Category("Autocompletion")]
	[Description("Whether to automatically choose an autocompletion item when it is the only one in the list.")]
	public bool AutoCChooseSingle
	{
		get
		{
			return DirectMessage(2114) != IntPtr.Zero;
		}
		set
		{
			IntPtr wParam = (value ? new IntPtr(1) : IntPtr.Zero);
			DirectMessage(2113, wParam);
		}
	}

	[DefaultValue(false)]
	[Category("Autocompletion")]
	[Description("Whether to delete any existing word characters following the caret after autocompletion.")]
	public bool AutoCDropRestOfWord
	{
		get
		{
			return DirectMessage(2271) != IntPtr.Zero;
		}
		set
		{
			IntPtr wParam = (value ? new IntPtr(1) : IntPtr.Zero);
			DirectMessage(2270, wParam);
		}
	}

	[DefaultValue(false)]
	[Category("Autocompletion")]
	[Description("Whether autocompletion word matching can ignore case.")]
	public bool AutoCIgnoreCase
	{
		get
		{
			return DirectMessage(2116) != IntPtr.Zero;
		}
		set
		{
			IntPtr wParam = (value ? new IntPtr(1) : IntPtr.Zero);
			DirectMessage(2115, wParam);
		}
	}

	[DefaultValue(5)]
	[Category("Autocompletion")]
	[Description("The maximum number of rows to display in an autocompletion list.")]
	public int AutoCMaxHeight
	{
		get
		{
			return DirectMessage(2211).ToInt32();
		}
		set
		{
			value = Helpers.ClampMin(value, 0);
			DirectMessage(2210, new IntPtr(value));
		}
	}

	[DefaultValue(0)]
	[Category("Autocompletion")]
	[Description("The width of the autocompletion list measured in characters.")]
	public int AutoCMaxWidth
	{
		get
		{
			return DirectMessage(2209).ToInt32();
		}
		set
		{
			value = Helpers.ClampMin(value, 0);
			DirectMessage(2208, new IntPtr(value));
		}
	}

	[DefaultValue(Order.Presorted)]
	[Category("Autocompletion")]
	[Description("The order of words in an autocompletion list.")]
	public Order AutoCOrder
	{
		get
		{
			return (Order)DirectMessage(2661).ToInt32();
		}
		set
		{
			DirectMessage(2660, new IntPtr((int)value));
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int AutoCPosStart
	{
		get
		{
			int pos = DirectMessage(2103).ToInt32();
			return Lines.ByteToCharPosition(pos);
		}
	}

	[DefaultValue(' ')]
	[Category("Autocompletion")]
	[Description("The autocompletion list word delimiter. The default is a space character.")]
	public char AutoCSeparator
	{
		get
		{
			int num = DirectMessage(2107).ToInt32();
			return (char)num;
		}
		set
		{
			byte value2 = (byte)value;
			DirectMessage(2106, new IntPtr(value2));
		}
	}

	[DefaultValue('?')]
	[Category("Autocompletion")]
	[Description("The autocompletion list image type delimiter.")]
	public char AutoCTypeSeparator
	{
		get
		{
			int num = DirectMessage(2285).ToInt32();
			return (char)num;
		}
		set
		{
			byte value2 = (byte)value;
			DirectMessage(2286, new IntPtr(value2));
		}
	}

	[DefaultValue(AutomaticFold.None)]
	[Category("Behavior")]
	[Description("Options for allowing the control to automatically handle folding.")]
	[TypeConverter(typeof(FlagsEnumConverter))]
	public AutomaticFold AutomaticFold
	{
		get
		{
			return (AutomaticFold)(int)DirectMessage(2664);
		}
		set
		{
			DirectMessage(2663, new IntPtr((int)value));
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override Color BackColor
	{
		get
		{
			return base.BackColor;
		}
		set
		{
			base.BackColor = value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override Image BackgroundImage
	{
		get
		{
			return base.BackgroundImage;
		}
		set
		{
			base.BackgroundImage = value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override ImageLayout BackgroundImageLayout
	{
		get
		{
			return base.BackgroundImageLayout;
		}
		set
		{
			base.BackgroundImageLayout = value;
		}
	}

	[DefaultValue(BorderStyle.Fixed3D)]
	[Category("Appearance")]
	[Description("Indicates whether the control should have a border.")]
	public BorderStyle BorderStyle
	{
		get
		{
			return borderStyle;
		}
		set
		{
			if (borderStyle != value)
			{
				if (!Enum.IsDefined(typeof(BorderStyle), value))
				{
					throw new InvalidEnumArgumentException("value", (int)value, typeof(BorderStyle));
				}
				borderStyle = value;
				UpdateStyles();
				OnBorderStyleChanged(EventArgs.Empty);
			}
		}
	}

	[DefaultValue(true)]
	[Category("Misc")]
	[Description("Determines whether drawing is double-buffered.")]
	public bool BufferedDraw
	{
		get
		{
			return DirectMessage(2034) != IntPtr.Zero;
		}
		set
		{
			IntPtr wParam = (value ? new IntPtr(1) : IntPtr.Zero);
			DirectMessage(2035, wParam);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool CallTipActive => DirectMessage(2202) != IntPtr.Zero;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool CanPaste => DirectMessage(2173) != IntPtr.Zero;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool CanRedo => DirectMessage(2016) != IntPtr.Zero;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool CanUndo => DirectMessage(2174) != IntPtr.Zero;

	[DefaultValue(typeof(Color), "Black")]
	[Category("Caret")]
	[Description("The caret foreground color.")]
	public Color CaretForeColor
	{
		get
		{
			int win32Color = DirectMessage(2138).ToInt32();
			return ColorTranslator.FromWin32(win32Color);
		}
		set
		{
			int value2 = ColorTranslator.ToWin32(value);
			DirectMessage(2069, new IntPtr(value2));
		}
	}

	[DefaultValue(typeof(Color), "Yellow")]
	[Category("Caret")]
	[Description("The background color of the current line.")]
	public Color CaretLineBackColor
	{
		get
		{
			int win32Color = DirectMessage(2097).ToInt32();
			return ColorTranslator.FromWin32(win32Color);
		}
		set
		{
			int value2 = ColorTranslator.ToWin32(value);
			DirectMessage(2098, new IntPtr(value2));
		}
	}

	[DefaultValue(256)]
	[Category("Caret")]
	[Description("The transparency of the current line background color.")]
	public int CaretLineBackColorAlpha
	{
		get
		{
			return DirectMessage(2471).ToInt32();
		}
		set
		{
			value = Helpers.Clamp(value, 0, 256);
			DirectMessage(2470, new IntPtr(value));
		}
	}

	[DefaultValue(false)]
	[Category("Caret")]
	[Description("Determines whether to highlight the current caret line.")]
	public bool CaretLineVisible
	{
		get
		{
			return DirectMessage(2095) != IntPtr.Zero;
		}
		set
		{
			IntPtr wParam = (value ? new IntPtr(1) : IntPtr.Zero);
			DirectMessage(2096, wParam);
		}
	}

	[DefaultValue(530)]
	[Category("Caret")]
	[Description("The caret blink rate in milliseconds.")]
	public int CaretPeriod
	{
		get
		{
			return DirectMessage(2075).ToInt32();
		}
		set
		{
			value = Helpers.ClampMin(value, 0);
			DirectMessage(2076, new IntPtr(value));
		}
	}

	[DefaultValue(CaretStyle.Line)]
	[Category("Caret")]
	[Description("The caret display style.")]
	public CaretStyle CaretStyle
	{
		get
		{
			return (CaretStyle)DirectMessage(2513).ToInt32();
		}
		set
		{
			DirectMessage(2512, new IntPtr((int)value));
		}
	}

	[DefaultValue(1)]
	[Category("Caret")]
	[Description("The width of the caret line measured in pixels (between 0 and 3).")]
	public int CaretWidth
	{
		get
		{
			return DirectMessage(2189).ToInt32();
		}
		set
		{
			value = Helpers.Clamp(value, 0, 3);
			DirectMessage(2188, new IntPtr(value));
		}
	}

	protected override CreateParams CreateParams
	{
		get
		{
			if (moduleHandle == IntPtr.Zero)
			{
				string text = GetModulePath();
				moduleHandle = NativeMethods.LoadLibrary(text);
				if (moduleHandle == IntPtr.Zero)
				{
					string message = string.Format(CultureInfo.InvariantCulture, "Could not load the Scintilla module at the path '{0}'.", new object[1] { text });
					throw new Win32Exception(message, new Win32Exception());
				}
				IntPtr procAddress = NativeMethods.GetProcAddress(new HandleRef(this, moduleHandle), "Scintilla_DirectFunction");
				if (procAddress == IntPtr.Zero)
				{
					string message2 = "The Scintilla module has no export for the 'Scintilla_DirectFunction' procedure.";
					throw new Win32Exception(message2, new Win32Exception());
				}
				directFunction = (NativeMethods.Scintilla_DirectFunction)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(NativeMethods.Scintilla_DirectFunction));
			}
			CreateParams createParams = base.CreateParams;
			createParams.ClassName = "Scintilla";
			createParams.ExStyle &= -513;
			createParams.Style &= -8388609;
			switch (borderStyle)
			{
			case BorderStyle.Fixed3D:
				createParams.ExStyle |= 512;
				break;
			case BorderStyle.FixedSingle:
				createParams.Style |= 8388608;
				break;
			}
			return createParams;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int CurrentLine
	{
		get
		{
			int value = DirectMessage(2008).ToInt32();
			return DirectMessage(2166, new IntPtr(value)).ToInt32();
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int CurrentPosition
	{
		get
		{
			int pos = DirectMessage(2008).ToInt32();
			return Lines.ByteToCharPosition(pos);
		}
		set
		{
			value = Helpers.Clamp(value, 0, TextLength);
			int value2 = Lines.CharToBytePosition(value);
			DirectMessage(2141, new IntPtr(value2));
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override Cursor Cursor
	{
		get
		{
			return base.Cursor;
		}
		set
		{
			base.Cursor = value;
		}
	}

	protected override Cursor DefaultCursor => Cursors.IBeam;

	protected override Size DefaultSize => new Size(200, 100);

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public Document Document
	{
		get
		{
			IntPtr value = DirectMessage(2357);
			return new Document
			{
				Value = value
			};
		}
		set
		{
			Eol eolMode = EolMode;
			bool useTabs = UseTabs;
			int tabWidth = TabWidth;
			int indentWidth = IndentWidth;
			IntPtr value2 = value.Value;
			DirectMessage(2358, IntPtr.Zero, value2);
			InitDocument(eolMode, useTabs, tabWidth, indentWidth);
			Lines.RebuildLineData();
		}
	}

	[DefaultValue(typeof(Color), "Silver")]
	[Category("Long Lines")]
	[Description("The background color to use when indicating long lines.")]
	public Color EdgeColor
	{
		get
		{
			int win32Color = DirectMessage(2364).ToInt32();
			return ColorTranslator.FromWin32(win32Color);
		}
		set
		{
			int value2 = ColorTranslator.ToWin32(value);
			DirectMessage(2365, new IntPtr(value2));
		}
	}

	[DefaultValue(0)]
	[Category("Long Lines")]
	[Description("The number of columns at which to display long line indicators.")]
	public int EdgeColumn
	{
		get
		{
			return DirectMessage(2360).ToInt32();
		}
		set
		{
			value = Helpers.ClampMin(value, 0);
			DirectMessage(2361, new IntPtr(value));
		}
	}

	[DefaultValue(EdgeMode.None)]
	[Category("Long Lines")]
	[Description("Determines how long lines are indicated.")]
	public EdgeMode EdgeMode
	{
		get
		{
			return (EdgeMode)(int)DirectMessage(2362);
		}
		set
		{
			DirectMessage(2363, new IntPtr((int)value));
		}
	}

	internal Encoding Encoding
	{
		get
		{
			int num = (int)DirectMessage(2137);
			return (num == 0) ? Encoding.Default : Encoding.GetEncoding(num);
		}
	}

	[DefaultValue(true)]
	[Category("Scrolling")]
	[Description("Determines whether the maximum vertical scroll position ends at the last line or can scroll past.")]
	public bool EndAtLastLine
	{
		get
		{
			return DirectMessage(2278) != IntPtr.Zero;
		}
		set
		{
			IntPtr wParam = (value ? new IntPtr(1) : IntPtr.Zero);
			DirectMessage(2277, wParam);
		}
	}

	[DefaultValue(Eol.CrLf)]
	[Category("Line Endings")]
	[Description("Determines the characters added into the document when the user presses the Enter key.")]
	public Eol EolMode
	{
		get
		{
			return (Eol)(int)DirectMessage(2030);
		}
		set
		{
			DirectMessage(2031, new IntPtr((int)value));
		}
	}

	[DefaultValue(0)]
	[Category("Whitespace")]
	[Description("Extra whitespace added to the ascent (top) of each line.")]
	public int ExtraAscent
	{
		get
		{
			return DirectMessage(2526).ToInt32();
		}
		set
		{
			DirectMessage(2525, new IntPtr(value));
		}
	}

	[DefaultValue(0)]
	[Category("Whitespace")]
	[Description("Extra whitespace added to the descent (bottom) of each line.")]
	public int ExtraDescent
	{
		get
		{
			return DirectMessage(2528).ToInt32();
		}
		set
		{
			DirectMessage(2527, new IntPtr(value));
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int FirstVisibleLine
	{
		get
		{
			return DirectMessage(2152).ToInt32();
		}
		set
		{
			value = Helpers.ClampMin(value, 0);
			DirectMessage(2613, new IntPtr(value));
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override Font Font
	{
		get
		{
			return base.Font;
		}
		set
		{
			base.Font = value;
		}
	}

	[DefaultValue(FontQuality.Default)]
	[Category("Misc")]
	[Description("Specifies the anti-aliasing method to use when rendering fonts.")]
	public FontQuality FontQuality
	{
		get
		{
			return (FontQuality)(int)DirectMessage(2612);
		}
		set
		{
			DirectMessage(2611, new IntPtr((int)value));
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override Color ForeColor
	{
		get
		{
			return base.ForeColor;
		}
		set
		{
			base.ForeColor = value;
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int HighlightGuide
	{
		get
		{
			return DirectMessage(2135).ToInt32();
		}
		set
		{
			value = Helpers.ClampMin(value, 0);
			DirectMessage(2134, new IntPtr(value));
		}
	}

	[DefaultValue(true)]
	[Category("Scrolling")]
	[Description("Determines whether to show the horizontal scroll bar if needed.")]
	public bool HScrollBar
	{
		get
		{
			return DirectMessage(2131) != IntPtr.Zero;
		}
		set
		{
			IntPtr wParam = (value ? new IntPtr(1) : IntPtr.Zero);
			DirectMessage(2130, wParam);
		}
	}

	[DefaultValue(IdleStyling.None)]
	[Category("Misc")]
	[Description("Specifies how to use application idle time for styling.")]
	public IdleStyling IdleStyling
	{
		get
		{
			return (IdleStyling)(int)DirectMessage(2693);
		}
		set
		{
			DirectMessage(2692, new IntPtr((int)value));
		}
	}

	[DefaultValue(0)]
	[Category("Indentation")]
	[Description("The indentation size in characters or 0 to make it the same as the tab width.")]
	public int IndentWidth
	{
		get
		{
			return DirectMessage(2123).ToInt32();
		}
		set
		{
			value = Helpers.ClampMin(value, 0);
			DirectMessage(2122, new IntPtr(value));
		}
	}

	[DefaultValue(IndentView.None)]
	[Category("Indentation")]
	[Description("Indicates whether indentation guides are displayed.")]
	public IndentView IndentationGuides
	{
		get
		{
			return (IndentView)(int)DirectMessage(2133);
		}
		set
		{
			DirectMessage(2132, new IntPtr((int)value));
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int IndicatorCurrent
	{
		get
		{
			return DirectMessage(2501).ToInt32();
		}
		set
		{
			value = Helpers.Clamp(value, 0, Indicators.Count - 1);
			DirectMessage(2500, new IntPtr(value));
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public IndicatorCollection Indicators { get; private set; }

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int IndicatorValue
	{
		get
		{
			return DirectMessage(2503).ToInt32();
		}
		set
		{
			DirectMessage(2502, new IntPtr(value));
		}
	}

	[DefaultValue(Lexer.Container)]
	[Category("Lexing")]
	[Description("The current lexer.")]
	public Lexer Lexer
	{
		get
		{
			return (Lexer)(int)DirectMessage(4002);
		}
		set
		{
			DirectMessage(4001, new IntPtr((int)value));
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public unsafe string LexerLanguage
	{
		get
		{
			int num = DirectMessage(4012).ToInt32();
			if (num == 0)
			{
				return string.Empty;
			}
			fixed (byte* value = new byte[num + 1])
			{
				DirectMessage(4012, IntPtr.Zero, new IntPtr(value));
				return Helpers.GetString(new IntPtr(value), num, Encoding.ASCII);
			}
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				DirectMessage(4006, IntPtr.Zero, IntPtr.Zero);
				return;
			}
			fixed (byte* bytes = Helpers.GetBytes(value, Encoding.ASCII, zeroTerminated: true))
			{
				DirectMessage(4006, IntPtr.Zero, new IntPtr(bytes));
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public LineEndType LineEndTypesActive => (LineEndType)(int)DirectMessage(2658);

	[DefaultValue(LineEndType.Default)]
	[Category("Line Endings")]
	[Description("Line endings types interpreted by the control.")]
	[TypeConverter(typeof(FlagsEnumConverter))]
	public LineEndType LineEndTypesAllowed
	{
		get
		{
			return (LineEndType)(int)DirectMessage(2657);
		}
		set
		{
			DirectMessage(2656, new IntPtr((int)value));
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public LineEndType LineEndTypesSupported => (LineEndType)(int)DirectMessage(4018);

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public LineCollection Lines { get; private set; }

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int LinesOnScreen => DirectMessage(2370).ToInt32();

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int MainSelection
	{
		get
		{
			return DirectMessage(2575).ToInt32();
		}
		set
		{
			value = Helpers.ClampMin(value, 0);
			DirectMessage(2574, new IntPtr(value));
		}
	}

	[Category("Collections")]
	[Description("The margins collection.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public MarginCollection Margins { get; private set; }

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public MarkerCollection Markers { get; private set; }

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public bool Modified => DirectMessage(2159) != IntPtr.Zero;

	[DefaultValue(10000000)]
	[Category("Behavior")]
	[Description("The time in milliseconds the mouse must linger to generate a dwell start event. A value of 10000000 disables dwell events.")]
	public int MouseDwellTime
	{
		get
		{
			return DirectMessage(2265).ToInt32();
		}
		set
		{
			value = Helpers.ClampMin(value, 0);
			DirectMessage(2264, new IntPtr(value));
		}
	}

	[DefaultValue(false)]
	[Category("Multiple Selection")]
	[Description("Enable or disable the ability to switch to rectangular selection mode while making a selection with the mouse.")]
	public bool MouseSelectionRectangularSwitch
	{
		get
		{
			return DirectMessage(2669) != IntPtr.Zero;
		}
		set
		{
			IntPtr wParam = (value ? new IntPtr(1) : IntPtr.Zero);
			DirectMessage(2668, wParam);
		}
	}

	[DefaultValue(false)]
	[Category("Multiple Selection")]
	[Description("Enable or disable multiple selection with the CTRL key.")]
	public bool MultipleSelection
	{
		get
		{
			return DirectMessage(2564) != IntPtr.Zero;
		}
		set
		{
			IntPtr wParam = (value ? new IntPtr(1) : IntPtr.Zero);
			DirectMessage(2563, wParam);
		}
	}

	[DefaultValue(MultiPaste.Once)]
	[Category("Multiple Selection")]
	[Description("Determines how pasted text is applied to multiple selections.")]
	public MultiPaste MultiPaste
	{
		get
		{
			return (MultiPaste)(int)DirectMessage(2615);
		}
		set
		{
			DirectMessage(2614, new IntPtr((int)value));
		}
	}

	[DefaultValue(false)]
	[Category("Behavior")]
	[Description("Puts the caret into overtype mode.")]
	public bool Overtype
	{
		get
		{
			return DirectMessage(2187) != IntPtr.Zero;
		}
		set
		{
			IntPtr wParam = (value ? new IntPtr(1) : IntPtr.Zero);
			DirectMessage(2186, wParam);
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public new Padding Padding
	{
		get
		{
			return base.Padding;
		}
		set
		{
			base.Padding = value;
		}
	}

	[DefaultValue(true)]
	[Category("Line Endings")]
	[Description("Whether line endings in pasted text are converted to match the document end-of-line mode.")]
	public bool PasteConvertEndings
	{
		get
		{
			return DirectMessage(2468) != IntPtr.Zero;
		}
		set
		{
			IntPtr wParam = (value ? new IntPtr(1) : IntPtr.Zero);
			DirectMessage(2467, wParam);
		}
	}

	[DefaultValue(Phases.Two)]
	[Category("Misc")]
	[Description("Adjusts the number of phases used when drawing.")]
	public Phases PhasesDraw
	{
		get
		{
			return (Phases)(int)DirectMessage(2673);
		}
		set
		{
			DirectMessage(2674, new IntPtr((int)value));
		}
	}

	[DefaultValue(false)]
	[Category("Behavior")]
	[Description("Controls whether the document text can be modified.")]
	public bool ReadOnly
	{
		get
		{
			return DirectMessage(2140) != IntPtr.Zero;
		}
		set
		{
			IntPtr wParam = (value ? new IntPtr(1) : IntPtr.Zero);
			DirectMessage(2171, wParam);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int RectangularSelectionAnchor
	{
		get
		{
			int num = DirectMessage(2591).ToInt32();
			if (num <= 0)
			{
				return num;
			}
			return Lines.ByteToCharPosition(num);
		}
		set
		{
			value = Helpers.Clamp(value, 0, TextLength);
			value = Lines.CharToBytePosition(value);
			DirectMessage(2590, new IntPtr(value));
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int RectangularSelectionAnchorVirtualSpace
	{
		get
		{
			return DirectMessage(2595).ToInt32();
		}
		set
		{
			value = Helpers.ClampMin(value, 0);
			DirectMessage(2594, new IntPtr(value));
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int RectangularSelectionCaret
	{
		get
		{
			int num = DirectMessage(2589).ToInt32();
			if (num <= 0)
			{
				return 0;
			}
			return Lines.ByteToCharPosition(num);
		}
		set
		{
			value = Helpers.Clamp(value, 0, TextLength);
			value = Lines.CharToBytePosition(value);
			DirectMessage(2588, new IntPtr(value));
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int RectangularSelectionCaretVirtualSpace
	{
		get
		{
			return DirectMessage(2593).ToInt32();
		}
		set
		{
			value = Helpers.ClampMin(value, 0);
			DirectMessage(2592, new IntPtr(value));
		}
	}

	private IntPtr SciPointer
	{
		get
		{
			if (Control.CheckForIllegalCrossThreadCalls && base.InvokeRequired)
			{
				string message = string.Format(CultureInfo.InvariantCulture, "Control '{0}' accessed from a thread other than the thread it was created on.", new object[1] { base.Name });
				throw new InvalidOperationException(message);
			}
			if (sciPtr == IntPtr.Zero)
			{
				sciPtr = NativeMethods.SendMessage(new HandleRef(this, base.Handle), 2185, IntPtr.Zero, IntPtr.Zero);
			}
			return sciPtr;
		}
	}

	[DefaultValue(2000)]
	[Category("Scrolling")]
	[Description("The range in pixels of the horizontal scroll bar.")]
	public int ScrollWidth
	{
		get
		{
			return DirectMessage(2275).ToInt32();
		}
		set
		{
			DirectMessage(2274, new IntPtr(value));
		}
	}

	[DefaultValue(true)]
	[Category("Scrolling")]
	[Description("Determines whether to increase the horizontal scroll width as needed.")]
	public bool ScrollWidthTracking
	{
		get
		{
			return DirectMessage(2517) != IntPtr.Zero;
		}
		set
		{
			IntPtr wParam = (value ? new IntPtr(1) : IntPtr.Zero);
			DirectMessage(2516, wParam);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public SearchFlags SearchFlags
	{
		get
		{
			return (SearchFlags)DirectMessage(2199).ToInt32();
		}
		set
		{
			DirectMessage(2198, new IntPtr((int)value));
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public unsafe string SelectedText
	{
		get
		{
			int num = DirectMessage(2161).ToInt32() - 1;
			if (num <= 0)
			{
				return string.Empty;
			}
			fixed (byte* value = new byte[num + 1])
			{
				DirectMessage(2161, IntPtr.Zero, new IntPtr(value));
				return Helpers.GetString(new IntPtr(value), num, Encoding);
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int SelectionEnd
	{
		get
		{
			int pos = DirectMessage(2145).ToInt32();
			return Lines.ByteToCharPosition(pos);
		}
		set
		{
			value = Helpers.Clamp(value, 0, TextLength);
			value = Lines.CharToBytePosition(value);
			DirectMessage(2144, new IntPtr(value));
		}
	}

	[DefaultValue(false)]
	[Category("Selection")]
	[Description("Determines whether a selection should fill past the end of the line.")]
	public bool SelectionEolFilled
	{
		get
		{
			return DirectMessage(2479) != IntPtr.Zero;
		}
		set
		{
			IntPtr wParam = (value ? new IntPtr(1) : IntPtr.Zero);
			DirectMessage(2480, wParam);
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public SelectionCollection Selections { get; private set; }

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int SelectionStart
	{
		get
		{
			int pos = DirectMessage(2143).ToInt32();
			return Lines.ByteToCharPosition(pos);
		}
		set
		{
			value = Helpers.Clamp(value, 0, TextLength);
			value = Lines.CharToBytePosition(value);
			DirectMessage(2142, new IntPtr(value));
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public Status Status
	{
		get
		{
			return (Status)(int)DirectMessage(2383);
		}
		set
		{
			DirectMessage(2382, new IntPtr((int)value));
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public StyleCollection Styles { get; private set; }

	[DefaultValue(TabDrawMode.LongArrow)]
	[Category("Whitespace")]
	[Description("Style of visible tab characters.")]
	public TabDrawMode TabDrawMode
	{
		get
		{
			return (TabDrawMode)(int)DirectMessage(2698);
		}
		set
		{
			DirectMessage(2699, new IntPtr((int)value));
		}
	}

	[DefaultValue(4)]
	[Category("Indentation")]
	[Description("The tab size in characters.")]
	public int TabWidth
	{
		get
		{
			return DirectMessage(2121).ToInt32();
		}
		set
		{
			DirectMessage(2036, new IntPtr(value));
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int TargetEnd
	{
		get
		{
			int pos = Helpers.Clamp(DirectMessage(2193).ToInt32(), 0, DirectMessage(2183).ToInt32());
			return Lines.ByteToCharPosition(pos);
		}
		set
		{
			value = Helpers.Clamp(value, 0, TextLength);
			value = Lines.CharToBytePosition(value);
			DirectMessage(2192, new IntPtr(value));
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int TargetStart
	{
		get
		{
			int pos = Helpers.Clamp(DirectMessage(2191).ToInt32(), 0, DirectMessage(2183).ToInt32());
			return Lines.ByteToCharPosition(pos);
		}
		set
		{
			value = Helpers.Clamp(value, 0, TextLength);
			value = Lines.CharToBytePosition(value);
			DirectMessage(2190, new IntPtr(value));
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public unsafe string TargetText
	{
		get
		{
			int num = DirectMessage(2687).ToInt32();
			if (num == 0)
			{
				return string.Empty;
			}
			fixed (byte* value = new byte[num + 1])
			{
				DirectMessage(2687, IntPtr.Zero, new IntPtr(value));
				return Helpers.GetString(new IntPtr(value), num, Encoding);
			}
		}
	}

	[DefaultValue(Technology.Default)]
	[Category("Misc")]
	[Description("The rendering technology used to draw text.")]
	public Technology Technology
	{
		get
		{
			return (Technology)(int)DirectMessage(2631);
		}
		set
		{
			DirectMessage(2630, new IntPtr((int)value));
		}
	}

	[Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design", typeof(UITypeEditor))]
	public unsafe override string Text
	{
		get
		{
			int num = DirectMessage(2183).ToInt32();
			IntPtr intPtr = DirectMessage(2643, new IntPtr(0), new IntPtr(num));
			if (intPtr == IntPtr.Zero)
			{
				return string.Empty;
			}
			return new string((sbyte*)(void*)intPtr, 0, num, Encoding);
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				DirectMessage(2004);
				return;
			}
			fixed (byte* bytes = Helpers.GetBytes(value, Encoding, zeroTerminated: true))
			{
				DirectMessage(2181, IntPtr.Zero, new IntPtr(bytes));
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int TextLength => Lines.TextLength;

	[DefaultValue(false)]
	[Category("Indentation")]
	[Description("Determines whether indentation allows tab characters or purely space characters.")]
	public bool UseTabs
	{
		get
		{
			return DirectMessage(2125) != IntPtr.Zero;
		}
		set
		{
			IntPtr wParam = (value ? new IntPtr(1) : IntPtr.Zero);
			DirectMessage(2124, wParam);
		}
	}

	public new bool UseWaitCursor
	{
		get
		{
			return base.UseWaitCursor;
		}
		set
		{
			base.UseWaitCursor = value;
			int value2 = (value ? 4 : (-1));
			DirectMessage(2386, new IntPtr(value2));
		}
	}

	[DefaultValue(false)]
	[Category("Line Endings")]
	[Description("Display end-of-line characters.")]
	public bool ViewEol
	{
		get
		{
			return DirectMessage(2355) != IntPtr.Zero;
		}
		set
		{
			IntPtr wParam = (value ? new IntPtr(1) : IntPtr.Zero);
			DirectMessage(2356, wParam);
		}
	}

	[DefaultValue(WhitespaceMode.Invisible)]
	[Category("Whitespace")]
	[Description("Options for displaying whitespace characters.")]
	public WhitespaceMode ViewWhitespace
	{
		get
		{
			return (WhitespaceMode)(int)DirectMessage(2020);
		}
		set
		{
			DirectMessage(2021, new IntPtr((int)value));
		}
	}

	[DefaultValue(VirtualSpace.None)]
	[Category("Behavior")]
	[Description("Options for allowing the caret to move beyond the end of each line.")]
	[TypeConverter(typeof(FlagsEnumConverter))]
	public VirtualSpace VirtualSpaceOptions
	{
		get
		{
			return (VirtualSpace)(int)DirectMessage(2597);
		}
		set
		{
			DirectMessage(2596, new IntPtr((int)value));
		}
	}

	[DefaultValue(true)]
	[Category("Scrolling")]
	[Description("Determines whether to show the vertical scroll bar when needed.")]
	public bool VScrollBar
	{
		get
		{
			return DirectMessage(2281) != IntPtr.Zero;
		}
		set
		{
			IntPtr wParam = (value ? new IntPtr(1) : IntPtr.Zero);
			DirectMessage(2280, wParam);
		}
	}

	[DefaultValue(1)]
	[Category("Whitespace")]
	[Description("The size of whitespace dots.")]
	public int WhitespaceSize
	{
		get
		{
			return DirectMessage(2087).ToInt32();
		}
		set
		{
			DirectMessage(2086, new IntPtr(value));
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public unsafe string WordChars
	{
		get
		{
			int num = DirectMessage(2646, IntPtr.Zero, IntPtr.Zero).ToInt32();
			fixed (byte* value = new byte[num + 1])
			{
				DirectMessage(2646, IntPtr.Zero, new IntPtr(value));
				return Helpers.GetString(new IntPtr(value), num, Encoding.ASCII);
			}
		}
		set
		{
			if (value == null)
			{
				DirectMessage(2077, IntPtr.Zero, IntPtr.Zero);
				return;
			}
			fixed (byte* bytes = Helpers.GetBytes(value, Encoding.ASCII, zeroTerminated: true))
			{
				DirectMessage(2077, IntPtr.Zero, new IntPtr(bytes));
			}
		}
	}

	[DefaultValue(WrapIndentMode.Fixed)]
	[Category("Line Wrapping")]
	[Description("Determines how wrapped sublines are indented.")]
	public WrapIndentMode WrapIndentMode
	{
		get
		{
			return (WrapIndentMode)(int)DirectMessage(2473);
		}
		set
		{
			DirectMessage(2472, new IntPtr((int)value));
		}
	}

	[DefaultValue(WrapMode.None)]
	[Category("Line Wrapping")]
	[Description("The line wrapping strategy.")]
	public WrapMode WrapMode
	{
		get
		{
			return (WrapMode)(int)DirectMessage(2269);
		}
		set
		{
			DirectMessage(2268, new IntPtr((int)value));
		}
	}

	[DefaultValue(0)]
	[Category("Line Wrapping")]
	[Description("The amount of pixels to indent wrapped sublines.")]
	public int WrapStartIndent
	{
		get
		{
			return DirectMessage(2465).ToInt32();
		}
		set
		{
			value = Helpers.ClampMin(value, 0);
			DirectMessage(2464, new IntPtr(value));
		}
	}

	[DefaultValue(WrapVisualFlags.None)]
	[Category("Line Wrapping")]
	[Description("The visual indicator displayed on a wrapped line.")]
	[TypeConverter(typeof(FlagsEnumConverter))]
	public WrapVisualFlags WrapVisualFlags
	{
		get
		{
			return (WrapVisualFlags)(int)DirectMessage(2461);
		}
		set
		{
			DirectMessage(2460, new IntPtr((int)value));
		}
	}

	[DefaultValue(WrapVisualFlagLocation.Default)]
	[Category("Line Wrapping")]
	[Description("The location of wrap visual flags in relation to the line text.")]
	public WrapVisualFlagLocation WrapVisualFlagLocation
	{
		get
		{
			return (WrapVisualFlagLocation)(int)DirectMessage(2463);
		}
		set
		{
			DirectMessage(2462, new IntPtr((int)value));
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int XOffset
	{
		get
		{
			return DirectMessage(2398).ToInt32();
		}
		set
		{
			value = Helpers.ClampMin(value, 0);
			DirectMessage(2397, new IntPtr(value));
		}
	}

	[DefaultValue(0)]
	[Category("Appearance")]
	[Description("Zoom factor in points applied to the displayed text.")]
	public int Zoom
	{
		get
		{
			return DirectMessage(2374).ToInt32();
		}
		set
		{
			DirectMessage(2373, new IntPtr(value));
		}
	}

	[Category("Notifications")]
	[Description("Occurs when an autocompletion list is cancelled.")]
	public event EventHandler<EventArgs> AutoCCancelled
	{
		add
		{
			base.Events.AddHandler(autoCCancelledEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(autoCCancelledEventKey, value);
		}
	}

	[Category("Notifications")]
	[Description("Occurs when the user deletes a character while an autocompletion list is active.")]
	public event EventHandler<EventArgs> AutoCCharDeleted
	{
		add
		{
			base.Events.AddHandler(autoCCharDeletedEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(autoCCharDeletedEventKey, value);
		}
	}

	[Category("Notifications")]
	[Description("Occurs after autocompleted text has been inserted.")]
	public event EventHandler<AutoCSelectionEventArgs> AutoCCompleted
	{
		add
		{
			base.Events.AddHandler(autoCCompletedEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(autoCCompletedEventKey, value);
		}
	}

	[Category("Notifications")]
	[Description("Occurs when a user has selected an item in an autocompletion list.")]
	public event EventHandler<AutoCSelectionEventArgs> AutoCSelection
	{
		add
		{
			base.Events.AddHandler(autoCSelectionEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(autoCSelectionEventKey, value);
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public new event EventHandler BackColorChanged
	{
		add
		{
			base.BackColorChanged += value;
		}
		remove
		{
			base.BackColorChanged -= value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public new event EventHandler BackgroundImageChanged
	{
		add
		{
			base.BackgroundImageChanged += value;
		}
		remove
		{
			base.BackgroundImageChanged -= value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public new event EventHandler BackgroundImageLayoutChanged
	{
		add
		{
			base.BackgroundImageLayoutChanged += value;
		}
		remove
		{
			base.BackgroundImageLayoutChanged -= value;
		}
	}

	[Category("Notifications")]
	[Description("Occurs before text is deleted.")]
	public event EventHandler<BeforeModificationEventArgs> BeforeDelete
	{
		add
		{
			base.Events.AddHandler(beforeDeleteEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(beforeDeleteEventKey, value);
		}
	}

	[Category("Notifications")]
	[Description("Occurs before text is inserted.")]
	public event EventHandler<BeforeModificationEventArgs> BeforeInsert
	{
		add
		{
			base.Events.AddHandler(beforeInsertEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(beforeInsertEventKey, value);
		}
	}

	[Category("Property Changed")]
	[Description("Occurs when the value of the BorderStyle property changes.")]
	public event EventHandler BorderStyleChanged
	{
		add
		{
			base.Events.AddHandler(borderStyleChangedEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(borderStyleChangedEventKey, value);
		}
	}

	[Category("Notifications")]
	[Description("Occurs when an annotation has changed.")]
	public event EventHandler<ChangeAnnotationEventArgs> ChangeAnnotation
	{
		add
		{
			base.Events.AddHandler(changeAnnotationEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(changeAnnotationEventKey, value);
		}
	}

	[Category("Notifications")]
	[Description("Occurs when the user types a character.")]
	public event EventHandler<CharAddedEventArgs> CharAdded
	{
		add
		{
			base.Events.AddHandler(charAddedEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(charAddedEventKey, value);
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public new event EventHandler CursorChanged
	{
		add
		{
			base.CursorChanged += value;
		}
		remove
		{
			base.CursorChanged -= value;
		}
	}

	[Category("Notifications")]
	[Description("Occurs when text is deleted.")]
	public event EventHandler<ModificationEventArgs> Delete
	{
		add
		{
			base.Events.AddHandler(deleteEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(deleteEventKey, value);
		}
	}

	[Category("Notifications")]
	[Description("Occurs when the editor is double clicked.")]
	public new event EventHandler<DoubleClickEventArgs> DoubleClick
	{
		add
		{
			base.Events.AddHandler(doubleClickEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(doubleClickEventKey, value);
		}
	}

	[Category("Notifications")]
	[Description("Occurs when the mouse moves from its dwell start position.")]
	public event EventHandler<DwellEventArgs> DwellEnd
	{
		add
		{
			base.Events.AddHandler(dwellEndEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(dwellEndEventKey, value);
		}
	}

	[Category("Notifications")]
	[Description("Occurs when the mouse is kept in one position (hovers) for a period of time.")]
	public event EventHandler<DwellEventArgs> DwellStart
	{
		add
		{
			base.Events.AddHandler(dwellStartEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(dwellStartEventKey, value);
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public new event EventHandler FontChanged
	{
		add
		{
			base.FontChanged += value;
		}
		remove
		{
			base.FontChanged -= value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public new event EventHandler ForeColorChanged
	{
		add
		{
			base.ForeColorChanged += value;
		}
		remove
		{
			base.ForeColorChanged -= value;
		}
	}

	[Category("Notifications")]
	[Description("Occurs when the user clicks text styled with the hotspot flag.")]
	public event EventHandler<HotspotClickEventArgs> HotspotClick
	{
		add
		{
			base.Events.AddHandler(hotspotClickEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(hotspotClickEventKey, value);
		}
	}

	[Category("Notifications")]
	[Description("Occurs when the user double clicks text styled with the hotspot flag.")]
	public event EventHandler<HotspotClickEventArgs> HotspotDoubleClick
	{
		add
		{
			base.Events.AddHandler(hotspotDoubleClickEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(hotspotDoubleClickEventKey, value);
		}
	}

	[Category("Notifications")]
	[Description("Occurs when the user releases a click on text styled with the hotspot flag.")]
	public event EventHandler<HotspotClickEventArgs> HotspotReleaseClick
	{
		add
		{
			base.Events.AddHandler(hotspotReleaseClickEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(hotspotReleaseClickEventKey, value);
		}
	}

	[Category("Notifications")]
	[Description("Occurs when the user clicks text with an indicator.")]
	public event EventHandler<IndicatorClickEventArgs> IndicatorClick
	{
		add
		{
			base.Events.AddHandler(indicatorClickEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(indicatorClickEventKey, value);
		}
	}

	[Category("Notifications")]
	[Description("Occurs when the user releases a click on text with an indicator.")]
	public event EventHandler<IndicatorReleaseEventArgs> IndicatorRelease
	{
		add
		{
			base.Events.AddHandler(indicatorReleaseEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(indicatorReleaseEventKey, value);
		}
	}

	[Category("Notifications")]
	[Description("Occurs when text is inserted.")]
	public event EventHandler<ModificationEventArgs> Insert
	{
		add
		{
			base.Events.AddHandler(insertEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(insertEventKey, value);
		}
	}

	[Category("Notifications")]
	[Description("Occurs before text is inserted. Permits changing the inserted text.")]
	public event EventHandler<InsertCheckEventArgs> InsertCheck
	{
		add
		{
			base.Events.AddHandler(insertCheckEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(insertCheckEventKey, value);
		}
	}

	[Category("Notifications")]
	[Description("Occurs when the mouse is clicked in a sensitive margin.")]
	public event EventHandler<MarginClickEventArgs> MarginClick
	{
		add
		{
			base.Events.AddHandler(marginClickEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(marginClickEventKey, value);
		}
	}

	[Category("Notifications")]
	[Description("Occurs when the mouse is right-clicked in a sensitive margin.")]
	public event EventHandler<MarginClickEventArgs> MarginRightClick
	{
		add
		{
			base.Events.AddHandler(marginRightClickEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(marginRightClickEventKey, value);
		}
	}

	[Category("Notifications")]
	[Description("Occurs when an attempt is made to change text in read-only mode.")]
	public event EventHandler<EventArgs> ModifyAttempt
	{
		add
		{
			base.Events.AddHandler(modifyAttemptEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(modifyAttemptEventKey, value);
		}
	}

	[Category("Notifications")]
	[Description("Occurs when hidden (folded) text should be shown.")]
	public event EventHandler<NeedShownEventArgs> NeedShown
	{
		add
		{
			base.Events.AddHandler(needShownEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(needShownEventKey, value);
		}
	}

	internal event EventHandler<SCNotificationEventArgs> SCNotification
	{
		add
		{
			base.Events.AddHandler(scNotificationEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(scNotificationEventKey, value);
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public new event PaintEventHandler Paint
	{
		add
		{
			base.Paint += value;
		}
		remove
		{
			base.Paint -= value;
		}
	}

	[Category("Notifications")]
	[Description("Occurs when the control is painted.")]
	public event EventHandler<EventArgs> Painted
	{
		add
		{
			base.Events.AddHandler(paintedEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(paintedEventKey, value);
		}
	}

	[Category("Notifications")]
	[Description("Occurs when a save point is left and the document becomes dirty.")]
	public event EventHandler<EventArgs> SavePointLeft
	{
		add
		{
			base.Events.AddHandler(savePointLeftEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(savePointLeftEventKey, value);
		}
	}

	[Category("Notifications")]
	[Description("Occurs when a save point is reached and the document is no longer dirty.")]
	public event EventHandler<EventArgs> SavePointReached
	{
		add
		{
			base.Events.AddHandler(savePointReachedEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(savePointReachedEventKey, value);
		}
	}

	[Category("Notifications")]
	[Description("Occurs when the text needs styling.")]
	public event EventHandler<StyleNeededEventArgs> StyleNeeded
	{
		add
		{
			base.Events.AddHandler(styleNeededEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(styleNeededEventKey, value);
		}
	}

	[Category("Notifications")]
	[Description("Occurs when the control UI is updated.")]
	public event EventHandler<UpdateUIEventArgs> UpdateUI
	{
		add
		{
			base.Events.AddHandler(updateUIEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(updateUIEventKey, value);
		}
	}

	[Category("Notifications")]
	[Description("Occurs when the control is zoomed.")]
	public event EventHandler<EventArgs> ZoomChanged
	{
		add
		{
			base.Events.AddHandler(zoomChangedEventKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(zoomChangedEventKey, value);
		}
	}

	public void AddRefDocument(Document document)
	{
		IntPtr value = document.Value;
		DirectMessage(2376, IntPtr.Zero, value);
	}

	public void AddSelection(int caret, int anchor)
	{
		int textLength = TextLength;
		caret = Helpers.Clamp(caret, 0, textLength);
		anchor = Helpers.Clamp(anchor, 0, textLength);
		caret = Lines.CharToBytePosition(caret);
		anchor = Lines.CharToBytePosition(anchor);
		DirectMessage(2573, new IntPtr(caret), new IntPtr(anchor));
	}

	public unsafe void AddText(string text)
	{
		byte[] bytes = Helpers.GetBytes(text ?? string.Empty, Encoding, zeroTerminated: false);
		fixed (byte* value = bytes)
		{
			DirectMessage(2001, new IntPtr(bytes.Length), new IntPtr(value));
		}
	}

	public void AnnotationClearAll()
	{
		DirectMessage(2547);
	}

	public unsafe void AppendText(string text)
	{
		byte[] bytes = Helpers.GetBytes(text ?? string.Empty, Encoding, zeroTerminated: false);
		fixed (byte* value = bytes)
		{
			DirectMessage(2282, new IntPtr(bytes.Length), new IntPtr(value));
		}
	}

	public void AssignCmdKey(Keys keyDefinition, Command sciCommand)
	{
		int value = Helpers.TranslateKeys(keyDefinition);
		DirectMessage(2070, new IntPtr(value), new IntPtr((int)sciCommand));
	}

	public void AutoCCancel()
	{
		DirectMessage(2101);
	}

	public void AutoCComplete()
	{
		DirectMessage(2104);
	}

	public unsafe void AutoCSelect(string select)
	{
		fixed (byte* bytes = Helpers.GetBytes(select, Encoding, zeroTerminated: true))
		{
			DirectMessage(2108, IntPtr.Zero, new IntPtr(bytes));
		}
	}

	public unsafe void AutoCSetFillUps(string chars)
	{
		if (chars == null)
		{
			chars = string.Empty;
		}
		if (fillUpChars != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(fillUpChars);
			fillUpChars = IntPtr.Zero;
		}
		int num = Encoding.GetByteCount(chars) + 1;
		IntPtr intPtr = Marshal.AllocHGlobal(num);
		fixed (char* chars2 = chars)
		{
			Encoding.GetBytes(chars2, chars.Length, (byte*)(void*)intPtr, num);
		}
		((sbyte*)(void*)intPtr)[num - 1] = 0;
		fillUpChars = intPtr;
		DirectMessage(2112, IntPtr.Zero, fillUpChars);
	}

	public unsafe void AutoCShow(int lenEntered, string list)
	{
		if (string.IsNullOrEmpty(list))
		{
			return;
		}
		lenEntered = Helpers.ClampMin(lenEntered, 0);
		if (lenEntered > 0)
		{
			int num = DirectMessage(2008).ToInt32();
			int num2 = num;
			for (int i = 0; i < lenEntered; i++)
			{
				num2 = DirectMessage(2670, new IntPtr(num2), new IntPtr(-1)).ToInt32();
			}
			lenEntered = num - num2;
		}
		fixed (byte* bytes = Helpers.GetBytes(list, Encoding, zeroTerminated: true))
		{
			DirectMessage(2100, new IntPtr(lenEntered), new IntPtr(bytes));
		}
	}

	public unsafe void AutoCStops(string chars)
	{
		fixed (byte* bytes = Helpers.GetBytes(chars ?? string.Empty, Encoding.ASCII, zeroTerminated: true))
		{
			DirectMessage(2105, IntPtr.Zero, new IntPtr(bytes));
		}
	}

	public void BeginUndoAction()
	{
		DirectMessage(2078);
	}

	public void BraceBadLight(int position)
	{
		position = Helpers.Clamp(position, -1, TextLength);
		if (position > 0)
		{
			position = Lines.CharToBytePosition(position);
		}
		DirectMessage(2352, new IntPtr(position));
	}

	public void BraceHighlight(int position1, int position2)
	{
		int textLength = TextLength;
		position1 = Helpers.Clamp(position1, -1, textLength);
		if (position1 > 0)
		{
			position1 = Lines.CharToBytePosition(position1);
		}
		position2 = Helpers.Clamp(position2, -1, textLength);
		if (position2 > 0)
		{
			position2 = Lines.CharToBytePosition(position2);
		}
		DirectMessage(2351, new IntPtr(position1), new IntPtr(position2));
	}

	public int BraceMatch(int position)
	{
		position = Helpers.Clamp(position, 0, TextLength);
		position = Lines.CharToBytePosition(position);
		int num = DirectMessage(2353, new IntPtr(position), IntPtr.Zero).ToInt32();
		if (num > 0)
		{
			num = Lines.ByteToCharPosition(num);
		}
		return num;
	}

	public void CallTipCancel()
	{
		DirectMessage(2201);
	}

	public void CallTipSetForeHlt(Color color)
	{
		int value = ColorTranslator.ToWin32(color);
		DirectMessage(2207, new IntPtr(value));
	}

	public unsafe void CallTipSetHlt(int hlStart, int hlEnd)
	{
		hlStart = Helpers.Clamp(hlStart, 0, lastCallTip.Length);
		hlEnd = Helpers.Clamp(hlEnd, 0, lastCallTip.Length);
		fixed (char* ptr = lastCallTip)
		{
			hlEnd = Encoding.GetByteCount(ptr + hlStart, hlEnd - hlStart);
			hlStart = Encoding.GetByteCount(ptr, hlStart);
			hlEnd += hlStart;
		}
		DirectMessage(2204, new IntPtr(hlStart), new IntPtr(hlEnd));
	}

	public void CallTipSetPosition(bool above)
	{
		IntPtr wParam = (above ? new IntPtr(1) : IntPtr.Zero);
		DirectMessage(2213, wParam);
	}

	public unsafe void CallTipShow(int posStart, string definition)
	{
		posStart = Helpers.Clamp(posStart, 0, TextLength);
		if (definition != null)
		{
			lastCallTip = definition;
			posStart = Lines.CharToBytePosition(posStart);
			fixed (byte* bytes = Helpers.GetBytes(definition, Encoding, zeroTerminated: true))
			{
				DirectMessage(2200, new IntPtr(posStart), new IntPtr(bytes));
			}
		}
	}

	public void CallTipTabSize(int tabSize)
	{
		tabSize = Helpers.ClampMin(tabSize, 0);
		DirectMessage(2212, new IntPtr(tabSize));
	}

	public void ChangeLexerState(int startPos, int endPos)
	{
		int textLength = TextLength;
		startPos = Helpers.Clamp(startPos, 0, textLength);
		endPos = Helpers.Clamp(endPos, 0, textLength);
		startPos = Lines.CharToBytePosition(startPos);
		endPos = Lines.CharToBytePosition(endPos);
		DirectMessage(2617, new IntPtr(startPos), new IntPtr(endPos));
	}

	public int CharPositionFromPoint(int x, int y)
	{
		int pos = DirectMessage(2561, new IntPtr(x), new IntPtr(y)).ToInt32();
		return Lines.ByteToCharPosition(pos);
	}

	public int CharPositionFromPointClose(int x, int y)
	{
		int num = DirectMessage(2562, new IntPtr(x), new IntPtr(y)).ToInt32();
		if (num >= 0)
		{
			num = Lines.ByteToCharPosition(num);
		}
		return num;
	}

	public void ChooseCaretX()
	{
		DirectMessage(2399);
	}

	public void Clear()
	{
		DirectMessage(2180);
	}

	public void ClearAll()
	{
		DirectMessage(2004);
	}

	public void ClearCmdKey(Keys keyDefinition)
	{
		int value = Helpers.TranslateKeys(keyDefinition);
		DirectMessage(2071, new IntPtr(value));
	}

	public void ClearAllCmdKeys()
	{
		DirectMessage(2072);
	}

	public void ClearDocumentStyle()
	{
		DirectMessage(2005);
	}

	public void ClearRegisteredImages()
	{
		DirectMessage(2408);
	}

	public void ClearSelections()
	{
		DirectMessage(2571);
	}

	public void Colorize(int startPos, int endPos)
	{
		int textLength = TextLength;
		startPos = Helpers.Clamp(startPos, 0, textLength);
		endPos = Helpers.Clamp(endPos, 0, textLength);
		startPos = Lines.CharToBytePosition(startPos);
		endPos = Lines.CharToBytePosition(endPos);
		DirectMessage(4003, new IntPtr(startPos), new IntPtr(endPos));
	}

	public void ConvertEols(Eol eolMode)
	{
		DirectMessage(2029, new IntPtr((int)eolMode));
	}

	public void Copy()
	{
		DirectMessage(2178);
	}

	public void Copy(CopyFormat format)
	{
		Helpers.Copy(this, format, useSelection: true, allowLine: false, 0, 0);
	}

	public void CopyAllowLine()
	{
		DirectMessage(2519);
	}

	public void CopyAllowLine(CopyFormat format)
	{
		Helpers.Copy(this, format, useSelection: true, allowLine: true, 0, 0);
	}

	public void CopyRange(int start, int end)
	{
		int textLength = TextLength;
		start = Helpers.Clamp(start, 0, textLength);
		end = Helpers.Clamp(end, 0, textLength);
		start = Lines.CharToBytePosition(start);
		end = Lines.CharToBytePosition(end);
		DirectMessage(2419, new IntPtr(start), new IntPtr(end));
	}

	public void CopyRange(int start, int end, CopyFormat format)
	{
		int textLength = TextLength;
		start = Helpers.Clamp(start, 0, textLength);
		end = Helpers.Clamp(end, 0, textLength);
		if (start != end)
		{
			start = Lines.CharToBytePosition(start);
			end = Lines.CharToBytePosition(end);
			Helpers.Copy(this, format, useSelection: false, allowLine: false, start, end);
		}
	}

	public Document CreateDocument()
	{
		IntPtr value = DirectMessage(2375);
		return new Document
		{
			Value = value
		};
	}

	public ILoader CreateLoader(int length)
	{
		length = Helpers.ClampMin(length, 0);
		IntPtr intPtr = DirectMessage(2632, new IntPtr(length));
		if (intPtr == IntPtr.Zero)
		{
			return null;
		}
		return new Loader(intPtr, Encoding);
	}

	public void Cut()
	{
		DirectMessage(2177);
	}

	public void DeleteRange(int position, int length)
	{
		int textLength = TextLength;
		position = Helpers.Clamp(position, 0, textLength);
		length = Helpers.Clamp(length, 0, textLength - position);
		int num = Lines.CharToBytePosition(position);
		int num2 = Lines.CharToBytePosition(position + length);
		DirectMessage(2645, new IntPtr(num), new IntPtr(num2 - num));
	}

	public unsafe string DescribeKeywordSets()
	{
		int num = DirectMessage(4017).ToInt32();
		byte[] array = new byte[num + 1];
		fixed (byte* value = array)
		{
			DirectMessage(4017, IntPtr.Zero, new IntPtr(value));
		}
		return Encoding.ASCII.GetString(array, 0, num);
	}

	public unsafe string DescribeProperty(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return string.Empty;
		}
		fixed (byte* bytes = Helpers.GetBytes(name, Encoding.ASCII, zeroTerminated: true))
		{
			int num = DirectMessage(4016, new IntPtr(bytes), IntPtr.Zero).ToInt32();
			if (num == 0)
			{
				return string.Empty;
			}
			fixed (byte* value = new byte[num + 1])
			{
				DirectMessage(4016, new IntPtr(bytes), new IntPtr(value));
				return Helpers.GetString(new IntPtr(value), num, Encoding.ASCII);
			}
		}
	}

	internal IntPtr DirectMessage(int msg)
	{
		return DirectMessage(msg, IntPtr.Zero, IntPtr.Zero);
	}

	internal IntPtr DirectMessage(int msg, IntPtr wParam)
	{
		return DirectMessage(msg, wParam, IntPtr.Zero);
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public virtual IntPtr DirectMessage(int msg, IntPtr wParam, IntPtr lParam)
	{
		return DirectMessage(SciPointer, msg, wParam, lParam);
	}

	private static IntPtr DirectMessage(IntPtr sciPtr, int msg, IntPtr wParam, IntPtr lParam)
	{
		return directFunction(sciPtr, msg, wParam, lParam);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			if (reparent)
			{
				reparent = false;
				if (base.IsHandleCreated)
				{
					DestroyHandle();
				}
			}
			if (fillUpChars != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(fillUpChars);
				fillUpChars = IntPtr.Zero;
			}
		}
		base.Dispose(disposing);
	}

	public int DocLineFromVisible(int displayLine)
	{
		displayLine = Helpers.Clamp(displayLine, 0, Lines.Count);
		return DirectMessage(2221, new IntPtr(displayLine)).ToInt32();
	}

	public void DropSelection(int selection)
	{
		selection = Helpers.ClampMin(selection, 0);
		DirectMessage(2671, new IntPtr(selection));
	}

	public void EmptyUndoBuffer()
	{
		DirectMessage(2175);
	}

	public void EndUndoAction()
	{
		DirectMessage(2079);
	}

	public void ExecuteCmd(Command sciCommand)
	{
		DirectMessage((int)sciCommand);
	}

	public void FoldAll(FoldAction action)
	{
		DirectMessage(2662, new IntPtr((int)action));
	}

	public void FoldDisplayTextSetStyle(FoldDisplayText style)
	{
		DirectMessage(2701, new IntPtr((int)style));
	}

	public unsafe int GetCharAt(int position)
	{
		position = Helpers.Clamp(position, 0, TextLength);
		position = Lines.CharToBytePosition(position);
		int num = DirectMessage(2670, new IntPtr(position), new IntPtr(1)).ToInt32();
		int num2 = num - position;
		if (num2 <= 1)
		{
			return DirectMessage(2007, new IntPtr(position)).ToInt32();
		}
		fixed (byte* value = new byte[num2 + 1])
		{
			NativeMethods.Sci_TextRange* ptr = stackalloc NativeMethods.Sci_TextRange[1];
			ptr->chrg.cpMin = position;
			ptr->chrg.cpMax = num;
			ptr->lpstrText = new IntPtr(value);
			DirectMessage(2162, IntPtr.Zero, new IntPtr(ptr));
			string text = Helpers.GetString(new IntPtr(value), num2, Encoding);
			return text[0];
		}
	}

	public int GetColumn(int position)
	{
		position = Helpers.Clamp(position, 0, TextLength);
		position = Lines.CharToBytePosition(position);
		return DirectMessage(2129, new IntPtr(position)).ToInt32();
	}

	public int GetEndStyled()
	{
		int pos = DirectMessage(2028).ToInt32();
		return Lines.ByteToCharPosition(pos);
	}

	private static string GetModulePath()
	{
		if (modulePath == null)
		{
			string path = typeof(Scintilla).Assembly.GetName().Version.ToString(3);
			modulePath = Path.Combine(Path.Combine(Path.Combine(Path.Combine(Path.GetTempPath(), "ScintillaNET"), path), (IntPtr.Size == 4) ? "x86" : "x64"), "SciLexer.dll");
			if (!File.Exists(modulePath))
			{
				string text = ((GuidAttribute)typeof(Scintilla).Assembly.GetCustomAttributes(typeof(GuidAttribute), inherit: false).GetValue(0)).Value.ToString();
				string name = string.Format(CultureInfo.InvariantCulture, "Global\\{{{0}}}", new object[1] { text });
				using Mutex mutex = new Mutex(initiallyOwned: false, name);
				MutexAccessRule rule = new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow);
				MutexSecurity mutexSecurity = new MutexSecurity();
				mutexSecurity.AddAccessRule(rule);
				mutex.SetAccessControl(mutexSecurity);
				bool flag = false;
				try
				{
					try
					{
						flag = mutex.WaitOne(5000, exitContext: false);
						if (!flag)
						{
							string message = string.Format(CultureInfo.InvariantCulture, "Timeout waiting for exclusive access to '{0}'.", new object[1] { modulePath });
							throw new TimeoutException(message);
						}
					}
					catch (AbandonedMutexException)
					{
						flag = true;
					}
					if (!File.Exists(modulePath))
					{
						string directoryName = Path.GetDirectoryName(modulePath);
						if (!Directory.Exists(directoryName))
						{
							Directory.CreateDirectory(directoryName);
						}
						string name2 = string.Format(CultureInfo.InvariantCulture, "ScintillaNET.{0}.SciLexer.dll.gz", new object[1] { (IntPtr.Size == 4) ? "x86" : "x64" });
						using Stream stream = typeof(Scintilla).Assembly.GetManifestResourceStream(name2);
						using GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress);
						using FileStream destination = File.Create(modulePath);
						gZipStream.CopyTo(destination);
					}
				}
				finally
				{
					if (flag)
					{
						mutex.ReleaseMutex();
					}
				}
			}
		}
		return modulePath;
	}

	public unsafe string GetProperty(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return string.Empty;
		}
		fixed (byte* bytes = Helpers.GetBytes(name, Encoding.ASCII, zeroTerminated: true))
		{
			int num = DirectMessage(4008, new IntPtr(bytes)).ToInt32();
			if (num == 0)
			{
				return string.Empty;
			}
			fixed (byte* value = new byte[num + 1])
			{
				DirectMessage(4008, new IntPtr(bytes), new IntPtr(value));
				return Helpers.GetString(new IntPtr(value), num, Encoding.ASCII);
			}
		}
	}

	public unsafe string GetPropertyExpanded(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return string.Empty;
		}
		fixed (byte* bytes = Helpers.GetBytes(name, Encoding.ASCII, zeroTerminated: true))
		{
			int num = DirectMessage(4009, new IntPtr(bytes)).ToInt32();
			if (num == 0)
			{
				return string.Empty;
			}
			fixed (byte* value = new byte[num + 1])
			{
				DirectMessage(4009, new IntPtr(bytes), new IntPtr(value));
				return Helpers.GetString(new IntPtr(value), num, Encoding.ASCII);
			}
		}
	}

	public unsafe int GetPropertyInt(string name, int defaultValue)
	{
		if (string.IsNullOrEmpty(name))
		{
			return defaultValue;
		}
		fixed (byte* bytes = Helpers.GetBytes(name, Encoding.ASCII, zeroTerminated: true))
		{
			return DirectMessage(4010, new IntPtr(bytes), new IntPtr(defaultValue)).ToInt32();
		}
	}

	public int GetStyleAt(int position)
	{
		position = Helpers.Clamp(position, 0, TextLength);
		position = Lines.CharToBytePosition(position);
		return DirectMessage(2010, new IntPtr(position)).ToInt32();
	}

	public unsafe string GetTag(int tagNumber)
	{
		tagNumber = Helpers.Clamp(tagNumber, 1, 9);
		int num = DirectMessage(2616, new IntPtr(tagNumber), IntPtr.Zero).ToInt32();
		if (num <= 0)
		{
			return string.Empty;
		}
		fixed (byte* value = new byte[num + 1])
		{
			DirectMessage(2616, new IntPtr(tagNumber), new IntPtr(value));
			return Helpers.GetString(new IntPtr(value), num, Encoding);
		}
	}

	public string GetTextRange(int position, int length)
	{
		int textLength = TextLength;
		position = Helpers.Clamp(position, 0, textLength);
		length = Helpers.Clamp(length, 0, textLength - position);
		int num = Lines.CharToBytePosition(position);
		int num2 = Lines.CharToBytePosition(position + length);
		IntPtr intPtr = DirectMessage(2643, new IntPtr(num), new IntPtr(num2 - num));
		if (intPtr == IntPtr.Zero)
		{
			return string.Empty;
		}
		return Helpers.GetString(intPtr, num2 - num, Encoding);
	}

	public string GetTextRangeAsHtml(int position, int length)
	{
		int textLength = TextLength;
		position = Helpers.Clamp(position, 0, textLength);
		length = Helpers.Clamp(length, 0, textLength - position);
		int startBytePos = Lines.CharToBytePosition(position);
		int endBytePos = Lines.CharToBytePosition(position + length);
		return Helpers.GetHtml(this, startBytePos, endBytePos);
	}

	public FileVersionInfo GetVersionInfo()
	{
		string fileName = GetModulePath();
		return FileVersionInfo.GetVersionInfo(fileName);
	}

	public string GetWordFromPosition(int position)
	{
		int num = WordStartPosition(position, onlyWordCharacters: true);
		int num2 = WordEndPosition(position, onlyWordCharacters: true);
		return GetTextRange(num, num2 - num);
	}

	public void GotoPosition(int position)
	{
		position = Helpers.Clamp(position, 0, TextLength);
		position = Lines.CharToBytePosition(position);
		DirectMessage(2025, new IntPtr(position));
	}

	public void HideLines(int lineStart, int lineEnd)
	{
		lineStart = Helpers.Clamp(lineStart, 0, Lines.Count);
		lineEnd = Helpers.Clamp(lineEnd, lineStart, Lines.Count);
		DirectMessage(2227, new IntPtr(lineStart), new IntPtr(lineEnd));
	}

	public uint IndicatorAllOnFor(int position)
	{
		position = Helpers.Clamp(position, 0, TextLength);
		position = Lines.CharToBytePosition(position);
		return (uint)DirectMessage(2506, new IntPtr(position)).ToInt32();
	}

	public void IndicatorClearRange(int position, int length)
	{
		int textLength = TextLength;
		position = Helpers.Clamp(position, 0, textLength);
		length = Helpers.Clamp(length, 0, textLength - position);
		int num = Lines.CharToBytePosition(position);
		int num2 = Lines.CharToBytePosition(position + length);
		DirectMessage(2505, new IntPtr(num), new IntPtr(num2 - num));
	}

	public void IndicatorFillRange(int position, int length)
	{
		int textLength = TextLength;
		position = Helpers.Clamp(position, 0, textLength);
		length = Helpers.Clamp(length, 0, textLength - position);
		int num = Lines.CharToBytePosition(position);
		int num2 = Lines.CharToBytePosition(position + length);
		DirectMessage(2504, new IntPtr(num), new IntPtr(num2 - num));
	}

	private void InitDocument(Eol eolMode = Eol.CrLf, bool useTabs = false, int tabWidth = 4, int indentWidth = 0)
	{
		DirectMessage(2037, new IntPtr(65001));
		DirectMessage(2012, new IntPtr(1));
		DirectMessage(2031, new IntPtr((int)eolMode));
		DirectMessage(2124, useTabs ? new IntPtr(1) : IntPtr.Zero);
		DirectMessage(2036, new IntPtr(tabWidth));
		DirectMessage(2122, new IntPtr(indentWidth));
	}

	public unsafe void InsertText(int position, string text)
	{
		if (position < -1)
		{
			throw new ArgumentOutOfRangeException("position", "Position must be greater or equal to zero, or -1.");
		}
		if (position != -1)
		{
			int textLength = TextLength;
			if (position > textLength)
			{
				throw new ArgumentOutOfRangeException("position", "Position cannot exceed document length.");
			}
			position = Lines.CharToBytePosition(position);
		}
		fixed (byte* bytes = Helpers.GetBytes(text ?? string.Empty, Encoding, zeroTerminated: true))
		{
			DirectMessage(2003, new IntPtr(position), new IntPtr(bytes));
		}
	}

	public bool IsRangeWord(int start, int end)
	{
		int textLength = TextLength;
		start = Helpers.Clamp(start, 0, textLength);
		end = Helpers.Clamp(end, 0, textLength);
		start = Lines.CharToBytePosition(start);
		end = Lines.CharToBytePosition(end);
		return DirectMessage(2691, new IntPtr(start), new IntPtr(end)) != IntPtr.Zero;
	}

	public int LineFromPosition(int position)
	{
		position = Helpers.Clamp(position, 0, TextLength);
		return Lines.LineFromCharPosition(position);
	}

	public void LineScroll(int lines, int columns)
	{
		DirectMessage(2168, new IntPtr(columns), new IntPtr(lines));
	}

	public unsafe void LoadLexerLibrary(string path)
	{
		if (!string.IsNullOrEmpty(path))
		{
			fixed (byte* bytes = Helpers.GetBytes(path, Encoding.Default, zeroTerminated: true))
			{
				DirectMessage(4007, IntPtr.Zero, new IntPtr(bytes));
			}
		}
	}

	public void MarkerDeleteAll(int marker)
	{
		marker = Helpers.Clamp(marker, -1, Markers.Count - 1);
		DirectMessage(2045, new IntPtr(marker));
	}

	public void MarkerDeleteHandle(MarkerHandle markerHandle)
	{
		DirectMessage(2018, markerHandle.Value);
	}

	public void MarkerEnableHighlight(bool enabled)
	{
		IntPtr wParam = (enabled ? new IntPtr(1) : IntPtr.Zero);
		DirectMessage(2293, wParam);
	}

	public int MarkerLineFromHandle(MarkerHandle markerHandle)
	{
		return DirectMessage(2017, markerHandle.Value).ToInt32();
	}

	public void MultiEdgeAddLine(int column, Color edgeColor)
	{
		column = Helpers.ClampMin(column, 0);
		int value = ColorTranslator.ToWin32(edgeColor);
		DirectMessage(2694, new IntPtr(column), new IntPtr(value));
	}

	public void MultiEdgeClearAll()
	{
		DirectMessage(2695);
	}

	public void MultipleSelectAddEach()
	{
		DirectMessage(2689);
	}

	public void MultipleSelectAddNext()
	{
		DirectMessage(2688);
	}

	protected virtual void OnAutoCCancelled(EventArgs e)
	{
		if (base.Events[autoCCancelledEventKey] is EventHandler<EventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnAutoCCharDeleted(EventArgs e)
	{
		if (base.Events[autoCCharDeletedEventKey] is EventHandler<EventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnAutoCCompleted(AutoCSelectionEventArgs e)
	{
		if (base.Events[autoCCompletedEventKey] is EventHandler<AutoCSelectionEventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnAutoCSelection(AutoCSelectionEventArgs e)
	{
		if (base.Events[autoCSelectionEventKey] is EventHandler<AutoCSelectionEventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnBeforeDelete(BeforeModificationEventArgs e)
	{
		if (base.Events[beforeDeleteEventKey] is EventHandler<BeforeModificationEventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnBeforeInsert(BeforeModificationEventArgs e)
	{
		if (base.Events[beforeInsertEventKey] is EventHandler<BeforeModificationEventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnBorderStyleChanged(EventArgs e)
	{
		if (base.Events[borderStyleChangedEventKey] is EventHandler eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnChangeAnnotation(ChangeAnnotationEventArgs e)
	{
		if (base.Events[changeAnnotationEventKey] is EventHandler<ChangeAnnotationEventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnCharAdded(CharAddedEventArgs e)
	{
		if (base.Events[charAddedEventKey] is EventHandler<CharAddedEventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnDelete(ModificationEventArgs e)
	{
		if (base.Events[deleteEventKey] is EventHandler<ModificationEventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnDoubleClick(DoubleClickEventArgs e)
	{
		if (base.Events[doubleClickEventKey] is EventHandler<DoubleClickEventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnDwellEnd(DwellEventArgs e)
	{
		if (base.Events[dwellEndEventKey] is EventHandler<DwellEventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnDwellStart(DwellEventArgs e)
	{
		if (base.Events[dwellStartEventKey] is EventHandler<DwellEventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected unsafe override void OnHandleCreated(EventArgs e)
	{
		InitDocument();
		DirectMessage(2516, new IntPtr(1));
		DirectMessage(2212, new IntPtr(16));
		fixed (byte* bytes = Helpers.GetBytes("abcdefghijklmnopqrstuvwxyz_ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", Encoding.ASCII, zeroTerminated: true))
		{
			DirectMessage(2077, IntPtr.Zero, new IntPtr(bytes));
		}
		NativeMethods.RevokeDragDrop(base.Handle);
		base.OnHandleCreated(e);
	}

	protected virtual void OnHotspotClick(HotspotClickEventArgs e)
	{
		if (base.Events[hotspotClickEventKey] is EventHandler<HotspotClickEventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnHotspotDoubleClick(HotspotClickEventArgs e)
	{
		if (base.Events[hotspotDoubleClickEventKey] is EventHandler<HotspotClickEventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnHotspotReleaseClick(HotspotClickEventArgs e)
	{
		if (base.Events[hotspotReleaseClickEventKey] is EventHandler<HotspotClickEventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnIndicatorClick(IndicatorClickEventArgs e)
	{
		if (base.Events[indicatorClickEventKey] is EventHandler<IndicatorClickEventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnIndicatorRelease(IndicatorReleaseEventArgs e)
	{
		if (base.Events[indicatorReleaseEventKey] is EventHandler<IndicatorReleaseEventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnInsert(ModificationEventArgs e)
	{
		if (base.Events[insertEventKey] is EventHandler<ModificationEventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnInsertCheck(InsertCheckEventArgs e)
	{
		if (base.Events[insertCheckEventKey] is EventHandler<InsertCheckEventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnMarginClick(MarginClickEventArgs e)
	{
		if (base.Events[marginClickEventKey] is EventHandler<MarginClickEventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnMarginRightClick(MarginClickEventArgs e)
	{
		if (base.Events[marginRightClickEventKey] is EventHandler<MarginClickEventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnModifyAttempt(EventArgs e)
	{
		if (base.Events[modifyAttemptEventKey] is EventHandler<EventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (!doubleClick)
		{
			OnClick(e);
			OnMouseClick(e);
		}
		else
		{
			MouseEventArgs e2 = new MouseEventArgs(e.Button, 2, e.X, e.Y, e.Delta);
			OnDoubleClick(e2);
			OnMouseDoubleClick(e2);
			doubleClick = false;
		}
		base.OnMouseUp(e);
	}

	protected virtual void OnNeedShown(NeedShownEventArgs e)
	{
		if (base.Events[needShownEventKey] is EventHandler<NeedShownEventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnPainted(EventArgs e)
	{
		if (base.Events[paintedEventKey] is EventHandler<EventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnSavePointLeft(EventArgs e)
	{
		if (base.Events[savePointLeftEventKey] is EventHandler<EventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnSavePointReached(EventArgs e)
	{
		if (base.Events[savePointReachedEventKey] is EventHandler<EventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnStyleNeeded(StyleNeededEventArgs e)
	{
		if (base.Events[styleNeededEventKey] is EventHandler<StyleNeededEventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnUpdateUI(UpdateUIEventArgs e)
	{
		if (base.Events[updateUIEventKey] is EventHandler<UpdateUIEventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	protected virtual void OnZoomChanged(EventArgs e)
	{
		if (base.Events[zoomChangedEventKey] is EventHandler<EventArgs> eventHandler)
		{
			eventHandler(this, e);
		}
	}

	public void Paste()
	{
		DirectMessage(2179);
	}

	public int PointXFromPosition(int pos)
	{
		pos = Helpers.Clamp(pos, 0, TextLength);
		pos = Lines.CharToBytePosition(pos);
		return DirectMessage(2164, IntPtr.Zero, new IntPtr(pos)).ToInt32();
	}

	public int PointYFromPosition(int pos)
	{
		pos = Helpers.Clamp(pos, 0, TextLength);
		pos = Lines.CharToBytePosition(pos);
		return DirectMessage(2165, IntPtr.Zero, new IntPtr(pos)).ToInt32();
	}

	public unsafe string PropertyNames()
	{
		int num = DirectMessage(4014).ToInt32();
		if (num == 0)
		{
			return string.Empty;
		}
		fixed (byte* value = new byte[num + 1])
		{
			DirectMessage(4014, IntPtr.Zero, new IntPtr(value));
			return Helpers.GetString(new IntPtr(value), num, Encoding.ASCII);
		}
	}

	public unsafe PropertyType PropertyType(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return ScintillaNET.PropertyType.Boolean;
		}
		fixed (byte* bytes = Helpers.GetBytes(name, Encoding.ASCII, zeroTerminated: true))
		{
			return (PropertyType)(int)DirectMessage(4015, new IntPtr(bytes));
		}
	}

	public void Redo()
	{
		DirectMessage(2011);
	}

	public unsafe void RegisterRgbaImage(int type, Bitmap image)
	{
		if (image != null)
		{
			DirectMessage(2624, new IntPtr(image.Width));
			DirectMessage(2625, new IntPtr(image.Height));
			fixed (byte* value = Helpers.BitmapToArgb(image))
			{
				DirectMessage(2627, new IntPtr(type), new IntPtr(value));
			}
		}
	}

	public void ReleaseDocument(Document document)
	{
		IntPtr value = document.Value;
		DirectMessage(2377, IntPtr.Zero, value);
	}

	public unsafe void ReplaceSelection(string text)
	{
		fixed (byte* bytes = Helpers.GetBytes(text ?? string.Empty, Encoding, zeroTerminated: true))
		{
			DirectMessage(2170, IntPtr.Zero, new IntPtr(bytes));
		}
	}

	public unsafe int ReplaceTarget(string text)
	{
		if (text == null)
		{
			text = string.Empty;
		}
		byte[] bytes = Helpers.GetBytes(text, Encoding, zeroTerminated: false);
		fixed (byte* value = bytes)
		{
			DirectMessage(2194, new IntPtr(bytes.Length), new IntPtr(value));
		}
		return text.Length;
	}

	public unsafe int ReplaceTargetRe(string text)
	{
		byte[] bytes = Helpers.GetBytes(text ?? string.Empty, Encoding, zeroTerminated: false);
		fixed (byte* value = bytes)
		{
			DirectMessage(2195, new IntPtr(bytes.Length), new IntPtr(value));
		}
		return Math.Abs(TargetEnd - TargetStart);
	}

	private void ResetAdditionalCaretForeColor()
	{
		AdditionalCaretForeColor = Color.FromArgb(127, 127, 127);
	}

	public void RotateSelection()
	{
		DirectMessage(2606);
	}

	private void ScnDoubleClick(ref NativeMethods.SCNotification scn)
	{
		Keys modifiers = (Keys)(-65536 & (scn.modifiers << 16));
		DoubleClickEventArgs e = new DoubleClickEventArgs(this, modifiers, scn.position, scn.line);
		OnDoubleClick(e);
	}

	private void ScnHotspotClick(ref NativeMethods.SCNotification scn)
	{
		Keys modifiers = (Keys)(-65536 & (scn.modifiers << 16));
		HotspotClickEventArgs e = new HotspotClickEventArgs(this, modifiers, scn.position);
		switch (scn.nmhdr.code)
		{
		case 2019:
			OnHotspotClick(e);
			break;
		case 2020:
			OnHotspotDoubleClick(e);
			break;
		case 2027:
			OnHotspotReleaseClick(e);
			break;
		}
	}

	private void ScnIndicatorClick(ref NativeMethods.SCNotification scn)
	{
		switch (scn.nmhdr.code)
		{
		case 2023:
		{
			Keys modifiers = (Keys)(-65536 & (scn.modifiers << 16));
			OnIndicatorClick(new IndicatorClickEventArgs(this, modifiers, scn.position));
			break;
		}
		case 2024:
			OnIndicatorRelease(new IndicatorReleaseEventArgs(this, scn.position));
			break;
		}
	}

	private void ScnMarginClick(ref NativeMethods.SCNotification scn)
	{
		Keys modifiers = (Keys)(-65536 & (scn.modifiers << 16));
		MarginClickEventArgs e = new MarginClickEventArgs(this, modifiers, scn.position, scn.margin);
		if (scn.nmhdr.code == 2010)
		{
			OnMarginClick(e);
		}
		else
		{
			OnMarginRightClick(e);
		}
	}

	private void ScnModified(ref NativeMethods.SCNotification scn)
	{
		if ((scn.modificationType & 0x100000) > 0)
		{
			InsertCheckEventArgs e = new InsertCheckEventArgs(this, scn.position, scn.length, scn.text);
			OnInsertCheck(e);
			cachedPosition = e.CachedPosition;
			cachedText = e.CachedText;
		}
		if ((scn.modificationType & 0xC00) > 0)
		{
			ModificationSource source = (ModificationSource)(scn.modificationType & 0x70);
			BeforeModificationEventArgs e2 = new BeforeModificationEventArgs(this, source, scn.position, scn.length, scn.text);
			e2.CachedPosition = cachedPosition;
			e2.CachedText = cachedText;
			if ((scn.modificationType & 0x400) > 0)
			{
				OnBeforeInsert(e2);
			}
			else
			{
				OnBeforeDelete(e2);
			}
			cachedPosition = e2.CachedPosition;
			cachedText = e2.CachedText;
		}
		if ((scn.modificationType & 3) > 0)
		{
			ModificationSource source2 = (ModificationSource)(scn.modificationType & 0x70);
			ModificationEventArgs e3 = new ModificationEventArgs(this, source2, scn.position, scn.length, scn.text, scn.linesAdded);
			e3.CachedPosition = cachedPosition;
			e3.CachedText = cachedText;
			if ((scn.modificationType & 1) > 0)
			{
				OnInsert(e3);
			}
			else
			{
				OnDelete(e3);
			}
			cachedPosition = null;
			cachedText = null;
			OnTextChanged(EventArgs.Empty);
		}
		if ((scn.modificationType & 0x20000) > 0)
		{
			ChangeAnnotationEventArgs e4 = new ChangeAnnotationEventArgs(scn.line);
			OnChangeAnnotation(e4);
		}
	}

	public void ScrollCaret()
	{
		DirectMessage(2169);
	}

	public void ScrollRange(int start, int end)
	{
		int textLength = TextLength;
		start = Helpers.Clamp(start, 0, textLength);
		end = Helpers.Clamp(end, 0, textLength);
		start = Lines.CharToBytePosition(start);
		end = Lines.CharToBytePosition(end);
		DirectMessage(2569, new IntPtr(start), new IntPtr(end));
	}

	public unsafe int SearchInTarget(string text)
	{
		int num = 0;
		byte[] bytes = Helpers.GetBytes(text ?? string.Empty, Encoding, zeroTerminated: false);
		fixed (byte* value = bytes)
		{
			num = DirectMessage(2197, new IntPtr(bytes.Length), new IntPtr(value)).ToInt32();
		}
		if (num == -1)
		{
			return num;
		}
		return Lines.ByteToCharPosition(num);
	}

	public void SelectAll()
	{
		DirectMessage(2013);
	}

	public void SetAdditionalSelBack(Color color)
	{
		int value = ColorTranslator.ToWin32(color);
		DirectMessage(2601, new IntPtr(value));
	}

	public void SetAdditionalSelFore(Color color)
	{
		int value = ColorTranslator.ToWin32(color);
		DirectMessage(2600, new IntPtr(value));
	}

	public void SetEmptySelection(int pos)
	{
		pos = Helpers.Clamp(pos, 0, TextLength);
		pos = Lines.CharToBytePosition(pos);
		DirectMessage(2556, new IntPtr(pos));
	}

	public void SetFoldFlags(FoldFlags flags)
	{
		DirectMessage(2233, new IntPtr((int)flags));
	}

	public void SetFoldMarginColor(bool use, Color color)
	{
		int value = ColorTranslator.ToWin32(color);
		IntPtr wParam = (use ? new IntPtr(1) : IntPtr.Zero);
		DirectMessage(2290, wParam, new IntPtr(value));
	}

	public void SetFoldMarginHighlightColor(bool use, Color color)
	{
		int value = ColorTranslator.ToWin32(color);
		IntPtr wParam = (use ? new IntPtr(1) : IntPtr.Zero);
		DirectMessage(2291, wParam, new IntPtr(value));
	}

	public unsafe void SetKeywords(int set, string keywords)
	{
		set = Helpers.Clamp(set, 0, 8);
		fixed (byte* bytes = Helpers.GetBytes(keywords ?? string.Empty, Encoding.ASCII, zeroTerminated: true))
		{
			DirectMessage(4005, new IntPtr(set), new IntPtr(bytes));
		}
	}

	public static void SetDestroyHandleBehavior(bool reparent)
	{
		if (!reparentAll.HasValue)
		{
			reparentAll = reparent;
		}
	}

	public static void SetModulePath(string modulePath)
	{
		if (Scintilla.modulePath == null)
		{
			Scintilla.modulePath = modulePath;
		}
	}

	public unsafe void SetProperty(string name, string value)
	{
		if (string.IsNullOrEmpty(name))
		{
			return;
		}
		byte[] bytes = Helpers.GetBytes(name, Encoding.ASCII, zeroTerminated: true);
		byte[] bytes2 = Helpers.GetBytes(value ?? string.Empty, Encoding.ASCII, zeroTerminated: true);
		fixed (byte* value2 = bytes)
		{
			fixed (byte* value3 = bytes2)
			{
				DirectMessage(4004, new IntPtr(value2), new IntPtr(value3));
			}
		}
	}

	public void SetSavePoint()
	{
		DirectMessage(2014);
	}

	public void SetSel(int anchorPos, int currentPos)
	{
		if (anchorPos == currentPos)
		{
			anchorPos = -1;
		}
		int textLength = TextLength;
		if (anchorPos >= 0)
		{
			anchorPos = Helpers.Clamp(anchorPos, 0, textLength);
			anchorPos = Lines.CharToBytePosition(anchorPos);
		}
		if (currentPos >= 0)
		{
			currentPos = Helpers.Clamp(currentPos, 0, textLength);
			currentPos = Lines.CharToBytePosition(currentPos);
		}
		DirectMessage(2160, new IntPtr(anchorPos), new IntPtr(currentPos));
	}

	public void SetSelection(int caret, int anchor)
	{
		int textLength = TextLength;
		caret = Helpers.Clamp(caret, 0, textLength);
		anchor = Helpers.Clamp(anchor, 0, textLength);
		caret = Lines.CharToBytePosition(caret);
		anchor = Lines.CharToBytePosition(anchor);
		DirectMessage(2572, new IntPtr(caret), new IntPtr(anchor));
	}

	public void SetSelectionBackColor(bool use, Color color)
	{
		int value = ColorTranslator.ToWin32(color);
		IntPtr wParam = (use ? new IntPtr(1) : IntPtr.Zero);
		DirectMessage(2068, wParam, new IntPtr(value));
	}

	public void SetSelectionForeColor(bool use, Color color)
	{
		int value = ColorTranslator.ToWin32(color);
		IntPtr wParam = (use ? new IntPtr(1) : IntPtr.Zero);
		DirectMessage(2067, wParam, new IntPtr(value));
	}

	public void SetStyling(int length, int style)
	{
		int textLength = TextLength;
		if (length < 0)
		{
			throw new ArgumentOutOfRangeException("length", "Length cannot be less than zero.");
		}
		if (stylingPosition + length > textLength)
		{
			throw new ArgumentOutOfRangeException("length", "Position and length must refer to a range within the document.");
		}
		if (style < 0 || style >= Styles.Count)
		{
			throw new ArgumentOutOfRangeException("style", "Style must be non-negative and less than the size of the collection.");
		}
		int pos = stylingPosition + length;
		int num = Lines.CharToBytePosition(pos);
		DirectMessage(2033, new IntPtr(num - stylingBytePosition), new IntPtr(style));
		stylingPosition = pos;
		stylingBytePosition = num;
	}

	public void SetTargetRange(int start, int end)
	{
		int textLength = TextLength;
		start = Helpers.Clamp(start, 0, textLength);
		end = Helpers.Clamp(end, 0, textLength);
		start = Lines.CharToBytePosition(start);
		end = Lines.CharToBytePosition(end);
		DirectMessage(2686, new IntPtr(start), new IntPtr(end));
	}

	public void SetWhitespaceBackColor(bool use, Color color)
	{
		int value = ColorTranslator.ToWin32(color);
		IntPtr wParam = (use ? new IntPtr(1) : IntPtr.Zero);
		DirectMessage(2085, wParam, new IntPtr(value));
	}

	public void SetWhitespaceForeColor(bool use, Color color)
	{
		int value = ColorTranslator.ToWin32(color);
		IntPtr wParam = (use ? new IntPtr(1) : IntPtr.Zero);
		DirectMessage(2084, wParam, new IntPtr(value));
	}

	private bool ShouldSerializeAdditionalCaretForeColor()
	{
		return AdditionalCaretForeColor != Color.FromArgb(127, 127, 127);
	}

	public void ShowLines(int lineStart, int lineEnd)
	{
		lineStart = Helpers.Clamp(lineStart, 0, Lines.Count);
		lineEnd = Helpers.Clamp(lineEnd, lineStart, Lines.Count);
		DirectMessage(2226, new IntPtr(lineStart), new IntPtr(lineEnd));
	}

	public void StartStyling(int position)
	{
		position = Helpers.Clamp(position, 0, TextLength);
		int value = Lines.CharToBytePosition(position);
		DirectMessage(2032, new IntPtr(value));
		stylingPosition = position;
		stylingBytePosition = value;
	}

	public void StyleClearAll()
	{
		DirectMessage(2050);
	}

	public void StyleResetDefault()
	{
		DirectMessage(2058);
	}

	public void SwapMainAnchorCaret()
	{
		DirectMessage(2607);
	}

	public void TargetFromSelection()
	{
		DirectMessage(2287);
	}

	public void TargetWholeDocument()
	{
		DirectMessage(2690);
	}

	public unsafe int TextWidth(int style, string text)
	{
		style = Helpers.Clamp(style, 0, Styles.Count - 1);
		fixed (byte* bytes = Helpers.GetBytes(text ?? string.Empty, Encoding, zeroTerminated: true))
		{
			return DirectMessage(2276, new IntPtr(style), new IntPtr(bytes)).ToInt32();
		}
	}

	public void Undo()
	{
		DirectMessage(2176);
	}

	public void UsePopup(bool enablePopup)
	{
		IntPtr wParam = (enablePopup ? new IntPtr(1) : IntPtr.Zero);
		DirectMessage(2371, wParam);
	}

	public void UsePopup(PopupMode popupMode)
	{
		DirectMessage(2371, new IntPtr((int)popupMode));
	}

	private void WmDestroy(ref Message m)
	{
		if (reparent && base.IsHandleCreated)
		{
			NativeMethods.SetParent(base.Handle, new IntPtr(-3));
			m.Result = IntPtr.Zero;
		}
		else
		{
			base.WndProc(ref m);
		}
	}

	private void WmReflectNotify(ref Message m)
	{
		NativeMethods.SCNotification scn = (NativeMethods.SCNotification)Marshal.PtrToStructure(m.LParam, typeof(NativeMethods.SCNotification));
		if (scn.nmhdr.code >= 2000 && scn.nmhdr.code <= 2030)
		{
			if (base.Events[scNotificationEventKey] is EventHandler<SCNotificationEventArgs> eventHandler)
			{
				eventHandler(this, new SCNotificationEventArgs(scn));
			}
			switch (scn.nmhdr.code)
			{
			case 2013:
				OnPainted(EventArgs.Empty);
				break;
			case 2008:
				ScnModified(ref scn);
				break;
			case 2004:
				OnModifyAttempt(EventArgs.Empty);
				break;
			case 2000:
				OnStyleNeeded(new StyleNeededEventArgs(this, scn.position));
				break;
			case 2003:
				OnSavePointLeft(EventArgs.Empty);
				break;
			case 2002:
				OnSavePointReached(EventArgs.Empty);
				break;
			case 2010:
			case 2031:
				ScnMarginClick(ref scn);
				break;
			case 2007:
				OnUpdateUI(new UpdateUIEventArgs((UpdateChange)scn.updated));
				break;
			case 2001:
				OnCharAdded(new CharAddedEventArgs(scn.ch));
				break;
			case 2022:
				OnAutoCSelection(new AutoCSelectionEventArgs(this, scn.position, scn.text, scn.ch, (ListCompletionMethod)scn.listCompletionMethod));
				break;
			case 2030:
				OnAutoCCompleted(new AutoCSelectionEventArgs(this, scn.position, scn.text, scn.ch, (ListCompletionMethod)scn.listCompletionMethod));
				break;
			case 2025:
				OnAutoCCancelled(EventArgs.Empty);
				break;
			case 2026:
				OnAutoCCharDeleted(EventArgs.Empty);
				break;
			case 2016:
				OnDwellStart(new DwellEventArgs(this, scn.position, scn.x, scn.y));
				break;
			case 2017:
				OnDwellEnd(new DwellEventArgs(this, scn.position, scn.x, scn.y));
				break;
			case 2006:
				ScnDoubleClick(ref scn);
				break;
			case 2011:
				OnNeedShown(new NeedShownEventArgs(this, scn.position, scn.length));
				break;
			case 2019:
			case 2020:
			case 2027:
				ScnHotspotClick(ref scn);
				break;
			case 2023:
			case 2024:
				ScnIndicatorClick(ref scn);
				break;
			case 2018:
				OnZoomChanged(EventArgs.Empty);
				break;
			default:
				base.WndProc(ref m);
				break;
			}
		}
	}

	protected override void WndProc(ref Message m)
	{
		switch (m.Msg)
		{
		case 8270:
			WmReflectNotify(ref m);
			return;
		case 32:
			DefWndProc(ref m);
			return;
		case 515:
		case 518:
		case 521:
		case 525:
			doubleClick = true;
			break;
		case 2:
			WmDestroy(ref m);
			return;
		}
		base.WndProc(ref m);
	}

	public int WordEndPosition(int position, bool onlyWordCharacters)
	{
		IntPtr lParam = (onlyWordCharacters ? new IntPtr(1) : IntPtr.Zero);
		position = Helpers.Clamp(position, 0, TextLength);
		position = Lines.CharToBytePosition(position);
		position = DirectMessage(2267, new IntPtr(position), lParam).ToInt32();
		return Lines.ByteToCharPosition(position);
	}

	public int WordStartPosition(int position, bool onlyWordCharacters)
	{
		IntPtr lParam = (onlyWordCharacters ? new IntPtr(1) : IntPtr.Zero);
		position = Helpers.Clamp(position, 0, TextLength);
		position = Lines.CharToBytePosition(position);
		position = DirectMessage(2266, new IntPtr(position), lParam).ToInt32();
		return Lines.ByteToCharPosition(position);
	}

	public void ZoomIn()
	{
		DirectMessage(2333);
	}

	public void ZoomOut()
	{
		DirectMessage(2334);
	}

	public Scintilla()
	{
		if (!reparentAll.HasValue || reparentAll.Value)
		{
			reparent = true;
		}
		SetStyle(ControlStyles.CacheText, value: true);
		SetStyle(ControlStyles.UserPaint | ControlStyles.StandardClick | ControlStyles.StandardDoubleClick | ControlStyles.UseTextForAccessibility, value: false);
		borderStyle = BorderStyle.Fixed3D;
		Lines = new LineCollection(this);
		Styles = new StyleCollection(this);
		Indicators = new IndicatorCollection(this);
		Margins = new MarginCollection(this);
		Markers = new MarkerCollection(this);
		Selections = new SelectionCollection(this);
	}
}
