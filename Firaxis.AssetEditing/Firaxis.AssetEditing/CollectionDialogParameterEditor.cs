using System;
using System.Windows.Forms;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.AssetEditing;

public class CollectionDialogParameterEditor : EmbeddedCollectionEditor
{
	public override Control GetEditingControl(PropertyEditorControlContext context)
	{
		Button btn = new Button();
		btn.Text = "Edit";
		btn.Height = 20;
		EventHandler clickHandler = delegate
		{
			EmbeddedCollectionDialog embeddedCollectionDialog = new EmbeddedCollectionDialog(this, context);
			SkinService.ApplyActiveSkin(embeddedCollectionDialog);
			embeddedCollectionDialog.ShowDialog();
		};
		EventHandler disposedHandler = null;
		disposedHandler = delegate
		{
			btn.Click -= clickHandler;
			btn.Disposed -= disposedHandler;
		};
		btn.Click += clickHandler;
		btn.Disposed += disposedHandler;
		return btn;
	}
}
