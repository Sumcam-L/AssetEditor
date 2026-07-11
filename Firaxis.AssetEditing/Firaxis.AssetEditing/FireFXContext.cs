using Firaxis.ATF;
using Sce.Atf;
using Sce.Atf.Adaptation;

namespace Firaxis.AssetEditing;

public class FireFXContext : BaseEntityPropertyContext, IFireFXEditorContext, IEntityEditorContext, IObservableContext
{
	public FireFXEditor FireFXEditor { get; set; }

	public virtual bool HasScript => base.DomNode.As<FireFXInstanceAdapter>()?.HasScript ?? false;

	public virtual IFireFXScriptResource ScriptResource => base.DomNode.As<FireFXInstanceAdapter>()?.ScriptResource;

	protected override void Dispose(bool bDisposing)
	{
		if (bDisposing)
		{
			base.GUI?.Dispose();
			base.GUI = null;
		}
		base.Dispose(bDisposing);
	}
}
