# 在 Mac 上安装到自己的 iPhone

这是 **0.4.0** 游戏源码的 iPhone 构建流程。[iPhone 独立仓库](https://github.com/h644782259/emberfall_ios)与 [Windows 主项目](https://github.com/h644782259/emberfall_win)共享四职业、技能成长、副本、装备养成、多存档和物品快捷栏玩法。推荐在 Mac 克隆后导出 Xcode 工程，再由 Xcode 直接安装到自己的设备。Windows 发行包不能转换成 iPhone 安装包。

工程要求 Unity 6000.6.4f1。本机已完成该版本运行时验证、Xcode 导出和未签名原生构建；本轮未签名安装或完成 iPhone/iPad 真机验收。证据与边界见 [本机整合验证](Validation/iOS-Integration-20261007/README.md)。

## 1. 准备 Mac 和 iPhone

- 在 Mac 安装支持所连 iPhone iOS 版本的完整 Xcode，并启动一次完成其要求的首次配置和 iOS 平台支持下载；仅 Command Line Tools 不足以安装到 iPhone。
- 用 Unity Hub 安装 `ProjectSettings/ProjectVersion.txt` 指定的 Unity **6000.6.4f1**，勾选该编辑器的 **iOS Build Support**。脚本不会替你安装或更换模块。
- 用 USB 连接 iPhone，解锁设备并按提示信任这台 Mac。较新 iOS 若提示需要开发者模式，在 iPhone 的「设置 → 隐私与安全性 → 开发者模式」中按设备提示启用。
- 在 Xcode → Settings → Accounts 添加自己的 Apple Account。安装到自己的设备可以使用免费的 **Personal Team**，不要求先加入付费 Apple Developer Program，也不需要 TestFlight。

Personal Team 的开发签名有 Apple 的期限及设备/应用数量限制，通常需要每隔数天重新用 Xcode 部署；Xcode 显示的当前规则为准。此流程适合自己的 iPhone 调试，不是公开分发安装包。

## 2. 克隆和导入 Unity

```bash
git clone https://github.com/h644782259/emberfall_ios.git
cd emberfall_ios
```

Unity Hub → Add project from disk → 选择包含 `Assets`、`Packages`、`ProjectSettings` 的仓库根目录。等待导入和编译结束。场景由 `Assets/Scenes/Main.unity` 启动，运行时生成模型和世界，无需另找美术包。

图形配置为 Built-in/Metal，iPhone ARM64、IL2CPP，横屏双方向，最低目标 iOS 15.0。开发和首轮真机测试需关注发热、帧率、刘海/灵动岛安全区、手指遮挡与触屏施法；这些设置不保证所有设备已经达到目标帧率。

## 3. 导出 Xcode 源工程

日常更新使用下面的 Mac 脚本，导出前关闭此项目的 Unity 和 Xcode。默认实际目录固定为 `Builds/iOS/Xcode`，工程名固定为 `Emberfall.xcodeproj`，scheme 为 `Emberfall`。先在临时目录导出，成功后替换固定工程并删除旧导出；失败保留当前工程。脚本沿用当前工程唯一的签名团队，可用 `--team-id` 覆盖。日志固定为 `Logs/ios-export-latest.log`。Unity 菜单直接导出仍使用 Unity 默认工程名，非空目录会停止。

```bash
# 只检查工具和模块，不启动 Unity 或构建
bash Tools/Export-iOS.sh --check-only

# 导出；将 Bundle Identifier 改成属于自己且唯一的反向域名
bash Tools/Export-iOS.sh --bundle-id com.yourname.emberfall
```

若编辑器不在默认安装位置：

```bash
bash Tools/Export-iOS.sh \
  --unity "/Applications/Unity/Hub/Editor/6000.6.4f1/Unity.app/Contents/MacOS/Unity" \
  --bundle-id com.yourname.emberfall \
  --output Builds/iOS/MyFirstExport
```

`--team-id ABCDE12345` 是可选项；不知道 Team ID 就省略，稍后在 Xcode 选择 Personal Team。不要把 Apple 账号、证书、私钥、描述文件或密码提交到仓库。脚本不读取 Apple 凭据，不会自动签名、安装、发布或上传。

输出必须位于 `Builds/iOS` 下的实际子目录。脚本成功后包含 `Emberfall.xcodeproj`、Unity/IL2CPP 源代码和游戏数据；日志位于 `Logs/ios-export-latest.log`。复制到另一台 Mac 时带上**整个输出目录**。

Windows 可执行 `pwsh -File Tools/Export-iOS.ps1 -CheckOnly` 检查模块。缺少 iOS 模块时脚本明确报错并停止，不创建伪造工程。

## 4. Xcode 选择签名和设备

1. 打开 `Builds/iOS/Xcode/Emberfall.xcodeproj`。
2. 选择蓝色工程 → **TARGETS → Unity-iPhone → Signing & Capabilities**，勾选 **Automatically manage signing**。
3. 在 **Team** 选择自己的 **Personal Team** 或已有开发团队。确认 **Bundle Identifier** 与导出时填写的一致且唯一；若提示标识被占用，换一个唯一标识。保留 UnityFramework 的 Unity 生成配置，不要给它另加无关能力。
4. 顶部 Scheme 选择 **Emberfall**，运行目标选择已连接的真实 iPhone，而不是 Simulator 或 Any iOS Device。首次配对或准备调试支持时等待 Xcode 完成。
5. 点击 **Run ▶**（Cmd+R）。Xcode 构建、开发签名并安装到所选设备。若 iPhone 要求信任开发者或开启开发者模式，按其设置提示完成后再运行。

若失败，先读 Xcode 的具体签名/设备错误。常见原因是尚未选择 Team、Bundle ID 已被占用、手机没有信任电脑、Xcode 不支持手机的 iOS 版本或开发签名过期。免费 Team 不需要通过 Archive/TestFlight 流程才能在自己的设备运行。

## 5. 触屏操作与桌面调试

- 屏幕左侧三分之一空白区域按下出现浮动摇杆控制移动；可同时用另一根手指按住右下攻击按钮进行普通攻击。
- 攻击按钮左上方是闪现，再往左上方是跳跃；攻击按钮上方是生命药剂。跳跃与闪现可越过河水，起点与落点都需可通行，不能穿过树木、岩石或墙体。
- 战斗区保留八个主动技能按钮，不翻页；可用技能高亮，两被动技能放在状态卡展示。药剂使用独立按钮并显示数量角标。
- 点击范围技能会自动选有效焦点或附近目标并释放，无需二次选点；原有蓄力技能仍显示前摇进度，有效闪现可取消蓄力。空白区域上下拖动调整镜头俯仰；列表内容可直接手指拖动滚动。
- HUD 图标可打开行囊、技能树、操作指南和暂停菜单，以及返回营地、进入副本；进入副本需靠近传送门，战斗中不能直接回营。背包与技能界面会暂停战斗。

在 Unity 桌面调试时，WASD 移动，**Space 跳跃、Shift 闪现**，左键或 J 普攻，F 使用生命药剂。按住鼠标右键可水平旋转镜头和调整俯仰，单击右键取消选点或蓄力，滚轮缩放。按 I 打开行囊、K 打开技能树，技能与药剂使用各槽的自定义按键。镜头鼠标操作仅适用于桌面调试。

## 6. 多存档与迁移

标题页点击“继续冒险”打开存档列表，按职业、等级和保存时间选择后加载。新建角色创建独立存档，不覆盖其他角色；自动保存持续写回原角色槽位。暂停菜单只有一个手动保存入口，覆盖已有角色前确认；也能读取另一角色，选择保存当前进度后读取、放弃未保存进度或取消。取消本次手动保存不关闭原有自动保存。

每个存档包含一个 `emberfall-save*.json` 文件和同名 `.bak` 备份，例如 `emberfall-save-<槽位编号>.json` 与 `emberfall-save-<槽位编号>.json.bak`；旧版 `emberfall-save.json` 及其备份也应保留。备份和迁移时应一起复制全部这些文件，避免漏掉其他角色。主文件损坏但备份可用时，列表会标记可恢复存档；无法读取的槽位不能加载。

iPhone 存档在应用自己的沙盒中，Windows 的 `%USERPROFILE%` 路径不适用。目前没有云存档或专门的 iOS 文件导入界面。删除 App 或更换 Bundle Identifier 可能导致无法读取原沙盒进度；换号或删除前，先通过 Xcode 的设备容器工具备份完整容器及其中的存档文件。个人存档不应提交到源码仓库。

## 7. 真机验收

- 检查横屏切换、文字、安全区、多指移动与攻击、跳跃、闪现、药剂按钮、技能自动瞄准与蓄力取消、蓄力、背包/技能树、第四职业召唤、副本掉落和返回营地。
- 检查八主动技能和两被动状态展示、内容拖动不误点、空白区域镜头拖动，以及河岸安全落点、桥梁通行和实体障碍阻挡。
- 检查锁屏、切到后台、来电/音频中断后，游戏暂停与声音恢复是否正常；创建多个角色、覆盖确认/取消、暂停读取、退出重启后选择不同存档加载。
- 连续战斗与多召唤物场景检查性能和温度，按 `Tests/PLAYTEST.md` 验证核心玩法。桌面通过不能替代 iPhone 真机测试。

构建日志、`Builds/iOS`、Xcode 中间输出和个人签名材料应保持为本地产物，不提交 Git。升级源码后运行相同导出命令，仍打开固定工程；成功后不保留旧导出。仅签名团队自动沿用，其他手动 Xcode 配置应放入导出逻辑，否则重新导出会重置。本机构建缓存使用 `Builds/iOS/DerivedData`。

## iPad 安装

当前导出脚本使用 `iPhoneAndiPad` 通用设备族，iPhone 与 iPad 共用工程、玩法和触控输入。横屏全屏运行，不启用分屏；布局使用当前安全区按比例缩放，较高的 iPad 屏幕保留更多场景空间。使用相同导出步骤，在 Xcode 的运行目标选择已连接的 iPad，再签名安装。旧的 iPhone-only Xcode 工程需要重新导出才能获得此设置。

验收至少覆盖 iPad 4:3 与较宽屏比例：标题、存档列表、行囊、技能树、触屏指南、底部快捷栏、双指移动攻击、自动瞄准、内容滑动、后台恢复。源码编译和字体字形检查不代表 iPad 真机验收。
