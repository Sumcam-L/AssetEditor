# Hotload Deployed-Mod Staging Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Copy the current cook's ArtDef and BLP outputs into the active deployed Mod before Civ6 receives the `HOTLOAD` request.

**Architecture:** Add an isolated `HotLoadFileStager` that validates source/deployed Mod identity, maps relative hotload paths to cooked and deployed roots, and atomically replaces deployed files. `TunerService.HotLoad` materializes the request paths, invokes the stager only in ModTools mode, and returns without sending when staging throws.

**Tech Stack:** C#/.NET 8, `System.IO`, `System.Xml`, existing Firaxis Output logging, a standalone console regression project.

## Global Constraints

- Resolve Personal with `Environment.SpecialFolder.Personal`; do not hard-code a drive or username.
- Require matching `<Guid>` in `<project>.civ6proj` and `<Mod id>` in `<project>.modinfo`.
- Copy only ArtDef and BLP paths in the current `IHotLoadData`.
- Reject rooted and parent-traversal relative paths.
- Stage every requested file before sending `HOTLOAD`; any staging error suppresses the socket request.
- Leave dependency-only Hotload and non-ModTools AssetCloud behavior unchanged.

---

### Task 1: Hotload File Stager

**Files:**
- Create: `Firaxis.ATF/Firaxis.ATF/HotLoadFileStager.cs`
- Create: `docs/superpowers/repros/hotload-file-stager/HotLoadFileStager.Tests.csproj`
- Create: `docs/superpowers/repros/hotload-file-stager/Program.cs`

**Interfaces:**
- Produces: `HotLoadFileStager(string projectName, string gamePantry, string artDefOutputRoot, string xlpOutputRoot, string personalFolder)`.
- Produces: `void Stage(IEnumerable<string> relativeArtDefPaths, IEnumerable<string> relativePackagePaths)`.
- Throws: `InvalidOperationException`, `FileNotFoundException`, `IOException`, or XML exceptions; callers treat all as an aborted Hotload.

- [ ] **Step 1: Write the failing regression executable**

The executable creates temporary source/deployed trees, writes matching and mismatching `.civ6proj`/`.modinfo` XML, calls `Stage`, and asserts:

```csharp
Run("copies ArtDef and BLP", TestCopiesArtDefAndBlp);
Run("creates nested directories", TestCreatesNestedDirectories);
Run("rejects mismatched GUID", TestRejectsMismatchedGuid);
Run("rejects missing deployed Mod", TestRejectsMissingDeployedMod);
Run("rejects rooted paths", TestRejectsRootedPath);
Run("rejects parent traversal", TestRejectsParentTraversal);
Run("rejects missing cooked source", TestRejectsMissingSource);
Run("replaces existing destination", TestReplacesExistingDestination);
```

The test project links the production source directly:

```xml
<Compile Include="..\..\..\..\Firaxis.ATF\Firaxis.ATF\HotLoadFileStager.cs" Link="HotLoadFileStager.cs" />
```

- [ ] **Step 2: Run the regression executable and verify RED**

Run:

```powershell
rtk dotnet run --project "docs/superpowers/repros/hotload-file-stager/HotLoadFileStager.Tests.csproj"
```

Expected: build fails because `HotLoadFileStager.cs` does not exist.

- [ ] **Step 3: Implement the minimal stager**

Implement identity loading with namespace-independent XPath:

```csharp
string sourceId = LoadXmlValue(sourceProjectPath, "//*[local-name()='Guid']");
string targetId = LoadXmlValue(targetModInfoPath, "/*[local-name()='Mod']/@id");
if (!Guid.TryParse(sourceId, out Guid sourceGuid) ||
    !Guid.TryParse(targetId, out Guid targetGuid) ||
    sourceGuid != targetGuid)
{
    throw new InvalidOperationException("The deployed Mod does not match the active AssetEditor project.");
}
```

Map ArtDefs beneath `ArtDefOutputRoot` to `<Mod>\ArtDefs`, map BLPs beneath `XLPOutputRoot` to `<Mod>\Platforms\Windows\BLPs`, validate full paths remain under roots, and replace through a temporary sibling file.

- [ ] **Step 4: Run the regression executable and verify GREEN**

Run the same `dotnet run` command. Expected: eight PASS lines and exit code 0.

### Task 2: TunerService Integration

**Files:**
- Modify: `Firaxis.ATF/Firaxis.ATF/TunerService.cs:257-315`
- Modify: `docs/superpowers/repros/hotload-file-stager/Program.cs`

**Interfaces:**
- Consumes: `HotLoadFileStager.Stage(...)` from Task 1.
- Preserves: `ITunerService.HotLoad(IHotLoadData)` signature.

- [ ] **Step 1: Add a failing send-suppression regression**

Extract a testable internal staging wrapper if necessary, then assert that a staging exception causes the request callback count to remain zero and successful staging invokes it once. Keep dependency-only requests outside staging.

- [ ] **Step 2: Run the regression executable and verify RED**

Expected: failure because `TunerService.HotLoad` does not invoke staging.

- [ ] **Step 3: Integrate staging before socket send**

Materialize and deduplicate the three hotload path sets. When `Resources.ModTools` is true and either ArtDef or BLP paths exist, construct the stager from:

```csharp
ProjectInfo project = _civTechService.PrimaryProject;
ProjectPaths paths = project.Paths;
string personal = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
```

Catch staging exceptions, log an Output error containing the project name and exception message, and `return` before `SocketConnection.Request(...)`. Log every successful source/destination copy from the stager through an injected or returned record.

- [ ] **Step 4: Run regression and project builds**

Run:

```powershell
rtk dotnet run --project "docs/superpowers/repros/hotload-file-stager/HotLoadFileStager.Tests.csproj"
rtk dotnet build "Firaxis.ATF/Firaxis.ATF.csproj" --no-restore
rtk dotnet build "AssetEditor/AssetEditor.csproj" --no-restore
```

Expected: regression exit 0 and both builds report 0 errors.

### Task 3: Live WuwaCore Verification

**Files:**
- No production file changes.

**Interfaces:**
- Consumes: running AssetEditor, Civ6 `-Tuner`, and WuwaCore deployed Mod.

- [ ] **Step 1: Trigger an AssetEditor Hotload**

Modify/cook the selected WuwaCore Unit asset and invoke Hot Load.

- [ ] **Step 2: Compare cooked and deployed hashes**

Run `certutil -hashfile ... SHA256` for:

```text
Firaxis ModBuddy\Civilization VI\WuwaCore\Cooked\BLPs\units\units.blp
My Games\Sid Meier's Civilization VI\Mods\WuwaCore\Platforms\Windows\BLPs\units\units.blp
```

Expected: identical SHA-256 values.

- [ ] **Step 3: Verify game reload logs**

Inspect `AssetCloudTuner.log` and `ArtDef.log`. Expected: a new request and `Reloading Package` for the WuwaCore deployed BLP after staging.

- [ ] **Step 4: Check repository diff**

Run:

```powershell
rtk git diff --check
rtk git status --short
```

Expected: no whitespace errors and only intended changes plus pre-existing work.
