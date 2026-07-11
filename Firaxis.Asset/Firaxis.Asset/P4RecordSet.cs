using System.Collections;

namespace Firaxis.Asset;

public class P4RecordSet : P4BaseRecordSet, IEnumerable
{
	public P4Record[] Records => new P4Record[0];

	public string[] Messages => new string[0];

	public P4Record this[int Index] => new P4Record();

	IEnumerator IEnumerable.GetEnumerator()
	{
		return Records.GetEnumerator();
	}
}
