using System;
using Firaxis.CivTech.CookerInterface;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.ATF;

public interface ICookService
{
	event EventHandler<DocumentEventArgs> DocumentCooked;

	event EventHandler<DocumentEventArgs> DocumentCookFailed;

	CookResult Cook(IDocument document);

	CookResult Cook(Uri uriToCook);

	CookResult CookCustom(ICookerOptions options);
}
