# NgMusic

**Windows के लिए PowerShell-प्रेरित, terminal-first music controller, Google OAuth और YouTube के समर्थित embedded player के साथ।**

[English](README.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [Français](README.fr.md)

> **Project status:** शुरुआती चरण में है, लेकिन उपयोग योग्य है। Public releases GitHub Actions पर Windows x64, ARM64 और x86 के लिए build होती हैं।

## NgMusic क्या है

NgMusic Windows के लिए एक personal music shell है। यह उन उपयोगकर्ताओं के लिए बनाया गया है जो पारंपरिक media-player UI की जगह keyboard और terminal पसंद करते हैं। इसमें search, playback, volume, seek, queue और Google authentication के लिए PowerShell-जैसे commands हैं।

Playback हमेशा YouTube के visible official IFrame player के अंदर रहता है। NgMusic raw media streams extract नहीं करता, tracks download नहीं करता और hidden audio scraping नहीं करता।

## मुख्य सुविधाएँ

- Google OAuth 2.0 Desktop login, PKCE और loopback redirect।
- OAuth tokens **Windows Credential Manager** में।
- YouTube Data API v3 search।
- Terminal से visible YouTube IFrame playback control।
- Queue और playback history।
- x64, ARM64 और x86 के लिए MSI और portable ZIP।
- Self-contained releases, user को .NET install करने की जरूरत नहीं।
- GitHub Actions builds और SHA-256 checksums।

## Download

[Latest GitHub Release](https://github.com/ManuelPerilla/ngmusic/releases/latest) से package डाउनलोड करें।

| Architecture | Installer | Portable |
| --- | --- | --- |
| x64 | `*-win-x64.msi` | `*-win-x64-portable.zip` |
| ARM64 | `*-win-arm64.msi` | `*-win-arm64-portable.zip` |
| x86 | `*-win-x86.msi` | `*-win-x86-portable.zip` |

MSI Program Files में install करता है और `ngmusic` को system `PATH` में जोड़ता है। Admin approval की जरूरत पड़ सकती है। Portable build Program Files या `PATH` नहीं बदलती।

## Documentation

- [Installation](docs/i18n/hi/installation.md)
- [Configuration और OAuth](docs/i18n/hi/configuration.md)
- [Commands](docs/i18n/hi/commands.md)
- [Architecture](docs/i18n/hi/architecture.md)
- [Security और privacy](docs/i18n/hi/security.md)
- [Troubleshooting](docs/i18n/hi/troubleshooting.md)
- [Build और release](docs/i18n/hi/releasing.md)
- [Code signing policy](docs/i18n/hi/code-signing-policy.md)

English documentation canonical source है। किसी translation और English version में अंतर हो तो English version मान्य होगा।

## Signing status

SignPath Foundation approval pending है। Signing सक्रिय होने पर:

**Free code signing provided by SignPath.io, certificate by SignPath Foundation.**
