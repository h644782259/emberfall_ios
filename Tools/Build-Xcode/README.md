# 一键生成 Emberfall Xcode 工程

在 macOS 上双击本目录中的 **Build.command**，或在终端执行：

```bash
bash Tools/Build-Xcode/Build.command
```

默认按顺序生成模拟器和真机工程。脚本可从任意工作目录调用；以上相对路径命令需在仓库根目录运行。

```bash
# 只生成真机工程
bash Tools/Build-Xcode/Build.command device
# 只生成 Apple Silicon ARM64 模拟器工程
bash Tools/Build-Xcode/Build.command simulator
```

固定输出路径：

- 真机：`Builds/iOS/Xcode/Emberfall.xcodeproj`
- 模拟器：`Builds/iOS/Simulator/Emberfall.xcodeproj`

在 Xcode 中打开对应工程，选择 `Emberfall` Scheme。旧导出会被替换，不保留历史导出；导出失败时保留此前工程。个人签名团队沿用现有工程设置。

## 前提和操作范围

需要 macOS、完整 Xcode，以及 `ProjectSettings/ProjectVersion.txt` 指定版本的 Unity 和 iOS Build Support。若 Unity 不在默认安装位置，可设置 `UNITY_PATH` 为 Unity 可执行文件路径。

运行前关闭此项目的 Unity 和 Xcode，运行期间不要修改 Player Settings。脚本使用**本地当前代码（包含未提交修改）**，不会拉取代码、提交或推送，也不会执行 Xcode 编译、签名、安装或启动应用。

脚本会保存并恢复运行前的 `ProjectSettings.asset`，避免导出切换 SDK 留下配置修改；不会用 Git HEAD 覆盖你的本地设置。脚本通过锁目录防止自身重复运行，请勿同时手动运行底层导出脚本。

## 失败排查

Unity 详细日志：

- `Logs/ios-export-simulator-latest.log`
- `Logs/ios-export-device-latest.log`

终端出现错误后停止执行，可解决错误后重新运行。授权异常可先退出 Unity、重启 Unity Hub 并确认许可证正常。脚本被强制结束后，确认没有导出进程在运行，再删除 `Logs/xcode-export.lock`；系统临时目录中可能保留 `emberfall-player-settings.*` 设置备份。
