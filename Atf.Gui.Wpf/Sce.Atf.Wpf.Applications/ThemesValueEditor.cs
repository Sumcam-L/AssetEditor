using System.Linq;
using System.Windows;
using Sce.Atf.Wpf.Controls.PropertyEditing;
using Sce.Atf.Wpf.Skins;

namespace Sce.Atf.Wpf.Applications;

public class ThemesValueEditor : ValueEditor
{
	public override bool UsesCustomContext => true;

	public AppearanceService AppearanceService { get; set; }

	public ThemesValueEditor(AppearanceService appearanceService)
	{
		AppearanceService = appearanceService;
	}

	public override object GetCustomContext(PropertyNode node)
	{
		return (node == null) ? null : new StandardValuesEditorContext(node, AppearanceService.RegisteredSkins.Select((Skin x) => x.Name));
	}

	public override DataTemplate GetTemplate(PropertyNode node, DependencyObject container)
	{
		return ValueEditor.FindResource<DataTemplate>(PropertyGrid.StandardValuesEditorTemplateKey, container);
	}
}
