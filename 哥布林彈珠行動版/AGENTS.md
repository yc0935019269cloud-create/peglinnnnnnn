# 接手前先讀

請先讀 SESSION_NOTES.md、README.md 和 還原檢查報告.md。使用者以繁體中文溝通。

## 使用者確定的目標

- 純遊玩 Peglin，已取消刷題／學習功能。
- 手機／平板離開電腦也能獨立玩；優先研究瀏覽器版本。
- 要原版內容，不接受簡化重製品冒充完整移植。
- 最新狀態：正式版 Web 建置已在桌面瀏覽器實測可玩（主選單→地圖→戰鬥→獎勵→事件、存檔續玩、繁體中文）。下一步是實體手機（iOS Safari／Android Chrome）效能與記憶體、Boss 與後段地區。瀏覽器測試用 scripts/cdp.py（Edge 無頭，需 --autoplay-policy 旗標）。
- 使用者明確要求「隨時留下 session note」：每個重要變更／測試／新阻礙都立即追加 SESSION_NOTES.md，不以新摘要覆蓋舊結果。

## 工作範圍

- 本資料夾是獨立逆向與移植研究。
- 遊戲來源 E:\SteamLibrary\steamapps\common\Peglin 保持唯讀。
- 不改 E:\PeglinQuiz 或 E:\NEWTEST\新刷題庫框架 - TEST 1。
- 不執行來源遊戲或寫入玩家存檔。
- RecoveredProject\ExportedProject 是 AssetRipper 的原樣還原，後續修復請另建工作副本，記錄差異。
- 工作副本已建立在 E:\NEWTEST\PeglinBrowserLab\Unity6Project。最新實際匯入狀態見 SESSION_NOTES.md 及 reports\unity6，不能沿用第一階段尚無帳號的狀態。
- 不把額外 FirstpassSource 複製進 Unity 專案，專案的 Assets\Plugins\Assembly-CSharp-firstpass 已有 firstpass 程式。

## 已完成與驗證界限

- AssetRipper 2.0.0 讀取原版 Unity 2022.3.62f2 資料，還原場景、素材與 C#。
- scripts\check-compile.ps1 -Firstpass -ExportedReferences，接著 -ExportedReferences，均 0 錯誤。
- 這是靜態編譯；引擎／.NET 參考來自 Windows Player，沒有進行 Editor 匯入、WebGL/AOT 編譯或瀏覽器遊玩測試。
- scripts\audit-recovery.py 會重新比對來源所有檔案 SHA-256，並產生平台／場景／Shader／引用報告。
- GUID 與本地 fileID 審計有界限，詳見報告；不要把檔案存在推論成場景可運作。
- Shader 佔位輸出、缺件、Editor 停用插件、Windows Addressables／平台相依都尚未修復。
- 已有可玩的 Web 正式版；私人 repo：https://github.com/yc0935019269cloud-create/peglinnnnnnn （不得改為公開或部署成公開網站）。

## 接續順序

1. Unity Hub 與 6000.6.5f1 Web Build Support 已安裝；先完成工作副本 Editor 匯入，詳見 session note。
2. 在工作副本修復 Editor 匯入與套件／Shader／少量缺件。
3. 重建 WebGL Addressables，主選單→地圖→戰鬥的載入測試。
4. 本機瀏覽器測玩法与存檔，再做 iOS Safari／Android Chrome 實機觸控與效能驗證。
5. 達到可玩的 Web build 後再評估部署。公開商業遊戲程式／素材另需確認發布權限。
