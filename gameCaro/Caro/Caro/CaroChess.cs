using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Caro
{
    public enum KETTHUC
    {
        HoaCo,
        _LuotDi1,
        _LuotDi2,
        COM
    }
    class CaroChess
    {
        public static Pen pen;
        public static SolidBrush sbWhite, sbBlack, sbBG;
        private OCo[,] _MangOCo;
        private BanCo _BanCo;
        public Stack<OCo> stkCacNuocDaDi, stkCacNuocUndo;
        private int _LuotDi;
        private KETTHUC _ketthuc;
        private int _CheDoChoi;
        private bool _SanSang;
        private int[,] _MangTrangThaiBanCoHienTai;
        private int[]  MangDiemTanCong = new int[] { 0, 4, 28, 256, 2308 }; /*{ 0, 9, 54, 162, 1458, 13112, 118008 };*/
        private int[] MangDiemPhongNgu = new int[] { 0, 1, 9, 85, 769 };
        private string[] Truonghop1 = { @"\s11\s", @"\s1112", @"2111\s", @"\s111\s", @"\s11112", @"21111\s", @"2\s1111\s", @"\s1111\s2", @"\s1111\s", @"11111" };
        private string[] Truonghop2 = { @"\s22\s", @"\s2221", @"1222\s", @"\s222\s", @"\s22221", @"12222\s", @"1\s2222\s", @"\s2222\s1", @"\s2222\s", @"22222" };
        private int[] point = { 6, 4, 4, 12, 30, 30,15,15, 3000, 10000 };
        public int maxdepth = 6, _branch = 3;

        public bool SanSang { get => _SanSang; set => _SanSang = value; }
        public int CheDoChoi { get => _CheDoChoi; set => _CheDoChoi = value; }

        public CaroChess()
        {
            pen = new Pen(Color.Black);
            sbWhite = new SolidBrush(Color.Red);
            sbBlack = new SolidBrush(Color.Black);
            sbBG = new SolidBrush(Color.FromArgb(224, 224, 224));
            _BanCo = new BanCo(20, 20);
            _MangOCo = new OCo[_BanCo.SoDong, _BanCo.SoCot];
            stkCacNuocDaDi = new Stack<OCo>();
            stkCacNuocUndo = new Stack<OCo>(); 
            _LuotDi = 1;

        }
        #region Cac thao tac ban co
        public void VeBanCo(Graphics g)
        {
            _BanCo.VeBanCo(g);
        }

        public void KhoiTaoMangOCo()
        {
            for (int i = 0; i < _BanCo.SoDong; i++)
            {
                for (int j = 0; j < _BanCo.SoCot; j++)
                {
                    _MangOCo[i, j] = new OCo(i, j, new Point(j * OCo._ChieuRong, i * OCo._ChieuCao), 0);
                }
            }
        }

        public bool DanhCo(int MouseX, int MouseY, Graphics g)
        {
            if (MouseX % OCo._ChieuRong == 0 || MouseY % OCo._ChieuCao == 0)
            {
                return false;
            }
            int Cot = MouseX / OCo._ChieuRong;
            int Dong = MouseY / OCo._ChieuCao;
            if (_MangOCo[Dong, Cot].SoHuu != 0)
                return false;
            switch (_LuotDi)
            {
                default:
                    MessageBox.Show("Có lỗi");
                    break;
                case 1:
                    _MangOCo[Dong, Cot].SoHuu = 1;
                    _BanCo.VeQuanCo(g, _MangOCo[Dong, Cot].ViTri, sbBlack);
                    _LuotDi = 2;
                    break;
                case 2:
                    _MangOCo[Dong, Cot].SoHuu = 2;
                    _BanCo.VeQuanCo(g, _MangOCo[Dong, Cot].ViTri, sbWhite);
                    _LuotDi = 1;
                    break;
            }

            //ve quan co
            OCo oco = new OCo(_MangOCo[Dong, Cot].Dong, _MangOCo[Dong, Cot].Cot, _MangOCo[Dong, Cot].ViTri, _MangOCo[Dong, Cot].SoHuu);
            stkCacNuocDaDi.Push(oco);
            stkCacNuocUndo = new Stack<OCo>();

            return true;
        }

        public void VeLaiQuanCo(Graphics g)
        {
            foreach (OCo oco in stkCacNuocDaDi)
            {
                if (oco.SoHuu == 1)
                    _BanCo.VeQuanCo(g, oco.ViTri, sbBlack);
                else if (oco.SoHuu == 2)
                    _BanCo.VeQuanCo(g, oco.ViTri, sbWhite);
            }
        }
        #endregion
        #region kich hoat che do choi
        public void StartPlayerVsPlayer(Graphics g)
        {
            _LuotDi = 1;
            _SanSang = true;
            KhoiTaoMangOCo();
            CheDoChoi = 1;
            stkCacNuocDaDi = new Stack<OCo>();
            stkCacNuocUndo = new Stack<OCo>();
            VeBanCo(g);
        }
        public void StartPlayervsCom(Graphics g)
        {
            _LuotDi = 1; 
            _SanSang = true;
            KhoiTaoMangOCo();
            CheDoChoi = 2;
            stkCacNuocDaDi = new Stack<OCo>();
            stkCacNuocUndo = new Stack<OCo>();
            VeBanCo(g);
            KhoiDongComputer(g);
        }
        #endregion
        #region Undo Redo
        public void Undo(Graphics g)
        {
            if (_CheDoChoi == 1)
            {
                if (stkCacNuocDaDi.Count() != 0)
                {
                    OCo oco = stkCacNuocDaDi.Pop();
                    stkCacNuocUndo.Push(new OCo(oco.Dong, oco.Cot, oco.ViTri, oco.SoHuu));
                    _MangOCo[oco.Dong, oco.Cot].SoHuu = 0;
                    _BanCo.XoaQuanCo(g, oco.ViTri, sbBG);
                    if (_LuotDi == 1)
                        _LuotDi = 2;
                    else
                        _LuotDi = 1;
                }
            }
            else if(_CheDoChoi == 2)
            {
                OCo oco1 = stkCacNuocDaDi.Pop();               
                stkCacNuocUndo.Push(new OCo(oco1.Dong, oco1.Cot, oco1.ViTri, oco1.SoHuu));
                OCo oco2 = stkCacNuocDaDi.Pop();
                stkCacNuocUndo.Push(new OCo(oco2.Dong, oco2.Cot, oco2.ViTri, oco2.SoHuu));
                _MangOCo[oco1.Dong, oco1.Cot].SoHuu = 0;
                _BanCo.XoaQuanCo(g, oco1.ViTri, sbBG);
                _MangOCo[oco2.Dong, oco2.Cot].SoHuu = 0;
                _BanCo.XoaQuanCo(g, oco2.ViTri, sbBG);
            }

        }

        public void Redo(Graphics g)
        {
            if (_CheDoChoi == 1)
            {
                if (stkCacNuocUndo.Count() != 0)
                {
                    OCo oco = stkCacNuocUndo.Pop();
                    stkCacNuocDaDi.Push(new OCo(oco.Dong, oco.Cot, oco.ViTri, oco.SoHuu));
                    _MangOCo[oco.Dong, oco.Cot].SoHuu = oco.SoHuu;
                    _BanCo.VeQuanCo(g, oco.ViTri, oco.SoHuu == 1 ? sbBlack : sbWhite);
                    if (_LuotDi == 1)
                        _LuotDi = 2;
                    else
                        _LuotDi = 1;
                }
            }
            else if (_CheDoChoi == 2)
            {
                OCo oco1 = stkCacNuocUndo.Pop();
                stkCacNuocDaDi.Push(new OCo(oco1.Dong, oco1.Cot, oco1.ViTri, oco1.SoHuu));
                _MangOCo[oco1.Dong, oco1.Cot].SoHuu = oco1.SoHuu;
                _BanCo.VeQuanCo(g, oco1.ViTri, oco1.SoHuu == 1 ? sbBlack : sbWhite);

                OCo oco2 = stkCacNuocUndo.Pop();
                stkCacNuocDaDi.Push(new OCo(oco2.Dong, oco2.Cot, oco2.ViTri, oco2.SoHuu));
                _MangOCo[oco2.Dong, oco2.Cot].SoHuu = oco2.SoHuu;
                _BanCo.VeQuanCo(g, oco2.ViTri, oco2.SoHuu == 1 ? sbBlack : sbWhite);           
            }           
        }
        #endregion
        #region Duyet Chien thang
        public void KetThucTroChoi()
        {
            switch (_ketthuc)
            {
                case KETTHUC.HoaCo:
                    MessageBox.Show("Hòa!");
                    break;
                case KETTHUC._LuotDi1:
                    MessageBox.Show("Người chơi 1 thắng!");
                    break;
                case KETTHUC._LuotDi2:
                    MessageBox.Show("Người chơi 2 thắng!");
                    break;
                case KETTHUC.COM:
                    MessageBox.Show("Computer thắng!");
                    break;
            }
            _SanSang = false;
        }

        public bool KiemTraChienThang()
        {

            if (stkCacNuocDaDi.Count == _BanCo.SoCot * _BanCo.SoDong)
            {
                _ketthuc = KETTHUC.HoaCo;
                return true;
            }

            foreach (OCo oco in stkCacNuocDaDi)
            {
                if (DuyetDoc(oco.Dong, oco.Cot, oco.SoHuu) || DuyetNgang(oco.Dong, oco.Cot, oco.SoHuu) || DuyetCheoNguoc(oco.Dong, oco.Cot, oco.SoHuu) || DuyetCheoXuoi(oco.Dong, oco.Cot, oco.SoHuu))
                {
                    if (CheDoChoi == 2)
                    {
                        _ketthuc = oco.SoHuu == 1 ? KETTHUC.COM : KETTHUC._LuotDi2;
                        return true;
                    }
                    else
                    {
                        _ketthuc = oco.SoHuu == 1 ? KETTHUC._LuotDi1 : KETTHUC._LuotDi2;
                        return true;
                    }
                }
            }
            return false;
        }

        private bool DuyetDoc(int currDong, int currCot, int currSoHuu)
        {
            if (currDong > _BanCo.SoDong - 5)
                return false;
            int Dem;
            for (Dem = 1; Dem < 5; Dem++)
            {
                if (_MangOCo[currDong + Dem, currCot].SoHuu != currSoHuu)
                    return false;
            }
            if (currDong == 0 || currDong + Dem == _BanCo.SoDong)
                return true;

            if (_MangOCo[currDong - 1, currCot].SoHuu == 0 || _MangOCo[currDong + Dem, currCot].SoHuu == 0)
            {
                return true;
            }

            return false;
        }

        private bool DuyetNgang(int currDong, int currCot, int currSoHuu)
        {
            if (currCot > _BanCo.SoCot - 5)
                return false;
            int Dem;
            for (Dem = 1; Dem < 5; Dem++)
            {
                if (_MangOCo[currDong, currCot + Dem].SoHuu != currSoHuu)
                    return false;
            }
            if (currCot == 0 || currCot + Dem == _BanCo.SoCot)
                return true;

            if (_MangOCo[currDong, currCot - 1].SoHuu == 0 || _MangOCo[currDong, currCot + Dem].SoHuu == 0)
            {
                return true;
            }

            return false;
        }
        private bool DuyetCheoXuoi(int currDong, int currCot, int currSoHuu)
        {
            if (currDong > _BanCo.SoDong - 5 || currCot > _BanCo.SoCot - 5)
                return false;
            int Dem;
            for (Dem = 1; Dem < 5; Dem++)
            {
                if (_MangOCo[currDong + Dem, currCot + Dem].SoHuu != currSoHuu)
                    return false;
            }
            if (currDong == 0 || currDong + Dem == _BanCo.SoDong || currCot == 0 || currCot + Dem == _BanCo.SoCot)
                return true;

            if (_MangOCo[currDong - 1, currCot - 1].SoHuu == 0 || _MangOCo[currDong + Dem, currCot + Dem].SoHuu == 0)
            {
                return true;
            }

            return false;
        }
        private bool DuyetCheoNguoc(int currDong, int currCot, int currSoHuu)
        {
            if (currDong < 4 || currCot > _BanCo.SoCot - 5)
                return false;
            int Dem;
            for (Dem = 1; Dem < 5; Dem++)
            {
                if (_MangOCo[currDong - Dem, currCot + Dem].SoHuu != currSoHuu)
                    return false;
            }
            if (currDong == 4 || currDong == _BanCo.SoDong - 1 || currCot == 0 || currCot + Dem == _BanCo.SoCot)
                return true;

            if (_MangOCo[currDong + 1, currCot - 1].SoHuu == 0 || _MangOCo[currDong - Dem, currCot + Dem].SoHuu == 0)
            {
                return true;
            }

            return false;
        }
        #endregion
        #region AI

        Random rand = new Random();



        public void KhoiDongComputer(Graphics g)
        {

            if (stkCacNuocDaDi.Count == 0)
            {

                DanhCo(_BanCo.SoCot / 2 * OCo._ChieuRong + 1, _BanCo.SoDong / 2 * OCo._ChieuCao + 1, g);
            }

            else
            {
                //OCo oco = TimKiemNuocDi(); // danh vao o co nuoc di tot nhat
                OCo oco = ChonNuocDiTotNhat();
                DanhCo(oco.ViTri.X + 1, oco.ViTri.Y + 1, g);
            }

        }

        private OCo ChonNuocDiTotNhat()
        {
            int[,] mangTrangthaiHienTai = new int[_BanCo.SoDong, _BanCo.SoCot];
            for (int i = 0; i < _BanCo.SoDong; i++)
            {
                for (int k = 0; k < _BanCo.SoCot; k++)
                {
                    mangTrangthaiHienTai[i, k] = _MangOCo[i, k].SoHuu;
                }
            }
            OCo KetQua;
            List<Point> DanhSachTrangThaiCanXemXet = new List<Point>();
            for (int i = 0; i < _branch; i++)
                DanhSachTrangThaiCanXemXet.Add(GetMaxNode(mangTrangthaiHienTai));
            int[] MangGiaTriCuaTungTrangThai = new int[_branch];

            int alpha = -10000, beta = 10000, depth = 0;
            int j = 0;
            foreach (Point i in DanhSachTrangThaiCanXemXet)
            {
                int v = MinValue(mangTrangthaiHienTai, i, alpha, beta, depth);
                MangGiaTriCuaTungTrangThai[j] = v;
                j++;
            }

            int tmp = 0;
            List<Point> list = new List<Point>();

            for (int i = 0; i < MangGiaTriCuaTungTrangThai.Length; i++)
            {
                if (MangGiaTriCuaTungTrangThai[tmp] < MangGiaTriCuaTungTrangThai[i])
                {
                    tmp = i;
                }
            }

            for (int i = 0; i < MangGiaTriCuaTungTrangThai.Length; i++)
            {
                if (MangGiaTriCuaTungTrangThai[tmp] == MangGiaTriCuaTungTrangThai[i])
                {
                    list.Add(new Point(DanhSachTrangThaiCanXemXet[i].X, DanhSachTrangThaiCanXemXet[i].Y));
                }
            }

            int x = rand.Next(0, list.Count);

            KetQua = new OCo(list[x].X, list[x].Y, _MangOCo[list[x].X, list[x].Y].ViTri, 1);
            return KetQua;
        }


        private Point GetMaxNode(int[,] mangTrangThaiHienTai)
        {   
            int[,] MangGiaTri = new int[_BanCo.SoDong, _BanCo.SoCot];

            int n = _BanCo.SoCot;
            int cComputer, cPlayer;
            int i, rw, cl;
            for (rw = 0; rw < n; rw++)
                for (cl = 0; cl < n - 4; cl++)
                {
                    cComputer = 0; cPlayer = 0;
                    for (i = 0; i < 5; i++)
                    {
                        if (mangTrangThaiHienTai[rw, cl + i] == 1) cComputer++;
                        if (mangTrangThaiHienTai[rw, cl + i] == 2) cPlayer++;
                    }
                    if (cComputer * cPlayer == 0 && cComputer != cPlayer)
                        for (i = 0; i < 5; i++)
                            if (mangTrangThaiHienTai[rw, cl + i] == 0)
                            {
                                if (cComputer == 0)
                                {
                                    if (_LuotDi == 1) MangGiaTri[rw, cl + i] += MangDiemPhongNgu[cPlayer];
                                    else MangGiaTri[rw, cl + i] += MangDiemTanCong[cPlayer];
                                    if (CheckPosition(rw, cl - 1) && CheckPosition(rw, cl + 5) && mangTrangThaiHienTai[rw, cl - 1] == 1 && mangTrangThaiHienTai[rw, cl + 5] == 1)
                                        MangGiaTri[rw, cl + i] = 0;
                                }
                                if (cPlayer == 0)
                                {
                                    if (_LuotDi == 2) MangGiaTri[rw, cl + i] += MangDiemPhongNgu[cComputer];
                                    else MangGiaTri[rw, cl + i] += MangDiemTanCong[cComputer];
                                    if (CheckPosition(rw, cl - 1) && CheckPosition(rw, cl + 5) && mangTrangThaiHienTai[rw, cl - 1] == 2 && mangTrangThaiHienTai[rw, cl + 5] == 2)
                                        MangGiaTri[rw, cl + i] = 0;
                                }
                                if ((cComputer == 4 || cPlayer == 4) && ((CheckPosition(rw, cl + i - 1) && mangTrangThaiHienTai[rw, cl + i - 1] == 0) || (CheckPosition(rw, cl + i + 1) && mangTrangThaiHienTai[rw, cl + i + 1] == 0)))
                                    MangGiaTri[rw, cl + i] *= 2;
                                else if (cComputer == 4 || cPlayer == 4)
                                    MangGiaTri[rw, cl + i] *= 2;
                            }
                }
            //Cot
            for (rw = 0; rw < n - 4; rw++)
                for (cl = 0; cl < n; cl++)
                {
                    cComputer = 0; cPlayer = 0;
                    for (i = 0; i < 5; i++)
                    {
                        if (mangTrangThaiHienTai[rw + i, cl] == 1) cComputer++;
                        if (mangTrangThaiHienTai[rw + i, cl] == 2) cPlayer++;
                    }
                    if (cComputer * cPlayer == 0 && cComputer != cPlayer)
                        for (i = 0; i < 5; i++)
                            if (mangTrangThaiHienTai[rw + i, cl] == 0)
                            {
                                if (cComputer == 0)
                                {
                                    if (_LuotDi == 1) MangGiaTri[rw + i, cl] += MangDiemPhongNgu[cPlayer];
                                    else MangGiaTri[rw + i, cl] += MangDiemTanCong[cPlayer];
                                    if (CheckPosition(rw - 1, cl) && CheckPosition(rw + 5, cl) && mangTrangThaiHienTai[rw - 1, cl] == 1 && mangTrangThaiHienTai[rw + 5, cl] == 1)
                                        MangGiaTri[rw + i, cl] = 0;
                                }
                                if (cPlayer == 0)
                                {
                                    if (_LuotDi == 2) MangGiaTri[rw + i, cl] += MangDiemPhongNgu[cComputer];
                                    else MangGiaTri[rw + i, cl] += MangDiemTanCong[cComputer];
                                    if (CheckPosition(rw - 1, cl) && CheckPosition(rw + 5, cl) && mangTrangThaiHienTai[rw - 1, cl] == 2 && mangTrangThaiHienTai[rw + 5, cl] == 2)
                                        MangGiaTri[rw + i, cl] = 0;
                                }
                                if ((cComputer == 4 || cPlayer == 4) && ((CheckPosition(rw + i - 1, cl) && mangTrangThaiHienTai[rw + i - 1, cl] == 0) || (CheckPosition(rw + i + 1, cl) && mangTrangThaiHienTai[rw + i + 1, cl] == 0)))
                                    MangGiaTri[rw + i, cl] *= 2;
                                else if (cComputer == 4 || cPlayer == 4)
                                    MangGiaTri[rw + i, cl] *= 2;
                            }
                }
            //Duong cheo xuong
            for (rw = 0; rw < n - 4; rw++)
                for (cl = 0; cl < n - 4; cl++)
                {
                    cComputer = 0; cPlayer = 0;
                    for (i = 0; i < 5; i++)
                    {
                        if (mangTrangThaiHienTai[rw + i, cl + i] == 1) cComputer++;
                        if (mangTrangThaiHienTai[rw + i, cl + i] == 2) cPlayer++;
                    }
                    //Luong gia..
                    if (cComputer * cPlayer == 0 && cComputer != cPlayer)
                        for (i = 0; i < 5; i++)
                            if (mangTrangThaiHienTai[rw + i, cl + i] == 0)
                            {
                                if (cComputer == 0)
                                {
                                    if (_LuotDi == 1) MangGiaTri[rw + i, cl + i] += MangDiemPhongNgu[cPlayer];
                                    else MangGiaTri[rw + i, cl + i] += MangDiemTanCong[cPlayer];
                                    if (CheckPosition(rw - 1, cl - 1) && CheckPosition(rw + 5, cl + 5) && mangTrangThaiHienTai[rw - 1, cl - 1] == 1 && mangTrangThaiHienTai[rw + 5, cl + 5] == 1)
                                        MangGiaTri[rw + i, cl + i] = 0;
                                }
                                if (cPlayer == 0)
                                {
                                    if (_LuotDi == 2) MangGiaTri[rw + i, cl + i] += MangDiemPhongNgu[cComputer];
                                    else MangGiaTri[rw + i, cl + i] += MangDiemTanCong[cComputer];
                                    if (CheckPosition(rw - 1, cl - 1) && CheckPosition(rw + 5, cl + 5) && mangTrangThaiHienTai[rw - 1, cl - 1] == 2 && mangTrangThaiHienTai[rw + 5, cl + 5] == 2)
                                        MangGiaTri[rw + i, cl + i] = 0;
                                }
                                if ((cComputer == 4 || cPlayer == 4) && ((CheckPosition(rw + i - 1, cl + i - 1) && mangTrangThaiHienTai[rw + i - 1, cl + i - 1] == 0) || (CheckPosition(rw + i + 1, cl + i + 1) && mangTrangThaiHienTai[rw + i + 1, cl + i + 1] == 0)))
                                    MangGiaTri[rw + i, cl + i] *= 2;
                                else if (cComputer == 4 || cPlayer == 4)
                                    MangGiaTri[rw + i, cl + i] *= 2;
                            }
                }
            //Duong cheo len
            for (rw = 4; rw < n; rw++)
                for (cl = 0; cl < n - 4; cl++)
                {
                    cComputer = 0; cPlayer = 0;
                    for (i = 0; i < 5; i++)
                    {
                        if (mangTrangThaiHienTai[rw - i, cl + i] == 1) cComputer++;
                        if (mangTrangThaiHienTai[rw - i, cl + i] == 2) cPlayer++;
                    }
                    //Luong gia..
                    if (cComputer * cPlayer == 0 && cComputer != cPlayer)
                        for (i = 0; i < 5; i++)
                            if (mangTrangThaiHienTai[rw - i, cl + i] == 0)
                            {
                                if (cComputer == 0)
                                {
                                    if (_LuotDi == 1) MangGiaTri[rw - i, cl + i] += MangDiemPhongNgu[cPlayer];
                                    else MangGiaTri[rw - i, cl + i] += MangDiemTanCong[cPlayer];
                                    if (CheckPosition(rw + 1, cl - 1) && CheckPosition(rw - 5, cl + 5) && mangTrangThaiHienTai[rw + 1, cl - 1] == 1 && mangTrangThaiHienTai[rw - 5, cl + 5] == 1)
                                        MangGiaTri[rw - i, cl + i] = 0;
                                }
                                if (cPlayer == 0)
                                {
                                    if (_LuotDi == 2) MangGiaTri[rw - i, cl + i] += MangDiemPhongNgu[cComputer];
                                    else MangGiaTri[rw - i, cl + i] += MangDiemTanCong[cComputer];
                                    if (CheckPosition(rw + 1, cl - 1) && CheckPosition(rw - 5, cl + 5) && mangTrangThaiHienTai[rw + 1, cl - 1] == 2 && mangTrangThaiHienTai[rw - 5, cl + 5] == 2)
                                        MangGiaTri[rw + i, cl + i] = 0;
                                }
                                if ((cComputer == 4 || cPlayer == 4) && ((CheckPosition(rw - i + 1, cl + i - 1) && mangTrangThaiHienTai[rw - i + 1, cl + i - 1] == 0) || (CheckPosition(rw - i - 1, cl + i + 1) && mangTrangThaiHienTai[rw - i - 1, cl + i + 1] == 0)))
                                    MangGiaTri[rw - i, cl + i] *= 2;
                                else if (cComputer == 4 || cPlayer == 4)
                                    MangGiaTri[rw - i, cl + i] *= 2;
                            }
                }



            Point MaxPoint = new Point(0, 0);
            List<Point> list = new List<Point>();
            for (int u = 0; u < _BanCo.SoDong; u++)
            {
                for (int j = 0; j < _BanCo.SoCot; j++)
                {
                    if (MangGiaTri[MaxPoint.X, MaxPoint.Y] <= MangGiaTri[u, j])
                    {
                        MaxPoint.X = u;
                        MaxPoint.Y = j;

                    }

                }
            }

            for (int u = 0; u < _BanCo.SoDong; u++)
            {
                for (int j = 0; j < _BanCo.SoCot; j++)
                {
                    if (MangGiaTri[MaxPoint.X, MaxPoint.Y] == MangGiaTri[u, j])
            list.Add(new Point(u, j));
                }
            }
                    int x = rand.Next(0, list.Count);

            return list[x];
        }

        public bool CheckPosition(int x, int y)
        {
            return (x >= 0 && y >= 0 && x < _BanCo.SoDong && y < _BanCo.SoCot);
        }


        #region MinMax
        private int MaxValue(int[,] MangTrangThaiHienTai, Point NuocDi, int alpha, int beta, int depth)
        {
            int LuotDiGiaDinh = 1;
            int value = DinhGiaTrangThaiKetThuc(MangTrangThaiHienTai, LuotDiGiaDinh);
            if (depth >= maxdepth || Math.Abs(value) > 3000)
                return value;
            List<Point> DanhSachCacNodeCon = new List<Point>();
            for (int i = 0; i < _branch; i++)
            {
                DanhSachCacNodeCon.Add(GetMaxNode(MangTrangThaiHienTai));

            }
            for (int i = 0; i < DanhSachCacNodeCon.Count; i++)
            {
                MangTrangThaiHienTai[DanhSachCacNodeCon[i].X, DanhSachCacNodeCon[i].Y] = 1;
                alpha = Math.Max(alpha, MinValue(MangTrangThaiHienTai, DanhSachCacNodeCon[i], alpha, beta, depth + 1));
                MangTrangThaiHienTai[DanhSachCacNodeCon[i].X, DanhSachCacNodeCon[i].Y] = 0;
                if (alpha >= beta)
                    break;
            }

            return alpha;

        }

        private int MinValue(int[,] MangTrangThaiHienTai, Point NuocDi, int alpha, int beta, int depth)
        {
            int LuotDiGiaDinh = 2;
            int value = DinhGiaTrangThaiKetThuc(MangTrangThaiHienTai, LuotDiGiaDinh);
            if (depth >= maxdepth || Math.Abs(value) > 3000)
                return value;
            List<Point> DanhSachCacNodeCon = new List<Point>();
            for (int i = 0; i < _branch; i++)
            {
                DanhSachCacNodeCon.Add(GetMaxNode(MangTrangThaiHienTai));
                
            }
            for (int i = 0; i < DanhSachCacNodeCon.Count; i++)
            {
                MangTrangThaiHienTai[DanhSachCacNodeCon[i].X, DanhSachCacNodeCon[i].Y] = 2;
                beta = Math.Min(beta, MaxValue(MangTrangThaiHienTai, DanhSachCacNodeCon[i], alpha, beta, depth + 1));
                MangTrangThaiHienTai[DanhSachCacNodeCon[i].X, DanhSachCacNodeCon[i].Y] = 0;
                if (alpha >= beta)
                    break;
            }

            return beta;

        }

        private int DinhGiaTrangThaiKetThuc(int[,] MangTrangThaiHienTai,int LuotDiHienTai)
        {

            int diem = 0;
            string s = "";
            int k = _BanCo.SoCot;
            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < k; j++)
                    s += MangTrangThaiHienTai[i, j];
                s += ";";
                for (int j = 0; j < k; j++)
                    s += MangTrangThaiHienTai[j, i];
                s += ";";
            }
            for (int i = 0; i < k - 4; i++)
            {
                for (int j = 0; j < k - i; j++)
                    s += MangTrangThaiHienTai[j, i + j];
                s += ";";
            }
            for (int i = k - 5; i > 0; i--)
            {
                for (int j = 0; j < k - i; j++)
                    s += MangTrangThaiHienTai[i + j, j];
                s += ";";
            }
            for (int i = 4; i < k; i++)
            {
                for (int j = 0; j <= i; j++)
                    s += MangTrangThaiHienTai[i - j, j];
                s += ";";
            }
            for (int i = k - 5; i > 0; i--)
            {
                for (int j = k - 1; j >= i; j--)
                    s += MangTrangThaiHienTai[j, i +k - j - 1];
                s += ";\n";
            }
            Regex regex1, regex2;
            for (int i = 0; i < Truonghop1.Length; i++)
            {
                regex1 = new Regex(Truonghop1[i]);
                regex2 = new Regex(Truonghop2[i]);
                if (_LuotDi == 1)
                {
                    diem += point[i] * regex2.Matches(s).Count;
                    diem -= point[i] * regex1.Matches(s).Count;
                }
                else
                {
                    diem -= point[i] * regex2.Matches(s).Count;
                    diem += point[i] * regex1.Matches(s).Count;
                }
            }
            return diem;
        }

        #endregion
        #endregion


    }
}


