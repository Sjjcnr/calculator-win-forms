using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Globalization;
using System.Collections.Generic;

namespace CalculatorWinForms
{
    public class CustomButton
    {
        public Rectangle Bounds;
        public string Text = "";
        public int Type; // 0=Digit, 1=Action, 2=Operator, 3=Equals, 4=Close
        public bool IsHovered;
        public bool IsPressed;
    }

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

        private List<CustomButton> _buttons = new List<CustomButton>();

        public Form1()
        {
            this.Size = new Size(360, 560);
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.KeyPreview = true;

            this.MouseDown += Form1_MouseDown;
            this.MouseMove += Form1_MouseMove;
            this.MouseUp += Form1_MouseUp;
            InitializeUI();
        }

        private void Form1_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                foreach (var btn in _buttons)
                {
                    if (btn.Bounds.Contains(e.Location))
                    {
                        btn.IsPressed = true;
                        this.Invalidate(btn.Bounds);
                        
                        if (btn.Type == 4) this.Close();
                        else ProcessInput(btn.Text);
                        
                        return;
                    }
                }

                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void Form1_MouseMove(object? sender, MouseEventArgs e)
        {
            bool redraw = false;
            foreach (var btn in _buttons)
            {
                bool hover = btn.Bounds.Contains(e.Location);
                if (btn.IsHovered != hover)
                {
                    btn.IsHovered = hover;
                    redraw = true;
                }
            }
            if (redraw) this.Invalidate();
        }

        private void Form1_MouseUp(object? sender, MouseEventArgs e)
        {
            bool redraw = false;
            foreach (var btn in _buttons)
            {
                if (btn.IsPressed)
                {
                    btn.IsPressed = false;
                    redraw = true;
                }
            }
            if (redraw) this.Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Fully override to custom draw the background
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

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Font btnFont = new Font("Segoe UI", 14f, FontStyle.Regular);
            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;

            foreach (var btn in _buttons)
            {
                Color fillColor;
                Color textColor = Color.White;

                if (btn.Type == 3) // Equals
                {
                    fillColor = btn.IsPressed ? Color.FromArgb(220, 124, 58, 237) :
                                btn.IsHovered ? Color.FromArgb(240, 124, 58, 237) :
                                                Color.FromArgb(255, 124, 58, 237);
                }
                else if (btn.Type == 2) // Operator
                {
                    fillColor = btn.IsPressed ? Color.FromArgb(60, 255, 255, 255) :
                                btn.IsHovered ? Color.FromArgb(80, 255, 255, 255) :
                                                Color.FromArgb(45, 255, 255, 255);
                    textColor = Color.FromArgb(255, 167, 139, 250);
                }
                else // Digit, Action, Close
                {
                    fillColor = btn.IsPressed ? Color.FromArgb(45, 255, 255, 255) :
                                btn.IsHovered ? Color.FromArgb(60, 255, 255, 255) :
                                                Color.FromArgb(30, 255, 255, 255);
                    if (btn.Type == 1 || btn.Type == 4) textColor = Color.FromArgb(255, 148, 163, 184);
                }

                int radius = 12;
                using (GraphicsPath path = GetRoundedRect(btn.Bounds, radius))
                {
                    using (SolidBrush brush = new SolidBrush(fillColor))
                    {
                        e.Graphics.FillPath(brush, path);
                    }
                    using (Pen pen = new Pen(Color.FromArgb(60, 255, 255, 255), 1))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                }

                if (!string.IsNullOrEmpty(btn.Text))
                {
                    using (SolidBrush textBrush = new SolidBrush(textColor))
                    {
                        e.Graphics.DrawString(btn.Text, btnFont, textBrush, btn.Bounds, sf);
                    }
                }
            }

            btnFont.Dispose();
            sf.Dispose();
        }

        private void InitializeUI()
        {
            CustomButton closeBtn = new CustomButton();
            closeBtn.Text = "×";
            closeBtn.Bounds = new Rectangle(this.Width - 45, 15, 30, 30);
            closeBtn.Type = 4;
            _buttons.Add(closeBtn);

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
                    CustomButton btn = new CustomButton();
                    btn.Text = text;
                    btn.Bounds = new Rectangle(startX + c * cellWidth + 4, startY + r * cellHeight + 4, cellWidth - 8, cellHeight - 8);

                    if (int.TryParse(text, out _)) btn.Type = 0;
                    else if (text == "=") btn.Type = 3;
                    else if (text == "+" || text == "−" || text == "×" || text == "÷") btn.Type = 2;
                    else btn.Type = 1;

                    _buttons.Add(btn);
                }
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
                if (repeatEquals) 
                {
                    // If we just pressed equals and then start typing a number, it resets everything.
                    previousValue = 0;
                    currentOperator = "";
                    lastOperator = "";
                    lastOperand = 0;
                    repeatEquals = false;
                }
            }
            else if (input == ".")
            {
                if (repeatEquals)
                {
                    previousValue = 0;
                    currentOperator = "";
                    lastOperator = "";
                    lastOperand = 0;
                    repeatEquals = false;
                    displayBox.Text = "0.";
                    isNewEntry = false;
                }
                else if (isNewEntry || displayBox.Text == "Cannot divide by 0")
                {
                    displayBox.Text = "0.";
                    isNewEntry = false;
                }
                else if (!displayBox.Text.Contains("."))
                {
                    displayBox.Text += ".";
                }
            }
            else if (input == "⌫")
            {
                if (!isNewEntry && !repeatEquals && displayBox.Text != "Cannot divide by 0")
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
                ClearAll();
            }
            else if (input == "CE")
            {
                if (repeatEquals) 
                {
                    ClearAll();
                }
                else 
                {
                    displayBox.Text = "0";
                    isNewEntry = true;
                }
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

                if (!isNewEntry && currentOperator != "" && !repeatEquals)
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

                if (!repeatEquals)
                {
                    if (string.IsNullOrEmpty(currentOperator)) return; // nothing to do
                    
                    double.TryParse(displayBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out lastOperand);
                    lastOperator = currentOperator;
                    
                    Calculate();
                    repeatEquals = true;
                }
                else
                {
                    // Repeat equals
                    double currentDisplay;
                    double.TryParse(displayBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out currentDisplay);
                    double result = DoMath(currentDisplay, lastOperand, lastOperator);
                    if (displayBox.Text != "Cannot divide by 0") 
                    {
                        displayBox.Text = result.ToString("G15", CultureInfo.InvariantCulture);
                        previousValue = result;
                        isNewEntry = true;
                    }
                }
            }
        }

        private void ClearAll()
        {
            previousValue = 0;
            currentValue = 0;
            currentOperator = "";
            lastOperand = 0;
            lastOperator = "";
            displayBox.Text = "0";
            isNewEntry = true;
            repeatEquals = false;
        }

        private void Calculate()
        {
            double.TryParse(displayBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out currentValue);
            
            double result = DoMath(previousValue, currentValue, currentOperator);
            
            if (displayBox.Text != "Cannot divide by 0")
            {
                displayBox.Text = result.ToString("G15", CultureInfo.InvariantCulture);
                previousValue = result;
                isNewEntry = true;
            }
        }

        private double DoMath(double a, double b, string op)
        {
            switch (op)
            {
                case "+": return a + b;
                case "−": return a - b;
                case "×": return a * b;
                case "÷":
                    if (b == 0)
                    {
                        displayBox.Text = "Cannot divide by 0";
                        return 0;
                    }
                    return a / b;
            }
            return a;
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
