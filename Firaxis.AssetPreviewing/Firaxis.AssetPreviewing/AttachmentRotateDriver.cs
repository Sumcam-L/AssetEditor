using System;
using System.Collections.Generic;
using Firaxis.AssetEditing;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf.VectorMath;

namespace Firaxis.AssetPreviewing;

internal class AttachmentRotateDriver : APWidgetDriver
{
	public Point3F AxisAngle { get; set; }

	public AttachmentRotateDriver(WidgetFlags flags, IEnumerable<AttachmentPointAdapter> aplist)
		: base(flags, aplist)
	{
		AxisAngle = new Point3F(0f, 0f, 0f);
	}

	public override string GetNativeWidgetName()
	{
		return "Rotate";
	}

	public override void GetCustomArguments(IValueSet args)
	{
		args.Push<IFloatValue>("SnapPrecision").ParameterValue = base.Flags.GridSnappingPrecision;
	}

	public override void OnWidgetEdit(IEntityChangeList changelist)
	{
		foreach (AttachmentPointAdapter attachment in base.AttachmentList)
		{
			changelist.CreateAttachmentChangedEvent(attachment.EntityAdapter.InstanceEntity, attachment.Name, attachment.Name, attachment.ModelInstanceName, attachment.BoneName, attachment.Position, ApplyAxisAngle(attachment.Orientation), attachment.Scale);
		}
	}

	public override void OnWidgetFinish()
	{
		foreach (AttachmentPointAdapter attachment in base.AttachmentList)
		{
			attachment.Orientation = ApplyAxisAngle(attachment.Orientation);
		}
	}

	private QuatF euler_to_quat(float[] euler)
	{
		QuatF q = QuatF.FromAxisAngle(new Vec3F(1f, 0f, 0f), euler[0]);
		QuatF q2 = QuatF.FromAxisAngle(new Vec3F(0f, 1f, 0f), euler[1]);
		QuatF q3 = QuatF.FromAxisAngle(new Vec3F(0f, 0f, 1f), euler[2]);
		return QuatF.Mul(q, QuatF.Mul(q2, q3));
	}

	private float[] ApplyAxisAngle(float[] euler)
	{
		Vec3F axis = new Vec3F(AxisAngle.x, AxisAngle.y, AxisAngle.z);
		float length = axis.Length;
		axis.Normalize();
		QuatF q = QuatF.FromAxisAngle(axis, length);
		QuatF q2 = euler_to_quat(euler);
		QuatF q3 = QuatF.Mul(q2, q);
		Matrix3F matrix3F = new Matrix3F();
		matrix3F.Set(q3);
		Vec3F vec3F = new Vec3F(matrix3F.M11, matrix3F.M12, matrix3F.M13);
		vec3F.Normalize();
		Vec3F vec3F2 = new Vec3F(matrix3F.M21, matrix3F.M22, matrix3F.M23);
		vec3F2.Normalize();
		Vec3F vec3F3 = new Vec3F(matrix3F.M31, matrix3F.M32, matrix3F.M33);
		vec3F3.Normalize();
		float[] array = new float[3];
		array[2] = (float)(0.0 - Math.Atan2(vec3F.Y, vec3F.X));
		array[1] = (float)(0.0 - Math.Atan2(0f - vec3F.Z, Math.Sqrt(vec3F3.Z * vec3F3.Z + vec3F2.Z * vec3F2.Z)));
		array[0] = (float)(0.0 - Math.Atan2(vec3F2.Z, vec3F3.Z));
		return array;
	}
}
