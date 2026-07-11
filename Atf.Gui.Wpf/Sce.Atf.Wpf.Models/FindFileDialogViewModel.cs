using System.ComponentModel;

namespace Sce.Atf.Wpf.Models;

public class FindFileDialogViewModel : DialogViewModelBase
{
	private static readonly PropertyChangedEventArgs s_originalPathArgs = ObservableUtil.CreateArgs((FindFileDialogViewModel x) => x.OriginalPath);

	private static readonly PropertyChangedEventArgs s_suggestedPathArgs = ObservableUtil.CreateArgs((FindFileDialogViewModel x) => x.SuggestedPath);

	private static readonly PropertyChangedEventArgs s_actionArgs = ObservableUtil.CreateArgs((FindFileDialogViewModel x) => x.Action);

	private string m_originalPath;

	private string m_suggestedPath;

	private FindFileAction m_action;

	public string OriginalPath
	{
		get
		{
			return m_originalPath;
		}
		set
		{
			m_originalPath = value;
			OnPropertyChanged(s_originalPathArgs);
		}
	}

	public string SuggestedPath
	{
		get
		{
			return m_suggestedPath;
		}
		set
		{
			m_suggestedPath = value;
			if (!string.IsNullOrEmpty(m_suggestedPath))
			{
				Action = FindFileAction.AcceptSuggestion;
			}
			OnPropertyChanged(s_suggestedPathArgs);
		}
	}

	public FindFileAction Action
	{
		get
		{
			return m_action;
		}
		set
		{
			m_action = value;
			OnPropertyChanged(s_actionArgs);
		}
	}

	public FindFileDialogViewModel()
	{
		base.Title = "Find File".Localize();
		Action = FindFileAction.UserSpecify;
	}
}
