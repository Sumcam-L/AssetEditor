# Hotload Deployed-Mod Staging Design

## Goal

Before AssetEditor sends a Civ6 `HOTLOAD` request, copy only the ArtDef and BLP files produced by the current cook into the deployed copy of the active Mod so the game reloads the newly cooked bytes rather than an older installed package.

## Destination Discovery

The deployed Mod root is:

```text
<Personal>\My Games\Sid Meier's Civilization VI\Mods\<active project name>
```

`<Personal>` comes from `Environment.SpecialFolder.Personal`; no drive or user path is hard-coded.

Hotload staging requires both identity files:

```text
Source: <active project GamePantry>\<project name>.civ6proj
Target: <deployed Mod root>\<project name>.modinfo
```

AssetEditor reads the source `<Guid>` and target `<Mod id="...">` case-insensitively. Missing files, malformed XML, missing IDs, or unequal IDs abort staging and Hotload.

## File Mapping

Only paths present in the current `IHotLoadData` are staged.

ArtDef mapping:

```text
<PrimaryProject.Paths.ArtDefOutputRoot>\<relative ArtDef path>
-> <deployed Mod root>\ArtDefs\<relative ArtDef path>
```

BLP mapping:

```text
<PrimaryProject.Paths.XLPOutputRoot>\<relative package path>
-> <deployed Mod root>\Platforms\Windows\BLPs\<relative package path>
```

Every relative path is normalized and validated to remain beneath both its source root and destination root. Rooted paths and `..` traversal are rejected.

Dependency files continue using the existing `DEPENDENCY:` absolute-path transfer and are not copied.

## Copy Semantics

Each destination directory is created as needed. Each source file is copied to a temporary sibling file, then atomically replaces the destination. Existing destination files may be overwritten only after project identity validation succeeds.

All requested files must stage successfully before AssetEditor sends `HOTLOAD`. A missing source or any copy failure aborts the complete request; no partial-success request is sent.

Successful copies are reported in Output as:

```text
Deployed hotload file: <source> -> <destination>
```

Failures report the active project, source, destination when available, and exception/rejection reason.

## Integration

`TunerService.HotLoad(IHotLoadData)` performs staging after it has deduplicated and validated the hotload path sets but before constructing/sending the socket request. Staging applies only when `Resources.ModTools` is true; the internal AssetCloud flow remains unchanged.

If there are no ArtDef or BLP paths, no deployment lookup is required and the current dependency-only behavior remains unchanged.

## Verification

Automated tests cover:

- deployed Mod root resolution;
- source and target GUID matching;
- missing/malformed/mismatched identity files;
- ArtDef and BLP destination mapping;
- nested destination directory creation;
- traversal/rooted-path rejection;
- source-file absence;
- replacement of an existing deployed file;
- no socket request when staging fails;
- socket request after all files stage successfully;
- dependency-only Hotload without deployed-Mod staging.

A live verification compares SHA-256 of the cooked and deployed BLP, triggers Hotload, and confirms `ArtDef.log` reloads the deployed WuwaCore path after the hashes match.
