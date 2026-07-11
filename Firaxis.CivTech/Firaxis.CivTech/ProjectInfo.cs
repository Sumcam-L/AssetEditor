using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace Firaxis.CivTech;

public class ProjectInfo
{
	public string Name { get; set; }

	public ProjectType ProjectType { get; set; }

	[Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
	public string Config { get; set; }

	[Editor(typeof(FileNameEditor), typeof(UITypeEditor))]
	public string Art { get; set; }

	[TypeConverter(typeof(NoTypeExpandbleConverter))]
	public Paths Paths { get; set; }

	public ProjectInfo()
	{
		Name = string.Empty;
		Config = string.Empty;
		Art = string.Empty;
		ProjectType = ProjectType.eNormal;
		Paths = new Paths();
	}
}
