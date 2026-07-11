using System;

namespace Sce.Atf.VectorMath;

public class Triangle3F : IFormattable
{
	public Vec3F V0;

	public Vec3F V1;

	public Vec3F V2;

	public Triangle3F(Vec3F v0, Vec3F v1, Vec3F v2)
	{
		V0 = v0;
		V1 = v1;
		V2 = v2;
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
		return string.Format("{0}{9} {1}{9} {2}{9} {3}{9} {4}{9} {5}{9} {6}{9} {7}{9} {8}", V0.X.ToString(format, formatProvider), V0.Y.ToString(format, formatProvider), V0.Z.ToString(format, formatProvider), V1.X.ToString(format, formatProvider), V1.Y.ToString(format, formatProvider), V1.Z.ToString(format, formatProvider), V2.X.ToString(format, formatProvider), V2.Y.ToString(format, formatProvider), V2.Z.ToString(format, formatProvider), numberListSeparator);
	}
}
