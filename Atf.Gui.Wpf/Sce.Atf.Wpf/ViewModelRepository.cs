using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;

namespace Sce.Atf.Wpf;

[Export]
[Export(typeof(IInitializable))]
public class ViewModelRepository : IPartImportsSatisfiedNotification, IInitializable
{
	private class RecompositionItemPending
	{
		public string VMContract { get; private set; }

		public WeakReference Reference { get; private set; }

		public bool IsShared { get; private set; }

		public RecompositionItemPending(string vmContractName, WeakReference weakReference, bool isShared)
		{
			VMContract = vmContractName;
			Reference = weakReference;
			IsShared = isShared;
		}
	}

	private static readonly List<RecompositionItemPending> s_unsatisfiedContracts = new List<RecompositionItemPending>();

	[ImportMany("ViewModel", AllowRecomposition = true)]
	public IEnumerable<Lazy<object, IViewModelMetadata>> ViewModelsLazy { get; set; }

	public void Initialize()
	{
	}

	public void OnImportsSatisfied()
	{
		lock (s_unsatisfiedContracts)
		{
			for (int i = 0; i < s_unsatisfiedContracts.Count; i++)
			{
				RecompositionItemPending recompositionItemPending = s_unsatisfiedContracts[i];
				object viewModel = GetViewModel(recompositionItemPending.VMContract, recompositionItemPending.IsShared);
				if (viewModel != null)
				{
					if (recompositionItemPending.Reference.IsAlive)
					{
						((FrameworkElement)recompositionItemPending.Reference.Target).DataContext = viewModel;
					}
					s_unsatisfiedContracts.RemoveAt(i);
					i--;
				}
			}
		}
	}

	public object GetViewModel(string contract, bool isShared)
	{
		if (isShared)
		{
			return ViewModelsLazy.SingleOrDefault((Lazy<object, IViewModelMetadata> v) => v.Metadata.Name.Equals(contract))?.Value;
		}
		throw new NotSupportedException("Only shared view models are currently supported by ViewModelRepository");
	}

	public static void AttachViewModelToView(string vmContract, FrameworkElement view, bool isShared)
	{
		if (Composer.Current != null)
		{
			ViewModelRepository exportedValueOrDefault = Composer.Current.Container.GetExportedValueOrDefault<ViewModelRepository>();
			if (exportedValueOrDefault != null)
			{
				object viewModel = exportedValueOrDefault.GetViewModel(vmContract, isShared);
				if (viewModel != null)
				{
					view.DataContext = viewModel;
				}
				else
				{
					RegisterMissingViewModel(vmContract, view, isShared);
				}
				return;
			}
		}
		RegisterMissingViewModel(vmContract, view, isShared);
	}

	private static void RegisterMissingViewModel(string vmContractName, FrameworkElement view, bool isShared)
	{
		lock (s_unsatisfiedContracts)
		{
			s_unsatisfiedContracts.Add(new RecompositionItemPending(vmContractName, new WeakReference(view), isShared));
		}
	}
}
