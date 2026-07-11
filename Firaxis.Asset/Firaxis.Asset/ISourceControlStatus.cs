using System.Collections.Generic;
using System.Drawing;

namespace Firaxis.Asset;

public interface ISourceControlStatus
{
	bool Controlled { get; }

	bool Owner { get; }

	bool Changed { get; }

	bool NewAdd { get; }

	bool HaveLatest { get; }

	bool Deleted { get; }

	SourceStatus Depot { get; }

	SourceStatus Client { get; }

	Image StatusImage { get; }

	string StatusText { get; }

	void Refresh();

	void ParseInfo(string key, string value);

	List<SourceRevision> GetSourceRevision();
}
