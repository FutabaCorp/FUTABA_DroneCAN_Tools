using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace MyCtrls.Leds
{
    public partial class LBLed : UserControl
    {
        public enum LedColorEnum
        {
            Off = 0,
            Green = 1,
            Yellow = 2,
            Red = 3,
        };

        public enum LabelPositionEnum
        {
            Top = 0,
            Left = 1,
            Right = 2,
            Bottom = 3,
        };

        private LedColorEnum m_LedColor1 = LedColorEnum.Off;
        private LabelPositionEnum m_LabelPosition1 = LabelPositionEnum.Top;
  

        public LBLed()
        {
            InitializeComponent(); 
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            Brush Gradationbrush = Brushes.DimGray;
            Color color1 = Color.DimGray;
            Color color2 = Color.DimGray;
            int graphic_top = 10;
            int graphic_left = 20;
            int graphic_width = 20;
            int graphic_height = 20;


            switch (m_LedColor1)
            {
                case LedColorEnum.Off:
                    color1 = Color.DimGray;
                    color2 = Color.DimGray;
                    break;
                case LedColorEnum.Green:
                    color1 = Color.LawnGreen;
                    color2 = Color.LimeGreen;
                    break;
                case LedColorEnum.Yellow:
                    color1 = Color.Yellow;
                    color2 = Color.DarkKhaki;
                    break;
                case LedColorEnum.Red:
                    color1 = Color.Salmon;
                    color2 = Color.Red;
                    break;
            }
                      
            switch (m_LabelPosition1)
            {
                case LabelPositionEnum.Top:
                    label1.Location = new Point(8, 3);
                    graphics.TranslateTransform(0 , 10);
                    break;
                case LabelPositionEnum.Left:
                    label1.Location = new Point(0, 14);   
                    graphics.TranslateTransform(label1.Width-20, 0);
                    break;
                case LabelPositionEnum.Right:
                    graphic_left = 0;
                    label1.Location = new Point(graphic_width + 5, 14);
                    break;
                case LabelPositionEnum.Bottom:
                    label1.Location = new Point(8, 35);
                    break;
            }

            path.AddEllipse(new Rectangle(graphic_left, graphic_top, graphic_width, graphic_height));
            Gradationbrush = new System.Drawing.Drawing2D.LinearGradientBrush(new Point(graphic_left, graphic_top),new Point(graphic_left + graphic_width, graphic_top + graphic_height), color1, color2);
            graphics.FillEllipse(Gradationbrush, graphic_left, graphic_top, graphic_width, graphic_height);
            graphics.Dispose();
        }

        [System.ComponentModel.Browsable(true),
        System.ComponentModel.Category("LedColor"),
        System.ComponentModel.Description("Color of the led.")]
        public LedColorEnum LedColor
        {
            get
            {
                return m_LedColor1;
            }
            set
            {
                if (m_LedColor1 != value)
                {
                    m_LedColor1 = value;
                    Refresh();
                }
            }
        }

        [System.ComponentModel.Browsable(true),
        System.ComponentModel.Category("LabelPosition"),
        System.ComponentModel.Description("Position of the label.")]
        public LabelPositionEnum LabelPosition
        {
            get
            {
                return m_LabelPosition1;
            }
            set
            {
                if (m_LabelPosition1 != value)
                {
                    m_LabelPosition1 = value;
                    Refresh();
                }
            }
        }

        [System.ComponentModel.Browsable(true),
        System.ComponentModel.Category("Label"),
        System.ComponentModel.Description("Text of the label.")]
        public string Label
        {
            get
            {
                return label1.Text;
            }
            set
            {
                if (label1.Text != value)
                {
                    label1.Text = value;
                    Refresh();
                }
            }
        }
    }
}
