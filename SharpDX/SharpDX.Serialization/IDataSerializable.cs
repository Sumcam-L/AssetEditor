namespace SharpDX.Serialization;

public interface IDataSerializable
{
	void Serialize(BinarySerializer serializer);
}
