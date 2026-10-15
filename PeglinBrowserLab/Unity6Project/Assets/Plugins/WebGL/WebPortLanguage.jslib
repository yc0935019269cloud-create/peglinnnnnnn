// Web port: Unity reports Application.systemLanguage for "zh-TW" as Chinese(Simplified).
// Expose the browser's own language tag so the game can pick Traditional Chinese.
mergeInto(LibraryManager.library, {
  WebPort_GetBrowserLanguage: function () {
    var lang = (navigator.languages && navigator.languages[0]) || navigator.language || "";
    var size = lengthBytesUTF8(lang) + 1;
    var buffer = _malloc(size);
    stringToUTF8(lang, buffer, size);
    return buffer;
  }
});
