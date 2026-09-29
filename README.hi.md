# NgMusic

**Windows के लिए PowerShell-inspired terminal music player।**

तीन supported packages हैं:

- Setup.exe: recommended graphical wizard
- MSI: native Windows installer, interactive install के बाद configurator
- Portable ZIP: extract and run

Interactive MSI और Setup.exe एक ही functional state बनाते हैं: Program Files install, PATH, OAuth Client ID और optional shortcuts।

Silent MSI enterprise deployment के लिए silent रहता है।

फिर सामान्य flow: `ngmusic → login → search → play`
