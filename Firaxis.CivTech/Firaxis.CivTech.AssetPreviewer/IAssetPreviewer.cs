using System;
using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.CivTech.AssetPreviewer;

public interface IAssetPreviewer : IAssemblyInstance, IDisposable
{
	IKnobManager KnobManager { get; }

	uint FrameNumber { get; }

	event LogEventHandler Logger;

	event EventHandler KnobChangesComplete;

	void Startup(ICivTechService civTechSvc, IXLPRegistry xlpRegistry);

	void Shutdown();

	void FlushMessages();

	void SetTargetFPS(float targetFPS);

	void SetThrottleMode(bool shouldThrottle);

	ICachedAsset CacheAsset(IInstanceEntity entity);

	void ReloadEntity(InstanceType enttype, string entname);

	IPreviewWindow OpenWindow(IntPtr hWindow, IInstanceSet pmInstanceSet);

	void CloseWindow(IPreviewWindow window);

	IPreviewDisplay CreateDisplay(IntPtr hWindow);

	void DestroyDisplay(IPreviewDisplay pmDisplay);

	IEnumerable<string> GetAllowedLightRigClasses(string moduleName);

	bool DoesModuleSupportsLighting(string moduleName);

	bool DoesSupportsModule(string moduleName);

	IEnumerable<IPreviewerSlotInfo> GetSlotsInfo(string moduleName);
}
