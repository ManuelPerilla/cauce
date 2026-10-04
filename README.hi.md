# Cauce

**संगीत की लय बनी रहे।**

Cauce, C#, WPF और .NET 10 में बना Windows का मूल संगीत प्लेयर है। प्रीव्यू **0.6.0-alpha.1 बिना डिजिटल हस्ताक्षर** है; x64 और ARM64 के लिए पोर्टेबल ZIP तैयार किए जाते हैं।

- Windows के माध्यम से स्थानीय MP3, WAV और M4A चलाता है; फ़ाइल और codec का समर्थन आवश्यक है।
- चुना हुआ genre बनाए रखता है, निश्चित नियमों से दोहराव और कलाकारों का अंतर नियंत्रित करता है। कतार रुकने का कारण बताता है।
- ऑडियो कॉपी या बदले बिना संदर्भ और मेटाडेटा सहेजता है: अधिकतम 10,000 संदर्भ और 16 MiB।
- उपलब्ध फ़ाइल, गुम फ़ाइल और HTTPS सेवा-लिंक अलग दिखाता है। लिंक ऐप के बाहर खुलते हैं।
- कॉम्पैक्ट मोड, ट्रे, पाँच थीम, कम गति और पारदर्शिता, ट्यूटोरियल तथा समीक्षा योग्य रिपोर्ट देता है।

स्थानीय संगीत के लिए खाता नहीं चाहिए। वैकल्पिक OIDC क्लाइंट Google, Apple, Facebook और Microsoft के वास्तविक broker तथा पंजीकरण के बिना बंद रहता है। स्ट्रीमिंग कैटलॉग या क्लाउड सिंक शामिल नहीं है। मौजूदा इंटरफ़ेस स्पेनिश में है।

Windows और .NET 10 SDK से:

```powershell
dotnet run --project src/Cauce.Desktop
```

[स्थापना](docs/i18n/hi/installation.md) · [प्राथमिकताएँ](docs/i18n/hi/configuration.md) · [नियंत्रण](docs/i18n/hi/commands.md) · [समाधान](docs/i18n/hi/troubleshooting.md)

[संरचना](docs/i18n/hi/architecture.md) · [सुरक्षा](docs/i18n/hi/security.md) · [रिलीज़](docs/i18n/hi/releasing.md) · [हस्ताक्षर](docs/i18n/hi/code-signing-policy.md)

[पूरी गाइड](docs/cauce.md) · [खाते](docs/cauce-accounts.md) · [कोड](https://github.com/ManuelPerilla/cauce)

[English](README.md) · [Español](README.es.md) · [Français](README.fr.md) · हिन्दी · [简体中文](README.zh-CN.md)
