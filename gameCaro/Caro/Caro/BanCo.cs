using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Caro
{
    class BanCo
    {
        private int _SoDong;
        private int _SoCot;

        public BanCo()
        {
            SoCot = 0;
            SoDong = 0;
        }
        public BanCo(int soDong, int soCot)
        {
            SoDong = soDong;
            SoCot = soCot;
        }

        public int SoDong { get => _SoDong; set => _SoDong = value; }
        public int SoCot { get => _SoCot; set => _SoCot = value; }

        public void VeBanCo(Graphics g)
        {
            for (int i = 0; i <= SoCot; i++)
            {
                g.DrawLine(CaroChess.pen, i * OCo._ChieuRong, 0, i * OCo._ChieuRong, SoDong * OCo._ChieuCao);
            }
            for (int i = 0; i <= SoDong; i++)
            {
                g.DrawLine(CaroChess.pen, 0, i * OCo._ChieuCao, SoCot * OCo._ChieuRong, i * OCo._ChieuCao);
            }
        }

        public void VeQuanCo(Graphics g, Point point, SolidBrush sb)
        {
            g.FillEllipse(sb, point.X + 2, point.Y + 2, OCo._ChieuRong - 4, OCo._ChieuCao - 4);

        }
        public void XoaQuanCo(Graphics g, Point point, SolidBrush sb)
        {
            g.FillRectangle(sb, point.X +1, point.Y +1 , OCo._ChieuRong - 2, OCo._ChieuCao - 2);
        }
    }
}
