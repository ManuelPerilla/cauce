# बिल्ड और रिलीज़

Windows और .NET 10 SDK चाहिए। रिपॉज़िटरी की जड़ से:

```powershell
dotnet run --project tests/Cauce.Core.Tests -c Release
dotnet run --project tests/Cauce.Auth.Tests -c Release
dotnet build src/Cauce.Desktop -c Release
dotnet run --project tests/Cauce.Desktop.Smoke -c Release -- artifacts/cauce-smoke
./build/cauce.ps1 -Runtime win-x64
./build/cauce.ps1 -Runtime win-arm64
./build/release.ps1 -Version 0.6.0-rc.1
```

टेस्ट लाइब्रेरी, कतार, पहचान और WPF स्थितियाँ जाँचते हैं। सिंथेटिक टेस्ट वास्तविक खाता-प्रदाताओं या दूसरे कंप्यूटरों के प्रदर्शन की पुष्टि नहीं करते।

स्क्रिप्ट रनटाइम और checksums सहित पोर्टेबल ZIP तैयार करती हैं। अधिकृत प्रमाणपत्र से हस्ताक्षर और सत्यापन न हो तो पैकेज बिना हस्ताक्षर का रहता है। इंस्टॉलर या Store पैकेज नहीं है।

`build/cauce.ps1` में `-Version` दे सकते हैं; आउटपुट `artifacts/cauce/` में है। `build/release.ps1` डिफ़ॉल्ट रूप से x64 और ARM64 बनाता है और ZIP तथा `SHA256SUMS.txt` को `artifacts/release/` में रखता है।

`.github/workflows/cauce.yml` जाँच के आर्टिफैक्ट रखता है। `release.yml` मैन्युअल रन या संस्करण टैग पर पैकेज बनाकर **ड्राफ़्ट रिलीज़** तैयार करता है। सार्वजनिक करने से पहले संस्करण, आर्किटेक्चर, checksums, जाँच के परिणाम और रिलीज़ नोट पढ़ें। हस्ताक्षर अनुरोधों पर [कोड-साइनिंग नीति](code-signing-policy.md) लागू होती है।

[मुख्य पृष्ठ](../../../README.hi.md) · [अंग्रेज़ी गाइड](../../releasing.md)
