using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Input;

namespace Sce.Atf.Controls.Adaptable;

[InheritedExport(typeof(IInitializable))]
[InheritedExport(typeof(IContextMenuCommandProvider))]
[InheritedExport(typeof(AnnotatingCommands))]
[PartCreationPolicy(CreationPolicy.Any)]
public class AnnotatingCommands : ICommandClient, IInitializable, IContextMenuCommandProvider
{
	public class ColorPreset
	{
		public string Name { get; set; }

		public Color Color { get; set; }
	}

	private ICommandService m_commandService;

	private IContextRegistry m_contextRegistry;

	private ColorPreset[] m_colorPresets;

	public ColorPreset[] ColorPresets
	{
		get
		{
			return m_colorPresets;
		}
		set
		{
			m_colorPresets = value;
		}
	}

	[ImportingConstructor]
	public AnnotatingCommands(ICommandService commandService, IContextRegistry contextRegistry)
	{
		m_commandService = commandService;
		m_contextRegistry = contextRegistry;
	}

	void IInitializable.Initialize()
	{
		InitColorPresets();
		for (int i = 0; i < m_colorPresets.Length; i++)
		{
			CommandInfo commandInfo = m_commandService.RegisterCommand(m_colorPresets[i], StandardMenu.Edit, StandardCommandGroup.EditOther, m_colorPresets[i].Name, m_colorPresets[i].Name, Sce.Atf.Input.Keys.None, null, CommandVisibility.ContextMenu, this);
			ToolStripMenuItem menuItem = commandInfo.GetMenuItem();
			menuItem.Image = CreateBackColorIcon(m_colorPresets[i].Color, 24, 24);
		}
	}

	public bool CanDoCommand(object commandTag)
	{
		bool result = false;
		if (commandTag is ColorPreset)
		{
			IColoringContext activeContext = m_contextRegistry.GetActiveContext<IColoringContext>();
			if (activeContext != null)
			{
				ISelectionContext selectionContext = activeContext.As<ISelectionContext>();
				if (selectionContext != null)
				{
					result = selectionContext.Selection.Any() && selectionContext.Selection.All((object x) => x.Is<IAnnotation>());
				}
			}
		}
		return result;
	}

	public void DoCommand(object commandTag)
	{
		if (!(commandTag is ColorPreset))
		{
			return;
		}
		IColoringContext context = m_contextRegistry.GetActiveContext<IColoringContext>();
		ITransactionContext context2 = m_contextRegistry.ActiveContext.As<ITransactionContext>();
		ColorPreset colorPreset = (ColorPreset)commandTag;
		context2.DoTransaction(delegate
		{
			ISelectionContext selectionContext = context.As<ISelectionContext>();
			foreach (IAnnotation item in selectionContext.Selection.AsIEnumerable<IAnnotation>())
			{
				context.SetColor(ColoringTypes.BackColor, item, colorPreset.Color);
			}
		}, "Annotation Color");
	}

	public void UpdateCommand(object commandTag, CommandState state)
	{
	}

	IEnumerable<object> IContextMenuCommandProvider.GetCommands(object context, object target)
	{
		if (context.Is<IColoringContext>())
		{
			return m_colorPresets;
		}
		return EmptyEnumerable<object>.Instance;
	}

	private void InitColorPresets()
	{
		if (m_colorPresets == null)
		{
			m_colorPresets = new ColorPreset[6];
			m_colorPresets[0] = new ColorPreset
			{
				Name = "Yellow".Localize(),
				Color = SystemColors.Info
			};
			m_colorPresets[1] = new ColorPreset
			{
				Name = "Blue".Localize(),
				Color = Color.LightSkyBlue
			};
			m_colorPresets[2] = new ColorPreset
			{
				Name = "Green".Localize(),
				Color = Color.FromArgb(178, 255, 161)
			};
			m_colorPresets[3] = new ColorPreset
			{
				Name = "Pink".Localize(),
				Color = Color.LightPink
			};
			m_colorPresets[4] = new ColorPreset
			{
				Name = "Purple".Localize(),
				Color = Color.FromArgb(182, 202, 255)
			};
			m_colorPresets[5] = new ColorPreset
			{
				Name = "Gray".Localize(),
				Color = Color.LightGray
			};
		}
	}

	private Image CreateBackColorIcon(Color color, int width, int height)
	{
		Image image = new Bitmap(width, height);
		Graphics graphics = Graphics.FromImage(image);
		graphics.Clear(color);
		graphics.Save();
		graphics.Dispose();
		return image;
	}
}
