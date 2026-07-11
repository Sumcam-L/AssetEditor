namespace Sce.Atf;

public interface ITransactionContext
{
	bool InTransaction { get; }

	int PendingOperationCount { get; }

	void Begin(string transactionName);

	void Cancel();

	void End();

	TransactionSuspensionReceipt SuspendTransactions();
}
