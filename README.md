# FairiesPoker

仙子主题的非商业粉丝斗地主游戏，包含单机 AI 和 TCP 联机模式。项目保留 Windows Forms 客户端，并提供面向桌面及 Android 的 MonoGame 客户端。

## 项目声明

本项目由爱好者独立制作，与 The Walt Disney Company（迪士尼）及其关联公司无隶属、赞助或授权关系，不是官方产品。

相关角色、名称、商标、音乐及其他第三方素材的权利归相应权利人所有。非商业和粉丝作品声明不代表已取得使用或再发布授权。仓库中的历史素材仍需逐项核对来源与许可，请勿把仓库 MIT 许可理解为对这些素材的授权。

[MIT 许可](LICENSE)适用于贡献者有权许可的原创代码；第三方与生成素材的说明见 [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)。本项目不提供收费、广告、付费皮肤或赞助入口。

## 关于页面插画

原电影海报已替换为 ImageGen 辅助生成的「羽翼牌组」，另一张「月光牌桌」作为候选保留。生成方式及提示词见 [插画记录](docs/artwork/README.md)。

## 出牌特效

MonoGame 客户端包含顺子、连对、飞机、炸弹及王炸的原创程序特效。主菜单的「特效预览」可单独查看；单机和联机成功出牌后自动播放。特效不参与规则、回合倒计时、手牌同步或结算判定。

设计、生命周期及验证方式见 [特效设计](docs/play-effects.md)。

## 构建与运行

默认解决方案仅构建桌面客户端、服务端与测试，需要 .NET 9/10 SDK。Visual Studio 2026 用户请安装「.NET 桌面开发」工作负载；当前 .NET 10.0.4xx SDK 需要 VS 18.9 或更高版本。

~~~powershell
.\tools\Build-Desktop.ps1
dotnet run --project FairiesPoker.MG/FairiesPoker.MG.csproj --framework net9.0 --no-build
dotnet run --project FairiesPoker/FairiesPoker.csproj
dotnet test FairiesPoker.MG.Tests/FairiesPoker.MG.Tests.csproj
~~~

也可直接在 Visual Studio 中打开 FairiesPoker.sln，将 FairiesPoker.MG 设为启动项目，选择 Debug / Any CPU 后按 F5。调试配置还提供 Effects preview 和 About 两个入口。

桌面项目默认不加载 Android 目标，不再自动还原或运行 MGCB。纹理通过 Texture2D.FromStream 加载，字体在运行时烘焙；旧 Content.mgcb 和工具清单仅作历史内容编辑参考。没有 Android 工作负载也可以正常加载、构建和调试。

若双击解决方案启动了旧安装目录中的 VS，可运行 tools/Open-VisualStudio.ps1，用 Visual Studio Installer 登记的最新 IDE 打开。本脚本不修改文件关联、全局设置或其他项目。

### Android（显式启用）

需要 .NET 10 SDK 及 Android 工作负载，示例：

~~~powershell
dotnet publish FairiesPoker.MG/FairiesPoker.MG.csproj -p:EnableAndroidTarget=true --framework net10.0-android --configuration Release
~~~

桌面或服务端跨平台发布时显式指定 -r（如 win-x64、linux-x64 或 osx-arm64）；日常还原不会预先下载所有平台运行时。

服务端：将 FPServer/appsettings.example.json 复制为本地 appsettings.json，填写自己的配置，然后运行 FPServer 项目。不要提交凭据。
