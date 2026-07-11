using System.IO;
using Microsoft.Scripting;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

internal sealed class NoLineFeedSourceContentProvider : TextContentProvider
{
	internal sealed class Reader : SourceCodeReader
	{
		internal Reader(string s)
			: base(new StringReader(s), null)
		{
		}

		public override string ReadLine()
		{
			return IOUtils.ReadTo(this, '\n');
		}

		public override bool SeekLine(int line)
		{
			int num = 1;
			while (true)
			{
				if (num == line)
				{
					return true;
				}
				if (!IOUtils.SeekTo(this, '\n'))
				{
					break;
				}
				num++;
			}
			return false;
		}
	}

	private readonly string _code;

	public NoLineFeedSourceContentProvider(string code)
	{
		_code = code;
	}

	public override SourceCodeReader GetReader()
	{
		return new Reader(_code);
	}
}
