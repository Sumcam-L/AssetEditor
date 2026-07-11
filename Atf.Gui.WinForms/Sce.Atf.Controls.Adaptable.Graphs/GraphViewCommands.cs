using System.ComponentModel.Composition;
using System.Drawing;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable.Graphs;

[InheritedExport(typeof(IInitializable))]
[InheritedExport(typeof(GraphViewCommands))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class GraphViewCommands : ICommandClient, IInitializable
{
	private readonly ICommandService m_commandService;

	private readonly IContextRegistry m_contextRegistry;

	private float[] m_zoomPresets = new float[9] { 0.25f, 0.5f, 0.75f, 1f, 1.25f, 1.5f, 1.75f, 2f, 3f };

	public float[] ZoomPresets
	{
		get
		{
			return m_zoomPresets;
		}
		set
		{
			m_zoomPresets = value;
		}
	}

	[ImportingConstructor]
	public GraphViewCommands(ICommandService commandService, IContextRegistry contextRegistry)
	{
		m_commandService = commandService;
		m_contextRegistry = contextRegistry;
	}

	void IInitializable.Initialize()
	{
		m_commandService.RegisterCommand(CommandInfo.ViewZoomIn, this);
		m_commandService.RegisterCommand(CommandInfo.ViewZoomOut, this);
		m_commandService.RegisterCommand(CommandInfo.ViewZoomReset, this);
	}

	bool ICommandClient.CanDoCommand(object commandTag)
	{
		bool result = false;
		if (commandTag is StandardCommand)
		{
			IViewingContext activeContext = m_contextRegistry.GetActiveContext<IViewingContext>();
			if (activeContext == null)
			{
				return false;
			}
			AdaptableControl adaptableControl = activeContext.As<AdaptableControl>();
			if (adaptableControl == null)
			{
				return false;
			}
			ITransformAdapter transformAdapter = adaptableControl.As<ITransformAdapter>();
			switch ((StandardCommand)commandTag)
			{
			case StandardCommand.ViewZoomIn:
			case StandardCommand.ViewZoomOut:
			case StandardCommand.ViewZoomReset:
				result = transformAdapter != null;
				break;
			}
		}
		return result;
	}

	void ICommandClient.DoCommand(object commandTag)
	{
		if (commandTag is StandardCommand)
		{
			switch ((StandardCommand)commandTag)
			{
			case StandardCommand.ViewZoomIn:
				ZoomIn();
				break;
			case StandardCommand.ViewZoomOut:
				ZoomOut();
				break;
			case StandardCommand.ViewZoomReset:
				ZoomReset();
				break;
			case StandardCommand.ViewZoomExtents:
				break;
			}
		}
	}

	void ICommandClient.UpdateCommand(object commandTag, CommandState commandState)
	{
	}

	private void ZoomIn()
	{
		IViewingContext activeContext = m_contextRegistry.GetActiveContext<IViewingContext>();
		AdaptableControl adaptableControl = activeContext.As<AdaptableControl>();
		ITransformAdapter transformAdapter = adaptableControl.As<ITransformAdapter>();
		PointF scale = transformAdapter.Scale;
		float[] zoomPresets = m_zoomPresets;
		foreach (float num in zoomPresets)
		{
			if (num > scale.X && num > scale.Y)
			{
				scale.X = num;
				scale.Y = num;
				break;
			}
		}
		ZoomView(scale, adaptableControl);
	}

	private void ZoomOut()
	{
		IViewingContext activeContext = m_contextRegistry.GetActiveContext<IViewingContext>();
		AdaptableControl adaptableControl = activeContext.As<AdaptableControl>();
		ITransformAdapter transformAdapter = adaptableControl.As<ITransformAdapter>();
		PointF scale = transformAdapter.Scale;
		for (int num = m_zoomPresets.Length - 1; num >= 0; num--)
		{
			float num2 = m_zoomPresets[num];
			if (num2 < scale.X && num2 < scale.Y)
			{
				scale.X = num2;
				scale.Y = num2;
				break;
			}
		}
		ZoomView(scale, adaptableControl);
	}

	private void ZoomView(PointF newScale, AdaptableControl adaptableControl)
	{
		Point point = adaptableControl.PointToClient(Cursor.Position);
		if (!adaptableControl.ClientRectangle.Contains(point))
		{
			point = new Point(adaptableControl.Width / 2, adaptableControl.Height / 2);
		}
		ITransformAdapter transformAdapter = adaptableControl.As<ITransformAdapter>();
		Point point2 = GdiUtil.InverseTransform(transformAdapter.Transform, point);
		PointF pointF = transformAdapter.ConstrainScale(newScale);
		PointF pointF2 = new PointF((transformAdapter.Scale.X - pointF.X) * (float)point2.X + transformAdapter.Translation.X, (transformAdapter.Scale.Y - pointF.Y) * (float)point2.Y + transformAdapter.Translation.Y);
		transformAdapter.SetTransform(pointF.X, pointF.Y, pointF2.X, pointF2.Y);
	}

	private void ZoomReset()
	{
		IViewingContext activeContext = m_contextRegistry.GetActiveContext<IViewingContext>();
		AdaptableControl adaptableControl = activeContext.As<AdaptableControl>();
		ITransformAdapter transformAdapter = adaptableControl.As<ITransformAdapter>();
		transformAdapter.SetTransform(1f, 1f, 0f, 0f);
	}
}
