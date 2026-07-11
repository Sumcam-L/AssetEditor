# 从 Assets 打开 XLP / ArtDef 实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 在 AssetEditor 的 `Open XLP` 与 `Open Art Def` 按钮下方新增两个按钮，用于从注册表 `AssetsPath` 或项目对应路径打开 XLP / ArtDef 文件。

**Architecture:** 复用 `AssetBrowserFileCommands` 现有的 ATF 命令注册与执行流程，新增两个 `Command` 枚举值、两条命令注册、一个注册表读取辅助方法、一个带自定义初始目录的文件打开辅助方法，并在 `DoCommand` 中分发。

**Tech Stack:** C# (.NET Framework 4.6.2), Windows Forms, ATF command service, Microsoft.Win32.Registry.

## Global Constraints
- 仅修改 `Firaxis.ATF\Firaxis.ATF\AssetBrowserFileCommands.cs`。
- 不改动现有 `Open XLP` / `Open Art Def` 的行为。
- 新按钮可见性为 `CommandVisibility.All`（工具栏 + File 菜单）。
- 默认目录优先读取注册表 `AssetsPath`；为空、不存在或目录不存在时回退到项目路径。
- XLP 回退路径：`CivTechService.PrimaryProject.Paths.XLPRoot`。
- ArtDef 回退路径：`CivTechService.PrimaryProject.Paths.ArtDefRoot`。

---

### Task 1: 添加注册表 using 与命令枚举值

**Files:**
- Modify: `Firaxis.ATF\Firaxis.ATF\AssetBrowserFileCommands.cs`

**Interfaces:**
- Consumes: N/A
- Produces: `Command.FileOpenXLPFromAssets`, `Command.FileOpenArtDefFromAssets`

- [ ] **Step 1: 在文件顶部添加 using 指令**

  在现有 using 块末尾添加：
  ```csharp
  using Microsoft.Win32;
  ```

- [ ] **Step 2: 扩展 Command 枚举**

  将枚举从：
  ```csharp
  private enum Command
  {
      FileNew,
      FileOpenEntity,
      FileOpenXLP,
      FileOpenArtDef,
      FileOpenArtSpecification,
      CopyFileToProject,
      MoveFileToProject,
      DeleteFile
  }
  ```
  改为：
  ```csharp
  private enum Command
  {
      FileNew,
      FileOpenEntity,
      FileOpenXLP,
      FileOpenArtDef,
      FileOpenArtSpecification,
      FileOpenXLPFromAssets,
      FileOpenArtDefFromAssets,
      CopyFileToProject,
      MoveFileToProject,
      DeleteFile
  }
  ```

- [ ] **Step 3: 编译检查**

  Run: `rtk dotnet build Firaxis.ATF\Firaxis.ATF.csproj`
  Expected: 成功（仅枚举新增，行为未变）。

---

### Task 2: 注册两个新命令

**Files:**
- Modify: `Firaxis.ATF\Firaxis.ATF\AssetBrowserFileCommands.cs`

**Interfaces:**
- Consumes: `Command.FileOpenXLPFromAssets`, `Command.FileOpenArtDefFromAssets`
- Produces: 两个新的 `CommandInfo` 注册项

- [ ] **Step 1: 在 Open Art Def 注册之后添加 From Assets 命令**

  定位到 `RegisterClientCommands()` 中类似如下代码：
  ```csharp
  if (documentClient2 != null)
  {
      CommandService.RegisterCommand(new CommandInfo(new FileCommandTag(Command.FileOpenArtDef, documentClient2), StandardMenu.File, StandardCommandGroup.FileNew, "Open Art Def".Localize("Name of a command"), "Open an existing art definition document".Localize(), shortcut4, documentClient2.Info.OpenIconName, CommandVisibility.All), this);
  }
  ```
  在其后添加：
  ```csharp
  if (documentClient != null)
  {
      CommandService.RegisterCommand(new CommandInfo(new FileCommandTag(Command.FileOpenXLPFromAssets, documentClient), StandardMenu.File, StandardCommandGroup.FileNew, "Open XLP From Assets".Localize("Name of a command"), "Open an existing XLP document from Assets".Localize(), Keys.None, documentClient.Info.OpenIconName, CommandVisibility.All), this);
  }
  if (documentClient2 != null)
  {
      CommandService.RegisterCommand(new CommandInfo(new FileCommandTag(Command.FileOpenArtDefFromAssets, documentClient2), StandardMenu.File, StandardCommandGroup.FileNew, "Open Art Def From Assets".Localize("Name of a command"), "Open an existing art definition document from Assets".Localize(), Keys.None, documentClient2.Info.OpenIconName, CommandVisibility.All), this);
  }
  ```

- [ ] **Step 2: 编译检查**

  Run: `rtk dotnet build Firaxis.ATF\Firaxis.ATF.csproj`
  Expected: 成功。

---

### Task 3: 实现注册表读取与从 Assets 打开文档的辅助方法

**Files:**
- Modify: `Firaxis.ATF\Firaxis.ATF\AssetBrowserFileCommands.cs`

**Interfaces:**
- Consumes: `FormatExtensionString`, `FileDialogService`, `m_documentClients`, `FindOrOpenExistingDocument`
- Produces: `GetAssetsInitialDirectory(string fallbackDirectory)`, `OpenExistingDocumentFromAssets(IDocumentClient client, string fallbackDirectory)`

- [ ] **Step 1: 添加注册表读取辅助方法**

  在类中任意合适位置（例如 `GetInitialDirectoryName` 附近）添加：
  ```csharp
  private string GetAssetsInitialDirectory(string fallbackDirectory)
  {
      try
      {
          using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey(@"Software\Firaxis\Civilization6_ModBuddy\2013\DialogPage\Firaxis.VisualStudio.Projects.Civ6.OptionsPages.OptionsDialogPage"))
          {
              if (registryKey != null && registryKey.GetValue("AssetsPath") is string assetsPath)
              {
                  if (!string.IsNullOrWhiteSpace(assetsPath) && Directory.Exists(assetsPath))
                  {
                      return assetsPath;
                  }
              }
          }
      }
      catch (System.Exception ex)
      {
          Outputs.WriteLine(OutputMessageType.Warning, "Failed to read AssetsPath from registry: {0}".Localize(), ex.Message);
      }
      return fallbackDirectory;
  }
  ```

- [ ] **Step 2: 添加从 Assets 打开文档辅助方法**

  在 `GetAssetsInitialDirectory` 之后添加：
  ```csharp
  private void OpenExistingDocumentFromAssets(IDocumentClient client, string fallbackDirectory)
  {
      if (client == null)
      {
          throw new ArgumentNullException("client");
      }
      string initialDirectory = GetAssetsInitialDirectory(fallbackDirectory);
      string filter = string.Format("{0} ({1})|{1}", client.Info.FileType, FormatExtensionString(client.Info.Extensions));
      string previousForcedInitialDirectory = FileDialogService.ForcedInitialDirectory;
      try
      {
          FileDialogService.ForcedInitialDirectory = initialDirectory;
          string[] pathNames = null;
          if (FileDialogService.OpenFileNames(ref pathNames, filter) == FileDialogResult.OK && pathNames != null)
          {
              foreach (string pathName in pathNames)
              {
                  IDocumentClient firstClientForPath = m_documentClients.Select((Lazy<IDocumentClient> lazy) => lazy.Value).GetFirstClientForPath(pathName);
                  if (firstClientForPath != null)
                  {
                      Uri docUri = new Uri(pathName, UriKind.RelativeOrAbsolute);
                      FindOrOpenExistingDocument(firstClientForPath, docUri);
                  }
              }
          }
      }
      finally
      {
          FileDialogService.ForcedInitialDirectory = previousForcedInitialDirectory;
      }
  }
  ```

- [ ] **Step 3: 编译检查**

  Run: `rtk dotnet build Firaxis.ATF\Firaxis.ATF.csproj`
  Expected: 成功。

---

### Task 4: 在 DoCommand 中分发新命令

**Files:**
- Modify: `Firaxis.ATF\Firaxis.ATF\AssetBrowserFileCommands.cs`

**Interfaces:**
- Consumes: `Command.FileOpenXLPFromAssets`, `Command.FileOpenArtDefFromAssets`, `OpenExistingDocumentFromAssets`
- Produces: 命令执行行为

- [ ] **Step 1: 修改 FileCommandTag 分支**

  定位到 `DoCommand` 中如下分支：
  ```csharp
  else if (commandTag is FileCommandTag { Editor: var editor } fileCommandTag)
  {
      if (fileCommandTag.Command == Command.FileNew)
      {
          OpenNewDocument(editor);
      }
      else
      {
          OpenExistingDocument(editor, null);
      }
  }
  ```
  改为：
  ```csharp
  else if (commandTag is FileCommandTag { Editor: var editor } fileCommandTag)
  {
      if (fileCommandTag.Command == Command.FileNew)
      {
          OpenNewDocument(editor);
      }
      else if (fileCommandTag.Command == Command.FileOpenXLPFromAssets)
      {
          OpenExistingDocumentFromAssets(editor, CivTechService.PrimaryProject.Paths.XLPRoot);
      }
      else if (fileCommandTag.Command == Command.FileOpenArtDefFromAssets)
      {
          OpenExistingDocumentFromAssets(editor, CivTechService.PrimaryProject.Paths.ArtDefRoot);
      }
      else
      {
          OpenExistingDocument(editor, null);
      }
  }
  ```

- [ ] **Step 2: 编译检查**

  Run: `rtk dotnet build Solution.sln`
  Expected: 整个解决方案成功编译。

---

### Task 5: 验证

**Files:**
- N/A（运行时验证）

**Interfaces:**
- Consumes: 编译后的 `AssetEditor.exe`

- [ ] **Step 1: 启动 AssetEditor**

  Run: 启动 `AssetEditor\bin\x64\Debug\net462\AssetEditor.exe`（或对应输出目录）。

- [ ] **Step 2: 确认 UI**

  - 在工具栏中，`Open XLP From Assets` 应位于 `Open XLP` 之后，`Open Art Def From Assets` 应位于 `Open Art Def` 之后。
  - 在 File 菜单中，新命令应出现在相同相对位置。

- [ ] **Step 3: 验证注册表优先路径**

  - 设置注册表项：
    - 路径：`HKEY_CURRENT_USER\Software\Firaxis\Civilization6_ModBuddy\2013\DialogPage\Firaxis.VisualStudio.Projects.Civ6.OptionsPages.OptionsDialogPage`
    - 值名称：`AssetsPath`
    - 值数据：一个存在的目录（例如 `C:\Temp\Assets`）。
  - 点击 `Open XLP From Assets` / `Open Art Def From Assets`，确认文件对话框默认目录为该目录。

- [ ] **Step 4: 验证回退路径**

  - 删除或清空 `AssetsPath`。
  - 重新打开对话框，确认默认目录分别回退到当前项目的 `XLPRoot` 和 `ArtDefRoot`。

- [ ] **Step 5: 验证原有按钮不受影响**

  - 点击原有 `Open XLP` / `Open Art Def`，确认默认目录仍按原有逻辑（项目 `XLPRoot` / `ArtDefRoot`）打开。
