# 代码签名政策

Cauce 0.6.0-rc.1 **未签名**。SignPath Foundation 审批仍在等待中；仓库目前也没有许可证。所有者需先选择许可证，再申请 SignPath Foundation 流程。不得将未签名包描述为已签名。

获得批准并启用签名后，使用以下署名：

**Free code signing provided by SignPath.io, certificate by SignPath Foundation.**

仅 [ManuelPerilla/cauce](https://github.com/ManuelPerilla/cauce) 中经维护脚本生成的产物符合条件。不得使用该签名身份为其他项目或来源不可核验的二进制签名。

ManuelPerilla 担任 committer、reviewer 和 signing approver。外部贡献合并前必须审核。**每次签名请求都需要负责人明确的人工批准。** 仓库和签名服务访问要求 MFA；私钥和凭据不得进入源码、日志或构建产物。

官方发布前，应验证来源、产品元数据、构建结果及带时间戳的签名。便携 ZIP 不修改 PATH、不要求管理员权限；数据与移除方式见[安装](installation.md)和[安全](security.md)。

发生事件时，应暂停签名和发布，确认受影响版本，保留证据，调查原因，并协调撤销和修复公告。

[主页](../../../README.zh-CN.md) · [英文指南](../../code-signing-policy.md)
