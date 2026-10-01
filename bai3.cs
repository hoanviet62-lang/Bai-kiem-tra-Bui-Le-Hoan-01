using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TechMartProductManager
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class Product
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string ImagePath { get; set; } = string.Empty;
    }

    public partial class Form1 : Form
    {
        private MenuStrip menuStrip;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;
        private TableLayoutPanel mainLayout;
        private TextBox txtProductId, txtProductName, txtUnitPrice, txtQuantity, txtSearch;
        private ComboBox cboCategory;
        private PictureBox picAvatar;
        private Button btnChooseImage, btnAdd, btnUpdate, btnDelete, btnExport;
        private ErrorProvider errorProvider;
        private DataGridView dgvProducts;

        private BindingList<Product> originalList = new BindingList<Product>();
        private BindingSource bindingSource = new BindingSource();

        public Form1()
        {
            InitializeCustomComponents();
            bindingSource.DataSource = originalList;
            dgvProducts.DataSource = bindingSource;
            UpdateStatusCount();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "TechMart Product Manager";
            this.Size = new Size(1100, 680);
            this.StartPosition = FormStartPosition.CenterScreen;

            errorProvider = new ErrorProvider();

            // Menu Strip
            menuStrip = new MenuStrip();
            var menuFile = new ToolStripMenuItem("File");
            menuFile.DropDownItems.Add(new ToolStripMenuItem("Export CSV", null, (s, e) => ExportToCSV(), Keys.Control | Keys.E));
            menuFile.DropDownItems.Add(new ToolStripMenuItem("Exit", null, (s, e) => Application.Exit(), Keys.Control | Keys.X));
            menuStrip.Items.Add(menuFile);

            // Status Strip
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel("Tổng số sản phẩm: 0");
            statusStrip.Items.Add(lblStatus);

            // Main TableLayoutPanel (35% - 65%)
            mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35F));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // Left Input Box
            GroupBox grpInput = new GroupBox { Text = "Thông tin sản phẩm", Dock = DockStyle.Fill, Padding = new Padding(10) };
            
            TableLayoutPanel inputLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 10,
                AutoScroll = true
            };
            // Tăng rộng cột Label lên 110px để không bị nhảy dòng
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
            inputLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            txtProductId = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            txtProductName = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            txtUnitPrice = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            txtQuantity = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };

            cboCategory = new ComboBox { Anchor = AnchorStyles.Left | AnchorStyles.Right, DropDownStyle = ComboBoxStyle.DropDownList };
            cboCategory.DataSource = new[]
            {
                new Category { Id = 1, Name = "Điện thoại" },
                new Category { Id = 2, Name = "Laptop" },
                new Category { Id = 3, Name = "Phụ kiện" }
            };
            cboCategory.DisplayMember = "Name";
            cboCategory.ValueMember = "Id";

            picAvatar = new PictureBox { SizeMode = PictureBoxSizeMode.Zoom, Height = 110, BorderStyle = BorderStyle.FixedSingle, Dock = DockStyle.Fill };
            btnChooseImage = new Button { Text = "Chọn Ảnh", Width = 90, Height = 28 };
            btnChooseImage.Click += BtnChooseImage_Click;

            AddInputRow(inputLayout, 0, "Mã SP:", txtProductId);
            AddInputRow(inputLayout, 1, "Tên SP:", txtProductName);
            AddInputRow(inputLayout, 2, "Danh mục:", cboCategory);
            AddInputRow(inputLayout, 3, "Đơn giá:", txtUnitPrice);
            AddInputRow(inputLayout, 4, "Số lượng:", txtQuantity);

            inputLayout.Controls.Add(new Label { Text = "Ảnh SP:", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 5);
            inputLayout.Controls.Add(btnChooseImage, 1, 5);
            
            inputLayout.Controls.Add(picAvatar, 1, 6);

            // Panel Nút chức năng
            FlowLayoutPanel btnPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Margin = new Padding(0, 10, 0, 10) };
            btnAdd = new Button { Text = "Thêm", Width = 65, Height = 30 };
            btnUpdate = new Button { Text = "Sửa", Width = 65, Height = 30 };
            btnDelete = new Button { Text = "Xóa", Width = 65, Height = 30 };
            btnExport = new Button { Text = "Xuất", Width = 65, Height = 30 };

            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnExport.Click += (s, e) => ExportToCSV();

            btnPanel.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnExport });
            inputLayout.Controls.Add(btnPanel, 0, 7);
            inputLayout.SetColumnSpan(btnPanel, 2);

            txtSearch = new TextBox { Anchor = AnchorStyles.Left | AnchorStyles.Right };
            txtSearch.TextChanged += TxtSearch_TextChanged;
            AddInputRow(inputLayout, 8, "Tìm kiếm:", txtSearch);

            grpInput.Controls.Add(inputLayout);

            // DataGridView Cột Phải
            dgvProducts = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductId", HeaderText = "Mã SP", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Tên SP", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "CategoryName", HeaderText = "Danh Mục", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "UnitPrice", HeaderText = "Đơn Giá", DefaultCellStyle = new DataGridViewCellStyle { Format = "N0" }, AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            dgvProducts.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Quantity", HeaderText = "Số Lượng", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });

            dgvProducts.SelectionChanged += DgvProducts_SelectionChanged;

            mainLayout.Controls.Add(grpInput, 0, 0);
            mainLayout.Controls.Add(dgvProducts, 1, 0);

            this.Controls.Add(mainLayout);
            this.Controls.Add(statusStrip);
            this.Controls.Add(menuStrip);
            this.MainMenuStrip = menuStrip;
        }

        private void AddInputRow(TableLayoutPanel layout, int row, string labelText, Control control)
        {
            layout.Controls.Add(new Label { Text = labelText, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row);
            layout.Controls.Add(control, 1, row);
        }

        private void UpdateStatusCount() => lblStatus.Text = $"Tổng số sản phẩm: {originalList.Count}";

        private void BtnChooseImage_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog { Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                    picAvatar.ImageLocation = ofd.FileName;
            }
        }

        private bool ValidateInput()
        {
            errorProvider.Clear();
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                errorProvider.SetError(txtProductName, "Tên sản phẩm không được để trống!");
                isValid = false;
            }

            if (!decimal.TryParse(txtUnitPrice.Text, out decimal price) || price <= 0)
            {
                errorProvider.SetError(txtUnitPrice, "Đơn giá phải là số > 0!");
                isValid = false;
            }

            if (!int.TryParse(txtQuantity.Text, out int qty) || qty < 0)
            {
                errorProvider.SetError(txtQuantity, "Số lượng phải là số >= 0!");
                isValid = false;
            }

            return isValid;
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            string id = string.IsNullOrWhiteSpace(txtProductId.Text) ? $"SP{originalList.Count + 1:D3}" : txtProductId.Text;
            var category = (Category)cboCategory.SelectedItem!;

            originalList.Add(new Product
            {
                ProductId = id,
                ProductName = txtProductName.Text.Trim(),
                CategoryId = category.Id,
                CategoryName = category.Name,
                UnitPrice = decimal.Parse(txtUnitPrice.Text),
                Quantity = int.Parse(txtQuantity.Text),
                ImagePath = picAvatar.ImageLocation ?? ""
            });

            UpdateStatusCount();
            ClearInputs();
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow?.DataBoundItem is Product p)
            {
                if (!ValidateInput()) return;

                p.ProductName = txtProductName.Text.Trim();
                var category = (Category)cboCategory.SelectedItem!;
                p.CategoryId = category.Id;
                p.CategoryName = category.Name;
                p.UnitPrice = decimal.Parse(txtUnitPrice.Text);
                p.Quantity = int.Parse(txtQuantity.Text);
                p.ImagePath = picAvatar.ImageLocation ?? "";

                bindingSource.ResetBindings(false);
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow?.DataBoundItem is Product p)
            {
                if (MessageBox.Show($"Bạn có chắc chắn muốn xóa sản phẩm [{p.ProductName}] không?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    originalList.Remove(p);
                    UpdateStatusCount();
                    ClearInputs();
                }
            }
        }

        private void DgvProducts_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow?.DataBoundItem is Product p)
            {
                txtProductId.Text = p.ProductId;
                txtProductName.Text = p.ProductName;
                cboCategory.SelectedValue = p.CategoryId;
                txtUnitPrice.Text = p.UnitPrice.ToString("0.##");
                txtQuantity.Text = p.Quantity.ToString();
                picAvatar.ImageLocation = p.ImagePath;
            }
        }

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(keyword))
            {
                bindingSource.DataSource = originalList;
            }
            else
            {
                var filtered = originalList.Where(p => p.ProductName.ToLower().Contains(keyword)).ToList();
                bindingSource.DataSource = new BindingList<Product>(filtered);
            }
        }

        private void ExportToCSV()
        {
            using (SaveFileDialog sfd = new SaveFileDialog { Filter = "CSV Files (*.csv)|*.csv", FileName = "DanhSachSanPham.csv" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("Mã SP,Tên SP,Danh Mục,Đơn Giá,Số Lượng");
                    foreach (var p in originalList)
                        sb.AppendLine($"\"{p.ProductId}\",\"{p.ProductName}\",\"{p.CategoryName}\",{p.UnitPrice},{p.Quantity}");

                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất file CSV thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ClearInputs()
        {
            txtProductId.Clear();
            txtProductName.Clear();
            txtUnitPrice.Clear();
            txtQuantity.Clear();
            picAvatar.ImageLocation = null;
            errorProvider.Clear();
        }
    }
}