using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BMI計算機
{
    // 繼承 IMessageFilter，強制在最底層攔截鍵盤按鍵
    public partial class frmBMI : Form, IMessageFilter
    {
        // 呼叫 Windows API 來設定 TextBox 提示字 (浮水印)
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);
        private const int EM_SETCUEBANNER = 0x1501;

        // ==========================================
        // 宣告 MenuStrip 相關元件
        // ==========================================
        private MenuStrip topMenu;
        private ToolStripMenuItem menuManage;
        private ToolStripMenuItem menuSave;
        private ToolStripMenuItem menuLoad;

        public frmBMI()
        {
            InitializeComponent();
            SetupMenu();
            BeautifyUI();

            // 啟動全域按鍵攔截器
            Application.AddMessageFilter(this);

            // 鍵盤監聽事件綁定
            this.txtHeight.KeyDown += new KeyEventHandler(this.txtHeight_KeyDown);
            this.txtWeight.KeyDown += new KeyEventHandler(this.txtWeight_KeyDown);

            // (已移除限制只能輸入數字的防呆，現在可以輸入任何字元)

            // 綁定按鈕點擊事件
            this.btnRun.Click += new EventHandler(this.btnRun_Click);
            this.btnClear.Click += new EventHandler(this.btnClear_Click);

            // 綁定按鈕的繪圖事件與焦點事件
            this.btnRun.Paint += new PaintEventHandler(this.GradientButton_Paint);
            this.btnClear.Paint += new PaintEventHandler(this.GradientButton_Paint);
            this.btnRun.Enter += new EventHandler(this.Button_FocusChanged);
            this.btnRun.Leave += new EventHandler(this.Button_FocusChanged);
            this.btnClear.Enter += new EventHandler(this.Button_FocusChanged);
            this.btnClear.Leave += new EventHandler(this.Button_FocusChanged);
        }

        // 關閉程式時記得解除攔截器，避免記憶體洩漏
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Application.RemoveMessageFilter(this);
            base.OnFormClosed(e);
        }

        // ==========================================
        // 全域快捷鍵攔截 (無滑鼠操作挑戰)
        // ==========================================
        public bool PreFilterMessage(ref Message m)
        {
            const int WM_KEYDOWN = 0x0100;
            if (m.Msg == WM_KEYDOWN)
            {
                Keys keyCode = (Keys)(int)m.WParam & Keys.KeyCode;

                if (Control.ModifierKeys == Keys.Control)
                {
                    if (keyCode == Keys.M) // Ctrl + M
                    {
                        if (menuManage.DropDown.Visible) menuManage.HideDropDown();
                        else menuManage.ShowDropDown();
                        return true;
                    }
                    else if (keyCode == Keys.S) // Ctrl + S
                    {
                        if (menuManage.DropDown.Visible) menuManage.HideDropDown();
                        MenuSave_Click(this, EventArgs.Empty);
                        return true;
                    }
                    else if (keyCode == Keys.L) // Ctrl + L
                    {
                        if (!menuManage.DropDown.Visible) menuManage.ShowDropDown();
                        menuLoad.ShowDropDown();
                        return true;
                    }
                }
            }
            return false;
        }

        // ==========================================
        // 建立與設定下拉選單 (MenuStrip)
        // ==========================================
        private void SetupMenu()
        {
            topMenu = new MenuStrip();
            topMenu.BackColor = Color.WhiteSmoke;

            menuManage = new ToolStripMenuItem("紀錄管理(Ctrl+M)");

            menuSave = new ToolStripMenuItem("儲存當前資料(Ctrl+S)");
            menuSave.Click += MenuSave_Click;

            menuLoad = new ToolStripMenuItem("載入資料(Ctrl+L)");

            ToolStripMenuItem emptyLoad = new ToolStripMenuItem("(目前無儲存紀錄)");
            emptyLoad.Enabled = false;
            menuLoad.DropDownItems.Add(emptyLoad);

            menuManage.DropDownItems.Add(menuSave);
            menuManage.DropDownItems.Add(new ToolStripSeparator());
            menuManage.DropDownItems.Add(menuLoad);
            topMenu.Items.Add(menuManage);

            this.Controls.Add(topMenu);
            this.MainMenuStrip = topMenu;
        }

        // ==========================================
        // 儲存紀錄邏輯 
        // ==========================================
        private void MenuSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtHeight.Text) || string.IsNullOrWhiteSpace(txtWeight.Text))
            {
                ShowError("儲存失敗：請先輸入身高與體重", txtHeight);
                return;
            }

            string recordName = PromptForName();
            if (!string.IsNullOrWhiteSpace(recordName))
            {
                if (menuLoad.DropDownItems.Count == 1 && !menuLoad.DropDownItems[0].Enabled)
                {
                    menuLoad.DropDownItems.Clear();
                }

                ToolStripMenuItem newItem = new ToolStripMenuItem(recordName);
                newItem.Tag = new string[] { txtHeight.Text, txtWeight.Text };
                newItem.Click += LoadRecord_Click;

                menuLoad.DropDownItems.Add(newItem);

                lblResult.Text = $"✅ 已成功儲存紀錄：{recordName}";
                lblResult.BackColor = Color.LightYellow;
                lblResult.ForeColor = Color.Blue;
            }
        }

        // ==========================================
        // 載入紀錄邏輯
        // ==========================================
        private void LoadRecord_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is string[] data)
            {
                txtHeight.Text = data[0];
                txtWeight.Text = data[1];
                btnRun.PerformClick();
            }
        }

        // ==========================================
        // 自製的輸入彈出視窗 
        // ==========================================
        private string PromptForName()
        {
            Form prompt = new Form()
            {
                Width = 320,
                Height = 160,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "儲存紀錄",
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };
            Label textLabel = new Label() { Left = 20, Top = 20, Text = "請輸入儲存名稱 (例如: 本人增肌期)：", Width = 250, Font = new Font("微軟正黑體", 10F) };
            TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 260, Font = new Font("微軟正黑體", 10F) };
            Button confirmation = new Button() { Text = "確定", Left = 180, Width = 100, Top = 85, DialogResult = DialogResult.OK, FlatStyle = FlatStyle.System };
            Button cancel = new Button() { Text = "取消", Left = 70, Width = 100, Top = 85, DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.System };

            prompt.Controls.Add(textLabel);
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(cancel);

            prompt.AcceptButton = confirmation;
            prompt.CancelButton = cancel;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }

        private void Button_FocusChanged(object sender, EventArgs e)
        {
            if (sender is Button btn) btn.Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (this.ClientRectangle.Width == 0 || this.ClientRectangle.Height == 0) return;

            using (LinearGradientBrush brush = new LinearGradientBrush(
                this.ClientRectangle,
                Color.LightCyan,
                Color.DeepSkyBlue,
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }

        private void GradientButton_Paint(object sender, PaintEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            using (LinearGradientBrush brush = new LinearGradientBrush(
                btn.ClientRectangle,
                Color.LightPink,
                Color.HotPink,
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, btn.ClientRectangle);
            }

            TextRenderer.DrawText(
                e.Graphics,
                btn.Text,
                btn.Font,
                btn.ClientRectangle,
                btn.ForeColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            if (btn.Focused)
            {
                using (Pen pen = new Pen(Color.Black, 2))
                {
                    Rectangle rect = btn.ClientRectangle;
                    rect.Width -= 1;
                    rect.Height -= 1;
                    e.Graphics.DrawRectangle(pen, rect);
                }
            }
        }

        private void BeautifyUI()
        {
            this.ForeColor = SystemColors.ControlText;
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;

            Font baseFont = new Font("微軟正黑體", 12F, FontStyle.Regular);
            this.Font = baseFont;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;

            grpInput.Font = new Font("微軟正黑體", 14F, FontStyle.Bold);
            grpOutput.Font = new Font("微軟正黑體", 14F, FontStyle.Bold);

            grpInput.BackColor = Color.Transparent;
            grpOutput.BackColor = Color.Transparent;

            txtHeight.Font = baseFont;
            txtWeight.Font = baseFont;
            btnRun.Font = baseFont;
            btnClear.Font = baseFont;
            lblResult.Font = baseFont;

            txtHeight.BackColor = txtWeight.BackColor = SystemColors.Window;
            txtHeight.ForeColor = txtWeight.ForeColor = Color.Purple;
            txtHeight.BorderStyle = txtWeight.BorderStyle = BorderStyle.Fixed3D;

            txtHeight.TabIndex = 0;
            txtWeight.TabIndex = 1;
            btnRun.TabIndex = 2;
            btnClear.TabIndex = 3;

            SendMessage(txtHeight.Handle, EM_SETCUEBANNER, 1, "例如：180");
            SendMessage(txtWeight.Handle, EM_SETCUEBANNER, 1, "例如：60");

            btnRun.FlatStyle = FlatStyle.Flat;
            btnRun.FlatAppearance.BorderSize = 0;
            btnRun.ForeColor = Color.White;
            btnRun.Cursor = Cursors.Hand;

            btnClear.FlatStyle = FlatStyle.Flat;
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.ForeColor = Color.White;
            btnClear.Cursor = Cursors.Hand;

            lblResult.TextAlign = ContentAlignment.MiddleCenter;
            lblResult.ForeColor = Color.Black;
            lblResult.BackColor = Color.Transparent;
        }

        private void txtHeight_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                txtWeight.Focus();
                txtWeight.SelectAll();
            }
        }

        private void txtWeight_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                btnRun.PerformClick();
            }
        }

        // ==========================================
        // 精準輸出指定的錯誤訊息文字，符合一切輸入情況
        // ==========================================
        private void btnRun_Click(object sender, EventArgs e)
        {
            lblResult.BackColor = Color.Transparent;
            lblResult.ForeColor = Color.Black;

            bool isHeightValid = double.TryParse(txtHeight.Text, out double h);
            bool isWeightValid = double.TryParse(txtWeight.Text, out double w);

            // 驗證身高輸入
            if (isHeightValid)
            {
                if (h <= 0)
                {
                    ShowError("身高必須大於零，身高值錯誤", txtHeight);
                    return;
                }
            }
            else
            {
                ShowError("請輸入有效的身高數值，身高值錯誤", txtHeight);
                return;
            }

            // 驗證體重輸入
            if (isWeightValid)
            {
                if (w <= 0)
                {
                    ShowError("體重必須大於零，體重值錯誤", txtWeight);
                    return;
                }
            }
            else
            {
                ShowError("請輸入有效的體重數值，體重值錯誤", txtWeight);
                return;
            }

            // 計算邏輯
            h /= 100;
            double bmi = w / (h * h);
            double idealW = 22 * (h * h);

            string[] results = { "體重過輕", "健康體位", "體位過重", "輕度肥胖", "中度肥胖", "重度肥胖" };
            Color[] colors = { Color.DeepSkyBlue, Color.LimeGreen, Color.Bisque, Color.Coral, Color.Tomato, Color.MediumOrchid };

            int idx = bmi < 18.5 ? 0 : bmi < 24 ? 1 : bmi < 27 ? 2 : bmi < 30 ? 3 : bmi < 35 ? 4 : 5;

            lblResult.Text = $"BMI：{bmi:F2} ({results[idx]})\r\n目標體重：{idealW:F1} kg";
            lblResult.BackColor = colors[idx];
            lblResult.ForeColor = Color.Black;
        }

        private void ShowError(string msg, TextBox target)
        {
            lblResult.ForeColor = Color.Red;
            lblResult.BackColor = Color.White; // 讓錯誤訊息底色反白更清楚
            lblResult.Text = msg;
            target.Focus();
            target.SelectAll();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtHeight.Clear();
            txtWeight.Clear();
            lblResult.Text = "已清除";
            lblResult.BackColor = Color.Transparent;
            lblResult.ForeColor = Color.Black;
            txtHeight.Focus();
        }
    }
}