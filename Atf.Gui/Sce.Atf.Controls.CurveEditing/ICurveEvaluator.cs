namespace Sce.Atf.Controls.CurveEditing;

public interface ICurveEvaluator
{
	void Reset();

	float Evaluate(float x);
}
