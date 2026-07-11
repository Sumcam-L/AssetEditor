using System;
using System.Collections.Generic;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Dom;

public class DomNodeQueryable : DomNodeAdapter, IQueryableContext, IQueryableResultContext, IQueryableReplaceContext
{
	private readonly List<object> m_results = new List<object>();

	public IEnumerable<object> Results => m_results;

	public event EventHandler ResultsChanged;

	public IEnumerable<object> Query(IQueryPredicate predicate)
	{
		m_results.Clear();
		foreach (DomNode item in base.DomNode.Subtree)
		{
			Dictionary<IQueryPredicate, IList<IQueryMatch>> dictionary = new Dictionary<IQueryPredicate, IList<IQueryMatch>>();
			if (predicate.Test(item, out var matchList))
			{
				if (matchList != null)
				{
					dictionary[predicate] = matchList;
				}
				m_results.Add(new DomNodeQueryMatch(item, dictionary));
			}
		}
		this.ResultsChanged.Raise(this, EventArgs.Empty);
		return m_results;
	}

	public IEnumerable<object> Replace(object replaceInfo)
	{
		ITransactionContext transactionContext = null;
		try
		{
			foreach (DomNodeQueryMatch result in m_results)
			{
				ITransactionContext transactionContext2 = result.DomNode?.GetRoot().As<ITransactionContext>();
				if (transactionContext2 != transactionContext)
				{
					transactionContext?.End();
					transactionContext = transactionContext2;
					transactionContext?.Begin("Replace".Localize());
				}
				foreach (IQueryPredicate key in result.PredicateMatchResults.Keys)
				{
					key.Replace(result.PredicateMatchResults[key], replaceInfo);
				}
			}
		}
		catch (InvalidTransactionException ex)
		{
			if (transactionContext != null && transactionContext.InTransaction)
			{
				transactionContext.Cancel();
			}
			if (ex.ReportError)
			{
				Outputs.WriteLine(OutputMessageType.Error, ex.Message);
			}
		}
		finally
		{
			if (transactionContext != null && transactionContext.InTransaction)
			{
				transactionContext.End();
			}
		}
		this.ResultsChanged.Raise(this, EventArgs.Empty);
		return m_results;
	}
}
