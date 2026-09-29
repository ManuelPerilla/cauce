# Security और privacy

NgMusic एक desktop client है और project-operated telemetry/account backend नहीं चलाता।

यह search queries, OAuth tokens, basic Google profile data और YouTube metadata process कर सकता है।

Google login, token refresh, YouTube search और playback के लिए network requests होती हैं। Local OAuth/player services केवल `127.0.0.1` पर listen करती हैं।

OAuth tokens Windows Credential Manager में `NgMusic.GoogleOAuth` के तहत store होते हैं।

Project का अपना backend नहीं होने के कारण NgMusic project-operated systems को data नहीं भेजता। Google/YouTube features उपयोग करने पर data उन services को जाता है।

Sensitive vulnerabilities या credentials public issue में पोस्ट न करें।