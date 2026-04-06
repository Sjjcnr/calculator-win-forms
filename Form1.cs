using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Globalization;

namespace CalculatorWinForms
{
    public class Form1 : Form
    {
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        private TextBox displayBox = null!;
        private double currentValue = 0;
        private double previousValue = 0;
        private string currentOperator = "";
        private bool isNewEntry = true;
        
        private double lastOperand = 0;
        private string lastOperator = "";
        private bool repeatEquals = false;

        public Form1()
        {
            this.Size = new Size(360, 560);
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.KeyPreview = true;

            this.MouseDown += Form1_MouseDown;
            InitializeUI();
        }

        private void Form1_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(this.ClientRectangle, 
                ColorTranslator.FromHtml("#1a0533"), ColorTranslator.FromHtml("#0f1a4a"), 45F))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw glowing orbs
            using (SolidBrush purpleOrb = new SolidBrush(Color.FromArgb(40, 124, 58, 237)))
            {
                e.Graphics.FillEllipse(purpleOrb, -50, -50, 200, 200);
                e.Graphics.FillEllipse(purpleOrb, 250, 400, 150, 150);
            }

            using (SolidBrush blueOrb = new SolidBrush(Color.FromArgb(40, 37, 99, 235)))
            {
                e.Graphics.FillEllipse(blueOrb, 200, 100, 180, 180);
                e.Graphics.FillEllipse(blueOrb, -20, 350, 120, 120);
            }

            // Draw glass rounded rect for calculator body
            Rectangle rect = new Rectangle(15, 15, this.Width - 30, this.Height - 30);
            int radius = 20;

            using (GraphicsPath path = GetRoundedRect(rect, radius))
            {
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(35, 255, 255, 255)))
                {
                    e.Graphics.FillPath(brush, path);
                }
                using (Pen pen = new Pen(Color.FromArgb(80, 255, 255, 255), 1))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }

            // Draw glass rounded rect for display area
            Rectangle displayRect = new Rectangle(30, 70, this.Width - 60, 60);
            using (GraphicsPath path = GetRoundedRect(displayRect, 10))
            {
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(20, 255, 255, 255)))
                {
                    e.Graphics.FillPath(brush, path);
                }
                using (Pen pen = new Pen(Color.FromArgb(60, 255, 255, 255), 1))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }

        private void InitializeUI()
        {
            GlassButton closeBtn = new GlassButton();
            closeBtn.Text = "×";
            closeBtn.Size = new Size(30, 30);
            closeBtn.Location = new Point(this.Width - 45, 15);
            closeBtn.Type = GlassButton.ButtonType.Action;
            closeBtn.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            closeBtn.Click += (s, e) => this.Close();
            this.Controls.Add(closeBtn);

            displayBox = new TextBox();
            displayBox.ReadOnly = true;
            displayBox.Text = "0";
            displayBox.TextAlign = HorizontalAlignment.Right;
            displayBox.Font = new Font("Segoe UI Light", 28f);
            displayBox.ForeColor = Color.White;
            displayBox.BackColor = Color.FromArgb(255, 32, 23, 62);
            displayBox.BorderStyle = BorderStyle.None;
            // Center textbox vertically inside its drawing rect
            displayBox.Location = new Point(40, 75);
            displayBox.Width = this.Width - 80;
            this.Controls.Add(displayBox);

            string[,] buttons = new string[,]
            {
                { "C", "CE", "⌫", "÷" },
                { "7", "8", "9", "×" },
                { "4", "5", "6", "−" },
                { "1", "2", "3", "+" },
                { "+/−", "0", ".", "=" }
            };

            int startX = 30;
            int startY = 150;
            int tableWidth = this.Width - 60;
            int tableHeight = 370;
            int cellWidth = tableWidth / 4;
            int cellHeight = tableHeight / 5;

            for (int r = 0; r < 5; r++)
            {
                for (int c = 0; c < 4; c++)
                {
                    string text = buttons[r, c];
                    GlassButton btn = new GlassButton();
                    btn.Text = text;
                    btn.Size = new Size(cellWidth - 8, cellHeight - 8);
                    btn.Location = new Point(startX + c * cellWidth + 4, startY + r * cellHeight + 4);

                    if (int.TryParse(text, out _)) btn.Type = GlassButton.ButtonType.Digit;
                    else if (text == "=") btn.Type = GlassButton.ButtonType.Equals;
                    else if (text == "+" || text == "−" || text == "×" || text == "÷") btn.Type = GlassButton.ButtonType.Operator;
                    else btn.Type = GlassButton.ButtonType.Action;

                    btn.Click += Button_Click;
                    this.Controls.Add(btn);
                }
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();
            if (radius == 0) { path.AddRectangle(bounds); return path; }

            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void Button_Click(object? sender, EventArgs e)
        {
            if (sender is GlassButton btn)
            {
                ProcessInput(btn.Text);
            }
        }

        private void ProcessInput(string input)
        {
            if (int.TryParse(input, out int digit))
            {
                if (isNewEntry || displayBox.Text == "0" || displayBox.Text == "Cannot divide by 0")
                {
                    displayBox.Text = input;
                    isNewEntry = false;
                }
                else
                {
                    displayBox.Text += input;
                }
                repeatEquals = false;
            }
            else if (input == ".")
            {
                if (isNewEntry || displayBox.Text == "Cannot divide by 0")
                {
                    displayBox.Text = "0.";
                    isNewEntry = false;
                }
                else if (!displayBox.Text.Contains("."))
                {
                    displayBox.Text += ".";
                }
                repeatEquals = false;
            }
            else if (input == "⌫")
            {
                if (!isNewEntry && displayBox.Text != "Cannot divide by 0")
                {
                    if (displayBox.Text.Length > 1 && !(displayBox.Text.Length == 2 && displayBox.Text.StartsWith("-")))
                        displayBox.Text = displayBox.Text.Substring(0, displayBox.Text.Length - 1);
                    else
                    {
                        displayBox.Text = "0";
                        isNewEntry = true;
                    }
                }
            }
            else if (input == "C")
            {
                currentValue = 0;
                previousValue = 0;
                currentOperator = "";
                displayBox.Text = "0";
                isNewEntry = true;
                repeatEquals = false;
            }
            else if (input == "CE")
            {
                displayBox.Text = "0";
                isNewEntry = true;
            }
            else if (input == "+/−")
            {
                if (displayBox.Text != "0" && displayBox.Text != "Cannot divide by 0")
                {
                    if (displayBox.Text.StartsWith("-"))
                        displayBox.Text = displayBox.Text.Substring(1);
                    else
                        displayBox.Text = "-" + displayBox.Text;
                }
            }
            else if (input == "+" || input == "−" || input == "×" || input == "÷")
            {
                if (displayBox.Text == "Cannot divide by 0") return;

                if (!isNewEntry && currentOperator != "")
                {
                    Calculate();
                }
                else
                {
                    double.TryParse(displayBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out previousValue);
                }
                
                currentOperator = input;
                isNewEntry = true;
                repeatEquals = false;
            }
            else if (input == "=")
            {
                if (displayBox.Text == "Cannot divide by 0") return;

                if (repeatEquals)
                {
                    previousValue = double.Parse(displayBox.Text, CultureInfo.InvariantCulture);
                }
                else
                {
                    if (!double.TryParse(displayBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out lastOperand))
                        lastOperand = 0;
                    lastOperator = currentOperator;
                }

                CalculateRepeat();
            }
        }

        private void Calculate()
        {
            double.TryParse(displayBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out currentValue);
            double result = 0;

            switch (currentOperator)
            {
                case "+": result = previousValue + currentValue; break;
                case "−": result = previousValue - currentValue; break;
                case "×": result = previousValue * currentValue; break;
                case "÷":
                    if (currentValue == 0)
                    {
                        displayBox.Text = "Cannot divide by 0";
                        isNewEntry = true;
                        currentOperator = "";
                        return;
                    }
                    result = previousValue / currentValue; 
                    break;
                default: 
                    result = currentValue; 
                    break;
            }

            displayBox.Text = result.ToString("G15", CultureInfo.InvariantCulture);
            previousValue = result;
            isNewEntry = true;
        }

        private void CalculateRepeat()
        {
            if (string.IsNullOrEmpty(lastOperator)) return;

            double currentDisplay;
            double.TryParse(displayBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out currentDisplay);
            double result = currentDisplay;

            switch (lastOperator)
            {
                case "+": result = currentDisplay + lastOperand; break;
                case "−": result = currentDisplay - lastOperand; break;
                case "×": result = currentDisplay * lastOperand; break;
                case "÷":
                    if (lastOperand == 0)
                    {
                        displayBox.Text = "Cannot divide by 0";
                        isNewEntry = true;
                        return;
                    }
                    result = currentDisplay / lastOperand; 
                    break;
            }

            displayBox.Text = result.ToString("G15", CultureInfo.InvariantCulture);
            previousValue = result;
            isNewEntry = true;
            repeatEquals = true;
        }
        
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Enter: ProcessInput("="); return true;
                case Keys.Escape: ProcessInput("C"); return true;
                case Keys.Back: ProcessInput("⌫"); return true;
                case Keys.Add: ProcessInput("+"); return true;
                case Keys.Subtract: ProcessInput("−"); return true;
                case Keys.Multiply: ProcessInput("×"); return true;
                case Keys.Divide: ProcessInput("÷"); return true;
                case Keys.Decimal: ProcessInput("."); return true;
                case Keys.OemPeriod: ProcessInput("."); return true;
                case Keys.OemMinus: ProcessInput("−"); return true;
                case Keys.Oemplus: ProcessInput("="); return true; 
                case Keys.Shift | Keys.Oemplus: ProcessInput("+"); return true;
                case Keys.Shift | Keys.D8: ProcessInput("×"); return true;
                case Keys.Shift | Keys.OemMinus: ProcessInput("−"); return true;
            }
            
            if (keyData >= Keys.D0 && keyData <= Keys.D9) 
            {
                int val = keyData - Keys.D0;
                ProcessInput(val.ToString());
                return true;
            }
            if (keyData >= Keys.NumPad0 && keyData <= Keys.NumPad9)
            {
                int val = keyData - Keys.NumPad0;
                ProcessInput(val.ToString());
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
