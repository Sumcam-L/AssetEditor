using System.Collections.Generic;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering;

public interface INode : INameable
{
	IList<IMesh> Meshes { get; }

	IList<INode> ChildNodes { get; }

	IList<IJoint> ChildJoints { get; }

	Matrix4F Transform { get; set; }
}
