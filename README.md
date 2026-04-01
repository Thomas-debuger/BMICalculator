# WinForms BMI Calculator Pro 

## 一. 專案簡介

這是我為了挑戰 C# WinForms 框架極限所開發的 BMI 計算機，同時也是我在元智大學資訊工程學系的實作專案。

原本的作業要求只是實作基礎的 BMI 運算，但我希望能突破原生 WinForms 僵硬的灰色視窗限制。因此，我在這個專案中實作了 **OS 級別的全域按鍵攔截 (Message Filtering)**、**GDI+ 底層 UI 重繪 (Custom Rendering)**，以及完整的 **File I/O 狀態管理**，打造出符合現代 UX 標準、且能 100% 透過鍵盤驅動 (Keyboard-First) 的流暢操作體驗。

### 核心技術亮點：
* **深度 UI 魔改**：覆寫 `OnPaintBackground`，利用 `LinearGradientBrush` 實作平滑垂直漸層，並支援一鍵無縫切換「漸層 / 純色扁平化」渲染模式。另外，直接呼叫 Windows 底層 API (`user32.dll`) 實作原生 TextBox 浮水印 (CueBanner)。
* **絕對無滑鼠操作**：透過繼承 `IMessageFilter` 攔截作業系統底層的 `WM_KEYDOWN`，解決原生 WinForms 焦點落入選單後快捷鍵即失效的痛點，實現真正的全鍵盤控制。
* **記憶體與狀態管理**：動態生成 InputBox 以寫入記憶體狀態，並嚴格利用 `using` 區塊確保動態資源 (Form, ColorDialog) 釋放，防止 Memory Leak。
* **無干擾防呆機制**：捨棄傳統干擾體驗的 `MessageBox` 彈窗，改採即時 UI 反白與自動 `SelectAll()` 焦點轉移，確保雙手無需離開鍵盤即可修正錯誤。

---

## 二. 執行說明

### 1. 開發與執行環境
* **IDE**: Visual Studio 2019 或更新版本
* **Framework**: .NET Framework 4.7.2 (或相容版本)

### 2. 編譯與啟動步驟
1. 將本專案 Clone 或下載至本機端。
2. 雙擊開啟 `BMI計算機.sln` 解決方案檔。
3. 按下 `F5` 或是點擊上方工具列的「開始」進行編譯並執行。

### 3. 操作與快捷鍵指南 (全鍵盤流)
本系統設計為可完全脫離滑鼠操作，程式啟動後，請直接使用鍵盤進行以下指令：

| 快捷鍵 | 觸發動作 | 開發實作備註 |
| :--- | :--- | :--- |
| `Enter` | 欄位跳轉 / 執行計算 | 攔截 Enter 原生行為，自訂焦點移轉 |
| `Ctrl + M` | 展開/收起「紀錄管理」 | |
| `Ctrl + S` | 儲存目前紀錄 | 動態生成 InputBox 寫入記憶體 |
| `Ctrl + L` | 展開「載入資料」 | 讀取 ToolStripMenuItem.Tag 並觸發重算 |
| `Ctrl + F` | **匯出紀錄至 TXT** | 整合 SaveFileDialog 進行檔案 I/O |
| `Ctrl + A` | 展開/收起「個性化」 | |
| `Ctrl + B` | 更改視窗背景色 | 呼叫系統 ColorDialog |
| `Ctrl + C` | 更改按鈕顏色 | 呼叫系統 ColorDialog |
| `Ctrl + D` | 更改文字顏色 | 呼叫系統 ColorDialog |
| `Ctrl + E` | **切換渲染模式** | 觸發 `Invalidate()` 重新繪製漸層/純色 UI |
| `Ctrl + G` | 關於本系統 | 顯示開發資訊 |

> **開發者踩坑筆記 (ColorDialog 鍵盤操作 Issue)：**
> Windows 內建的 `ColorDialog` 在純鍵盤操作下有個反人類的設計：用方向鍵移動到目標顏色後，**必須先按 `Space (空白鍵)` 確實選取**，才能按 `Enter`。若直接按 Enter 會回傳預設黑色，導致漸層渲染破圖。我已在程式碼中塞入原顏色作為初始值防呆，特此備註。

---

## 三. 專案截圖

*(說明：以下為系統實際運行之畫面截圖)*

### 1. 主畫面與 GDI+ 漸層渲染展示

<img width="1388" height="902" alt="螢幕擷取畫面 2026-04-01 150907" src="https://github.com/user-attachments/assets/0844b70b-85bc-4a90-9748-de4c456a03e6" />

> 展示自訂的平滑漸層背景、按鈕渲染，以及底層 API 實作的 TextBox 浮水印效果。

### 2. 無干擾防呆機制與錯誤攔截實測

<img width="1391" height="905" alt="螢幕擷取畫面 2026-04-01 151024" src="https://github.com/user-attachments/assets/8a9cf8b0-a601-49f4-972b-e328108277f4" />

<img width="1391" height="907" alt="螢幕擷取畫面 2026-04-01 151117" src="https://github.com/user-attachments/assets/7b21c037-94c3-4a0b-a855-cca0136f54be" />


> 輸入無效字元或負數時，系統不彈出 MessageBox，而是直接以紅色反白提示並鎖定錯誤欄位，維持鍵盤操作的流暢度。

### 3. 動態紀錄管理與 TXT 檔案匯出成果

<img width="1392" height="907" alt="螢幕擷取畫面 2026-04-01 151223" src="https://github.com/user-attachments/assets/60191b86-8192-4d32-a80d-e198767a88f9" />

<img width="1384" height="897" alt="螢幕擷取畫面 2026-04-01 151244" src="https://github.com/user-attachments/assets/ab8bce2e-152d-4dad-8e55-1addd362812c" />

> 透過 `System.IO` 與 `SaveFileDialog`，將記憶體中的多筆 BMI 紀錄格式化並輸出至純文字檔的實測結果。
