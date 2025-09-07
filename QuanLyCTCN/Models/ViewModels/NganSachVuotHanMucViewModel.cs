using QuanLyCTCN.Models;

namespace QuanLyCTCN.Models.ViewModels
{
    public class NganSachVuotHanMucViewModel
    {
        public NganSach NganSach { get; set; } = null!;
        public decimal TongChiTieu { get; set; }
        public decimal PhanTramDaSuDung { get; set; }
    }
}
