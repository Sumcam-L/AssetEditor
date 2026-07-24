# AssetEditor

Civilization VI SDK Asset Editor 增强版，基于 Firaxis 官方 SDK 工具逆向工程并优化。

## 主要改进

### Bug 修复

1. 修复了删除多个 Attachment Points 时崩溃的问题，并优化了删除性能
2. 修复了在 Ast 中直接导入动画崩溃的问题
3. 修复了 Artdef 中首次添加条目只显示第一列属性的问题
4. 修复了特效文件只有在打开 ast 时才会加载，后续修改 ast 中的特效不会加载特效文件的问题
5. 修复了一些其他崩溃 bug

### 功能开放

6. 开放了 FbxImporter 的导入设置界面
7. 开放了音频预览功能
   - 需要在 Assets 包里面新建 `LooseAssets` 文件夹，需要加载的 bnk 放到里面（可以复制，也可以使用链接功能）
   - **注意**：`Banks.ini`、`Init.bnk`、`Init.xml`、`Init.txt` 这几个文件必须要在 `LooseAssets` 目录下，否则 bnk 无法正常加载
   - Mod 的 bnk 放在 `Platforms\Windows\Audio` 下即可，无需额外操作
8. 开放了 AssetEditor 中的 Hot Load 和 Start Civ6 功能
   - AE 里修改 ast 可以热更，不需要热更就把 Tuner 功能关闭

### 性能优化

9. 优化了程序启动、文档打开、文档切换、Asset Browser 加载的速度
10. 优化了 Artdef 的 UI 性能

---

UI性能优化全是AI做的，我完全看不懂，AI真是太强了喵。