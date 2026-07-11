using System;
using System.Collections.Generic;

namespace Firaxis.Granny;

public interface IGrannyMaterial : IDisposable
{
	string Name { get; set; }

	List<IGrannyMap> Maps { get; }

	IGrannyTexture Texture { get; }

	string ShaderSet { get; set; }

	int AlphaMode { get; set; }

	int ZMode { get; set; }

	int AlphaRef { get; set; }

	int SkinBoneCount { get; set; }

	string typeName { get; }

	string Tex0 { get; }

	int GetExtendedDataInt(string FieldName);

	float GetExtendedDataFloat(string FieldName);

	string GetExtendedDataString(string FieldName);
}
