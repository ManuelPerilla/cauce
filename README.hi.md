# NgMusic

**Windows के लिए PowerShell-प्रेरित terminal music controller।**

[English](README.md) · [Español](README.es.md) · [简体中文](README.zh-CN.md) · [हिन्दी](README.hi.md) · [Français](README.fr.md)

## आसान OAuth setup

अब बस चलाएँ:

```text
PS Music:\> login
```

अगर Google OAuth Client ID configured नहीं है, NgMusic interactive setup wizard खोलता है। **Desktop app** Client ID एक बार paste करें।

Non-secret Client ID यहाँ save होता है:

`%LOCALAPPDATA%\NgMusic\config.json`

OAuth tokens Windows Credential Manager में रहते हैं।

Commands:

`setup`, `config show`, `config path`, `config reset`

Environment variables अभी भी supported हैं और local config से priority लेते हैं।

[Latest Release](https://github.com/ManuelPerilla/ngmusic/releases/latest)

English documentation canonical है।
