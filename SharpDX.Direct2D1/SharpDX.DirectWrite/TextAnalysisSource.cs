using System;
using System.Runtime.InteropServices;

namespace SharpDX.DirectWrite;

[Guid("688e1a58-5094-47c8-adc8-fbcea60ae92b")]
[Shadow(typeof(TextAnalysisSourceShadow))]
public interface TextAnalysisSource : ICallbackable, IDisposable
{
	ReadingDirection ReadingDirection { get; }

	string GetTextAtPosition(int textPosition);

	string GetTextBeforePosition(int textPosition);

	string GetLocaleName(int textPosition, out int textLength);

	NumberSubstitution GetNumberSubstitution(int textPosition, out int textLength);
}
