using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IAttachmentPointSet
{
	IEnumerable<IAttachmentPoint> Items { get; }

	IAttachmentPoint FindByName(string name);

	IAttachmentPoint AddAttachmentPoint(string name, string boneName, string modelInstName);

	void RemoveAttachmentPoint(string name);

	void RemoveAttachmentPoints(IEnumerable<string> name);
}
