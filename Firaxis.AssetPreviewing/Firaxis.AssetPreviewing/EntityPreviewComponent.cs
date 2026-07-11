using System;
using System.Collections.Generic;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetPreviewing;

public class EntityPreviewComponent : Validator, IEntityPreviewComponent
{
	private class NullEntityChangeList : IEntityChangeList, IAssemblyInstance, IDisposable
	{
		private IEntityChangeList m_devNull;

		public IEnumerable<IEntityChangedEvent> EntityChanges => Enumerable.Empty<IEntityChangedEvent>();

		public void Clear()
		{
		}

		public void Dispose()
		{
			m_devNull.Dispose();
			m_devNull = null;
		}

		public T Push<T>(EntityID entity) where T : IEntityChangedEvent
		{
			return m_devNull.Push<T>(entity);
		}

		public NullEntityChangeList()
		{
			m_devNull = Context.Get<CivTechContext>().CreateInstance<IEntityChangeList>();
		}
	}

	private class PreviewUpdateSuspender : IDisposable
	{
		private readonly EntityPreviewComponent Owner;

		public PreviewUpdateSuspender(EntityPreviewComponent owner)
		{
			Owner = owner;
			Owner.DoSuspendPreviewUpdates();
		}

		public void Dispose()
		{
			Owner.DoUnsuspendPreviewUpdates();
		}
	}

	private int _suspendUpdates = 0;

	private IEntityChangeList _entityChanges;

	private IEntityChangeList _nullEntityChanges = new NullEntityChangeList();

	private string _lastSender = string.Empty;

	public IEntityChangeList EntityChanges
	{
		get
		{
			if (!ArePreviewUpdatesSuspended())
			{
				BugSubmitter.SilentAssert(_entityChanges != null, "The last context sender was \"{0}\" @summary Accessing the EntityChangeList outside of a transaction @assign bwhitman", _lastSender);
				return _entityChanges;
			}
			return _nullEntityChanges;
		}
		private set
		{
			_entityChanges = value;
		}
	}

	public IDisposable SuspendPreviewerUpdates()
	{
		return new PreviewUpdateSuspender(this);
	}

	private void DoSuspendPreviewUpdates()
	{
		_suspendUpdates++;
	}

	private void DoUnsuspendPreviewUpdates()
	{
		_suspendUpdates--;
		BugSubmitter.Assert(_suspendUpdates >= 0, "Unmatched previewer update suspension!");
	}

	private bool ArePreviewUpdatesSuspended()
	{
		return _suspendUpdates > 0;
	}

	private IEntityChangeList CreateEntityChangeList()
	{
		if (base.DomNode.Is<IShadowDocument>())
		{
			return _nullEntityChanges;
		}
		return Context.Get<CivTechContext>().CreateInstance<IEntityChangeList>();
	}

	protected override void OnBeginning(object sender, EventArgs e)
	{
		if (!ArePreviewUpdatesSuspended())
		{
			_lastSender = sender.ToString();
			BugSubmitter.SilentAssert(_entityChanges == null, "Transaction beginning while another previewable change is in flight! @assign bwhitman");
			_entityChanges = CreateEntityChangeList();
		}
	}

	protected override void OnEnded(object sender, EventArgs e)
	{
		if (!ArePreviewUpdatesSuspended())
		{
			BugSubmitter.SilentAssert(_entityChanges != null, "Transaction ending but a previewable change was never started! @assign bwhitman");
			_entityChanges?.Clear();
			_entityChanges = null;
		}
	}

	protected override void OnCancelled(object sender, EventArgs e)
	{
		if (!ArePreviewUpdatesSuspended())
		{
			BugSubmitter.SilentAssert(_entityChanges != null, "Transaction canceled but a previewable change was never started! @assign bwhitman");
			_entityChanges?.Clear();
			_entityChanges = null;
		}
	}
}
