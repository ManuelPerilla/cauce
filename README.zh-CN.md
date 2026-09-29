# NgMusic

**Windows PowerShell 风格终端音乐播放器。**

支持三种分发方式：

- Setup.exe：推荐的图形向导。
- MSI：Windows 原生安装，交互式安装完成后自动打开配置器。
- Portable ZIP：解压即用。

交互式 MSI 与 Setup.exe 最终达到相同配置状态：Program Files、PATH、OAuth Client ID 和可选快捷方式。

静默 MSI 保持完全静默，适用于企业部署。

安装后通常只需要：

`ngmusic → login → search → play`
