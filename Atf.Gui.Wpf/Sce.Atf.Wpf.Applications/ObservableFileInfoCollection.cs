using System.Collections.ObjectModel;
using System.Linq;

namespace Sce.Atf.Wpf.Applications;

public class ObservableFileInfoCollection : ObservableCollection<ObservableFileInfo>
{
	public ObservableFileInfo GetFile(string fullName)
	{
		return this.FirstOrDefault((ObservableFileInfo file) => string.CompareOrdinal(file.FileInfo.FullName, fullName) == 0);
	}
}
