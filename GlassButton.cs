using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CalculatorWinForms
{
    public class GlassButton : Button
    {
        private bool isHovered = false;
        private bool isPressed = false;
        public enum ButtonType { Digit, Operator, Equals, Action }
        public ButtonType Type { get; set; } = ButtonType.Digit;

        public GlassButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.BackColor = Color.Transparent;
            this.Font = new Font("Segoe UI", 14f, FontStyle.Regular);
            this.ForeColor = Color.White;
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            isHovered = true;
            this.Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            isHovered = false;
            this.Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            isPressed = true;
            this.Invalidate();
            base.OnMouseDown(mevent);
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            isPressed = false;
            this.Invalidate();
            base.OnMouseUp(mevent);
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Color fillColor;
            Color borderColor;

            if (Type == ButtonType.Digit || Type == ButtonType.Action)
            {
                fillColor = isPressed ? Color.FromArgb(45, 255, 255, 255) :
                            isHovered ? Color.FromArgb(60, 255, 255, 255) :
                                        Color.FromArgb(30, 255, 255, 255);
                borderColor = Color.FromArgb(50, 255, 255, 255);
            }
            else if (Type == ButtonType.Operator)
            {
                fillColor = isPressed ? Color.FromArgb(170, 124, 58, 237) :
                            isHovered ? Color.FromArgb(130, 124, 58, 237) :
                                        Color.FromArgb(90, 124, 58, 237);
                borderColor = Color.FromArgb(140, 167, 139, 255);
            }
            else // Equals
            {
                fillColor = isPressed ? Color.FromArgb(230, 124, 58, 237) :
                            isHovered ? Color.FromArgb(215, 124, 58, 237) :
                                        Color.FromArgb(200, 124, 58, 237);
                borderColor = Color.FromArgb(140, 167, 139, 255);
            }

            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            int radius = 12;

            using (GraphicsPath path = GetRoundedRect(rect, radius))
            {
                using (SolidBrush brush = new SolidBrush(fillColor))
                {
                    pevent.Graphics.FillPath(brush, path);
                }

                if (Type == ButtonType.Equals)
                {
                    using (GraphicsPath arcPath = new GraphicsPath())
                    {
                        arcPath.AddArc(0, 0, radius * 2, radius * 2, 180, 90);
                        arcPath.AddArc(this.Width - 1 - radius * 2, 0, radius * 2, radius * 2, 270, 90);
                        using (Pen arcPen = new Pen(Color.FromArgb(80, 255, 255, 255), 1.5f))
                        {
                            pevent.Graphics.DrawPath(arcPen, arcPath);
                        }
                    }
                }

                using (Pen pen = new Pen(borderColor, 1))
                {
                    pevent.Graphics.DrawPath(pen, path);
                }
            }

            using (StringFormat sf = new StringFormat())
            {
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                using (SolidBrush textBrush = new SolidBrush(this.ForeColor))
                {
                    pevent.Graphics.DrawString(this.Text, this.Font, textBrush, this.ClientRectangle, sf);
                }
            }
        }

        private GraphicsPath GetRoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

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
    }
}
