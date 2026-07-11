using Sce.Atf.Dom;
using Sce.Atf.VectorMath;

namespace Sce.Atf.Rendering;

public static class DomNodeUtil
{
	public static Vec3F GetVector(DomNode domNode, AttributeInfo attribute)
	{
		return new Vec3F((float[])domNode.GetAttribute(attribute));
	}

	public static bool GetVector(DomNode domNode, AttributeInfo attribute, out Vec3F result)
	{
		if (domNode.GetAttribute(attribute) is float[] coords)
		{
			result = new Vec3F(coords);
			return true;
		}
		result = default(Vec3F);
		return false;
	}

	public static void SetVector(DomNode domNode, AttributeInfo attribute, Vec3F v)
	{
		domNode.SetAttribute(attribute, v.ToArray());
	}

	public static Matrix4F GetMatrix(DomNode domNode, AttributeInfo attribute)
	{
		return new Matrix4F((float[])domNode.GetAttribute(attribute));
	}

	public static void SetMatrix(DomNode domNode, AttributeInfo attribute, Matrix4F m)
	{
		domNode.SetAttribute(attribute, m.ToArray());
	}

	public static Sphere3F GetSphere(DomNode domNode, AttributeInfo attribute)
	{
		Sphere3F result = default(Sphere3F);
		if (domNode.GetAttribute(attribute) is float[] array)
		{
			result.Center = new Vec3F(array[0], array[1], array[2]);
			result.Radius = array[3];
		}
		return result;
	}

	public static void SetSphere(DomNode domNode, AttributeInfo attribute, Sphere3F s)
	{
		domNode.SetAttribute(attribute, new float[4]
		{
			s.Center.X,
			s.Center.Y,
			s.Center.Z,
			s.Radius
		});
	}

	public static Box GetBox(DomNode domNode, AttributeInfo attribute)
	{
		if (domNode.GetAttribute(attribute) is float[] array)
		{
			return new Box(new Vec3F(array[0], array[1], array[2]), new Vec3F(array[3], array[4], array[5]));
		}
		return new Box();
	}

	public static void SetBox(DomNode domNode, AttributeInfo attribute, Box b)
	{
		domNode.SetAttribute(attribute, new float[6]
		{
			b.Min.X,
			b.Min.Y,
			b.Min.Z,
			b.Max.X,
			b.Max.Y,
			b.Max.Z
		});
	}
}
