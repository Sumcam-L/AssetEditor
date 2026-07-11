using System;
using System.Collections.Generic;
using Firaxis.Error;

namespace Firaxis.CivTech;

public interface IFBXInterface : IAssemblyInstance, IDisposable
{
	IEnumerable<string> GetAnimations(string fbxFilePath);

	IEnumerable<string> GetMeshes(string fbxFilePath);

	IEnumerable<string> GetRootModels(string fbxFilePath);

	ResultCode ExportAnimation(string fbxFilePath, string outputFolder, string animName, string fgxFileName);

	ResultCode ExportGeometry(string fbxFilePath, string outputFolder, string nodeName, string fgxFileName);

	ResultCode ExportWIG(string fbxFilePath, string outputFolder, string nodeName, string wigFileName);
}
