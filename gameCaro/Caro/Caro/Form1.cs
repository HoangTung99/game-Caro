using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Caro
{
    public partial class FrmCoCaro : Form
    {
        private Graphics grs;
        private CaroChess caroChess; 
        public FrmCoCaro()
        {
            InitializeComponent();
            caroChess = new CaroChess();
            caroChess.KhoiTaoMangOCo();
            grs = pnlBanCo.CreateGraphics();
            btnPlayervsPlayer.Click += new EventHandler(PvsP);
            playerVsComToolStripMenuItem.Click += new EventHandler(PvsC_Click);
            btnPlayervsCom.Click += new EventHandler(PvsC_Click);
        }

        private void tmChuChay_Tick(object sender, EventArgs e)
        {
            lblChuoiChu.Location = new Point(lblChuoiChu.Location.X, lblChuoiChu.Location.Y - 1);
            if (lblChuoiChu.Location.Y + lblChuoiChu.Height < 0)
            {
                lblChuoiChu.Location = new Point(lblChuoiChu.Location.X, pnlChayChu.Height);
            }

        }

        private void pnlChayChu_Paint(object sender, PaintEventArgs e)
        {

        }

        private void FrmCoCaro_Load(object sender, EventArgs e)
        {
            lblChuoiChu.Text = "Đồ án game cờ Caro sử dụng thuật\ntoán Minimax có cắt tỉa alpha, beta.\n\n\nSinh viên thực hiện: \n   Hồ Hoàng Tùng - 17521232\n   Ngô Anh Vũ - 17521272 \nLớp:   CS106.K22.KHCL \nGiảng viên:\n   Nguyễn Đình Hiển";
            tmChuChay.Enabled = true;
           
        }

        private void pnlBanCo_Paint(object sender, PaintEventArgs e)
        {
            caroChess.VeBanCo(grs);
            caroChess.VeLaiQuanCo(grs);
        }

        private void pnlBanCo_MouseClick(object sender, MouseEventArgs e)
        {
            if (!caroChess.SanSang)
                return;
            if (caroChess.DanhCo(e.X, e.Y, grs))
            {
                if (caroChess.KiemTraChienThang())
                    caroChess.KetThucTroChoi();
                else
                {
                    if (caroChess.CheDoChoi == 2)
                    {
                        caroChess.KhoiDongComputer(grs);
                        if (caroChess.KiemTraChienThang())
                            caroChess.KetThucTroChoi();

                    }
                }
            }
        }

        private void PvsP(object sender, EventArgs e)
        {
            grs.Clear(pnlBanCo.BackColor);
            caroChess.StartPlayerVsPlayer(grs);
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //grs.Clear(pnlBanCo.BackColor);
            caroChess.Undo(grs);
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            caroChess.Redo(grs); 
        }

        private void PvsC_Click(object sender, EventArgs e)
        {
            grs.Clear(pnlBanCo.BackColor);
            caroChess.StartPlayervsCom(grs);
        }

        private void bntUndo_Click(object sender, EventArgs e)
        {
            caroChess.Undo(grs);
        }

        private void bntRedo_Click(object sender, EventArgs e)
        {
            caroChess.Redo(grs);
        }
    }
}
