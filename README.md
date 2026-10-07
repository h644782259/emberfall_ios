# Emberfall iPhone / iPad

> **最新本机整合状态：** [2026-10-07 远征、战斗 UI 与验证边界](Docs/Validation/iOS-Integration-20261007/README.md)。历史源码说明见 [2026-10-03](Docs/Release-2026-10-03.md)。

> **0.4.0 源码：立体技能动画、十格触控、存读档、三城镇/三试炼与五房远征。** Windows/iOS已同步本轮功能；尚未发布新安装包或完成引擎画面/真机验收。见 [本轮说明与验证边界](Docs/Visual-Mobile-Release.md)。 后续数值与可靠性检查见 [第三轮检查](Docs/Combat-Review-3.md)。

Unity 3D 即时战斗 RPG 的 iPhone / iPad 源码工程，与 [Windows 主项目](https://github.com/h644782259/emberfall_win) 共享四职业战斗、分支技能、召唤、蓄力、副本、装备成长、多存档和地形交互。

已加入横屏触屏布局、浮动摇杆、攻击/闪现/跳跃/药剂按钮、八主动技能按钮与两被动状态展示、点击自动瞄准施法、内容拖动滚动与多指输入。既有 0.3.1 的运行验证在 Windows 完成；本开发分支仅更新源码，验证范围见上方升级说明。**本机已完成 Unity 运行时验证、Xcode 导出和未签名原生构建；尚未完成本轮 iPhone/iPad 真机安装验收**。

## 用 Mac 安装到自己的 iPhone

1. 安装完整 Xcode，并在 Unity Hub 安装 **6000.6.4f1 + iOS Build Support**。
2. 克隆此仓库，在 Unity Hub 导入项目并等待编译完成。
3. 关闭该项目的 Unity 编辑器，在仓库目录执行：

```bash
bash Tools/Export-iOS.sh --check-only
bash Tools/Export-iOS.sh --bundle-id com.yourname.emberfall
```

4. 打开固定路径 `Builds/iOS/Xcode/Emberfall.xcodeproj`，选择 `Emberfall` scheme。以后导出更新同一路径，成功后删除旧导出。
5. 在 Xcode 的 **Signing & Capabilities** 开启自动签名，选择自己的 **Personal Team**，连接并选择 iPhone，点击 **Run**。

免费 Personal Team 可用于自己的设备调试，但签名有有效期，过期需重新部署。完整步骤、设备信任、开发者模式和错误排查见 [Mac/Xcode 安装说明](Docs/iOS.md)。工程使用 iOS 15 起、ARM64、IL2CPP、Metal，实际设备兼容性和性能仍需真机验收。

## 存档与操作

四职业新角色在 1 级自动学习并配置首个主动技能一阶。终身技能点总预算为 `max(1, level - 1)`：1 级的 1 点已投入，升至 2 级不额外给点，此后每升一级增加 1 点；首个技能二阶/三阶仍需 10/20 级。旧档保留合法已有分配。

“继续冒险”会显示存档列表，可按职业、等级和保存时间选择。新建角色使用独立存档；自动保存写回该角色原槽位；暂停菜单有单一手动保存入口和读取存档入口，覆盖/放弃当前进度均需明确确认。移动端十格技能固定显示全部技能，药剂使用独立按钮。

强化永久绑定武器、护甲、饰品三个部位，各自最高 +10；换装自动继承部位等级，按新装备自身基础属性计算，无需手动转移。背包比较显示继承后的实际属性。旧档按每个部位现有装备的最高历史等级迁移（含待领取和回收箱），不叠加；替换或出售装备不会丢失部位强化。

iPhone 存档保存在应用沙盒内，每份 `emberfall-save*.json` 有同名 `.bak` 备份。迁移时保留可恢复 `.tmp` 与 `.delete-pending` 删除标记，不单独恢复旧备份。删除 App 前请先通过 Xcode 的设备容器工具备份；目前没有云存档或应用内文件导入界面。

桌面调试使用 WASD 移动、空格跳跃、Shift 闪现，按住鼠标右键旋转/调整镜头俯仰，单击右键取消施法，滚轮缩放。河流需要走桥或从有安全落点的位置跳跃/闪现，实体岩石和墙体阻挡通行。

## 开发

项目根目录包含 `Assets`、`Packages` 和 `ProjectSettings`，无需额外美术包。场景由 `Assets/Scenes/Main.unity` 启动后在运行时生成；地图、模型和动画仍为程序化内容。

- 构建入口：`Assets/Editor/IOSBuild.cs`
- 触屏输入：`Assets/Scripts/UI/MobileControls.cs`
- Mac 导出脚本：`Tools/Export-iOS.sh`
- 验收记录与步骤：[Tests/PLAYTEST.md](Tests/PLAYTEST.md)

只提交源码和 Unity `.meta` 文件。缓存、构建结果、个人存档、证书与签名材料由 `.gitignore` 排除。

## iPad 支持与字体

导出目标为 iPhone + iPad 通用应用，横屏全屏运行，保留按安全区和屏幕比例缩放的界面与多指触控。iPad 安装时在 Xcode 选择对应设备；暂不支持分屏。操作指南在移动端显示触屏说明。界面、场景标签和掉落物使用随包携带的 Noto Sans SC 中文字体，许可位于 `Assets/Resources/Fonts/LICENSE.txt`。设备上的显示与手感仍需实际安装验收。

本开发分支已整合时装宝箱、部位强化、副本祝福/挑战模式、机制装备、回收保护，以及第二轮数值和成长改进。iPhone/iPad 触控、全屏设备族和完整中文字库沿用最新主线；需要重新导出并完成对应设备验收。
