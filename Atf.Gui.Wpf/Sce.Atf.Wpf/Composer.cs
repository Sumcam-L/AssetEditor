using System.ComponentModel.Composition;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf;

[Export(typeof(Composer))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class Composer : IInitializable, IPartImportsSatisfiedNotification
{
	[Import]
	private IComposer m_composer = null;

	public static IComposer Current { get; private set; }

	public void OnImportsSatisfied()
	{
		Current = m_composer;
	}

	public void Initialize()
	{
	}

	public static void Configure(IComposer composer)
	{
		Current = composer;
	}
}
