# Session Notes

此檔為本專案持續交接紀錄。每個重要改動、驗證結果或新阻礙都要追加，不以新摘要覆蓋舊結果。

## 2026-10-14｜第一階段還原檢查

- 使用者目標改為純玩原版 Peglin，不需要刷題；希望手機／平板離開電腦也能玩，優先研究瀏覽器版本。
- Steam 遊戲來源：E:\SteamLibrary\steamapps\common\Peglin；Unity 2022.3.62f2 / Mono。
- 原樣還原：RecoveredProject\ExportedProject，保留不修改。18 場景、1,309 prefab、2,264 PNG、2,221 C#。
- 從還原 firstpass 原始碼及核心原始碼重新編譯，兩組均 0 C# 錯誤。這只是以 Windows Player 引擎參考做的靜態驗證。
- 原版 227 個檔案 SHA-256 比對無改動。
- 靜態 GUID／本地 fileID 檢查：場景及 prefab 未發現本地 fileID 缺口；少量字型圖集和 Editor 設定缺件。85 個 Shader 為佔位版本，75 份 DLL 的 Editor 平台原本停用。
- 未做 WebGL 建置或瀏覽器遊玩，未發布 repo。

## 2026-10-14 22:19–22:30｜Unity 6 工作副本與首次實際匯入

- 使用者已建立 Unity 帳號並完成 Unity Hub 安裝 Unity 6.6（6000.6.5f1），包含 Web Build Support。Unity 日誌確認授權成功更新。
- 工作副本：E:\NEWTEST\PeglinBrowserLab\Unity6Project。英文路徑用於之後 Editor／Web 建置。
- 複製原樣還原專案後，啟用 75 份相依 DLL 的 Editor 匯入設定，暫未引入新的 package DLL，避免先混入重複套件。
- 將副本 companyName / productName 改為 LocalPeglinResearch / PeglinBrowserLab，移除原開發商 cloudProjectId / organizationId。沒有執行遊戲，不碰原版存檔。
- 第一次 Editor 編譯：4 個唯一錯誤，都是 Unity 6.6 禁用 GetInstanceID API。六份工作副本原始碼改用 EntityId；排序改 CompareTo，傷害文字索引字典及空值同步調整。
- 第二次 Editor 編譯：1 個唯一錯誤，PreventRotation 的 quaternion 在舊 Unity.Mathematics DLL 與新 engine module 撞名。改用等價的 UnityEngine.Quaternion.identity。
- 加入 Editor 診斷方法 BrowserResearchProbe.Inspect，用來輸出場景／資源／Shader／組件數量與 Web Build Support 狀態，不進入 Play Mode。
- 第三次編譯已產生 Assembly-CSharp-firstpass.dll、Assembly-CSharp.dll、Assembly-CSharp-Editor.dll，未找到 C# 編譯錯誤；**資源匯入仍未完成，不能宣稱完整匯入成功。**
- 第三次後續資源匯入大量重複輸出 `Do not use ReadObjectThreaded on scene objects!`，日誌約 589 MB。為避免無界重複，停止了本聊天啟動的 batchmode Editor（PID 27932）；沒有關閉使用者 Unity Hub。
- 日誌：reports\unity6\editor-import-20261014-222424.log。editor-probe.json 尚未產生。尚未進入 Play Mode、尚未建置 WebGL。
- 已知：初次 PowerShell 直接呼叫 GUI exe 不等待退出，latest-import 曾出現 null exitCode；腳本已修正用 Start-Process / WaitForExit。後續還要將日誌分析改為有界串流，避免讀入巨大重複日誌。

### 現在的下一步

1. 對第三次匯入日誌做有界分析，找出大量警告前正在匯入的資源及上下文。
2. 檢查工作副本 AssetDatabase／日誌，縮小失敗資源；必要時用單一 import worker 或工作副本隔離問題資源，不修改原樣還原。
3. 完成 Editor 匯入後再執行診斷；Shader 佔位、Windows Addressables bundles、平台服务與 WebGL 尚未修復。
4. 重要結果立即追加 session note，同步 README / AGENTS / CLAUDE 的狀態。

### 可重跑入口

- scripts\prepare-unity6.py：準備副本，已有 preparation.json 時會拒絕重跑。
- scripts\patch-unity6-ids.py：識別 API 適配；已完成，不要重跑把 migration.json 覆蓋為空清單。
- scripts\import-unity6.ps1 -Inspect：實際 batchmode 匯入；重跑前先修復有界日誌讀取並檢查是否仍有同專案 Editor 執行。
- 工作副本 Editor 腳本：Assets\Editor\BrowserResearchProbe.cs。

## 2026-10-14｜大量重複匯入警告的定位與修復測試

- 有界串流統計第三次日誌：589,313,542 bytes、11,990,364 行，其中 `ReadObjectThreaded` 警告 11,978,970 次。完整原始日誌保留；摘要為同名 .summary.json。
- 第一次警告前最後正在匯入：Assets/MonoBehaviour/MonoBehaviour_89.asset，GUID 007d706b98e8fdc428bfc529323199fb。
- 該資源是 I2.Loc.LocalizeTarget_TextMeshPro_UGUI 的 ScriptableObject，mTarget 外部指向 MainMenu.unity 的場景元件。主選單也引用該資源，造成 asset→scene→asset 的載入問題。
- 反編譯原始程式 LocalizeTarget<T>.IsValid 已具備 mTarget 為 null 時用 cmp.GetComponent<T>() 重新綁定的邏輯。因此修復只針對已知 I2 LocalizeTarget 資源中「指向 .unity 場景」的 mTarget 清為 0，保留其他多語／排版設定；不改 prefab target，不改原樣還原。
- 修復腳本：scripts/fix-localization-scene-links.py；變更紀錄：reports/unity6/localization-scene-links.json。
- 正在重新匯入以驗證此修復。尚未宣稱完整匯入成功。
- scripts/import-unity6.ps1 已改為有界 Python 串流分析日誌；若日誌超過 100 MB 且尾端仍大量重複 ReadObjectThreaded，會停止該腳本自己啟動的 Editor，避免無界日誌成長。
- summarize-unity-log.py 保留第一次警告前上下文、最後匯入資源、唯一 C# 錯誤及重複次數。應使用此腳本處理長日誌，禁止直接把完整日誌或持續增長中的檔案輸出至對話。

## 2026-10-14 22:33｜Unity 6 完整匯入通過

- 清除 98 個 I2 localization cache 中不合法的跨 scene 引用後，第四次完整匯入成功，Editor return code 0，唯一 C# 錯誤 0，沒有觸發日誌停止保護。
- 日誌：reports/unity6/editor-import-20261014-223135.log；有界摘要為同名 .summary.json。
- Editor 診斷已實際執行，結果：E:\NEWTEST\PeglinBrowserLab\Reports\editor-probe.json。
- Unity 實際版本 6000.6.5f1。18 場景、1,309 prefab、2,337 Texture2D（含其他型別及預設素材，不等同第一階段 2,264 PNG 檔數）。
- 三組組件 Assembly-CSharp-firstpass.dll、Assembly-CSharp.dll、Assembly-CSharp-Editor.dll 均產生。
- Web Build Support=True。85 個 Shader 的 Editor 語法檢查無錯誤，但仍然是佔位 Shader，**不代表畫面恢復**。
- 工作副本儲存目錄已隔離為 C:/Users/USER/AppData/LocalLow/LocalPeglinResearch/PeglinBrowserLab。
- 工作副本 ProjectVersion.txt 已由 Unity 升級至 6000.6.5f1。原樣還原仍保持 2022.3.62f2。
- Windows Addressables bundles 仍有 13 個，尚未重建 Web 版。尚未進入 Play Mode、尚未產生可玩瀏覽器版本。
- 下一步：WebGL 平台建置試驗，先確認 IL2CPP/WebAssembly 是否能編譯；即使產生測試 build，也不能稱為完整可玩版。

## 2026-10-14 22:35｜開始 WebGL 平台建置試驗

- BrowserResearchProbe.BuildPlatformSmoke 已啟動，目標 WebGL，輸出預定 E:\NEWTEST\PeglinBrowserLab\Builds\PlatformSmoke。
- 這個測試只帶 PreMainMenu 場景，用來確認平台程式碼／IL2CPP／WebAssembly 建置鏈能否通過。不是完整遊戲，原版 Windows Addressables 和佔位 Shader 仍在，**不得當成可玩版提供給使用者或上傳 repo**。
- Native build 尚在執行；結果需看 web-build-summary.txt、實際 .wasm 是否產生及 Editor exit code。
- 日誌：reports/unity6/editor-import-20261014-223516.log。需要以 summarize-unity-log.py 做有界分析，不能直接輸出完整日誌。

## 2026-10-14 22:39｜首次 WebGL 建置失敗，定位到設定與字型 ABI

- 首次 WebGL 平台測試：Failed，Editor exit 1，BuildReport errors=2，未產生 .wasm。
- 確定的設定錯誤：`No WebGL template selected!`。測試建置方法已明確選 APPLICATION:Default。
- 同次日誌還出現 `MissingMethodException: Method not found: int UnityEngine.TextCore.FaceInfo.get_pointSize()`，表示原版預編譯字型套件呼叫舊引擎 API。這不是 C# 原始碼編譯錯誤，需要套件適配。
- 正在分析並適配旧預編譯 DLL 的 FaceInfo.pointSize 呼叫，所有修改僅在工作副本，保留 DLL GUID。
- 新增 Shader 修復索引：reports/unity6/shader-repair-index.json；優先影響字型 SDF 及 URP Sprite-Lit，所有目前仍為佔位 Shader。
- 下一個 WebGL 重跑結果必須另外記錄，不能把先前 Editor 匯入 exit 0 當作 WebGL 成功。

## 2026-10-14｜TextMeshPro 字型套件 ABI 適配

- 從已安裝的 Unity 6.6 引擎確認 FaceInfo.pointSize 現在是 float，原版 DLL 呼叫舊 int getter／setter，導致 MissingMethodException。
- 使用本機 Mono.Cecil 0.11.5 工具，在工作副本 Unity.TextMeshPro.dll 適配 35 個 getter 呼叫與 1 個 setter 呼叫。
- getter 呼叫新 float API 後 conv.i4 保留原套件預期的 int；setter 輸入 conv.r4 再呼叫新 float API。沒有更動型別名稱、序列化欄位或 DLL .meta GUID。
- 修改前 DLL 保存：E:\NEWTEST\PeglinBrowserLab\RecoveredLibraries\Originals\Unity.TextMeshPro.dll。
- 適配原始碼：scripts/AdaptFaceInfo.cs；工具：tools/face-info-adapter；結果：reports/unity6/face-info-adaptation.txt。
- 目前只完成靜態 ABI 適配；需要接下來 Editor／WebGL 建置驗證，不可先稱字型渲染已恢復。原樣還原及 Steam DLL 均未修改。

## 2026-10-14｜第二次 WebGL 建置與舊引擎模組適配

- 第二次 WebGL 試驗已不再報「沒有網頁樣板」或 FaceInfo.pointSize 舊 getter。仍 Failed，未產生 .wasm。
- 確定的建置阻礙：Unity CIL Linker 無法解析舊 UnityEngine.SharedInternalsModule；同次資源處理還報 FontEngine.GetGlyphIndex(uint) 舊 API 不存在。
- 擴充適配器：依已安裝 Unity 6 引擎型別的實際定義模組，移動舊 DLL 裡引用已不存在 UnityEngine 模組的 TypeReference；字型 getter 改呼叫 TryGetGlyphIndex 包裝方法。
- 初次適配器對 Unity.Postprocessing.Runtime.dll 寫入時因缺少組件解析目錄失敗，導致工作副本該 DLL 暫為 0 bytes；已立即從原樣還原恢復。原樣還原／Steam 原版沒有被寫入。
- 已將適配器改成「先寫暫存檔→重新讀取驗證 assembly identity→再取代工作 DLL」；並加入新引擎、工作 DLL、原版 framework 的 resolver 目錄。後續失敗不會再截斷工作 DLL。
- 此項模組／glyph 適配仍在驗證，尚未重跑成功，不可宣稱 WebGL 已完成。

## 2026-10-14｜第三次 WebGL 建置：通過資源處理，卡在 ILLink 的 UIElements 模組（新聊天接手）

- 接手後檢查 reports/unity6/editor-import-20261014-225629.log：模組／glyph 適配有效，不再出現 SharedInternalsModule 或 GetGlyphIndex 錯誤；PreMainMenu 的 Player 資料（82.3 MB）已處理，Shader 已為 gles3 編譯。
- 新的唯一阻礙（Editor exit 1，仍無 .wasm）：`ILLink error IL1011: Failed to write ...ManagedStripped\UnityEngine.UIElementsModule.dll` → `Member 'UnityEngine.UIElements.Panel' is declared in another module and needs to be imported`。失敗的是引擎自己的模組，不是被適配的四份 DLL。
- 專案程式沒有 using UnityEngine.UIElements；只有 Plugins 的 MoreMountains.Tools.dll 與 UnityEngine.UI.dll 在檔案字串中含 UIElementsModule 參照。
- 試驗：在工作副本 Packages/manifest.json 移除 com.unity.modules.uielements，備份於 E:\NEWTEST\PeglinBrowserLab\RecoveredLibraries\Originals\manifest.json.before-uielements-removal。重跑 import-unity6.ps1 -BuildSmoke，結果待追加。若 Editor 因此出現新編譯錯誤，要還原 manifest。

## 2026-10-14｜WebGL 建置二分：UIElements 錯誤來自舊 UnityEngine.UI.dll（驗證中）

- 第四次建置（移除 com.unity.modules.uielements）結果相同，仍 IL1011；原因是 packages-lock 裡其他內建模組仍依賴 UIElements，無法移除。已還原 manifest。
- 基準測試：空專案（BaselineWebGL，同為 6000.6.5f1）WebGL 建置 Succeeded，產生 .wasm（24.8 MB）。→ 引擎與 IL2CPP/WebAssembly 工具鏈正常，問題在 Peglin 工作副本內容。日誌：reports/unity6/baseline-webgl-empty.log。
- 基準專案加入 Plugins/UnityEngine.UI.dll 仍成功（空場景沒有 UI 組件，未觸發保留），所以只能說明「單放 DLL 不會失敗」。
- Cecil 檢查：Plugins/UnityEngine.UI.dll（2022 舊版）參照 37 個 UIElements 型別，包含內部型別 BaseRuntimePanel、IRuntimePanelComponent、UIElementsRuntimeUtility、BaseVisualElementPanel；Unity 6.6 的 UIElements 內部已改版，是 Panel 錯誤的最可能來源（尚為假設）。
- 修復試驗：用 Unity 6.6 內建 com.unity.ugui 2.6.0 編譯出 UnityEngine.UI.dll，替換工作副本 Plugins/UnityEngine.UI.dll，保留原 .meta GUID（d3e719b5…），讓場景／prefab 的組件參照不變。舊 DLL 備份：RecoveredLibraries\Originals\UnityEngine.UI.dll。新 DLL 缺 3 個已棄用公開型別（IMask、BaseVertexEffect、IVertexModifier），舊版有。
- 風險：Unity.TextMeshPro.dll（已適配）與新 UI.dll 的相容性、序列化欄位差異尚未驗證。結果待 Editor 編譯與 WebGL 重建後追加。

## 2026-10-14｜替換 Unity 6.6 的 UI／TMP DLL 試驗：不採用，已還原

- 換入 Unity 6.6 編譯的 UnityEngine.UI.dll 後，Unity 6.6 的 UIElements IL1011 消失，但：(a) Editor 版 DLL 參照 UnityEditor.dll，Player 建置拒絕；(b) 改用 player 編譯版後 ILLink 轉為同型錯誤 `Failed to write UnityEngine.TextCoreFontEngineModule.dll ... Member 'UnityEngine.TextCore.GlyphRect' is declared in another module`；(c) 再把 Unity.TextMeshPro.dll 也換成 6.6 版時 Editor 崩潰（exit -1073740940，堆積毀損，日誌 editor-import-20261014-231754.log），該版參照 Unity.InternalAPIEngineBridge.004。此路不通，已將 UnityEngine.UI.dll 還原為原版、Unity.TextMeshPro.dll 還原為 FaceInfo 適配版（備份 Originals\Unity.TextMeshPro.facinfo-adapted.dll）。Rewired 的 TouchInputModule 修改隨之還原。
- 新的判斷：兩次 IL1011 型別都是「舊 DLL 的 TypeReference 指向某引擎模組，但該型別在 6.6 已搬到別的模組（舊模組只留 forwarder）」。下一步改成在適配器中，對所有 Plugins DLL 把這類 TypeReference 的 scope 改寫為 6.6 實際定義型別的模組，而不是只處理模組不存在的情況。先用唯讀掃描統計不一致的數量。

## 2026-10-14｜定位 IL1011：可在 7 秒內重現的 UnityLinker 實驗室

- 新增可手動重現的方法：直接用建置產生的 `Library\Bee\artifacts\rsp\*.rsp` 呼叫 `il2cpp\build\deploy\UnityLinker.exe`（工作目錄為專案），不必每次跑完整 Editor 建置（約 7 秒）。實驗腳本在 E:\NEWTEST\PeglinBrowserLab\LinkerLab\bisect2.py。
- 二分結果：失敗必須同時有「Plugins 目錄」與「任一會引用 UnityEngine.UI.dll 的 root／link.xml」。只 root Unity.Postprocessing.Runtime 或 DOTween 則通過。去掉 link.xml、去掉 PlayerScriptAssemblies、去掉 editor-data-file 都不影響。
- 排除項：AddressablesLink/link.xml（停用重建仍失敗）；ProjectSettings stripEngineCode 0→1（仍失敗，備份 Originals\ProjectSettings.asset.stripEngineCode0，之後可決定是否還原）；移除 UIElements 模組（packages-lock 仍被其他模組依賴）。
- 驗證成功的組合（實驗室）：Plugins 中以 Unity 6.6 ugui 2.6.0 的 **player 編譯版** UnityEngine.UI.dll + Unity.TextMeshPro.dll，並加入 Unity.InternalAPIEngineBridge.004.dll（且列入 --allowed-assembly）→ UnityLinker exit 0。來源：BaselineWebGL\Library\Bee\artifacts\2000b0aPDev.dag 與 PlayerScriptAssemblies。僅 Editor 版 UI.dll 會因參照 UnityEditor.dll 被 Player 建置拒絕。
- 套用到工作副本：Plugins 的 UnityEngine.UI.dll／Unity.TextMeshPro.dll 內容換成 6.6 版（保留原 .meta GUID，場景／prefab 的組件參照以型別名稱雜湊為準，尚未驗證）；新增 Unity.InternalAPIEngineBridge.004.dll 與新 .meta；Rewired 的 RewiredStandaloneInputModule 移除已被 uGUI 刪除的 TouchInputModule 呼叫。備份：Originals\UnityEngine.UI.dll、Originals\Unity.TextMeshPro.facinfo-adapted.dll。
- 風險待驗：TMP 6.6 版與遊戲 TMP 字型資產／Settings 的序列化相容性；先前把 TMP 換成 6.6 版時 Editor 崩潰（當時缺 bridge.004），這次補上 bridge 後重測。

## 2026-10-14｜ILLink 通過；IL2CPP 資料模型失敗於舊 Burst；改用 Unity 6.6 套件

- 以 Unity 6.6 版 UnityEngine.UI.dll、Unity.TextMeshPro.dll、Unity.InternalAPIEngineBridge.004.dll 取代後，完整 Editor WebGL 建置的 **ILLink 階段通過**（日誌 editor-import-20261014-232925.log）。
- 新阻礙在 il2cpp.exe：`Found a method reference we cannot resolve ... BurstCompilerService::GetOrCreateSharedMemory(Hash128&,uint,uint)`。原因：Unity 6.6 已把 Burst 放進引擎（UnityEngine.BurstModule），Plugins 中 2022 版 Unity.Burst.dll 的呼叫簽章過時。
- 唯讀 Cecil 掃描（reports/unity6/unresolved-engine-method-refs.json）：舊 Plugins DLL 對 6.6 引擎無法解析的方法參照只有 6 筆，全在 Unity.Burst／Unity.Collections／Unity.2D.SpriteShape（BurstCompiler.GetILPPMethodFunctionPointer2／CompileILPPMethod2、NativeArray.ReadOnly ctor）。遊戲自己的 Assets/Scripts 完全不使用 Burst／Collections／Mathematics，只有套件內部相依。
- 處理：把 Unity.Burst、Unity.Burst.Unsafe、Unity.Collections、Unity.Collections.LowLevel.ILSupport、Unity.Mathematics 五份舊 DLL（含 .meta）移到 RecoveredLibraries\Originals\RemovedPlugins，改由 Package Manager 安裝 com.unity.burst 1.8.31、com.unity.collections 2.7.1、com.unity.mathematics 1.3.3（登錄庫可連線，已確認）。manifest 備份：Originals\manifest.json.before-burst-collections。
- 結果待追加。仍須留意：SpriteShape、URP、Addressables 等其他舊套件 DLL 之後可能出現同類問題。

## 2026-10-14｜Burst／Collections 套件有效；Purchasing 與 SpriteShape 的最後相容性修正

- Burst／Collections／Mathematics 改用 Package Manager 後，Editor C# 編譯仍 0 錯誤，IL2CPP 往前一階段。
- 新阻礙 1：`UnityEngine.Purchasing.dll` 的 `UnityEngine.Analytics.AnalyticsResult` 參照指向 UnityAnalyticsModule，但 6.6 已搬到 UnityAnalyticsCommonModule 且沒有 forwarder。以 Cecil 掃描全部 Plugins DLL（reports/unity6/typeref-scope-unforwarded.json）：全專案只有這 1 筆。已重新指向並驗證 assembly identity，原檔備份 Originals\UnityEngine.Purchasing.dll；紀錄在 reports/unity6/engine-module-adaptation.txt。
- 新阻礙 2：IL2CPP `Unresolved type reference : System.Object in module UnityEngine.BurstModule.dll with scope UnityEngine.ScriptingModule`。實驗室（LinkerLab\bisect3.py + check-burst.ps1，以 Cecil 檢查 ManagedStripped\UnityEngine.BurstModule.dll）重現：只有 root Assembly-CSharp.dll 或任何 link.xml 才會出現；URP／Core／TMP 等單獨 root 都正常。Assembly-CSharp 參照 Unity.2D.SpriteShape.Runtime（長釘 LongPegs／SplineFollow 預製物使用，GUID 12831dbd…），而舊版 SpriteShape 呼叫已被移除的 Burst ILPP API（BurstCompiler.CompileILPPMethod2／GetILPPMethodFunctionPointer2）。
- 修正：在空專案加入 com.unity.2d.spriteshape 17.0.1（與 burst／collections／mathematics 同版）編譯，WebGL 建置成功；取其 player 編譯版 Unity.2D.SpriteShape.Runtime.dll 與 Unity.2D.Common.Runtime.dll 換入工作副本，保留原 .meta GUID。舊檔備份在 Originals。序列化相容性（LongPegs.asset、SplineFollow、LongPegGenerator prefab）尚未驗證。
- 注意：已確認 BaselineWebGL 空專案＋Burst 套件可成功建置，故 Burst 本身沒問題。

## 2026-10-14｜IL2CPP 通過 SpriteShape，卡在 Rewired 混淆 DLL 的 VTable 衝突

- 換入 6.6 版 SpriteShape 後 IL2CPP 往前，新錯誤：`Error while creating VTable for GwKC…/RfFMKu…: The base method ... is implemented both by T::M() and T::Base.get_M()`。來源是 Rewired_Core／Rewired_Windows（商業輸入庫，已混淆；原遊戲用 Mono 執行，從未經 IL2CPP 檢驗）。
- 型態：混淆後的子類同時有「明確 MethodImpl 覆寫基底虛擬方法 B」和「同名同簽章、非 newslot 的虛擬方法 A」。Mono 以 MethodImpl 為準，IL2CPP 則兩個都算。全部 9 處（Core 3、Windows 6），每處 A 只有 1 個 callvirt 呼叫點，沒有 call／ldftn。
- 第一版修正（把 A 降為 assembly 可見度）無效，已被取代——原因是我當時假設衝突發生在介面，實際是基底類別 MethodImpl。
- 第二版：scripts/adapt-rewired-vtable.ps1（Cecil，預設 dry-run，-Apply 才寫入；永遠從 Originals\Rewired 的原始備份重新產生，可重跑）：A 改 newslot，並把該呼叫點改指 B，等同 Mono 的槽位分派。假設：Mono 的 vtable 中明確 MethodImpl 贏過依名稱的隱式覆寫（尚未用執行驗證；只能等 Web build 可執行時才能確認）。報告：reports/unity6/rewired-vtable-adaptation.txt。

## 2026-10-14 23:54｜WebGL 平台建置試驗首次成功（仍不是可玩版）

- 補上 WebGL 記憶體設定（ProjectSettings 原本全為 0，emcc 報 `INITIAL_MEMORY must be larger than STACK_SIZE, was 0`）：memorySize 32／initial 32／max 2048／growth mode 2／geometric 0.2／cap 96／linear 16／powerPreference 2／exceptionSupport 1（同空專案預設）。備份 Originals\ProjectSettings.asset.before-webgl-memory。
- 結果：reports/unity6/editor-import-20261014-235333.log，Editor exit 0，BuildReport result=Succeeded、errors=0、warnings=17。**產生 PlatformSmoke.wasm（129.8 MB，Development build）**，.data 31.1 MB，framework/loader js、index.html；整個輸出 293.6 MB（大宗是仍殘留的 Windows Addressables bundles）。這是第一次 IL2CPP → WebAssembly 完整鏈通過。
- 範圍限制（很重要）：只帶 PreMainMenu 場景；尚未在瀏覽器載入、尚未執行；Shader 仍為佔位版；Addressables 仍是 Windows 版 bundles；StreamingAssets 中的 aa\StandaloneWindows64 在 Web 上沒有用。不得宣稱可玩或已移植。
- 本次累計對工作副本的變更（全部可還原，備份皆在 E:\NEWTEST\PeglinBrowserLab\RecoveredLibraries\Originals）：
  1. Plugins：UnityEngine.UI／Unity.TextMeshPro 改為 Unity 6.6 ugui 2.6.0 的 player 編譯版（保留 GUID）；新增 Unity.InternalAPIEngineBridge.004.dll。
  2. Plugins：Unity.2D.SpriteShape.Runtime／Unity.2D.Common.Runtime 改為 com.unity.2d.spriteshape 17.0.1 的 player 編譯版（保留 GUID）。
  3. 移除舊 Unity.Burst／Burst.Unsafe／Collections／Collections.LowLevel.ILSupport／Mathematics DLL，manifest 加 com.unity.burst 1.8.31、collections 2.7.1、mathematics 1.3.3。
  4. UnityEngine.Purchasing.dll：AnalyticsResult 型別 scope 重指到 UnityAnalyticsCommonModule。
  5. Rewired_Core／Rewired_Windows：9 處 VTable 衝突修正（scripts/adapt-rewired-vtable.ps1）。
  6. 原始碼：RewiredStandaloneInputModule 移除 uGUI 已刪除的 TouchInputModule 呼叫。
  7. ProjectSettings：stripEngineCode 0→1（沒有解決任何問題，建議之後還原；備份 Originals\ProjectSettings.asset.stripEngineCode0），WebGL 記憶體設定如上。
- 尚未驗證的相容性風險：TMP／UI／SpriteShape 改版後的序列化欄位（LongPegs.asset、SplineFollow、TMP 字型資產）；Rewired 修正的 Mono 語意假設；其他舊 DLL（URP、Core.Runtime、Addressables、ResourceManager、Timeline、Postprocessing）在執行期的 API 相容性——建置通過不代表執行期沒有 MissingMethodException。
- 下一步：(1) 在本機伺服器用內建瀏覽器載入 PlatformSmoke，記錄主控台第一個錯誤；(2) 依錯誤處理 Shader（85 個佔位）與 Addressables（需為 WebGL 重建）；(3) 擴大到完整場景清單。

## 2026-10-14 23:58｜首次瀏覽器載入：引擎與遊戲程式在 WebGL 上開機，第一個執行期阻礙是同步 Addressables

- 以 python http.server 在 127.0.0.1:8765 提供 Builds\PlatformSmoke，由內建瀏覽器載入（Chromium，WebGL 2.0）。測完已關閉該伺服器。
- 實際主控台證據：Unity 6000.6.5f1 引擎初始化成功、建立 WebGL 2.0 context、PhysX 物理後端載入、Input Manager、Odin Serializer（WebGLPlayer，啟用非對齊讀寫）、Unity IAP、遊戲自己的 DataSerializer 讀存檔（目前為空）全部執行 → **遊戲的 C# 程式（含已修改的 Rewired／TMP／UI 相容 DLL）在 WebAssembly 上確實開始執行。**
- 第一個致命錯誤：`WebGLPlayer does not support synchronous Addressable loading. Please do not use WaitForCompletion on the WebGLPlayer platform.`，之後遊戲自行 `Quitting...`。
- 非致命但需處理：steam_api64 不存在（Steamworks.NET 被遊戲自己攔截並忽略，成就資料略過）；UnityServicesProjectConfiguration.json 路徑錯誤與 Unity Services 未連結專案（`UnityProjectNotLinkedException`）；`Local keyword BLIT_DECODE_HDR doesn't exist in the shader`（佔位 Shader 的症狀）。
- 同步 Addressables 呼叫分佈在 15 個 C# 檔（grep WaitForCompletion 去除 DOTween 同名方法後）：Loading/AssetLoading.cs（Bestiary）、Loading/Pegboards/PegLayoutLoader.cs、PegboardParser.cs、EnemyManager.cs（含 InstantiateAsync(...).WaitForCompletion）、DeckManager.cs、Battle/BattleController.cs、Battle/Enemies/Enemy.cs 等。這是 Web 移植的核心設計問題，不是單一 bug：
  - 方案 A：啟動時非同步預載全部 Addressables，再讓 WaitForCompletion 在已完成的控制代碼上執行（只對 LoadAssetAsync 有效；InstantiateAsync 仍需改寫）。缺點：記憶體壓力大，手機瀏覽器（尤其 iOS Safari）風險高。
  - 方案 B：改寫呼叫點為「進場景前以標籤／鍵值非同步載入、之後以同步字典取得」或改成協程。工作量較大，但可控制記憶體。
  - 兩案都需要先用 WebGL 為目標重建 Addressables bundles（目前仍是 Windows 版，Web 上無法載入）。
- 尚未做：Addressables 的 WebGL 內容重建、Shader 修復、遊玩測試。不得宣稱可玩。

## 2026-10-15｜Web Addressables 路線：資料盤點與決策

- 使用者選擇「預載方案但工作量要小」。盤點結果：Windows Addressables 非場景資源合計只有約 26 MB（壓縮）：enemies 16.0、orbs 4.4、duplicateassetisolation 2.1、bestiary 1.3、scriptableobjects 0.8、pegboarddata 0.6、其餘 <0.3；場景包 defaultlocalgroup_scenes 105.7 MB。所以不需要逐場景精細管理，只要啟動預載小的共用資源，enemies 可留到進戰鬥前。
- 關鍵發現（反編譯 Unity.ResourceManager.dll）：`AsyncOperationHandle<T>.WaitForCompletion()` 在操作已完成時**不會**擲錯（只有未完成才走到 WebGL 的 throw）。所以 ≥90% 的呼叫點可以完全不改，只要資源「以相同型別」預先載入並持有控制代碼。ResourceManager 的快取鍵是 (location, 請求型別)，所以預載必須用與呼叫端相同的型別（GameObject／Sprite／AudioClip／AudioMixer／TextAsset／ScriptableObject／object）。唯一必改：EnemyManager.cs:251 的 `InstantiateAsync(...).WaitForCompletion()`（實例化操作不會同步完成）。
- 專案內沒有 AddressableAssetsData（編輯器設定沒被還原），但 catalog.json 有 845 個 Assets/ 路徑，其中 825 個在還原專案的相同路徑存在（20 個缺的是 .wav）。因此可寫 Editor 腳本依 catalog 重建群組／位址，再用 WebGL 為目標建置 bundle。
- 步驟：A. 以 com.unity.addressables 4.1.1 套件取代 Plugins 的 Unity.Addressables／ResourceManager／ScriptableBuildPipeline／Profiling.Core DLL（備份 Originals\RemovedPlugins，manifest 備份 Originals\manifest.json.before-addressables）；B. 依 catalog 重建 Addressables 設定並建 WebGL bundle；C. Boot 場景＋型別對應預載器；D. 修 EnemyManager 的 InstantiateAsync；E. 瀏覽器重測並量記憶體。

## 2026-10-15｜依 catalog 重建 Addressables 設定與 GUID 對應

- Addressables 4.1.1 套件換入後 Editor 編譯 0 錯誤（exit 0）。
- Editor 腳本（工作副本 Assets\Editor\RebuildAddressablesFromCatalog.cs，Dump／Apply／BuildContent；runner：scripts/run-unity-method.ps1）解析 Windows catalog.json：1004 個位置、931 個資產項、11 個 bundle。標籤：Orbs／Enemies／Pegboards／Bestiary。Apply 重建 AddressableAssetsData：**11 個群組、825 個項目**；20 個資產（多為音效 .wav/.mp3 與 silver.ttf）在還原專案的原路徑不存在，清單在 E:\NEWTEST\PeglinBrowserLab\Reports\addressables-missing-assets.txt。
- 發現：還原專案的資產 GUID 與 catalog 內的原始 GUID **完全不同**（0/872 相符），而場景／預製物裡序列化的 AssetReference 仍是原始 GUID（209 個不同值中 206 個在 catalog）。若不處理，所有 AssetReference 在執行期都找不到。處理：scripts/patch-addressable-guids.py 依 Apply 輸出的對應表，把 `m_AssetGUID` 改為還原後的 GUID（可 `--revert`，變更紀錄 reports/unity6/addressable-guid-patch.json）。

## 2026-10-15 00:45｜WebGL Addressables 內容建置成功；預載器在瀏覽器開始運作（測速待分頁可見）

- 修正：缺的 20 個資產其實是 AssetRipper 改了副檔名（.wav/.mp3→.ogg、silver.ttf→.otf），Apply 加副檔名回退後 **845/845 項目、0 缺漏**。
- 舊 Windows 的 StreamingAssets\aa 已移出專案（備份 Originals\StreamingAssets_windows，126 MB）。以 WebGL 為目標建置 Addressables 內容成功（121 秒，無錯誤）：全部 bundle 合計 47.3 MB，其中場景 34.4、enemies 6.8、orbs 2.1、其餘合計約 4 MB（比 Windows 版 131 MB 小很多，LZ4）。
- 新增（僅工作副本）：Assets\Scripts\WebPort\WebAddressablesBoot.cs（啟動預載器）、Assets\Scenes\WebBoot.unity（建置清單第 0 個場景，之後載入 PreMainMenu）、Assets\Editor\WebPortSetup.cs（CreateBootScene／BuildWeb）、Assets\Editor\RebuildAddressablesFromCatalog.cs。預載器對 catalog 每個非場景 (位置, 型別) 呼叫 ResourceManager.ProvideResource<T> 並永久持有控制代碼；TextAsset 另以 object 型別再載一次（PegLayoutLoader 用 LoadAssetAsync<object>）。原遊戲呼叫點完全沒改。
- 瀏覽器結果：WebTest（Builds\WebTest，Development build，總 210 MB）載入後 Addressables 初始化、catalog.bin 與 bundle 全部下載並快取成功，預載器共 1046 個 (位置,型別) 操作；已觀察到進度推進（done 69/1046 時 failures=0）。
- 限制：內建瀏覽器窗格被隱藏時 rAF 被節流，Unity 幀數極低，所以進度只有約每秒 1 個操作，這不是真實效能。需要使用者讓瀏覽器窗格可見，或用自己的 Chrome 開 http://127.0.0.1:8766/index.html（伺服器仍在執行，PID 見 netstat 8766）才能得到真實載入時間與記憶體。
- 尚未驗證：預載是否讓所有 WaitForCompletion 呼叫點通過（特別是 InstantiateAsync 的 EnemyManager.cs:251、AudioMixer 類型、Bestiary）；PreMainMenu 後續流程；Shader 佔位。

## 2026-10-15 01:05｜預載器效能問題查明：IMGUI 的 OnGUI 讓每幀 >1 秒；移除後預載 20 秒完成

- 症狀：預載期間頁面約每 3 秒一幀（rAF 0.33 fps），1046 個操作要跑好幾分鐘。排除項：顯示卡正常（RTX 2060，ANGLE D3D11）；停用 URP（GraphicsSettings 的 m_CustomRenderPipeline 設 0）仍一樣慢；JS 取樣顯示主執行緒幾乎全在原生碼；每個慢幀 dt 恆為 1.00s（Unity clamp），與完成的資源種類無關。
- 根因：預載器自己的 `OnGUI`（每幀 new GUIStyle(GUI.skin.label) 並繪製動態字型文字）。移除 OnGUI 後 rAF 回到 60 fps，**1046 個 (位置,型別) 操作約 20 秒完成，failures=0，managed 14 MB**。使用者那邊 Unity 原生記憶體約 147 MB（預載完成後，Loaded Objects 49,001）。這也暗示遊戲本身若大量使用 IMGUI／動態字型會很慢（尚未驗證）。
- 預載完成後遊戲進入 PreMainMenu：載入畫面（哥布林＋進度條）出現，並在 SteamManager.Awake 的 DllNotFoundException 分支呼叫 Application.Quit() 而結束 Unity 實例（日誌 `Quitting...`）。處理：工作副本 SteamManager.cs 在 WebGL 非 Editor 下直接略過 Steam（m_bInitialized=false 並 return）。
- 診斷用的 GraphicsSettings 改動（m_CustomRenderPipeline 設 0）已還原為原檔（備份 OriginalsGraphicsSettings.asset.original 仍保留）。

## 2026-10-15 01:10｜Steam 略過後：不再 Quit，遊戲在 PreMainMenu 繼續跑；下一個阻礙是 Odin Serializer 在 IL2CPP

- SteamManager 在 WebGL 略過後重建（URP 已還原為原設定）：預載 1046 操作 0 失敗（約 20 秒），**不再出現「同步 Addressables」例外，也不再 Application.Quit**；遊戲繼續執行 PreMainMenu 的啟動流程（DataSerializer 讀／寫存檔、字型 TMP 資產、IAP 初始化）。
- 新錯誤（連續出現）：`InvalidOperationException: Was not in array when exiting array. at Peglin.OdinSerializer.BaseDataReaderWriter.PopArray()`，緊接在 DataSerializer 為 `Dictionary<string, ISerializable>`、`IEqualityComparer<string/int>` 建立序列化器之後。解讀（尚為假設）：Odin 在 IL2CPP/AOT 沒有 Emit，需要預先產生的 AOT 泛型格式器；缺少時元素序列化內部丟例外被吞掉，造成陣列進出失衡。可能同時影響存檔（DataSerializer）與釘板版面（PegLayoutLoader.DeserializeValue<PegboardData>）。需要的證據：以 il2cppStacktraceInformation 打開完整堆疊、或在 DataSerializer／PegLayoutLoader 外層 try-catch 印出 InnerException。
- 其他非致命：FMOD getSystemObject UNIMPLEMENTED（WebGL 音訊後端，只是日誌）；`BLIT_DECODE_HDR`（佔位 Shader）；Unity Services 設定檔路徑；螢幕仍是黑色，尚無主選單畫面。
- 本地伺服器：8767（含 Document-Policy 標頭，LinkerLab\hdr_server.py，服務 Builds\WebTest）。8766 的簡易伺服器也仍開著。

## 2026-10-15 01:18｜Odin 存檔錯誤根因已修；主選單場景已載入但畫面全黑（渲染問題）

- 根因（反編譯確認）：Peglin.OdinSerializer.dll 是為 Windows Mono 編譯的，`EmitUtilities.CanEmit` 被編成常數 true。WebGL/IL2CPP 沒有 Reflection.Emit，Odin 卻不走反射／weak formatter 的備援路徑。DictionaryFormatter.SerializeImplementation 的 finally 一律呼叫 EndArrayNode，所以真正的例外被「Was not in array when exiting array」蓋掉。
- 修正：scripts/adapt-odin-canemit.ps1（Cecil）把 get_CanEmit 改成回傳 false（等同 Odin 官方 IL2CPP 版本行為），原檔備份 Originals\Peglin.OdinSerializer.dll。另加 Assets\link.xml 保留 Assembly-CSharp／firstpass／Peglin.OdinSerializer，避免反射用到的欄位被剝除。
- 驗證（Chrome 前景分頁）：探針 `[OdinProbe]` comparer／空 Dictionary<string,ISerializable>／Dictionary<int,string> 序列化全部 OK（字典比較器在 IL2CPP 是 InternalStringComparer）；遊戲啟動流程不再出現 PopArray 錯誤。
- 之後的實際進度：defaultlocalgroup_scenes bundle 被載入（Addressables 場景載入成功），Rewired 找不到 user32.dll 後自動「fall back to Unity input」（預期行為），沒有新的例外。FMOD getSystemObject 的訊息每幾秒出現（WebGL 音訊後端未實作的查詢，非致命）。
- 目前畫面：全黑，只看得到 Development Console。判斷是渲染層：85 個 AssetRipper 佔位 Shader + Plugins 中 2022 版 URP 執行期 DLL 跑在 6.6 引擎上。下一個主要工作是渲染：以 Unity 6.6 的 URP 套件取代舊 URP DLL（保留/重對應 script GUID），並以套件的真實 Shader 內容取代同名佔位 Shader（保留 .meta GUID）。
- 暫存診斷碼：WebAddressablesBoot.cs 內的 OdinProbe 與 slow-frame 日誌，之後要移除。

## 2026-10-15｜接續渲染修復：正式 URP 與 Shader（驗證中）

- 先用 Editor 實際匯出舊渲染 DLL 的 91 個 MonoScript 型別／GUID／fileID（Reports/render-old-scripts.json），再將場景與資產的引用轉至 Unity 6.6 內建 URP 17.6.0 的同名來源腳本；URP/Core 舊 DLL 與 .meta 移出 Assets，manifest 加正式 URP 套件。OrbInfoWidget 的 PixelPerfectCamera namespace 隨新版調整。
- Unity 6.6 已不含舊 SRP DebugUIHandler 型別。確認 29 個 DebugUI prefab 集合沒有被集合外遊戲資產引用後，將它們連同 .meta 保存到備份（非遊戲選單 UI）。
- 腳本 scripts/migrate-render-packages.py；原檔備份 E:/NEWTEST/PeglinBrowserLab/RecoveredLibraries/RenderMigration，報告 Reports/render-package-migration.json。第一遍因套件搜尋篩選與 HDRP 同名型別而中止；第二遍完成剩餘引用，所有已修改原檔仍有備份。不能直接重跑移動步驟，備份存在時會拒絕覆蓋。
- scripts/restore-package-shaders.py 以 Editor 正式 URP/Core/TMP Essential Resources 的同名 Shader 恢復 49/85 個佔位檔，保留原 GUID 並修正相對 include。仍有 36 個舊後處理／UIEffect／自訂 Shader 未恢復，詳見 Reports/shader-restoration.json。
- 正在做 Editor 匯入檢查；尚未重建內容或驗證瀏覽器畫面。

## 2026-10-15｜URP 匯入通過、UIEffect Shader 補回，開始完整渲染測試建置

- 首次正式 URP 匯入 Editor exit 0，C# 無錯誤，85 個專案 Shader 的 Editor 檢查無編譯錯誤（不等於 85 個都已恢復）。
- 找到 MoreMountains.Tools.dll 仍指向舊組件的 Light2D 型別；scripts/adapt-render-typerefs.ps1 以新編譯套件的型別定義掃描，只有這 1 處需要改為 Unity.RenderPipelines.Universal.2D.Runtime。-Apply 已套用，原 DLL 備份在 RenderMigration。
- 從 UIEffect 作者的官方 upm 分支固定 commit 332340e7bc2f8257816dd28784607a1edf6b6c2a 取回 5 個同名 UIEffect/UIDissolve/UIHsvModifier/UIShiny/UITransition Shader 與 include，原有材質 property 名稱都涵蓋，保留 GUID。來源與 MIT LICENSE 存 RecoveredLibraries/UIEffect-upm-source，報告 Reports/uieffect-shader-restoration.json。
- 合計已恢復 54/85 個 Shader；剩餘 31 個尚未處理。開始 RenderMigrationProbe.ValidateAndBuild：檢查 4 場景 missing script、重建 Web Addressables、建置 WebTest，結果待追加。

## 2026-10-15｜新版渲染資源包通過

- RenderMigrationProbe 的場景讀取檢查：PreMainMenu/MainMenu/ForestMap/Battle 全部 missingScripts=0，URP camera 各 1；SpriteRenderer 分別 0/10/462/104。報告 Reports/render-scene-validation.txt。這是 Editor 反序列化檢查，非遊玩驗證。
- 正式 URP + 已恢復 Shader 的 Addressables BuildContent 成功（error=none，163.9 秒）。同一 runner 接續 WebTest Player 建置中。
- MoreMountains.Tools 的 Light2D 仍有一個 RequireComponent attribute 內嵌 assembly-qualified type 字串，Cecil GetTypeReferences 不會列出，所以第一次適配沒涵蓋。已擴充 scripts/adapt-render-typerefs.ps1 掃描 attribute 參數，等這次建置結束後套用。此錯誤在 Editor 為非致命，還要檢查 IL2CPP。
- RenderMigration 備份已生成完整 SHA-256 變更帳本 Reports/render-migration-ledger.json，涵蓋第一次中止前已修改的引用；172 個變更／隔離檔，原檔都有備份。

## 2026-10-15 02:45｜（新聊天接手）主選單首次在瀏覽器正確顯示

- 接手時：02:02 的 WebTest 建置成功，但瀏覽器仍全黑，主控台唯一錯誤 `Missing shader (in 'LensFlareScreenSpace'). PostProcessing render passes will not execute`——2022 版 renderer 的 PostProcessData 缺新版必備 Shader。上一位已在 RenderMigrationProbe 加入改指 URP 套件 PostProcessData.asset，但 `UnityEditor.Rendering.ResourceReloader` 命名空間寫錯（Unity 6.6 是 `UnityEngine.Rendering.ResourceReloader`，在 Core 執行期組件內以 UNITY_EDITOR 包住），導致 02:05 那次編譯失敗。已修正。
- 重跑 RenderMigrationProbe.ValidateAndBuild（日誌 reports/unity6/method-RenderMigrationProbe_ValidateAndBuild-20261015-023350.log）：4 場景 missingScripts=0；Addressables BuildContent error=none（98.7 秒）；WebTest Player Build Succeeded。
- adapt-render-typerefs.ps1 乾跑（含 attribute 參數）：0 筆待改。注意此腳本乾跑也會覆寫 Reports/render-typeref-adaptation.txt，已手動補回原本 1 筆紀錄。
- 新測試工具：scripts/cdp.py（DevTools Protocol）。內建瀏覽器窗格隱藏時會節流；使用者的 Chrome 不允許第二個除錯實例，所以改用 Edge 無頭模式（獨立暫存 profile，ANGLE D3D11 實際用到 RTX 2060），主控台寫入 PeglinBrowserLab/Reports/browser-run-*.log。
- 發現：無頭模式若未加 `--autoplay-policy=no-user-gesture-required`，AudioContext 無法啟動，預載會從 ~333 項後每幀 1 秒（音效相關）。真實使用者需要先點一下頁面才會解鎖音訊——之後要確認手機上第一次點擊前不會卡住預載。
- 加入該旗標後：預載 1046 項 0 失敗約 20 秒；PreMainMenu → **主選單實際渲染出來**（Peglin 標題、哥布林、選單文字、右側彈珠背景動畫），截圖 PeglinBrowserLab/Reports/shot-1.png。後處理錯誤消失。
- 仍見的非致命：UnityServicesProjectConfiguration.json 路徑錯（ServicesCore）；Steam DLC 讀不到；FMOD getSystemObject；I2 Localization 嘗試連開發者的 Google Apps Script 更新語言（CORS 擋下，之後應在 Web 版關閉此外部請求）。語言預設為簡體中文，使用者偏好繁體，之後在設定選單確認。
- 尚未驗證：點擊進入新遊戲／地圖／戰鬥、觸控、存檔持久化、手機效能。

## 2026-10-15 02:55｜主選單→角色→遺物→地圖→戰鬥 在瀏覽器實際跑通（首次可操作遊玩）

- 崩潰原因（browser-run-2.log）：進地圖時 PegLayoutLoader.CheckAndSavePegboardAsset 以 `AssetReference("RegularPeg").LoadAssetAsync<object>()` 取釘子 prefab；ResourceManager 快取鍵是 (location, 請求型別)，預載器只對 TextAsset 補了 object 型別，GameObject 沒有 → 同步載入例外 → 釘子 prefab 為 null → PreInstancePegboardRecursive 記憶體越界，Unity 停止。
- 修正（僅工作副本）：
  1. WebAddressablesBoot：GameObject 位置也以 object 型別預載。預載數 1046→1567 個 (位置,型別)，0 失敗，仍約 20 秒，managed 17 MB。
  2. EnemyManager.cs:251：`InstantiateAsync(...).WaitForCompletion()` 在 WebGL 改為以已預載 prefab `Addressables.LoadAssetAsync<GameObject>(RuntimeKey).WaitForCompletion()` 後直接 Instantiate（#if UNITY_WEBGL && !UNITY_EDITOR，其他平台保留原碼）。
- 只重建 Player（WebPortSetup.BuildWeb，日誌 method-WebPortSetup_BuildWeb-20261015-024656.log，exit 0）。
- 實測（Edge 無頭，browser-run-3.log，截圖 Reports/shot-7~10.png）：主選單→新遊戲→角色選擇→開始→開場遺物（拾取）→ ForestMap 地圖 → 森林關卡 1 戰鬥。戰鬥畫面完整：釘板、敵人與血條、彈珠資訊、遺物欄。**滑鼠點擊發射彈珠：物理彈跳、擊中釘子、傷害結算（史萊姆 60→30）、金幣增加、原版教學對話框都正常。** 過程中沒有新的 [error]／例外（只剩 ServicesCore 設定檔與 Steam 的已知非致命訊息）。
- 這是第一次「可以操作的遊玩」證據，但仍未驗證：整場戰鬥結束→獎勵→回地圖、多關卡／Boss、存檔重新整理後續玩、手機觸控、效能、Release（非 Development）建置。

## 2026-10-15 03:03｜完整回合循環與存檔續玩驗證通過

- 同一建置（browser-run-3.log，shot-11~18）：第一場戰鬥打完 → 原版彈珠升級獎勵 → 商店式獎勵面板（升級／回血）→「繼續」→ 戰後選路（把哥布林射進下方問號槽）→ ForestMap → TextScenario 事件（森林 關卡 2，「奇怪的樹」，選項可選並出結果）。整段沒有 [error]／例外。
- 存檔：Unity WebGL 的 persistentDataPath 寫進瀏覽器 IndexedDB `/idbfs`，可見 Save_0.data、Save_0.data.bak、Save_0r.data（進行中冒險）、Stats_0.data、PlayerPrefs。重新整理頁面（browser-run-4.log）後主選單出現「继续」，按下後回到 ForestMap，金幣 28 與遺物都保留（shot-19~21）。
- 觀察：第一次點擊後畫面從 1256×674 變為 800×600——遊戲設定為全螢幕，Unity WebGL 在使用者手勢時才進入全螢幕，無頭模式的全螢幕是 800×600。手機上應會直接全螢幕（待實機確認）。
- 小問題：部分 TMP 文字的內嵌圖示沒顯示（例如「点击左边的垃圾桶图标 时」「一个 蛋掉落下来」中間空白），但其他內嵌圖示（金幣、釘子）正常。待查 TMP sprite asset／shader。
- 語言目前為簡體中文（遊戲依系統語言或預設），設定選單內應可改繁體，尚未操作。

## 2026-10-15｜觸控、中文缺字修復、繁體預設、正式版建置

- 觸控：scripts/cdp.py 新增 `tap`（只送 touch 事件、不送滑鼠）。在 ForestMap 以觸控點地圖節點可進入事件，事件選項也能用觸控點選；遊戲偵測到觸控後自動隱藏鍵盤提示（I/M/F/ESC）。截圖 shot-22、shot-23。尚未在實體手機驗證。
- 中文缺字根因：遊戲的中日韓字型（indienovaBC 12px、Silver、ShinonomeMaru 等）是 TMP「動態」字型資產，不在預建圖集中的字要在執行期寫進圖集。AssetRipper 把圖集匯出成一般 PNG，Web 平台自動壓成 DXT5，執行期無法寫入 → 新字變空白（例：「烧红的铁砧」只剩「的铁」）。
- 修正：Assets/Editor/FontAtlasFix.cs，把所有動態 TMP 字型的 29 張圖集改為未壓縮、可讀、Alpha8（原本都是 DXT5，見 PeglinBrowserLab/Reports/font-atlas-fix.txt）；原 .meta 備份在 RecoveredLibraries/FontAtlasMetas。日誌 method-FontAtlasFix_ApplyAndBuild-20261015-030721.log，Addressables 與 Player 均成功。驗證：遺物說明裡大量不在預建圖集的字（如繁體「帶電油脂」「鄰近」）正常顯示（shot-27-28.png）。代價：indienova 兩份圖集各 4096×8192 Alpha8（各約 32 MB 記憶體），手機記憶體要再觀察。
- 已知原版資料問題（非移植造成）：簡體模式下個別遺物說明混入繁體並露出 `style=hit>` 標籤。
- 語言：Unity WebGL 把瀏覽器 zh-TW 回報成 ChineseSimplified，所以預設是簡體。新增 Assets/Plugins/WebGL/WebPortLanguage.jslib 取 navigator.languages[0]；I2 LocalizationManager.DetectDeviceLanguage 在 WebGL 下：zh 且含 TW/HK/MO/Hant → 繁體，其他 zh → 簡體。I2Languages 內有「繁體中文 zh-TW」。已存在的瀏覽器 PlayerPrefs 會保留先前選擇，新裝置才會自動繁體；隨時可在設定選單換。
- 關閉 Web 版 I2 向開發者 Google Apps Script 更新翻譯（LanguageSourceData.Import_Google 在 WebGL 直接 return；原本也被 CORS 擋下），遊戲使用內建翻譯，離線可玩。
- 預載器整理：移除 OdinProbe 與 slow-frame 診斷（含診斷版備份 RecoveredLibraries/WebPortHistory/），加入單一 uGUI Text 的「Loading xx%」（只在百分比改變時更新，避免 OnGUI 的效能問題）。
- 新增 WebPortSetup.BuildWebRelease：非 Development、Gzip＋decompressionFallback（任何靜態伺服器可用）、dataCaching，輸出 Builds/WebRelease。結果待追加。

## 2026-10-15 09:40｜正式版（Release）Web 建置驗證通過

- WebPortSetup.BuildWebRelease：Succeeded，errors=0（日誌 reports/unity6/method-WebPortSetup_BuildWebRelease-20261015-092202.log）。輸出 PeglinBrowserLab/Builds/WebRelease 共 76 MB：wasm 16.3 MB、data 15.6 MB（皆 gzip）、Addressables bundles 約 44 MB。比 Development 版（wasm 135 MB）小很多。
- 本機伺服器：scripts/serve-web.py。注意本機 Windows 保留了 8768–8967 等 TCP 埠（netsh excludedportrange），會出現 WinError 10013；改用 8760。
- 全新 Edge 設定檔測試（browser-release-2.log、Reports/rel-*.png）：冷啟動（含下載）31 秒到主選單；預載時畫面顯示「Loading xx%」；**瀏覽器語言 zh-TW 時自動繁體中文**（新的遊戲／哥布林寶典／選項…），角色說明等繁體字完整（動態補字正常）；無開發者主控台。新遊戲→遺物→森林關卡 1 戰鬥，可發射彈珠；此關含 SpriteShape 曲線長釘，正常顯示——先前「SpriteShape 17 序列化相容性未驗證」的風險在這裡得到實際驗證。除已知 ServicesCore 設定檔訊息外無錯誤。
- 尚未驗證：實體手機（iOS Safari／Android Chrome）效能與記憶體、Boss 與後段地區、長時間遊玩。

## 2026-10-15｜推上私人 GitHub repo

- 使用者把 https://github.com/yc0935019269cloud-create/peglinnnnnnn 改為私人後，要求推整個資源（未登入 API 查詢回 404，確認非公開）。
- 結構：git 資料在 哥布林彈珠行動版\.git，`core.worktree=E:/NEWTEST`，`.git/info/exclude` 白名單只收 `哥布林彈珠行動版/` 與 `PeglinBrowserLab/`（Unity 專案必須留在英文路徑，所以不搬資料夾）。E:\NEWTEST 其他專案不受影響。之後在 `哥布林彈珠行動版` 資料夾內執行 git 指令即可。
- 分四批提交推送：研究筆記／腳本／報告 → RecoveredProject → Unity6Project＋RecoveredLibraries → Reports＋Builds/WebRelease。
- 未收錄：tools/ 下載檔、原始 Unity 日誌、Development 建置（wasm 135 MB）、Steam 原版 Windows Addressables 副本（一個 bundle 105.7 MB 超過 GitHub 上限）。
- repo 必須保持私人；不要開 GitHub Pages 或其他公開網址（公開散布商業遊戲內容）。

## 2026-10-15｜GitHub Pages 與實體 iPhone 首次載入問題

- 使用者本次明確要求設定 GitHub Pages；當時遠端 repo 已是 public。已新增 .github/workflows/pages.yml，只發布 PeglinBrowserLab/Builds/WebRelease，Pages 來源改為 GitHub Actions。提交 5ccc1f8c62abf594b60f5e4377a768d149281c52，run 38669154765 success；兩個舊的整庫 Jekyll run 均取消。
- 公開網址 https://yc0935019269cloud-create.github.io/peglinnnnnnn/ 回應 200，桌面 Chrome 實際到繁體中文主選單。此結果不是手機驗證。
- 使用者回報 iPhone 14 Plus：到哥布林載入畫面後整個網頁重新載入。手機瀏覽器種類與 iOS 版本尚未提供。懷疑 WebContent 程序因記憶體壓力或其他崩潰而重啟，但無裝置 crash/記憶體紀錄，不能判定根因。
- 原始碼檢查：WebAddressablesBoot concurrent=48，預載所有非場景 Addressables 並永久保留 handles；手機 index.html 的 devicePixelRatio=1 仍被註解；ProjectSettings 初始 heap 32 MB、上限 2048 MB、growth 20%。既有字型修復筆記記載兩份 4096x8192 Alpha8 圖集。這些是需量測的記憶體風險，不代表已確認 OOM。
- 本次診斷尚未更動遊戲或重新建置；手機問題尚未修復，不能宣稱可正常手機遊玩。

## 2026-10-16｜滿版網頁外框

- WebRelease/index.html 改為黑色滿版畫布，移除預設 Unity logo、favicon、白邊、底部工具列與固定 1024x576 容器；保留簡潔載入進度及錯誤提示。
- 新增 Assets/WebGLTemplates/FullWindow，BuildWeb 與 BuildWebRelease 均選 PROJECT:FullWindow，後續建置保留外觀。既有 wasm、資料檔與存檔識別不變。
- 瀏覽器實測 DOM：424x608 與 1280x720 畫布均為 (0,0,viewportWidth,viewportHeight)，無頁面溢出或 footer；可進入 Unity 資源預載。這次未重建 Unity，未驗證實體 iPhone 載入重啟問題。
- 使用 100dvh 因應視窗高度；填滿網頁視窗不等於能自動隱藏瀏覽器網址列。遊戲相機內的比例留黑由原遊戲控制，避免裁切 UI。
