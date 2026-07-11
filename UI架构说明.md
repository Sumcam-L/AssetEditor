# AE 项目 UI 架构说明

## 概述

本项目是 Firaxis Games 的 Civilization 6 资产编辑器工具集，UI 采用 **WinForms + WPF 混合架构**，基于 Sony ATF (Authoring Tools Framework) 框架。项目包含三个独立应用：

- **AssetEditor** — 主应用（WinForms + WPF 混合）
- **FireTuner** — Lua 调试/调参工具（纯 WinForms）
- **Nexus (CivNexus6)** — 资产转换工具（纯 WinForms）

---

## UI 框架

| 框架 | 角色 |
|---|---|
| **Windows Forms (WinForms)** | 主应用外壳、停靠系统、多数编辑器控件 |
| **WPF (Windows Presentation Foundation)** | 新视图（Asset Browser、对话框、停靠面板），通过 `WpfContentHost` 嵌入 WinForms |
| **Sce.Atf (Sony ATF)** | 核心 Shell 架构（文档管理、停靠、命令系统、属性编辑） |

### 辅助库

| 库 | 用途 |
|---|---|
| WeifenLuo DockPanel Suite | WinForms 停靠框架 |
| ScintillaNET | 代码/文本编辑器控件（脚本控制台） |
| SharpDX | 2D/3D 渲染（资产预览） |
| MEF (Managed Extensibility Framework) | 依赖注入与组件组合 |

---

## UI 代码目录结构

### AssetEditor（主应用）

```
AssetEditor/
├── AssetEditor/AssetEditor/          # 主入口
│   ├── Program.cs                    # 启动引导，MEF 组合，创建主窗口
│   ├── App.cs                        # WPF Application 类
│   └── AssetEditorForm.cs            # 主窗口（继承 MainForm）
│
├── Atf.Gui.WinForms/                 # WinForms UI 框架层
│   └── Sce.Atf.Applications/
│       └── MainForm.cs               # ATF 主窗口基类（菜单、工具栏、状态栏、窗口布局）
│
├── Atf.Gui.Wpf/                      # WPF UI 框架层
│   ├── Sce.Atf.Wpf.Controls/         # WPF 控件（MainWindow.xaml、PropertyGrid 等）
│   ├── Sce.Atf.Wpf.Applications/     # WPF 应用服务（AtfApp、ControlHostService）
│   ├── Sce.Atf.Wpf.Docking/          # WPF 停靠面板系统
│   └── themes/                       # WPF 主题/控件模板
│
├── Atf.Gui/                          # 共享 GUI 抽象（跨 WinForms/WPF）
│   └── 平台无关控件、曲线编辑、时间线、属性编辑、Direct2D 渲染
│
├── Firaxis.MVVMBase/                 # MVVM 基础设施
│   ├── IViewModel.cs                 # ViewModel 接口
│   ├── BaseViewModel.cs              # 抽象基类（[DependsOn] 属性依赖系统）
│   ├── DelegateCommand.cs            # ICommand 实现
│   ├── RelayCommand.cs               # ICommand 实现
│   ├── DialogService.cs              # ViewModel → View 对话框映射
│   ├── WindowFactory.cs              # 从 ViewModel 创建 WPF 对话框窗口
│   └── WpfContentHost.cs             # WinForms ↔ WPF 桥接控件（关键）
│
├── Firaxis.AssetBrowser/             # Asset Browser（WPF MVVM）
│   ├── AssetBrowserView.xaml         # 主浏览器视图（搜索、过滤、6 种显示模式）
│   ├── AssetBrowserDialogWindow.xaml # 对话框包装窗口
│   └── AssetBrowserViewModel.cs      # 主 ViewModel（过滤、选择、分页、拖放）
│
├── Firaxis.AssetEditing/             # 各类实体编辑器（318 个文件）
│   ├── BaseEntityEditor.cs           # 编辑器抽象基类
│   ├── AssetEditor.cs                # .ast 资产编辑器
│   ├── ArtDefEditor.cs               # ArtDef 编辑器
│   ├── AnimationEditor.cs            # 动画编辑器
│   ├── MaterialEditor.cs             # 材质编辑器
│   ├── GeometryEditor.cs             # 几何体编辑器
│   ├── BehaviorEditor.cs             # 行为/状态机编辑器
│   ├── EntityEditorControl.cs        # 核心实体编辑控件
│   ├── TimelineEditorControl.cs      # 时间线编辑
│   └── CurveValueEditor.cs           # 曲线编辑
│
├── Firaxis.Controls/                 # 可复用 WinForms 控件
│   ├── PropertyGrid                  # 属性网格
│   ├── ScrollableTree               # 可滚动树控件
│   ├── TimeLineControl              # 时间线控件
│   └── SliderEditor                 # 滑块编辑器
│
├── Firaxis.Theme/                    # 自定义视觉主题（72 个文件）
│   ├── FiraxisTheme.cs               # VS2015 风格停靠主题
│   ├── FiraxisToolStripRenderer.cs   # 工具条渲染器
│   └── *Palette.cs                   # 各 UI 元素颜色/样式定义
│
├── Firaxis.ATF/                      # Firaxis ATF 扩展（304 个文件）
│   ├── AssetBrowserDockWindow.cs     # 停靠式浏览器（WPF 嵌入 WinForms）
│   ├── 版本控制、项目管理、Cooking 等服务
│   └── 停靠窗口、转换器等扩展
│
├── Firaxis.AssetPreviewing/          # 3D 资产预览（Widget 系统）
├── Firaxis.Wig/                      # Wig 格式支持
├── WeifenLuo.WinFormsUI.Docking*/    # DockPanel Suite（3 个主题变体）
└── ScintillaNET/                     # Scintilla 文本编辑器
```

### FireTuner（纯 WinForms）

```
FireTuner/
└── FireTuner2/
    ├── frmMainForm.cs                # 主窗口（TabControl 面板宿主）
    ├── CustomUI.cs                   # 自定义 UI 面板基类
    ├── LuaConsole.cs                 # Lua 脚本控制台
    ├── PanelBuilder.cs               # 从 Lua 定义动态构建面板
    └── FloatControl, IntegerControl, BooleanControl, TableView 等控件
```

### Nexus / CivNexus6（纯 WinForms）

```
Nexus/
└── NexusBuddy/
    └── CivNexusSixApplicationForm.cs # 主窗口（3092 行，PropertyGrid + TabControl 布局）
```

---

## 架构层次

```
┌──────────────────────────────────────────────────────────┐
│                    应用层 (Application)                    │
│  Program.cs — WinForms 引导 + WPF App 创建                │
├──────────────────────────────────────────────────────────┤
│               ATF Shell (Sce.Atf.Applications)            │
│  MainForm (WinForms) — 菜单、工具栏、状态栏、停靠布局       │
│  CommandService / ControlHostService / DocumentRegistry   │
├────────────────────────────┬─────────────────────────────┤
│      WinForms 层            │         WPF 层               │
│  Atf.Gui.WinForms          │  Atf.Gui.Wpf                 │
│  - DockPanel Suite          │  - DockPanel (WPF)           │
│  - PropertyGrid             │  - PropertyGrid (WPF)        │
│  - TreeControl              │  - TreeListView              │
│  - Dialogs                  │  - AssetBrowser (MVVM)       │
├────────────────────────────┴─────────────────────────────┤
│          WpfContentHost (WinForms ↔ WPF 桥接)              │
├────────────────────────────┬─────────────────────────────┤
│    Firaxis.Controls        │    Firaxis.MVVMBase           │
│  - ScrollableTree           │  - BaseViewModel              │
│  - TimeLineControl          │  - RelayCommand               │
│  - PropertyGridEx           │  - DialogService              │
│  - SliderEditor             │  - WindowFactory              │
├────────────────────────────┴─────────────────────────────┤
│            Firaxis.Theme（自定义视觉主题）                   │
├──────────────────────────────────────────────────────────┤
│         Firaxis.ATF（304 个服务/扩展文件）                   │
│  AssetBrowserDockWindow / 版本控制 / 项目管理 / Cooking     │
├──────────────────────────────────────────────────────────┤
│       Firaxis.AssetEditing（318 个实体编辑器文件）           │
│  ArtDef / Asset / Animation / Material / Geometry ...     │
└──────────────────────────────────────────────────────────┘
```

---

## 关键架构模式

### 1. MEF 组合

所有服务和编辑器通过 `[Export]` / `[Import]` 特性进行依赖注入和组件组合，实现松耦合。

### 2. MVVM（WPF 部分）

`Firaxis.MVVMBase` 提供完整的 MVVM 基础设施：
- `BaseViewModel` — 带 `[DependsOn]` 属性依赖的抽象基类
- `DelegateCommand` / `RelayCommand` — ICommand 实现
- `DialogService` — ViewModel 到 View 的对话框映射
- `WindowFactory` — 从 ViewModel 创建对话框窗口

### 3. 混合宿主（WinForms ↔ WPF 桥接）

`WpfContentHost` 是核心桥接控件，允许 WPF 视图（如 AssetBrowser）嵌入 WinForms 停靠面板系统，实现新旧 UI 共存。

### 4. 文档中心

每种实体类型（Asset、ArtDef、Animation、Material 等）有独立的编辑器、文档类和上下文，由 ATF 的 `DocumentRegistry` 统一管理。

---

## 主题系统

`Firaxis.Theme` 实现了完整的 VS2015 风格自定义主题，包含 72 个文件：
- `FiraxisTheme.cs` — 停靠主题主类
- `FiraxisToolStripRenderer.cs` — 工具条自定义渲染
- `*Palette.cs` — 各 UI 元素（标题栏、标签页、菜单、滚动条等）的颜色和样式定义

---

## 启动流程

### 时序图

```
MainImpl(args)
│
├─ [1] PauseForDebugAttach         ← 可选：注册表 DebugCloud=1 时弹窗等待调试器
├─ [2] Application.EnableVisualStyles / SetCompatibleTextRenderingDefault
├─ [3] new App()                   ← WPF Application 创建
├─ [4] ImportResourceDictionary    ← 加载 Shared.xaml 资源字典
├─ [5] 构建类型列表                  ← GetCoreTypes + GetToolAppTypes + ...
│                                    GetAssetEditorTypes + GetAssetPreviewerTypes
│                                    RegisterEnvironmentParts + SettingsService
├─ [6] 初始化 EmbeddedCollectionEditor 图片资源
├─ [7] 创建 SplashScreen + Show()  ← 启动画面出现（~1s）
├─ [8] new TypeCatalog(list)       ← MEF 类型目录（耗时，包含 ~100+ 类型）
│     new CompositionContainer(...)
├─ [9] 顺序获取 MEF 导出            ← 每个 GetExport<T>().Value 触发延迟构造
│   ├─ IMessageBoxService / MessageBoxes / LogOutputWriter
│   ├─ ComposeEnvironmentParts     ← 注册环境相关导出
│   ├─ ICrashSubmissionService
│   ├─ new AssetEditorForm         ← 创建主窗口
│   ├─ compositionContainer.Compose(batch)  ← 将主窗口加入容器
│   ├─ ISingleInstanceService / ColumnarOutputService / Outputs
│   ├─ ISplashScreenOutputWriter
│   ├─ IAssetCloudSettingService   ← 触发 AssetCloudSettingService 构造（文件 I/O）
│   ├─ CivTechContext.EnsureCreated ← 加载原生 C++/CLI 代理 DLL
│   ├─ IProjectSelectionService     ← 触发构造（读 AssetCloud.env JSON × 2）
│   ├─ IProjectConfigService / IVersionControlSelectionService
│   ├─ IProjectMapService           ← 触发构造（目录创建、Art.xml 读取、依赖遍历）
│   ├─ IWorkspaceDependencyRegistryService
│   └─ ICivTechService / AutoDocumentService
├─ [10] compositionContainer.InitializeAll()
│   │                             ← 调用 ~60+ 服务的 Initialize() 方法
│   │                             包括 SettingsService / SkinService /
│   │                             ControlHostService / WindowLayoutService 等
├─ [11] 注册 splash.Close() 到 form.Shown 事件
└─ [12] Application.Run(assetEditorForm)  ← 消息循环开始
                              随后 form.Shown 触发 → splash.Close()
```

### 各阶段耗时（Profiler 实测）

| 阶段 | 耗时 | 说明 |
|------|:----:|------|
| MEF Lazy 初始化 | ~4051ms | `Lazy<T>.LazyInitValue` — 合成依赖解析 |
| InitializeAll | ~1487ms | 60+ 服务的 `Initialize()` 调用 |
| 消息循环 | ~2034ms | `RunMessageLoop` — 含首次布局/绘制 |
| 资源加载 | ~233ms | `Resources.cctor` |
| SplashScreen Show | ~206ms | 首次窗口创建/显示 |
| ComposeEnvironmentParts | ~106ms | 环境参数处理 |
| 其他 | ~200ms | App 创建、StringLocalizer、少量反射 |

### 启动瓶颈一览

| 瓶颈 | 类型 | 影响 | 状态 |
|------|------|:----:|------|
| MEF TypeCatalog 创建（100+ 类型反射） | 架构层 | ~1s | 未优化 |
| InitializeAll 顺序调用 60+ 服务 | 架构层 | ~1.5s | 未优化 |
| AssetCloudSettingService 验证文件存在 | 文件 I/O | ~100ms | 未优化 |
| ProjectMapService 目录创建 + Art.xml 读取 | 文件 I/O | ~200ms | 未优化 |
| ProjectSelectionService JSON 文件读取 | 文件 I/O | ~100ms | 未优化 |
| CivTechContext 原生 DLL 加载 | 反射 | ~200ms | 未优化 |
| **SplashScreen 显示过晚** | 视觉 | **~4s 空窗** | 可修复：移到 TypeCatalog 前 |

---

## 运行时渲染链路

### WinForms 消息流

```
用户操作（鼠标、键盘、resize）
  │
  ├→ Windows 消息队列
  │     │
  │     ├→ WM_NCPAINT (0x0085)     ── FormNcRenderer.WndProc
  │     │                              └→ PaintTitleBar()
  │     │                                 用 GetDCEx + BufferedGraphics
  │     │                                 深色背景 + 自定义按钮 + 标题文字
  │     │
  │     ├→ WM_PAINT (0x000F)        ── DockPanel.OnPaint（根面板）
  │     │                              └→ 递归子控件 OnPaint
  │     │                                  ├→ FiraxisDockPane.OnPaint
  │     │                                  │    → Firaxis 主题绘制面板内容
  │     │                                  ├→ FiraxisDockPaneCaption.OnPaint
  │     │                                  │    → 面板标题栏
  │     │                                  ├→ FiraxisDockPaneStrip.OnPaint
  │     │                                  │    → 面板标签条
  │     │                                  ├→ PropertyGridView.OnPaint
  │     │                                  │    → 属性网格
  │     │                                  └→ 各种子控件 OnPaint
  │     │
  │     ├→ WM_SIZE / WM_SIZING      ── Control.OnSizeChanged
  │     │                              └→ SuspendLayout / ResumeLayout 级联
  │     │                                  DockPane.OnSizeChanged
  │     │                                  → DockPane.OnLayout
  │     │                                  → DockPane.SetContentBounds
  │     │                                  → DockContent.OnSizeChanged（递归）
  │     │
  │     ├→ WM_NCLBUTTONDOWN         ── FormNcRenderer 拦截自定义按钮点击
  │     ├→ WM_NCMOUSEMOVE           ── 标题栏按钮悬停状态刷新
  │     └→ WM_NCACTIVATE            ── 激活/非激活状态切换
  │
  └→ System.Windows.Forms.Application.Idle
        └→ 各服务检查命令状态 → 决定是否 Invalidate()
            ├→ CommandService.Application_Idle（已节流 100ms）
            ├→ CommandControl.Application_Idle（已节流 100ms）
            ├→ ModelInstanceStateEditor.Application_Idle（已节流 100ms）
            └→ TimelineTreeCommands.Application_Idle（已节流 100ms）
```

### 布局级联链（性能关键路径）

```
Form.OnSizeChanged
  → FormNcRenderer 忽略（NC 区域单独处理）
  → DockPanel.OnLayout          ← WeifenLuo 根布局
    → 每个 DockPane.OnSizeChanged
      → DockPane.OnLayout
        → DockPane.SetContentBounds
          → DockContent.OnSizeChanged  ← 每个停靠内容递归
            → 内部控件 OnSizeChanged
              → 进一步 SuspendLayout/ResumeLayout
```

### Profiler 渲染热点

| 函数 | CPU 占比 | 文件 |
|------|:--------:|------|
| `FormNcRenderer.WndProc` | ~3-5% | `Sce.Atf.Applications.FormNcRenderer` |
| `DockPane.WndProc` | ~4.5% | WeifenLuo |
| `FiraxisDockPane.OnPaint` | ~0.3% | Firaxis.Theme |
| `PropertyGridView.OnPaint` | ~0.3% | Atf.Gui.WinForms |
| 其他 OnPaint 合计 | ~0.2% | 多个 |

> **关键结论**：Firaxis 主题 OnPaint 只占 ~0.3%，不是性能瓶颈。
> 真正开销来自：自定义 NC 绘制 + 布局级联 + Application.Idle 命令刷新。

---

## Tab 切换 UI 刷新链路结论

### 现象拆分

多文档 tab 切换时的视觉问题不是单一来源，至少包含三条不同链路：

| 区域 | 根因 | 当前结论 |
|------|------|----------|
| Asset Previewer 本体 | `PreviewerDocumentService.ActiveDocumentChanging` 提前 `ActiveDisplay.UnbindWindow()` | 已确认：preview-to-preview 切换时应保持旧 display 绑定，等新 preview 绑定后直接替换内容 |
| Previewer Knobs 的 subgroup tabs | `KnobSetEditingControl` 每次 `RemoveAllContent()`，导致 DockContent 被 Hide/Dispose 后重建 | 已确认：同结构 subgroup tabs 应复用现有 `PropertyTreeControl`，只替换 `TypeDescriptor` |
| 中间/右侧 UI 的空白 | `DockPanelExtensions.SetLayoutState()` 重新加载 XML layout，将 DockContent 临时设为 `DockState.Unknown` / `Pane=null` / `Parent=null` | 已确认：真正空白来自 DockContent 被从 DockPane 摘除，不是 `OnPaintBackground` 或 active tab 选择问题 |

### Asset Previewer 稳定性的关键

原流程：

```
DocumentRegistry.ActiveDocumentChanging
  → ActiveDisplay.UnbindWindow()
  → ActiveWindow = null
  → 新文档 ActiveDocumentChanged
  → BindWindow(...)
```

这会在新 preview 绑定前制造一个“无 native preview window”的空档。正确模型是：

```
preview 文档 → preview 文档：保持旧 ActiveDisplay 绑定，直到新内容 BindWindow
preview 文档 → 无 preview 文档：显式 UnbindWindow 清空旧内容
```

对应位置：

```
Firaxis.AssetPreviewing/Firaxis.AssetPreviewing/PreviewerDocumentService.cs
```

### KnobSetEditingControl 的正确刷新模型

原流程：

```
ActiveKnobSet 切换
  → RemoveAllSubgroubTabs()
  → DockHostControl.RemoveAllContent()
  → DockContent.Hide()
  → DockContent.Dispose()
  → AddSubgroubTabs()
```

这会让 subgroup tab 区域进入真实空状态。正确模型是：

```
新旧 subgroup 名称集合一致：
  → 复用现有 DockContent / PropertyTreeControl
  → PropertyTreeControl.SetTypeDescriptor(new proxy)

新旧 subgroup 结构不同：
  → 才允许 remove/add rebuild
```

对应位置：

```
Firaxis.AssetPreviewing/Firaxis.AssetPreviewing/KnobSetEditingControl.cs
Firaxis.AssetPreviewing/Firaxis.AssetPreviewing/PropertyTreeControl.cs
```

### LayoutState 是 DockPanel 空白的主要来源

底层调用栈已确认，普通 tab 切换或 preview context 激活时可能触发：

```
PreviewerDocumentService.ActivateDocumentPreview
  → PreviewerKnobService.SetActivePreviewModule
  → PreviewerKnobService.LoadGlobalPreviewLayout
  → DockHostControl.LayoutState = ...
  → DockPanelExtensions.SetLayoutState(...)
  → DockContent.DockState = DockState.Unknown
  → DockContent.DockPanel = null
  → DockContent.Pane = null
  → DockContent.Form.Parent = null
```

典型 log 证据：

```
DockTraceStack: content=Entity visible=Document->Unknown
stack=DockContentHandler.SetDockState
  <- DockContent.set_DockState
  <- DockPanelExtensions.SetLayoutState
  <- DockHostControl.set_LayoutState
  <- PreviewerKnobService.LoadGlobalPreviewLayout
  <- PreviewerKnobService.SetActivePreviewModule
  <- PreviewerDocumentService.ActivateDocumentPreview
```

所以这类空白不是绘制层擦背景导致的，下面这些方向已验证无效：

```
冻结整个 AssetEditorForm
冻结外层 DockPanel
冻结 BaseEntityEditor.Activate 的 document control
恢复 AssetEditorControl 内部 ActiveContent
在 AssetEditorControl.OnPaintBackground 跳过 base.OnPaintBackground
```

这些都挡不住 `DockState.Unknown / Pane=null / Parent=null`，因为子窗口已经被 DockPanel 状态机摘除了。

### ThemeChanged 误触发问题

另一个已确认的 detach/recreate 来源是 theme change：

```
ThemeService.ThemeChanged
  → DockHostControl.ThemeService_ThemeChanged
  → DetachControlInfos()
  → DockContent.Hide()
  → DockContent.Dispose()
  → ReattachControlInfos(...)
```

启动时 `ThemeService` 默认已经是 `new FiraxisTheme()`，但 `AssetEditorConfigurer` 又执行：

```
themeSvc.ActiveTheme = new FiraxisTheme();
```

如果只按对象引用比较，会把同类型 theme 当成新 theme，从而触发所有 DockHostControl 的 detach/reattach。正确模型是：同类型 theme 的重复赋值不应触发 `ThemeChanged`。

对应位置：

```
Firaxis.ATF/Firaxis.ATF/ThemeService.cs
AssetEditor/AssetEditor/AssetEditorConfigurer.cs
Firaxis.ATF/Firaxis.ATF/DockHostControl.cs
```

### PreviewerKnobService LayoutState 防抖

`DockHostControl.LayoutState` 内部虽然比较：

```
if (m_dockPanel.GetLayoutState() != value)
    m_dockPanel.SetLayoutState(value, m_dockContent);
```

但 `GetLayoutState()` 生成的 XML 可能与保存的 XML 在格式、顺序或运行时状态上不同，导致同一布局被反复应用。正确策略是在更高层记录“已应用 layout string”，避免对相同 general/global/entity layout 重复调用 `LayoutState = ...`。

对应位置：

```
Firaxis.AssetPreviewing/Firaxis.AssetPreviewing/PreviewerKnobService.cs
```

推荐缓存维度：

```
m_appliedGeneralLayout
m_appliedGlobalLayout
m_appliedEntityLayout
```

### 构建注意事项

本仓库部分底层项目不会因为主 `AssetEditor.csproj` 构建而自动重新编译。修改底层 docking 或 ATF 项目后，应直接构建对应项目，再构建主应用：

```
rtk dotnet build WeifenLuo.WinFormsUI.Docking\WeifenLuo.WinFormsUI.Docking.csproj
rtk dotnet build Firaxis.ATF\Firaxis.ATF.csproj
rtk dotnet build Firaxis.AssetPreviewing\Firaxis.AssetPreviewing.csproj
rtk dotnet build AssetEditor\AssetEditor.csproj
```

否则可能出现源代码已修改但 SDK 目录仍加载旧 DLL 的情况。

---

## 后台服务架构

### EntityCacheService

实体缓存预热是启动后最大的后台工作负载。

```
EntityCacheService 构造函数
  └→ UpdateEntityMap()
       ├→ 清空 EntityDataCache / SeenEntities
       ├→ foreach (project)                      ← 原 Parallel.ForEach 已改为 foreach
       │    └→ Parallel.ForEach (files)
       │         ├→ depReg.GetFileInfo(uri)       ← 文件数据库查询
       │         ├→ StaticMethods.GetEntityIDFromPath ← URI → EntityID 解析
       │         ├→ [已删除] LayeredPantry.GetPantryPath  ← 原 48% CPU 瓶颈
       │         ├→ CivTechService.GetProjectName ← 项目归属解析
       │         └→ AddToCache_Locking            ← 写入缓存（写锁）
       │
       └→ 注册 WorkspaceDependencyWatcher 事件
            ├→ WorkspaceItemAdded   → AddFileToCache
            ├→ WorkspaceItemRemoved → RemoveFileFromCache
            └→ WorkspaceItemChanged → UpdateCachedEntity
```

| 项 | 修复前 | 修复后 |
|----|:-----:|:-----:|
| CPU 占比 | ~56% (23010ms) | ~14.6% (3130ms) |
| `GetPantryPath` (native C++) | 48% (19706ms) | **已删除** |
| 外层 Project 循环 | `Parallel.ForEach` | `foreach`（顺序） |
| 内层 File 循环 | `Parallel.ForEach` | `Parallel.ForEach`（保留） |

### Application.Idle 刷新模式

ATF 使用 WinForms `Application.Idle` 事件作为"心跳"，在 UI 线程空闲时刷新命令状态。这是 ATF 框架层设计的核心模式。

```
Application.Idle 事件
  │
  ├→ CommandService.Application_Idle（+100ms 节流）
  │    └→ 遍历 ICommandClient 列表，检查 CanDoCommand
  │        → 更新 UI 状态（启用/禁用、选中）
  │        → 部分操作调用 Invalidate()
  │
  ├→ CommandControl.Application_Idle（+100ms 节流）
  │    └→ 更新当前控件关联的命令状态
  │
  ├→ ModelInstanceStateEditor.Application_Idle（+100ms 节流）
  │    └→ 检查模型实例状态一致性
  │
  └→ TimelineTreeCommands.Application_Idle（+100ms 节流）
       └→ 更新时间线命令状态
```

| 位置 | 原始 CPU | 节流后 CPU | 方法 |
|------|:-------:|:----------:|------|
| `CommandService` | 540ms (3.15%) | 45ms (0.25%) | 100ms throttle |
| `CommandControl` | 313ms (1.83%) | 27ms (0.15%) | 100ms throttle |
| `ModelInstanceStateEditor` | 168ms | 7ms (0.04%) | 100ms throttle |
| `TimelineTreeCommands` | 20ms | 1ms (0.01%) | 100ms throttle |

**每个 Idle 处理链都包含 `File.Exists` + `PathCompare` + `CanDoCommand` 方法调用**，这是原始性能问题的根因。

---

## 性能瓶颈汇总

### 按归属分类

| 归属 | 热点 | CPU 占比(前) | CPU 占比(后) | 优化手段 |
|------|------|:----------:|:----------:|----------|
| **ATF 框架** | `CommandService.Application_Idle` | ~3.15% | ~0.25% | 节流 100ms |
| ATF 框架 | `FormNcRenderer.WndProc` | ~3-5% | ~3-5% | 未优化（可跳过非活动窗口绘制） |
| ATF 框架 | `ControlHostService` | ~0.5% | ~0.5% | 未优化 |
| ATF 框架 | MEF 启动合成 | ~4s | ~4s | 可提前 splash 显示 |
| ATF 框架 | `InitializeAll()` | ~1.5s | ~1.5s | 未优化 |
| **Firaxis 扩展** | `EntityCacheService.UpdateEntityMap` | ~56% | ~14.6% | 删除无用 GetPantryPath + 简化并行 |
| Firaxis 扩展 | `ProjectMapService` 目录创建 | ~200ms | ~200ms | 未优化（首次启动后为空操作） |
| Firaxis 扩展 | 项目 JSON/XML 文件 I/O | ~300ms | ~300ms | 未优化 |
| Firaxis 扩展 | `AudioLoader.Dispose()` 抛异常 | 崩溃 | 已修复 | 移除 NotImplementedException |
| Firaxis 扩展 | `ToolHostLoaderService` 路径查找 | ~100ms | ~100ms | 已添加注册表回退 |
| **第三方** | WeifenLuo 布局级联（resize） | ~10% | ~10% | SuspendLayout 反效果，回退 |

### 优化前后总 CPU 对比

| 指标 | 优化前 | 优化后 | 变化 |
|------|:-----:|:-----:|:----:|
| 总 CPU (典型场景) | 40877ms | 21440ms | **-47%** |
| EntityCache 后台工作 | 23010ms (56%) | 3130ms (14.6%) | **-86%** |
| `GetPantryPath` (native) | 19706ms (48%) | 0ms (0%) | **完全消除** |
| Application.Idle 刷新合计 | ~1041ms | ~80ms | **-92%** |
| 线程池负载 | ~28900ms | ~6558ms | **-77%** |
