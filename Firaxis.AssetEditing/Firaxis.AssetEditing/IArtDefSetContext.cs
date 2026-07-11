using System;
using Sce.Atf;

namespace Firaxis.AssetEditing;

public interface IArtDefSetContext
{
	string TemplateName { get; set; }

	string Description { get; set; }

	event EventHandler<ItemChangedEventArgs<string>> TemplateChanged;
}
