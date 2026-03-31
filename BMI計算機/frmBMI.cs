using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BMI計算機
{
    public partial class frmBMI : Form, IMessageFilter
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);
        private const int EM_SETCUEBANNER = 0x1501;

        private MenuStrip topMenu;
        private ToolStripMenuItem menuManage, menuSave, menuLoad;
        private ToolStripMenuItem menuColor, menuBgColor, menuBtnColor, menuTxtColor;
        private ToolStripMenuItem menuToggleGradient;

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
                    }
                }
            }
            return false;
        }

        private void SetupMenu()
        {
            topMenu = new MenuStrip { BackColor = Color.WhiteSmoke };

            menuManage = new ToolStripMenuItem("紀錄管理(Ctrl+M)");
            menuSave = new ToolStripMenuItem("儲存當前資料(Ctrl+S)");
            menuSave.Click += MenuSave_Click;
            menuLoad = new ToolStripMenuItem("載入資料(Ctrl+L)");
            menuLoad.DropDownItems.Add(new ToolStripMenuItem("(目前無儲存紀錄)") { Enabled = false });
            menuManage.DropDownItems.Add(menuSave);
            menuManage.DropDownItems.Add(new ToolStripSeparator());
            menuManage.DropDownItems.Add(menuLoad);

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

            topMenu.Items.Add(menuManage);
            topMenu.Items.Add(menuColor);
            this.Controls.Add(topMenu);
            this.MainMenuStrip = topMenu;
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
            SendMessage(txtWeight.Handle, EM_SETCUEBANNER, 1, "例如：80");

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

        // [重要修改] 加入 using 確保記憶體妥善釋放
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