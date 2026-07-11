using System;

namespace Sce.Atf.VectorMath;

public struct BezierPoint : IFormattable
{
	public Vec3F Position;

	public Vec3F Tangent1;

	public Vec3F Tangent2;

	public BezierPoint(Vec3F position, Vec3F incomingTangent, Vec3F outgoingTangent)
	{
		Position = position;
		Tangent1 = incomingTangent;
		Tangent2 = outgoingTangent;
	}

	public override string ToString()
	{
		return ToString(null, null);
	}

	public string ToString(string format, IFormatProvider formatProvider)
	{
		string numberListSeparator = StringUtil.GetNumberListSeparator(formatProvider);
		if (format == null)
		{
			format = "R";
		}
		return string.Format("{0}{9} {1}{9} {2}{9} {3}{9} {4}{9} {5}{9} {6}{9} {7}{9} {8}", Position.X.ToString(format, formatProvider), Position.Y.ToString(format, formatProvider), Position.Z.ToString(format, formatProvider), Tangent1.X.ToString(format, formatProvider), Tangent1.Y.ToString(format, formatProvider), Tangent1.Z.ToString(format, formatProvider), Tangent2.X.ToString(format, formatProvider), Tangent2.Y.ToString(format, formatProvider), Tangent2.Z.ToString(format, formatProvider), numberListSeparator);
	}
}
