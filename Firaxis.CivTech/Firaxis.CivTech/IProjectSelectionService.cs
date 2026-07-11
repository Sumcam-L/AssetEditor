using System;

namespace Firaxis.CivTech;

public interface IProjectSelectionService
{
	string ActiveProject { get; set; }

	IProjectInfoMap Projects { get; }

	event EventHandler ProjectChanged;
}
