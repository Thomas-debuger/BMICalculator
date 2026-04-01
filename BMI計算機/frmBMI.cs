using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.IO;
using System.Text;

namespace BMI計算機
{
    public partial class frmBMI : Form, IMessageFilter
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);
        private const int EM_SETCUEBANNER = 0x1501;

        private MenuStrip topMenu;
        private ToolStripMenuItem menuManage, menuSave, menuLoad, menuExport;
        private ToolStripMenuItem menuColor, menuBgColor, menuBtnColor, menuTxtColor;
        private ToolStripMenuItem menuToggleGradient;
        private ToolStripMenuItem menuAbout; // [新增] 關於選單

        private Label lblKeyboardTip;

        private Color customBgColor = Color.DeepSkyBlue;
        private Color customBtnColor = Color.HotPink;
        private Color customTxtColor = Color.Purple;
        private bool isGradient = true;

        public frmBMI()
        {
            InitializeComponent();
            SetupMenu();
            BeautifyUI();
            Application.AddMessageFilter(this);

            this.txtHeight.KeyDown += new KeyEventHandler(this.txtHeight_KeyDown);
            this.txtWeight.KeyDown += new KeyEventHandler(this.txtWeight_KeyDown);
            this.btnRun.Click += new EventHandler(this.btnRun_Click);
            this.btnClear.Click += new EventHandler(this.btnClear_Click);
            this.btnRun.Paint += new PaintEventHandler(this.CustomButton_Paint);
            this.btnClear.Paint += new PaintEventHandler(this.CustomButton_Paint);
            this.btnRun.Enter += new EventHandler(this.Button_FocusChanged);
            this.btnRun.Leave += new EventHandler(this.Button_FocusChanged);
            this.btnClear.Enter += new EventHandler(this.Button_FocusChanged);
            this.btnClear.Leave += new EventHandler(this.Button_FocusChanged);
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            Application.RemoveMessageFilter(this);
            base.OnFormClosed(e);
        }

        public bool PreFilterMessage(ref Message m)
        {
            const int WM_KEYDOWN = 0x0100;
            if (m.Msg == WM_KEYDOWN)
            {
                Keys keyCode = (Keys)(int)m.WParam & Keys.KeyCode;
                if (Control.ModifierKeys == Keys.Control)
                {
                    switch (keyCode)
                    {
                        case Keys.M:
                            if (menuManage.DropDown.Visible) menuManage.HideDropDown();
                            else menuManage.ShowDropDown();
                            return true;
                        case Keys.S:
                            if (menuManage.DropDown.Visible) menuManage.HideDropDown();
                            MenuSave_Click(this, EventArgs.Empty);
                            return true;
                        case Keys.L:
                            if (!menuManage.DropDown.Visible) menuManage.ShowDropDown();
                            menuLoad.ShowDropDown();
                            return true;
                        case Keys.F:
                            if (menuManage.DropDown.Visible) menuManage.HideDropDown();
                            MenuExport_Click(this, EventArgs.Empty);
                            return true;
                        case Keys.A:
                            if (menuColor.DropDown.Visible) menuColor.HideDropDown();
                            else menuColor.ShowDropDown();
                            return true;
                        case Keys.B:
                            if (menuColor.DropDown.Visible) menuColor.HideDropDown();
                            MenuBgColor_Click(this, EventArgs.Empty);
                            return true;
                        case Keys.C:
                            if (menuColor.DropDown.Visible) menuColor.HideDropDown();
                            MenuBtnColor_Click(this, EventArgs.Empty);
                            return true;
                        case Keys.D:
                            if (menuColor.DropDown.Visible) menuColor.HideDropDown();
                            MenuTxtColor_Click(this, EventArgs.Empty);
                            return true;
                        case Keys.E:
                            if (menuColor.DropDown.Visible) menuColor.HideDropDown();
                            MenuToggleGradient_Click(this, EventArgs.Empty);
                            return true;
                        case Keys.G: // [修改] Ctrl + G: 關於本系統
                            MenuAbout_Click(this, EventArgs.Empty);
                            return true;
                    }
                }
            }
            return false;
        }

        private void SetupMenu()
        {
            topMenu = new MenuStrip { BackColor = Color.WhiteSmoke };

            // 紀錄管理
            menuManage = new ToolStripMenuItem("紀錄管理(Ctrl+M)");
            menuSave = new ToolStripMenuItem("儲存當前資料(Ctrl+S)");
            menuSave.Click += MenuSave_Click;
            menuLoad = new ToolStripMenuItem("載入資料(Ctrl+L)");
            menuLoad.DropDownItems.Add(new ToolStripMenuItem("(目前無儲存紀錄)") { Enabled = false });

            menuExport = new ToolStripMenuItem("匯出紀錄至文字檔(Ctrl+F)");
            menuExport.Click += MenuExport_Click;

            menuManage.DropDownItems.Add(menuSave);
            menuManage.DropDownItems.Add(new ToolStripSeparator());
            menuManage.DropDownItems.Add(menuLoad);
            menuManage.DropDownItems.Add(new ToolStripSeparator());
            menuManage.DropDownItems.Add(menuExport);

            // 個性化顏色
            menuColor = new ToolStripMenuItem("個性化顏色(Ctrl+A)");
            menuBgColor = new ToolStripMenuItem("背景顏色(Ctrl+B)");
            menuBgColor.Click += MenuBgColor_Click;
            menuBtnColor = new ToolStripMenuItem("按鈕顏色(Ctrl+C)");
            menuBtnColor.Click += MenuBtnColor_Click;
            menuTxtColor = new ToolStripMenuItem("Textbox文字顏色(Ctrl+D)");
            menuTxtColor.Click += MenuTxtColor_Click;

            menuToggleGradient = new ToolStripMenuItem("啟用漸層效果(Ctrl+E)");
            menuToggleGradient.CheckOnClick = true;
            menuToggleGradient.Checked = isGradient;
            menuToggleGradient.Click += MenuToggleGradient_Click;

            menuColor.DropDownItems.Add(menuBgColor);
            menuColor.DropDownItems.Add(menuBtnColor);
            menuColor.DropDownItems.Add(menuTxtColor);
            menuColor.DropDownItems.Add(new ToolStripSeparator());
            menuColor.DropDownItems.Add(menuToggleGradient);

            // 關於選單 [修改快捷鍵文字]
            menuAbout = new ToolStripMenuItem("關於(Ctrl+G)");
            menuAbout.Click += MenuAbout_Click;

            topMenu.Items.Add(menuManage);
            topMenu.Items.Add(menuColor);
            topMenu.Items.Add(menuAbout);
            this.Controls.Add(topMenu);
            this.MainMenuStrip = topMenu;
        }

        // ==========================================
        // 關於本系統 邏輯
        // ==========================================
        private void MenuAbout_Click(object sender, EventArgs e)
        {
            string aboutMsg = "【 BMI 計算機 Pro 版 】\n\n" +
                              "這是一款結合視覺美學與極致操作體驗的健康管理工具。\n\n" +
                              "系統核心特色：\n" +
                              "• 智慧防呆：支援全字元防錯與精準 BMI 體位分析\n" +
                              "• 極致操作：100% 全鍵盤快捷支援 (無滑鼠挑戰)\n" +
                              "• 資料管理：內建紀錄管理器，支援儲存、載入\n" +
                              "• 檔案匯出：一鍵匯出歷史紀錄至 TXT 檔案\n" +
                              "• 深度客製：自由更改視窗背景、按鈕與文字顏色\n" +
                              "• 視覺切換：無縫切換平滑漸層與純色扁平化風格\n\n" +
                              "開發單位：元智大學資訊工程學系 ";

            MessageBox.Show(aboutMsg, "關於本系統", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void MenuBgColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                cd.Color = customBgColor;
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    customBgColor = cd.Color;
                    this.Invalidate();
                }
            }
        }

        private void MenuBtnColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                cd.Color = customBtnColor;
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    customBtnColor = cd.Color;
                    btnRun.Invalidate();
                    btnClear.Invalidate();
                }
            }
        }

        private void MenuTxtColor_Click(object sender, EventArgs e)
        {
            using (ColorDialog cd = new ColorDialog())
            {
                cd.Color = customTxtColor;
                if (cd.ShowDialog() == DialogResult.OK)
                {
                    customTxtColor = cd.Color;
                    txtHeight.ForeColor = customTxtColor;
                    txtWeight.ForeColor = customTxtColor;
                }
            }
        }

        private void MenuToggleGradient_Click(object sender, EventArgs e)
        {
            isGradient = !isGradient;
            menuToggleGradient.Checked = isGradient;
            this.Invalidate();
            btnRun.Invalidate();
            btnClear.Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (this.ClientRectangle.Width <= 0 || this.ClientRectangle.Height <= 0) return;

            if (isGradient)
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(this.ClientRectangle, Color.White, customBgColor, LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, this.ClientRectangle);
                }
            }
            else
            {
                using (SolidBrush brush = new SolidBrush(customBgColor))
                {
                    e.Graphics.FillRectangle(brush, this.ClientRectangle);
                }
            }
        }

        private void CustomButton_Paint(object sender, PaintEventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;

            if (isGradient)
            {
                using (LinearGradientBrush brush = new LinearGradientBrush(btn.ClientRectangle, Color.WhiteSmoke, customBtnColor, LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, btn.ClientRectangle);
                }
            }
            else
            {
                using (SolidBrush brush = new SolidBrush(customBtnColor))
                {
                    e.Graphics.FillRectangle(brush, btn.ClientRectangle);
                }
            }

            TextRenderer.DrawText(e.Graphics, btn.Text, btn.Font, btn.ClientRectangle, Color.Black, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            if (btn.Focused)
            {
                using (Pen pen = new Pen(Color.Black, 2))
                {
                    Rectangle rect = btn.ClientRectangle; rect.Width -= 1; rect.Height -= 1;
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
            txtHeight.ForeColor = customTxtColor;
            txtWeight.ForeColor = customTxtColor;
            txtHeight.BorderStyle = txtWeight.BorderStyle = BorderStyle.Fixed3D;

            txtHeight.TabIndex = 0;
            txtWeight.TabIndex = 1;
            btnRun.TabIndex = 2;
            btnClear.TabIndex = 3;

            SendMessage(txtHeight.Handle, EM_SETCUEBANNER, 1, "例如：180");
            SendMessage(txtWeight.Handle, EM_SETCUEBANNER, 1, "例如：70");

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

            lblKeyboardTip = new Label();
            lblKeyboardTip.Text = "💡 提示：用鍵盤選顏色時，請先按 [空白鍵] 選中顏色再按 [Enter]";
            lblKeyboardTip.Font = new Font("微軟正黑體", 18F, FontStyle.Italic);
            lblKeyboardTip.ForeColor = Color.DimGray;
            lblKeyboardTip.BackColor = Color.Transparent;
            lblKeyboardTip.AutoSize = false;
            lblKeyboardTip.Width = this.ClientSize.Width;
            lblKeyboardTip.Height = 45;
            lblKeyboardTip.TextAlign = ContentAlignment.MiddleCenter;
            lblKeyboardTip.Dock = DockStyle.Bottom;
            this.Controls.Add(lblKeyboardTip);
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

        private void btnRun_Click(object sender, EventArgs e)
        {
            lblResult.BackColor = Color.Transparent;
            lblResult.ForeColor = Color.Black;

            bool isHeightValid = double.TryParse(txtHeight.Text, out double h);
            bool isWeightValid = double.TryParse(txtWeight.Text, out double w);

            if (isHeightValid)
            {
                if (h <= 0) { ShowError("身高必須大於零，身高值錯誤", txtHeight); return; }
            }
            else { ShowError("請輸入有效的身高數值，身高值錯誤", txtHeight); return; }

            if (isWeightValid)
            {
                if (w <= 0) { ShowError("體重必須大於零，體重值錯誤", txtWeight); return; }
            }
            else { ShowError("請輸入有效的體重數值，體重值錯誤", txtWeight); return; }

            h /= 100;
            double currentBmi = w / (h * h);
            double idealW = 22 * (h * h);

            string[] results = { "體重過輕", "健康體位", "體位過重", "輕度肥胖", "中度肥胖", "重度肥胖" };
            Color[] colors = { Color.DeepSkyBlue, Color.LimeGreen, Color.Bisque, Color.Coral, Color.Tomato, Color.MediumOrchid };

            int idx = currentBmi < 18.5 ? 0 : currentBmi < 24 ? 1 : currentBmi < 27 ? 2 : currentBmi < 30 ? 3 : currentBmi < 35 ? 4 : 5;

            lblResult.Text = $"BMI：{currentBmi:F2} ({results[idx]})\r\n目標體重：{idealW:F1} kg";
            lblResult.BackColor = colors[idx];
            lblResult.ForeColor = Color.Black;
        }

        private void ShowError(string msg, TextBox target)
        {
            lblResult.ForeColor = Color.Red;
            lblResult.BackColor = Color.White;
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

        private void LoadRecord_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is string[] data)
            {
                txtHeight.Text = data[0];
                txtWeight.Text = data[1];
                btnRun.PerformClick();
            }
        }

        private void MenuExport_Click(object sender, EventArgs e)
        {
            if (menuLoad.DropDownItems.Count == 1 && !menuLoad.DropDownItems[0].Enabled)
            {
                ShowError("匯出失敗：目前沒有任何儲存的紀錄", txtHeight);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "文字檔 (*.txt)|*.txt";
                sfd.Title = "匯出 BMI 紀錄";
                sfd.FileName = $"BMI紀錄_{DateTime.Now:yyyyMMdd}.txt";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("=== BMI 歷史紀錄 ===");
                    sb.AppendLine($"匯出時間：{DateTime.Now:yyyy/MM/dd HH:mm:ss}");
                    sb.AppendLine("--------------------------------");

                    foreach (ToolStripItem item in menuLoad.DropDownItems)
                    {
                        if (item is ToolStripMenuItem menuItem && menuItem.Tag is string[] data)
                        {
                            double h = double.Parse(data[0]) / 100;
                            double w = double.Parse(data[1]);
                            double bmi = w / (h * h);
                            sb.AppendLine($"紀錄名稱：{menuItem.Text}");
                            sb.AppendLine($"身高：{data[0]} cm, 體重：{data[1]} kg");
                            sb.AppendLine($"計算 BMI：{bmi:F2}");
                            sb.AppendLine("--------------------------------");
                        }
                    }

                    File.WriteAllText(sfd.FileName, sb.ToString());

                    lblResult.Text = $"✅ 紀錄已成功匯出！";
                    lblResult.BackColor = Color.LightGreen;
                    lblResult.ForeColor = Color.DarkGreen;
                }
            }
        }

        private string PromptForName()
        {
            using (Form prompt = new Form()
            {
                Width = 320,
                Height = 160,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = "儲存紀錄",
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            })
            {
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
        }

        private void Button_FocusChanged(object sender, EventArgs e)
        {
            if (sender is Button btn) btn.Invalidate();
        }
    }
}