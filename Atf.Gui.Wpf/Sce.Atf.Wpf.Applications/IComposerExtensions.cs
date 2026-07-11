using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;

namespace Sce.Atf.Wpf.Applications;

public static class IComposerExtensions
{
	public static ComposablePart AddPart(this IComposer composer, object attributedPart)
	{
		Requires.NotNull(composer, "composer");
		Requires.NotNull(attributedPart, "attributedPart");
		ComposablePart composablePart = AttributedModelServices.CreatePart(attributedPart);
		CompositionBatch batch = new CompositionBatch(new ComposablePart[1] { composablePart }, Enumerable.Empty<ComposablePart>());
		composer.Container.Compose(batch);
		return composablePart;
	}

	public static void RemovePart(this IComposer composer, ComposablePart composablePart)
	{
		Requires.NotNull(composer, "composer");
		Requires.NotNull(composablePart, "composablePart");
		CompositionBatch batch = new CompositionBatch(Enumerable.Empty<ComposablePart>(), new ComposablePart[1] { composablePart });
		composer.Container.Compose(batch);
	}
}
