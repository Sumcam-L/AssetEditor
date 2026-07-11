using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Markup;

internal class TransactionBindingCore
{
	private class TransactionBeginEdit : ValidationRule
	{
		private readonly TransactionBindingCore m_owner;

		public TransactionBeginEdit(TransactionBindingCore owner)
			: base(ValidationStep.RawProposedValue, validatesOnTargetUpdated: false)
		{
			m_owner = owner;
		}

		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			m_owner.BeginTransaction();
			return ValidationResult.ValidResult;
		}
	}

	private class TransactionEndEdit : ValidationRule
	{
		private readonly TransactionBindingCore m_owner;

		public TransactionEndEdit(TransactionBindingCore owner)
			: base(ValidationStep.UpdatedValue, validatesOnTargetUpdated: false)
		{
			m_owner = owner;
		}

		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			m_owner.EndTransaction();
			return ValidationResult.ValidResult;
		}
	}

	private static IContextRegistry s_cachedContextRegistry;

	private ITransactionContext m_currentTransactionContext;

	public string Transaction { get; set; }

	public TransactionBindingCore(ICollection<ValidationRule> rules)
	{
		rules.Add(new TransactionBeginEdit(this));
		rules.Add(new TransactionEndEdit(this));
	}

	public void CancelTransaction()
	{
		if (m_currentTransactionContext != null)
		{
			m_currentTransactionContext.Cancel();
			m_currentTransactionContext = null;
		}
	}

	private void BeginTransaction()
	{
		ITransactionContext currentTransactionContext = GetCurrentTransactionContext();
		if (currentTransactionContext != null && !currentTransactionContext.InTransaction)
		{
			m_currentTransactionContext = currentTransactionContext;
			currentTransactionContext.Begin(Transaction);
		}
	}

	private void EndTransaction()
	{
		if (m_currentTransactionContext != null)
		{
			if (m_currentTransactionContext.InTransaction)
			{
				m_currentTransactionContext.End();
			}
			m_currentTransactionContext = null;
		}
	}

	private static ITransactionContext GetCurrentTransactionContext()
	{
		if (s_cachedContextRegistry == null)
		{
			IComposer current = Composer.Current;
			if (current != null)
			{
				s_cachedContextRegistry = current.Container.GetExportedValueOrDefault<IContextRegistry>();
			}
		}
		if (s_cachedContextRegistry != null)
		{
			return s_cachedContextRegistry.GetActiveContext<ITransactionContext>();
		}
		return null;
	}
}
