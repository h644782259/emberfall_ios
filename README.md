# Emberfall iPhone

> **开发分支：玩法与界面升级，尚未发布新安装包。** 已增加可执行纯逻辑测试与 Unity API 源码编译检查；Unity 引擎启动在云端受本地 IPC 阻塞，画面、手感和平台构建尚未实测。功能与验收范围见 [升级说明](Docs/Gameplay-Upgrades.md) 和 [云端验证环境](Tools/cloud-validation-environment.md)。

Unity 3D 即时战斗 RPG 的 iPhone 源码工程，与 [Windows 主项目](https://github.com/h644782259/emberfall_win) 共享 0.3.1 玩法：四职业、分支技能树、召唤、蓄力、副本、装备养成、多存档选择、地形阻挡与物品快捷栏。

已加入横屏触屏布局、左摇杆、攻击/闪现/跳跃/药剂按钮、十格技能与物品栏、选点施法和多指输入。既有 0.3.1 的运行验证在 Windows 完成；本开发分支仅更新源码，验证范围见上方升级说明。**尚未导出 Xcode 工程、签名 IPA 或完成 iPhone 真机测试**。

## 用 Mac 安装到自己的 iPhone

1. 安装完整 Xcode，并在 Unity Hub 安装 **6000.6.3f1 + iOS Build Support**。
2. 克隆此仓库，在 Unity Hub 导入项目并等待编译完成。
3. 关闭该项目的 Unity 编辑器，在仓库目录执行：

```bash
bash Tools/Export-iOS.sh --check-only
bash Tools/Export-iOS.sh --bundle-id com.yourname.emberfall
```

4. 打开输出目录中的 `Unity-iPhone.xcworkspace`（若存在），否则打开 `Unity-iPhone.xcodeproj`。
5. 在 Xcode 的 **Signing & Capabilities** 开启自动签名，选择自己的 **Personal Team**，连接并选择 iPhone，点击 **Run**。

免费 Personal Team 可用于自己的设备调试，但签名有有效期，过期需重新部署。完整步骤、设备信任、开发者模式和错误排查见 [Mac/Xcode 安装说明](Docs/iOS.md)。工程使用 iOS 15 起、ARM64、IL2CPP、Metal，实际设备兼容性和性能仍需真机验收。

## 存档与操作

“继续冒险”会显示存档列表，可按职业、等级和保存时间选择。新建角色使用独立存档；暂停菜单可另存快照，后续自动保存写入新槽。生命药剂可从背包放入快捷栏，并与技能拖动交换位置。

强化永久绑定武器、护甲、饰品三个部位，各自最高 +10；换装自动继承部位等级，按新装备自身基础属性计算，无需手动转移。背包比较显示继承后的实际属性。旧档按每个部位现有装备的最高历史等级迁移（含待领取和回收箱），不叠加；替换或出售装备不会丢失部位强化。

iPhone 存档保存在应用沙盒内，每份 `emberfall-save*.json` 有同名 `.bak` 备份。删除 App 前请先通过 Xcode 的设备容器工具备份；目前没有云存档或应用内文件导入界面。

桌面调试使用 WASD 移动、空格跳跃、Shift 闪现，按住鼠标右键旋转/调整镜头俯仰，单击右键取消施法，滚轮缩放。河流需要走桥或从有安全落点的位置跳跃/闪现，实体岩石和墙体阻挡通行。

## 开发

项目根目录包含 `Assets`、`Packages` 和 `ProjectSettings`，无需额外美术包。场景由 `Assets/Scenes/Main.unity` 启动后在运行时生成；地图、模型和动画仍为程序化内容。

- 构建入口：`Assets/Editor/IOSBuild.cs`
- 触屏输入：`Assets/Scripts/UI/MobileControls.cs`
- Mac 导出脚本：`Tools/Export-iOS.sh`
- 验收记录与步骤：[Tests/PLAYTEST.md](Tests/PLAYTEST.md)

只提交源码和 Unity `.meta` 文件。缓存、构建结果、个人存档、证书与签名材料由 `.gitignore` 排除。
