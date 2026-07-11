# 从 Assets 打开 XLP / ArtDef 设计文档

## 背景
AssetEditor 的工具栏与 File 菜单中已有 `Open XLP` 和 `Open Art Def` 按钮。用户希望在这两个按钮下方新增 `Open XLP From Assets` 和 `Open Art Def From Assets` 按钮。点击后弹出的打开文件对话框的默认目录优先取自注册表 `AssetsPath`，若缺失或为空则回退到项目对应路径。

## 目标
- 不改动现有 `Open XLP` / `Open Art Def` 的行为。
- 新增的两个按钮在工具栏和 File 菜单中显示在现有按钮之后。
- 默认目录逻辑：
  1. 读取 `HKEY_CURRENT_USER\Software\Firaxis\Civilization6_ModBuddy\2013\DialogPage\Firaxis.VisualStudio.Projects.Civ6.OptionsPages.OptionsDialogPage` 中的 `AssetsPath`。
  2. 若值为空、不存在，或目录不存在，则回退到项目路径：
     - XLP → `CivTechService.PrimaryProject.Paths.XLPRoot`
     - ArtDef → `CivTechService.PrimaryProject.Paths.ArtDefRoot`

## 方案选择
采用**方案 A：新增两个独立命令**。原因：改动最小、完全复用现有 ATF 命令框架、顺序自然排在现有按钮之后、不影响原有逻辑。

## 改动范围
仅修改 `Firaxis.ATF\Firaxis.ATF\AssetBrowserFileCommands.cs`。
在该文件顶部添加 `using Microsoft.Win32;` 以读取注册表。

## 详细设计

### 1. 命令枚举
在 `AssetBrowserFileCommands.Command` 枚举中新增：
```csharp
FileOpenXLPFromAssets,
FileOpenArtDefFromAssets,
```

### 2. 命令注册
在 `RegisterClientCommands()` 方法中，紧接现有 Open XLP / Open Art Def 注册代码之后，新增两个命令：
- 菜单：`StandardMenu.File`
- 分组：`StandardCommandGroup.FileNew`
- 可见性：`CommandVisibility.All`
- 图标：复用对应文档客户端的 `OpenIconName`
- 快捷键：`Keys.None`
- 名称：
  - `Open XLP From Assets`
  - `Open Art Def From Assets`

### 3. 路径解析辅助方法
```csharp
private string GetAssetsInitialDirectory(string fallbackDirectory)
```
- 使用 `Microsoft.Win32.Registry.CurrentUser.OpenSubKey` 打开指定注册表项。
- 读取 `AssetsPath` 字符串值。
- 若值为空、不存在，或目录不存在，返回 `fallbackDirectory`。
- 读取异常时返回 `fallbackDirectory` 并输出 warning 日志。

### 4. 从 Assets 打开文档
```csharp
private void OpenExistingDocumentFromAssets(IDocumentClient client, string fallbackDirectory)
```
- 通过 `GetAssetsInitialDirectory(fallbackDirectory)` 得到初始目录。
- 使用 `FormatExtensionString(client.Info.Extensions)` 构造文件过滤器。
- 临时设置 `FileDialogService.ForcedInitialDirectory`，调用 `FileDialogService.OpenFileNames`。
- 弹窗结束后恢复 `ForcedInitialDirectory`。
- 对选中的每个文件，调用 `FindOrOpenExistingDocument` 打开。

### 5. 命令执行
在 `DoCommand` 的 `FileCommandTag` 分支中增加：
- `FileOpenXLPFromAssets` → `OpenExistingDocumentFromAssets(documentClient, CivTechService.PrimaryProject.Paths.XLPRoot)`
- `FileOpenArtDefFromAssets` → `OpenExistingDocumentFromAssets(documentClient2, CivTechService.PrimaryProject.Paths.ArtDefRoot)`

### 6. 错误处理
- 注册表读取失败不影响主流程，静默回退。
- 对话框取消无操作。
- 非兼容扩展名按现有逻辑报错。

## 验证
- 编译 `AssetEditor` 解决方案成功。
- 启动后确认工具栏和 File 菜单中新增按钮位于 `Open XLP` / `Open Art Def` 之后。
- 设置注册表 `AssetsPath` 为有效目录，点击新按钮，对话框默认目录正确。
- 清空或删除注册表 `AssetsPath`，点击新按钮，对话框默认目录回退到项目 `XLPRoot` / `ArtDefRoot`。
