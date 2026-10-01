Console.OutputEncoding = System.Text.Encoding.UTF8;
QuanLyPhuongTien ql = new QuanLyPhuongTien();

Console.WriteLine("=== KỊCH BẢN KIỂM THỬ ===");

try { new OTo("OT01", "Toyota", 1850, 1000000m, 5, 2.0); }
catch (Exception ex) { Console.WriteLine($"[TC01] Bắt lỗi thành công: {ex.Message}"); }

OTo oto = new OTo("OT02", "Mercedes", 2023, 1000000000m, 5, 2.0);
XeMay xeMay = new XeMay("XM01", "Honda", 2023, 50000000m, 150);
ql.AddPhuongTien(oto);
ql.AddPhuongTien(xeMay);

Console.WriteLine("\n[TC02, TC03, TC04] Đa hình tính Giá lăn bánh:");
ql.DisplayAll();

var ptMax = ql.FindMaxGiaLanBanh();
Console.WriteLine($"\n[TC05] Giá Max: {ptMax?.GetInfo()} -> {ptMax?.TinhGiaLanBanh():N0} VNĐ");

public abstract class PhuongTien
{
    private string _maPT = "PT000", _tenHang = "";
    private int _namSanXuat;
    private decimal _giaGoc;

    public string MaPT { get => _maPT; set => _maPT = string.IsNullOrWhiteSpace(value) ? "PT000" : value; }
    public string TenHang { get => _tenHang; set => _tenHang = string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Tên không được trống") : value; }
    public int NamSanXuat { get => _namSanXuat; set => _namSanXuat = (value < 1900 || value > DateTime.Now.Year) ? throw new ArgumentException("Năm sản xuất không hợp lệ!") : value; }
    public decimal GiaGoc { get => _giaGoc; set => _giaGoc = value <= 0 ? throw new ArgumentException("Giá gốc > 0") : value; }

    protected PhuongTien(string ma, string ten, int nam, decimal gia) 
    { MaPT = ma; TenHang = ten; NamSanXuat = nam; GiaGoc = gia; }

    public abstract decimal TinhGiaLanBanh();
    public virtual string GetInfo() => $"[{MaPT}] {TenHang} - {NamSanXuat} | Giá gốc: {GiaGoc:N0}";
}

public class OTo : PhuongTien
{
    private int _soChoNgoi;
    private double _dungTichDongCo;

    public int SoChoNgoi { get => _soChoNgoi; set => _soChoNgoi = value > 0 ? value : throw new ArgumentException("Chỗ ngồi > 0"); }
    public double DungTichDongCo { get => _dungTichDongCo; set => _dungTichDongCo = value > 0 ? value : throw new ArgumentException("Động cơ > 0"); }

    public OTo(string ma, string ten, int nam, decimal gia, int cho, double dongCo) : base(ma, ten, nam, gia)
    { SoChoNgoi = cho; DungTichDongCo = dongCo; }

    public override decimal TinhGiaLanBanh() => SoChoNgoi <= 9 ? GiaGoc * 1.42m : GiaGoc * 1.10m;
    public override string GetInfo() => base.GetInfo() + $" | {SoChoNgoi} chỗ | {DungTichDongCo}L";
}

public class XeMay : PhuongTien
{
    private int _dungTichXylanh;
    public int DungTichXylanh { get => _dungTichXylanh; set => _dungTichXylanh = value > 0 ? value : throw new ArgumentException("Xy-lanh > 0"); }

    public XeMay(string ma, string ten, int nam, decimal gia, int xylanh) : base(ma, ten, nam, gia) 
    { DungTichXylanh = xylanh; }

    public override decimal TinhGiaLanBanh() => DungTichXylanh < 175 ? GiaGoc * 1.02m : GiaGoc * 1.05m;
    public override string GetInfo() => base.GetInfo() + $" | {DungTichXylanh}cc";
}

public class QuanLyPhuongTien
{
    private List<PhuongTien> _ds = new();

    public void AddPhuongTien(PhuongTien pt) => _ds.Add(pt);
    public void DisplayAll() => _ds.ForEach(pt => Console.WriteLine($"{pt.GetInfo()} => LĂN BÁNH: {pt.TinhGiaLanBanh():N0} VNĐ"));
    public PhuongTien FindMaxGiaLanBanh() => _ds.OrderByDescending(p => p.TinhGiaLanBanh()).FirstOrDefault();
    public List<PhuongTien> SearchByName(string kw) => _ds.Where(p => p.TenHang.Contains(kw, StringComparison.OrdinalIgnoreCase)).ToList();
}
