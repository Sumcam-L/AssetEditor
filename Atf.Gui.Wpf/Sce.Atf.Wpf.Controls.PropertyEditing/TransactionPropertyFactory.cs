using System.ComponentModel;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

public class TransactionPropertyFactory : IPropertyFactory
{
	private static TransactionPropertyFactory s_instance;

	public static TransactionPropertyFactory Instance
	{
		get
		{
			if (s_instance == null)
			{
				s_instance = new TransactionPropertyFactory();
			}
			return s_instance;
		}
	}

	public static PropertyNode CreateTransactionProperty(object instance, PropertyDescriptor descriptor, bool isEnumerable, ITransactionContext context)
	{
		TransactionPropertyNode transactionPropertyNode = new TransactionPropertyNode(context);
		transactionPropertyNode.Initialize(instance, descriptor, isEnumerable);
		return transactionPropertyNode;
	}

	public PropertyNode CreateProperty(object instance, PropertyDescriptor descriptor, bool isEnumerable, ITransactionContext context)
	{
		return CreateTransactionProperty(instance, descriptor, isEnumerable, context);
	}
}
