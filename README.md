## External Avatar Logger for VRChat
[![HitCount](https://hits.dwyl.com/notunixian/ExternalLogger.svg?style=flat-square)](http://hits.dwyl.com/notunixian/ExternalLogger)


A extremely simple & lightweight proxy/logger for VRChat avatars powered by the amazing [Titanium Web Proxy](https://github.com/justcoding121/titanium-web-proxy).

**This will only log avatars that you have never downloaded/not cached.**
If you want to clear your cache to log avatars you have cached, you are free to do that inside of VRChat's settings.

### How It Works

* Creates a proxy using the mentioned web proxy library.
* Waits for a request to VRChat's CDN servers.
* Retrives the URL to be redirected to.
* Depending on what you chose, it will either store the download link in a text file or download it on the fly and save it to a folder.

As of right now, it's just a simple console application but I have no plans to make it an actual UI with toggles and everything as it just seems kind of pointless.





