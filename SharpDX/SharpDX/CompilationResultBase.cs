using System;

namespace SharpDX;

public abstract class CompilationResultBase<T> : DisposeBase where T : class, IDisposable
{
	public T Bytecode { get; private set; }

	public Result ResultCode { get; private set; }

	public bool HasErrors => ResultCode.Failure;

	public string Message { get; private set; }

	protected CompilationResultBase(T bytecode, Result resultCode, string message = null)
	{
		Bytecode = bytecode;
		ResultCode = resultCode;
		Message = message;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && Bytecode != null)
		{
			T bytecode = Bytecode;
			bytecode.Dispose();
			Bytecode = null;
		}
	}
}
