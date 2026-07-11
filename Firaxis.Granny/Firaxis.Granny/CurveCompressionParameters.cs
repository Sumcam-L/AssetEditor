namespace Firaxis.Granny;

public class CurveCompressionParameters
{
	public int MaxDegree = 2;

	public int DesiredDegree = 2;

	public bool UseQuantizedCurves = true;

	public bool AllowDegreeReduction = true;

	public bool AllowReductionOnMissedTolerance = true;

	public float ErrorTolerance = 0.1f;

	public float C0Threshold = 1f;

	public float C1Threshold = 1f;

	public CurveDataType[] PossibleCompressionTypes = new CurveDataType[1] { CurveDataType.DaK32fC32fType };

	public CurveDataType ConstantCompressionType;

	public CurveDataType IdentityCompressionType = CurveDataType.DaIdentityType;

	public CurveConstant IdentityVector;
}
