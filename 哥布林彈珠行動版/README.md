# 哥布林彈珠瀏覽器移植研究

目標：純玩原版 Peglin，研究手機／平板瀏覽器獨立執行。不要刷題功能，不接受以簡化替代遊戲宣稱原版移植完成。

**目前狀態（2026-10-15）：正式版 Web 建置（`PeglinBrowserLab/Builds/WebRelease`，76 MB）在桌面 Chromium 瀏覽器實測可玩：主選單→角色→開場遺物→地圖→戰鬥（發射、物理、傷害、曲線長釘）→獎勵→選路→事件，存檔寫入瀏覽器 IndexedDB、重新整理後可「繼續」。觸控（模擬）可操作；瀏覽器語言為 zh-TW 時自動繁體中文。尚未在實體手機（iOS Safari／Android Chrome）驗證效能與記憶體，也還沒玩到 Boss 與後段地區。**

每一步的證據與阻礙都在 [SESSION_NOTES.md](SESSION_NOTES.md)（最新在最後），第一階段證據見 [還原檢查報告.md](還原檢查報告.md)。

## 在本機玩

```bash
python scripts/serve-web.py "E:/NEWTEST/PeglinBrowserLab/Builds/WebRelease" 8760
```

然後開 http://127.0.0.1:8760/ 。同一個 Wi-Fi 的手機要連，第三個參數改成這台電腦的區網 IP（例如 `... 8760 192.168.1.20`），手機開 `http://192.168.1.20:8760/`。注意：這台電腦保留了 8768–8967 等埠，用那些埠會出現 WinError 10013。

## 倉庫結構（私人 repo）

repo 的工作目錄是 `E:\NEWTEST`，但只收錄兩個資料夾（git 資料在 `哥布林彈珠行動版\.git`，`core.worktree=E:/NEWTEST`，白名單在 `.git/info/exclude`）：

- `哥布林彈珠行動版/`：研究筆記、`scripts/`（匯出／編譯／審計／DLL 適配／瀏覽器測試工具）、`reports/`、`RecoveredProject/`（AssetRipper 原樣還原，保留不改）。
- `PeglinBrowserLab/`：Unity 6.6 工作副本 `Unity6Project/`（Assets、Packages、ProjectSettings；Library 不收）、所有修改前的備份 `RecoveredLibraries/`、測試報告與截圖 `Reports/`、可玩的 `Builds/WebRelease/`。Unity 專案必須留在英文路徑。

沒有收進 repo：`tools/`（AssetRipper、ILSpy、Roslyn、Mono.Cecil 的下載檔，可重新下載）、原始 Unity 日誌（數百 MB，保留有界摘要 `*.summary.json`）、Development 建置（wasm 135 MB 超過 GitHub 100 MB 上限）、Steam 原版 Windows Addressables 的副本（其中一個 105.7 MB；原檔在 Steam 安裝目錄）。

## 主要移植修改（全部只在工作副本）

- 舊 2022 套件 DLL 對 Unity 6.6 的 ABI 適配（TMP FaceInfo、模組 TypeReference、Purchasing、Rewired VTable、Odin CanEmit），uGUI／TMP／SpriteShape／Burst／URP／Addressables 換成 6.6 對應版。
- 依原始 catalog 重建 Addressables 並以 WebGL 建置；`WebAddressablesBoot` 啟動時預載，讓原版的同步 `WaitForCompletion` 呼叫點不必改寫。
- 恢復 URP／TMP／UIEffect 正式 Shader；動態中文字型圖集改為未壓縮 Alpha8，修正缺字。
- Steam、Google 試算表翻譯更新在 Web 版略過；瀏覽器 zh-TW 對應繁體中文。

原版 Steam 安裝與刷題框架未修改；沒有執行原版遊戲或更動原版存檔。這是個人擁有遊戲的私人移植研究，repo 保持私人，不公開發布。
