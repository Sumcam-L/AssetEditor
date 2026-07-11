using System;

namespace Firaxis.Granny;

public interface IGrannyCurve : IDisposable
{
	ECurveType Type { get; }

	int Degree { get; }

	int Dimension { get; }

	int KnotCount { get; }

	bool IsIdentity { get; }

	bool IsConstant { get; }

	string ComponentName(int iDimension);

	float[] Sample(float fLocalClock, float fDuration, bool bNormalize, bool bBackLoop, bool bForwardLoop);
}
