using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sce.Atf.Applications;

public static class IDocumentClientExtensions
{
	public static IDocumentClient GetFirstClientForPath(this IEnumerable<IDocumentClient> clients, string pathName)
	{
		string pathExtension = Path.GetExtension(pathName);
		IDocumentClient documentClient = clients.FirstOrDefault((IDocumentClient client) => client.Info.Extensions.Contains(pathExtension));
		if (documentClient == null)
		{
			documentClient = clients.FirstOrDefault((IDocumentClient client) => client.Info.Extensions.Any((string ext) => pathName.EndsWith(ext, StringComparison.CurrentCultureIgnoreCase)));
		}
		return documentClient;
	}

	public static IEnumerable<IDocumentClient> GetAllClientsForPath(this IEnumerable<IDocumentClient> clients, string pathName)
	{
		return clients.Where((IDocumentClient client) => client.Info.Extensions.Any((string ext) => pathName.EndsWith(ext, StringComparison.CurrentCultureIgnoreCase)));
	}
}
