using System.Numerics;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime.Operations;

public class ExtensibleComplex : Extensible<Complex>
{
	public ExtensibleComplex()
	{
	}

	public ExtensibleComplex(double real)
		: base(MathUtils.MakeReal(real))
	{
	}

	public ExtensibleComplex(double real, double imag)
		: base(new Complex(real, imag))
	{
	}
}
