# 🚀 WinForms BMI Calculator Pro (進階魔改版)

這是我為了挑戰 C# WinForms 框架極限所開發的 BMI 計算機。

原本的作業要求只是做個簡單的 BMI 計算，但我不想只交一個死板的灰色原生視窗交差。因此，我在這個專案中實作了 **OS 級別的全域按鍵攔截**、**GDI+ 底層 UI 重繪**，以及完整的 **File I/O 狀態管理**，試圖在老舊的 WinForms 框架上，打造出符合現代 UX 標準的「純鍵盤 (Keyboard-First)」操作體驗。

## 💡 技術亮點與實作細節 (Technical Highlights)

不只是會動而已，這個專案解決了幾個 WinForms 開發常見的痛點：

### 1. 🎨 深度 UI 魔改 (Custom GDI+ Rendering)
原生 WinForms 的元件樣式非常受限。為此，我覆寫 (Override) 了 `OnPaintBackground` 與按鈕的 `Paint` 事件，利用 `LinearGradientBrush` 自己刻出平滑的垂直漸層，並實作了一鍵切換「漸層 / 純色 (Flat UI)」的渲染邏輯。
* **API Interop**: 拒絕在 TextBox 旁邊放醜醜的 Label，我直接 `DllImport` 呼叫 Windows 底層 API (`user32.dll` 的 `SendMessage`)，把提示字串以原生浮水印 (CueBanner) 的方式刻進輸入框裡。

### 2. ⌨️ 真正的 100% 無滑鼠操作 (Message Filtering)
一般做快捷鍵只會綁 `KeyDown`，但只要焦點 (Focus) 跑到下拉選單，快捷鍵就會失效。
為了達成絕對的「無滑鼠操作」，我讓 Form 繼承了 `IMessageFilter`，直接在作業系統派發訊息的底層攔截 `WM_KEYDOWN` (0x0100)。現在不管焦點在哪，`Ctrl+A~G` 的快捷鍵都能強制執行。

### 3. 💾 動態生成對話框與 I/O (File I/O & Memory Management)
* **狀態儲存**: 支援動態將目前的計算狀態存入 `MenuStrip` 的 `Tag` 中。因為不想依賴老舊的 `Microsoft.VisualBasic.Interaction`，我用純 code 動態 `new` 了一個輸入對話框 (InputBox)。
* **防範 Memory Leak**: 所有動態生成的 Form 與 ColorDialog，我都嚴格使用 `using` 區塊包覆，確保對話框關閉後資源會被 Garbage Collector 確實回收。
* **資料匯出**: 實作了 `SaveFileDialog` 與 `StringBuilder`，能將記憶體中的多筆紀錄格式化後匯出成 `.txt` 檔案，達成基本的資料持久化。

### 4. 🛡️ 無干擾防呆機制 (Non-Intrusive Validation)
傳統的防呆遇到錯誤就會無腦彈出 `MessageBox`，這對使用者極度干擾。
我將所有格式轉換 (`TryParse`) 與數值邏輯錯誤（如負數或零），全部改為在主畫面的 `Label` 即時反白回饋，並透過程式自動 `Focus()` 與 `SelectAll()` 錯誤的欄位，確保使用者的雙手不用離開鍵盤去點擊「確定」。

---

## ⚡ 快捷鍵指南 (Shortcuts)

系統設計為可完全脫離滑鼠操作，以下為實作的快捷鍵綁定：

| 快捷鍵 | 觸發動作 | 開發備註 |
| :--- | :--- | :--- |
| `Enter` | 欄位跳轉 / 執行計算 | 攔截 Enter 原生行為，自訂焦點移轉 (`SelectAll`) |
| `Ctrl + M` | 展開/收起「紀錄管理」 | |
| `Ctrl + S` | 儲存紀錄 | 動態生成 InputBox 寫入記憶體 |
| `Ctrl + L` | 展開「載入資料」 | 讀取 ToolStripMenuItem.Tag 並觸發重算 |
| `Ctrl + F` | **匯出紀錄至 TXT** | 呼叫 SaveFileDialog 進行檔案 I/O |
| `Ctrl + A` | 展開/收起「個性化」 | |
| `Ctrl + B` | 更改視窗背景色 | 呼叫 ColorDialog |
| `Ctrl + C` | 更改按鈕顏色 | 呼叫 ColorDialog |
| `Ctrl + D` | 更改文字顏色 | 呼叫 ColorDialog |
| `Ctrl + E` | **切換渲染模式** | 觸發 `Invalidate()` 重新繪製漸層/純色 UI |
| `Ctrl + G` | 關於本系統 | |

> ⚠️ **開發者踩坑筆記 (ColorDialog 鍵盤操作 issue)：**
> Windows 內建的 `ColorDialog` 在純鍵盤操作下有個反人類的設計：用方向鍵移動到目標顏色後，**必須先按 `Space (空白鍵)` 確實選取**，才能按 `Enter`。如果直接按 Enter 會回傳預設的黑色，導致漸層渲染壞掉。為此我已在程式碼中塞入 `cd.Color = current` 作為初始值防呆，但也在此特別備註給其他開發者參考。

---

## 🚀 環境與執行 (How to Run)

1. **開發環境**：Visual Studio (C# .NET Framework)。
2. **執行方式**：打開 `BMI計算機.sln`，按下 `F5` 即可編譯執行。
3. **快速測試建議**：
   * 直接打字 (例如身高 `175` -> `Enter` -> 體重 `70` -> `Enter`)。
   * 試著輸入英文字母或 `-5` 測試防呆機制是否不會彈出惱人視窗。
   * 按下 `Ctrl + B` 換個顏色，再按 `Ctrl + E` 看看 UI 渲染模式的切換。
   * 按下 `Ctrl + S` 隨便存個紀錄，接著按 `Ctrl + F` 匯出文字檔檢查 I/O 功能。

---
*Author: 元智大學 資訊工程學系*
