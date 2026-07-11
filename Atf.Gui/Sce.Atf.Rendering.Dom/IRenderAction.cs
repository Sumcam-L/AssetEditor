using System.Collections.Generic;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering.Dom;

public interface IRenderAction
{
	RenderStateGuardian RenderStateGuardian { get; }

	RenderState RenderState { get; }

	TraverseState TraverseState { get; }

	IRenderObject RenderObject { get; }

	Matrix4F TopMatrix { get; }

	string Title { get; set; }

	int ViewportWidth { get; set; }

	int ViewportHeight { get; set; }

	void Set(IRenderAction other);

	void Dispatch(Scene scene, Camera camera);

	void Dispatch(ICollection<TraverseNode> traverseList, Scene scene, Camera camera);

	ICollection<TraverseNode> BuildTraverseList(Camera camera, Scene scene);

	void TraverseSubGraph(Camera camera, ICollection<TraverseNode> traverseList, SceneNode node, Matrix4F transform);

	void Clear();

	void PushMatrix(Matrix4F matrix, bool multiply);

	Matrix4F PopMatrix();

	Matrix4F GetMatrixAt(int relativeIndex);

	TraverseNode GetUnusedNode();
}
