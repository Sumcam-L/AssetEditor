using System;

namespace Sce.Atf;

public static class TransactionContexts
{
	public static bool DoTransaction(this ITransactionContext context, Action transaction, string transactionName)
	{
		if (context == null)
		{
			transaction?.Invoke();
			return true;
		}
		if (context.InTransaction)
		{
			transaction?.Invoke();
			return true;
		}
		try
		{
			context.Begin(transactionName);
			transaction?.Invoke();
			context.End();
		}
		catch (InvalidTransactionException ex)
		{
			context?.Cancel();
			if (ex.ReportError)
			{
				Outputs.WriteLine(OutputMessageType.Error, ex.Message);
			}
			return false;
		}
		return true;
	}
}
