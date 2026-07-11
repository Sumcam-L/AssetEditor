using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Firaxis.AssetBrowser;
using Firaxis.AssetBrowser.ViewModels;
using Firaxis.AssetEditing;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;
using Sce.Atf;
using Sce.Atf.Adaptation;

namespace Firaxis.AssetPreviewing;

public class CustomEntityPickerPropertyEditor : IPaintedPropertyEditor
{
	private static readonly StringFormat kCenteredText = new StringFormat
	{
		LineAlignment = StringAlignment.Center,
		Alignment = StringAlignment.Center,
		FormatFlags = StringFormatFlags.NoWrap
	};

	private IValueKnob<string> Knob;

	private IEntityFilterProvider FilterProvider;

	private PropertyDescriptor Property;

	private bool CanClear;

	private readonly Icon DeleteIcon;

	private readonly Image AddIcon;

	public CustomEntityPickerPropertyEditor(PropertyDescriptor prop, IValueKnob<string> knob)
	{
		Knob = knob;
		Property = prop;
		FilterProvider = Knob.As<IEntityFilterProvider>();
		CanClear = Knob.As<IToolKnob>()?.CanClear ?? false;
		DeleteIcon = ResourceUtil.GetIcon(Firaxis.AssetEditing.Resources.DeleteIcon);
		AddIcon = ResourceUtil.GetImage16(Firaxis.AssetEditing.Resources.AddExistingIcon);
	}

	private Rectangle ComputeClearButtonRect(Rectangle valueRc)
	{
		int num = Math.Min(valueRc.Width, valueRc.Height);
		Rectangle result = new Rectangle(0, valueRc.Top, num, num);
		result.Offset(valueRc.Right - 2 * (result.Width - 1), 0);
		result.Inflate(-1, -1);
		return result;
	}

	private Rectangle ComputeModalButtonRect(Rectangle valueRc)
	{
		int num = Math.Min(valueRc.Width, valueRc.Height);
		Rectangle result = new Rectangle(0, valueRc.Top, num, num);
		result.Offset(valueRc.Right - result.Width - 1, 0);
		result.Inflate(-1, -1);
		return result;
	}

	public Rectangle HandleMouseDown(MouseButtons downBtns, Rectangle labelRc, Rectangle valueRc, Point clickPt)
	{
		return Rectangle.Empty;
	}

	public Rectangle HandleMouseUp(MouseButtons upBtns, Rectangle labelRc, Rectangle valueRc, Point clickPt)
	{
		return Rectangle.Empty;
	}

	public Rectangle HandleMouseMove(MouseButtons pressedBtns, Rectangle labelRc, Rectangle valueRc, Point clickPt)
	{
		Rectangle rectangle = ComputeModalButtonRect(valueRc);
		Rectangle rectangle2 = ComputeClearButtonRect(valueRc);
		if (rectangle.Contains(clickPt) || (CanClear && rectangle2.Contains(clickPt)))
		{
			Cursor.Current = Cursors.Hand;
		}
		else
		{
			Cursor.Current = Cursors.Arrow;
		}
		return Rectangle.Empty;
	}

	public Rectangle HandleMouseClick(MouseButtons btns, Rectangle labelRc, Rectangle valueRc, Point clickPt)
	{
		Rectangle result = ComputeModalButtonRect(valueRc);
		Rectangle rectangle = ComputeClearButtonRect(valueRc);
		if (result.Contains(clickPt))
		{
			IEntityFilteringContext filteringContext = CivTechRegistry.EntityFilteringService.GetFilteringContext(FilterProvider.AllowedTypes, FilterProvider.AllowedClasses);
			AssetBrowserDialogViewModel dialogViewModel;
			bool? flag = AssetBrowserDialog.CreateDialog(out dialogViewModel, filteringContext, (IWin32Window)Application.OpenForms.OfType<Form>().FirstOrDefault((Form fod) => fod.Visible), false);
			if (flag.HasValue && flag.Value && dialogViewModel.HasSelection)
			{
				string nameFromPath = GetNameFromPath(dialogViewModel.SelectedPath);
				Property.SetValue(null, nameFromPath);
				return valueRc;
			}
			return result;
		}
		if (CanClear && rectangle.Contains(clickPt))
		{
			Property.SetValue(null, string.Empty);
			return valueRc;
		}
		return Rectangle.Empty;
	}

	private InstanceType GetTypeFromExtension(string absolutePath)
	{
		string extension = Path.GetExtension(absolutePath);
		return EnumToStringConverter.GetTypeFromExtension(extension);
	}

	private string GetNameFromPath(string absolutePath)
	{
		InstanceType typeFromExtension = GetTypeFromExtension(absolutePath);
		string text = absolutePath.Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		foreach (string pantryRoot in CivTechRegistry.CivTechService.ProjectMapService.LayeredPantry.PantryRoots)
		{
			string text2 = StaticMethods.PantryRootForInstanceType(pantryRoot, typeFromExtension).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.AltDirectorySeparatorChar;
			if (text.StartsWith(text2, StringComparison.OrdinalIgnoreCase))
			{
				string text3 = Path.ChangeExtension(text, null);
				return text3.Replace(text2, "");
			}
		}
		return absolutePath;
	}

	public void PaintValue(PaintedStyleInfo colors, PaintedState state, object value, Graphics canvas, Rectangle rectangle)
	{
		Rectangle rectangle2 = ComputeModalButtonRect(rectangle);
		Rectangle rectangle3 = ComputeClearButtonRect(rectangle);
		rectangle.Width = rectangle3.Left - rectangle.Left - 1;
		string text = (string)Property.GetValue(null);
		if (!string.IsNullOrEmpty(text))
		{
			using SolidBrush brush = new SolidBrush(state.Selected ? colors.SelectedForeColor : colors.ForeColor);
			canvas.DrawString(text, colors.Font, brush, rectangle);
		}
		ButtonRenderer.DrawButton(canvas, rectangle2, PushButtonState.Normal);
		rectangle2.Inflate(-1, -1);
		canvas.DrawImage(AddIcon, rectangle2);
		if (CanClear)
		{
			ButtonRenderer.DrawButton(canvas, rectangle3, PushButtonState.Normal);
			rectangle3.Inflate(-1, -1);
			canvas.DrawIcon(DeleteIcon, rectangle3);
		}
	}
}
