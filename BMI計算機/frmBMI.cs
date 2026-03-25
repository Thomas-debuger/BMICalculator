using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BMI計算機
{
    public partial class frmBMI : Form
    {
        public frmBMI()
        {
            InitializeComponent();
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            // 宣告變數來接 TryParse 轉換後的數值
            double dH = 0;
            double dW = 0;

            // 使用 TryParse 檢查輸入是否為有效的數字，避免使用者亂填導致程式崩潰
            if (double.TryParse(this.txtHeight.Text, out dH) && double.TryParse(this.txtWeight.Text, out dW))
            {
                // 確保身高大於 0，避免發生除以零的錯誤
                if (dH > 0)
                {
                    // 假設使用者的輸入習慣是「公分 (cm)」，這裡需換算成「公尺 (m)」
                    // (註：如果你的系統本來就要求使用者輸入公尺，例如 1.75，請將下面這行刪除)
                    dH = dH / 100.0;

                    // 計算 BMI
                    double dBMI = dW / (dH * dH);

                    // 輸出結果，保留小數點後兩位
                    this.lblResult.Text = dBMI.ToString("0.00");

                    if(dBMI < 18.5)
                    {
                        this.lblResult.Text += " (過輕)";
                    }
                    else if (dBMI >= 18.5 && dBMI < 24.0)
                    {
                        this.lblResult.Text += " (正常)";
                    }
                    else if (dBMI >= 24.0 && dBMI < 27.0)
                    {
                        this.lblResult.Text += " (過重)";
                    }
                    else if (dBMI >= 27.0 && dBMI < 30.0)
                    {
                        this.lblResult.Text += " (輕度肥胖)";
                    }
                    else if (dBMI >= 30.0 && dBMI < 35.0)
                    {
                        this.lblResult.Text += " (中度肥胖)";
                    }
                    else if (dBMI >= 35.0)
                    {
                        this.lblResult.Text += " (重度肥胖)";
                    }
                }
                else
                {
                    this.lblResult.Text = "身高必須大於0";
                }
            }
            else
            {
                // 如果輸入包含非數字或空白，提醒使用者
                this.lblResult.Text = "請輸入有效的數字格式";
            }
        }

    }
}
