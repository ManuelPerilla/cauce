# 构建与发布

维护者需要 Windows 构建环境、.NET 10 SDK、网络访问和 WiX Toolset SDK。

```powershell
.\build\release.ps1 -Version 0.2.0
```

会生成 x64、ARM64、x86 的 MSI 和便携 ZIP。

Release 使用 self-contained、single-file、ReadyToRun，关闭 debug symbol，目前不启用 trimming。

发布检查：
1. 审查源代码和构建脚本。
2. 确认版本。
3. 构建全部架构。
4. 检查架构产物互不相同。
5. 生成 SHA-256。
6. 可用时完成签名。
7. 发布 GitHub Release。
8. 验证链接、签名和 hash。
9. 做 smoke test。