using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using ServiceStack.OrmLite;
using ToDoListApp.DataAccess;
using ToDoListApp.DataAccess.Models;
using DynamicData.Binding;
using NHibernate;

namespace ToDoListApp
{
    public partial class Form1 : Form
    {
        private Guna2Button selectedButton;
        private DateTime _date = DateTime.Today;
        private string _category = "All";
        private string _filter = "All";
        private string _searchTerm = "";
        private bool _isDateFilterEnabled = false;
        public Form1()
        {
            InitializeComponent();
            LoadCategories();
            this.Text = "ToDoList Application";
        }

        private void LoadCategories()
        {
            var db = DbContext.GetInstance();
            flowLayoutPanel1.Controls.Clear(); 

            // Button "All" mặc định được chọn
            Guna2Button btnAll = CreateCategoryButton("All", true);
            flowLayoutPanel1.Controls.Add(btnAll);
            selectedButton = btnAll; // Đặt "All" là nút được chọn ban đầu

            // Fetch categories từ database
            foreach (var category in db.Select<Category>())
            {
                Guna2Button btnCategory = CreateCategoryButton(category.CategoryName, false);
                flowLayoutPanel1.Controls.Add(btnCategory);
            }
        }

        private Guna2Button CreateCategoryButton(string categoryName, bool isSelected)
        {
            Guna2Button btn = new Guna2Button
            {
                Text = categoryName,
                AutoSize = true,
                Padding = new Padding(10),
                BorderRadius = 10,
                FillColor = isSelected ? Color.DarkOrange : Color.Teal, // Mặc định chọn "All"
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            btn.Click += (sender, e) => CategoryButton_Click(btn, categoryName);
            return btn;
        }

        private void CategoryButton_Click(Guna2Button clickedButton, string categoryName)
        {
            if (selectedButton != null)
            {
                // Reset màu button trước đó
                selectedButton.FillColor = Color.Teal;
            }

            // Đổi màu button hiện tại
            clickedButton.FillColor = Color.DarkOrange;
            selectedButton = clickedButton; // Cập nhật button đang được chọn

            // Update the category filter
            this._category = categoryName;

            // Nếu người dùng chọn "All" thì reset tất cả các filter
            if (categoryName == "All")
            {
                _filter = "All";
                _isDateFilterEnabled = false;
                _searchTerm = "";
                txtBoxSearch.Text = ""; // Clear the search textbox
            }

            // Reload data with the new filter
            reloadData();
        }

        private void reloadData()
        {
            var db = DbContext.GetInstance();
            var data = db.Select<TodoItem>();

            if (this._category != "All")
            {
                data = data.Where(r => r.CategoryId == Category.GetCategoryByName(this._category).id).ToList();
            }

            if (_filter == "Completed")
            {
                data = data.Where(t => t.Done).ToList();
            }
            else if (_filter == "Pending")
            {
                data = data.Where(t => !t.Done).ToList();
            }
            else if (_filter == "Today")
            {
                data = data.Where(t => t.StartDate.Date <= DateTime.Today && t.EndDate.Date >= DateTime.Today).ToList();
            }

            // Chỉ áp dụng lọc theo ngày khi _isDateFilterEnabled = true
            if (_isDateFilterEnabled)
            {
                data = data.Where(t => t.StartDate.Date <= _date.Date && t.EndDate.Date >= _date.Date).ToList();
            }

            if (!string.IsNullOrEmpty(_searchTerm))
            {
                data = data.Where(t =>
                    t.Description.ToLower().Contains(_searchTerm) ||
                    t.GetCategory().CategoryName.ToLower().Contains(_searchTerm)).ToList();
            }

            grid.DataSource = null;
            grid.DataSource = data.Select(t => new
            {
                t.id,
                t.Description,
                Category = t.GetCategory().CategoryName,
                t.StartDate,
                t.EndDate,
                Status = t.Done ? "Completed" : "Pending",
            }).ToList();
            grid.Columns["id"].Visible = false;
            grid.Columns["Description"].Width = 250;
            grid.RowHeadersVisible = false;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(217, 119, 0);
            grid.DefaultCellStyle.SelectionForeColor = Color.Black;

            grid.AlternatingRowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(217, 119, 0);
            grid.AlternatingRowsDefaultCellStyle.SelectionForeColor = Color.Black;

            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 12, FontStyle.Bold);


            foreach (DataGridViewRow row in grid.Rows)
            {
                if (row.Cells["Status"].Value != null)
                {
                    string status = row.Cells["Status"].Value.ToString();
                    if (status == "Completed")
                    {
                        row.Cells["Status"].Style.ForeColor = Color.FromArgb(77, 255,0); // Màu chữ trắng
                        row.Cells["Status"].Style.Font = new Font("Segoe UI", 9); // In đậm
                    }
                    else if (status == "Pending")
                    {
                        row.Cells["Status"].Style.ForeColor = Color.FromArgb(255, 73, 73); // Màu chữ trắng
                        row.Cells["Status"].Style.Font = new Font("Segoe UI", 9); // In đậm
                    }
                }
            }
        }


        private void guna2DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            _isDateFilterEnabled = true;
            this._date = guna2DateTimePicker1.Value;
            reloadData();
        }
        private void ShowNotification(string message, MessageDialogIcon icon)
        {
            Guna2MessageDialog messageDialog = new Guna2MessageDialog
            {
                Buttons = MessageDialogButtons.OK, // Nút bấm
                Icon = icon, // Icon (Information, Warning, Error)
                Style = MessageDialogStyle.Dark, // Giao diện tối
                Caption = "Notification", // Tiêu đề
                Text = message
            };
            messageDialog.Show(); // Hiển thị thông báo
        }


        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        

        private void Form1_Load(object sender, EventArgs e)
        {
            _isDateFilterEnabled = false;
            reloadData();
            this.grid.DefaultCellStyle.Font = new Font("Segoe UI", 9);
        }

        private void guna2Button8_Click(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count > 0)
            {
                var result = MessageBox.Show(this,"Are you sure you want to delete this task?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    var db = DbContext.GetInstance();
                    var id = (int)grid.SelectedRows[0].Cells["id"].Value;
                    db.DeleteById<TodoItem>(id);
                    reloadData();
                    ShowNotification("Task deleted successfully!", MessageDialogIcon.Information);
                }
            }
        }

        
        private void guna2Button7_Click(object sender, EventArgs e)
        {
            using (var form = new Form())
            {
                form.Text = "Add New Task";
                form.Size = new Size(300, 300);
                form.StartPosition = FormStartPosition.CenterParent;

                var txtDescription = new Guna2TextBox { Location = new Point(20, 20), Width = 240, PlaceholderText = "Task Description" };
                var dtpStart = new Guna2DateTimePicker { Location = new Point(20, 60), Width = 240, Value = DateTime.Today };
                var dtpEnd = new Guna2DateTimePicker { Location = new Point(20, 100), Width = 240, Value = DateTime.Today.AddDays(1) };
                var cbCategory = new Guna2ComboBox { Location = new Point(20, 140), Width = 240 };
                var btnSave = new Guna2Button { Location = new Point(20, 180), Width = 240, Text = "Save", FillColor = Color.Teal };

                var db = DbContext.GetInstance();
                cbCategory.Items.AddRange(db.Select<Category>().Select(c => c.CategoryName).ToArray());
                cbCategory.SelectedIndex = 0;

                btnSave.Click += (s, ev) =>
                {
                    var task = new TodoItem
                    {
                        Description = txtDescription.Text,
                        StartDate = dtpStart.Value,
                        EndDate = dtpEnd.Value,
                        CategoryId = db.Single<Category>(c => c.CategoryName == cbCategory.SelectedItem.ToString()).id
                    };
                    db.Insert(task);
                    reloadData();
                    form.Close();
                    ShowNotification("Task added successfully!", MessageDialogIcon.Information);
                };

                form.Controls.AddRange(new Control[] { txtDescription, dtpStart, dtpEnd, cbCategory, btnSave });
                form.ShowDialog(this);
            }
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            if (grid.SelectedRows.Count > 0)
            {
                var db = DbContext.GetInstance();
                var selectedId = (int)grid.SelectedRows[0].Cells["id"].Value;
                var task = db.SingleById<TodoItem>(selectedId);

                using (var form = new Form())
                {
                    form.Text = "Edit Task";
                    form.Size = new Size(300, 350);
                    form.StartPosition = FormStartPosition.CenterParent;

                    var txtDescription = new Guna2TextBox { Location = new Point(20, 20), Width = 240, Text = task.Description };
                    var dtpStart = new Guna2DateTimePicker { Location = new Point(20, 60), Width = 240, Value = task.StartDate };
                    var dtpEnd = new Guna2DateTimePicker { Location = new Point(20, 100), Width = 240, Value = task.EndDate };
                    var cbCategory = new Guna2ComboBox { Location = new Point(20, 140), Width = 240 };
                    var chkDone = new Guna2CheckBox { Location = new Point(20, 180), Text = "Completed", Checked = task.Done };
                    var btnSave = new Guna2Button { Location = new Point(20, 220), Width = 240, Text = "Save", FillColor = Color.Teal };

                    cbCategory.Items.AddRange(db.Select<Category>().Select(c => c.CategoryName).ToArray());
                    cbCategory.SelectedItem = task.GetCategory().CategoryName;

                    btnSave.Click += (s, ev) =>
                    {
                        task.Description = txtDescription.Text;
                        task.StartDate = dtpStart.Value;
                        task.EndDate = dtpEnd.Value;
                        task.CategoryId = db.Single<Category>(c => c.CategoryName == cbCategory.SelectedItem.ToString()).id;
                        task.Done = chkDone.Checked;

                        db.Update(task);
                        reloadData();
                        form.Close();
                        ShowNotification("Task updated successfully!", MessageDialogIcon.Information);
                    };

                    form.Controls.AddRange(new Control[] { txtDescription, dtpStart, dtpEnd, cbCategory, chkDone, btnSave });
                    form.ShowDialog(this);
                }
            }
            else
            {
                ShowNotification("Please select a task to edit!", MessageDialogIcon.Warning);
            }
        }

        private void btnManageCategory_Click(object sender, EventArgs e)
        {
            using (var form = new Form())
            {
                form.Text = "Manage Category";
                form.Size = new Size(400, 500);
                form.StartPosition = FormStartPosition.CenterParent;
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.MaximizeBox = false;
                form.MinimizeBox = false;

                // Tạo DataGridView để hiển thị danh sách danh mục
                var gridCategories = new DataGridView
                {
                    Location = new Point(20, 20),
                    Width = 340,
                    Height = 250,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    MultiSelect = false,
                    ReadOnly = true,
                    BackgroundColor = Color.White,
                    BorderStyle = BorderStyle.None,
                    RowHeadersVisible = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
                };

                // TextBox để nhập tên danh mục mới
                var txtCategoryName = new Guna2TextBox
                {
                    Location = new Point(20, 290),
                    Width = 340,
                    PlaceholderText = "Name of new category"
                };

                // Button để thêm danh mục mới
                var btnAdd = new Guna2Button
                {
                    Location = new Point(20, 330),
                    Width = 160,
                    Text = "Add Category",
                    FillColor = Color.Teal,
                    BorderRadius = 10,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                };

                // Button để sửa danh mục
                var btnEdit = new Guna2Button
                {
                    Location = new Point(200, 330),
                    Width = 160,
                    Text = "Edit",
                    FillColor = Color.DarkOrange,
                    BorderRadius = 10,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                };

                // Button để xóa danh mục
                var btnDelete = new Guna2Button
                {
                    Location = new Point(20, 380),
                    Width = 160,
                    Text = "Detete",
                    FillColor = Color.Crimson,
                    BorderRadius = 10,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                };

                // Button để đóng form
                var btnClose = new Guna2Button
                {
                    Location = new Point(200, 380),
                    Width = 160,
                    Text = "Close",
                    FillColor = Color.Gray,
                    BorderRadius = 10,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                };

                // Hàm để load danh sách danh mục
                void LoadCategoriesData()
                {
                    var db = DbContext.GetInstance();
                    var categories = db.Select<Category>();

                    gridCategories.DataSource = null;
                    gridCategories.DataSource = categories;

                    // Ẩn cột id
                    if (gridCategories.Columns.Contains("id"))
                    {
                        gridCategories.Columns["id"].Visible = false;
                    }

                    // Đổi tên cột CategoryName thành "Tên danh mục"
                    if (gridCategories.Columns.Contains("CategoryName"))
                    {
                        gridCategories.Columns["CategoryName"].HeaderText = "Tên danh mục";
                    }
                }

                // Load dữ liệu ban đầu
                LoadCategoriesData();

                // Xử lý sự kiện thêm danh mục
                btnAdd.Click += (s, ev) =>
                {
                    if (string.IsNullOrWhiteSpace(txtCategoryName.Text))
                    {
                        ShowNotification("Please enter name of category!", MessageDialogIcon.Warning);
                        return;
                    }

                    var db = DbContext.GetInstance();

                    // Kiểm tra danh mục đã tồn tại chưa
                    var existingCategory = db.Select<Category>().FirstOrDefault(c =>
                        c.CategoryName.Equals(txtCategoryName.Text, StringComparison.OrdinalIgnoreCase));

                    if (existingCategory != null)
                    {
                        ShowNotification("This category already exists!", MessageDialogIcon.Warning);
                        return;
                    }

                    var newCategory = new Category { CategoryName = txtCategoryName.Text };
                    db.Insert(newCategory);

                    txtCategoryName.Clear();
                    LoadCategoriesData();
                    LoadCategories(); // Cập nhật lại danh sách category ở form chính
                    ShowNotification("Category added successfully!", MessageDialogIcon.Information);
                };

                // Xử lý sự kiện sửa danh mục
                btnEdit.Click += (s, ev) =>
                {
                    if (gridCategories.SelectedRows.Count == 0)
                    {
                        ShowNotification("Please select a category to edit!", MessageDialogIcon.Warning);
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(txtCategoryName.Text))
                    {
                        ShowNotification("Please enter a new category name!", MessageDialogIcon.Warning);
                        return;
                    }

                    var db = DbContext.GetInstance();
                    var selectedRow = gridCategories.SelectedRows[0];
                    var categoryId = (int)selectedRow.Cells["id"].Value;

                    // Kiểm tra danh mục "All" (danh mục mặc định)
                    var categoryName = selectedRow.Cells["CategoryName"].Value.ToString();
                    if (categoryName == "All")
                    {
                        ShowNotification("Cannot edit default category!", MessageDialogIcon.Warning);
                        return;
                    }

                    // Kiểm tra tên mới đã tồn tại chưa
                    var existingCategory = db.Select<Category>().FirstOrDefault(c =>
                        c.CategoryName.Equals(txtCategoryName.Text, StringComparison.OrdinalIgnoreCase) &&
                        c.id != categoryId);

                    if (existingCategory != null)
                    {
                        ShowNotification("This category name already exists!", MessageDialogIcon.Warning);
                        return;
                    }

                    var category = db.SingleById<Category>(categoryId);
                    category.CategoryName = txtCategoryName.Text;
                    db.Update(category);

                    txtCategoryName.Clear();
                    LoadCategoriesData();
                    LoadCategories(); // Cập nhật lại danh sách category ở form chính
                    reloadData(); // Cập nhật lại danh sách task để hiển thị đúng tên category
                    ShowNotification("Category updated successfully!", MessageDialogIcon.Information);
                };

                // Xử lý sự kiện xóa danh mục
                btnDelete.Click += (s, ev) =>
                {
                    if (gridCategories.SelectedRows.Count == 0)
                    {
                        ShowNotification("Please select a category to delete!", MessageDialogIcon.Warning);
                        return;
                    }

                    var db = DbContext.GetInstance();
                    var selectedRow = gridCategories.SelectedRows[0];
                    var categoryId = (int)selectedRow.Cells["id"].Value;
                    var categoryName = selectedRow.Cells["CategoryName"].Value.ToString();

                    // Kiểm tra danh mục "All" (danh mục mặc định)
                    if (categoryName == "All")
                    {
                        ShowNotification("Cannot delete default category!", MessageDialogIcon.Warning);
                        return;
                    }

                    // Kiểm tra xem danh mục có đang được sử dụng không
                    var tasksUsingCategory = db.Select<TodoItem>().Count(t => t.CategoryId == categoryId);
                    if (tasksUsingCategory > 0)
                    {
                        var result = MessageBox.Show(
                            $"Có {tasksUsingCategory} The job is using this category. Are you sure you want to delete it?",
                            "Confirm deletion",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning
                        );

                        if (result == DialogResult.No)
                        {
                            return;
                        }
                    }
                    else
                    {
                        var result = MessageBox.Show(
                            "Are you sure you want to delete this category?",
                            "Confirm deletion",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question
                        );

                        if (result == DialogResult.No)
                        {
                            return;
                        }
                    }

                    // Xóa danh mục
                    db.DeleteById<Category>(categoryId);

                    // Cập nhật các task đang sử dụng danh mục này sang danh mục mặc định (nếu có)
                    var defaultCategory = db.Select<Category>().FirstOrDefault();
                    if (defaultCategory != null && tasksUsingCategory > 0)
                    {
                        var tasksToUpdate = db.Select<TodoItem>().Where(t => t.CategoryId == categoryId).ToList();
                        foreach (var task in tasksToUpdate)
                        {
                            task.CategoryId = defaultCategory.id;
                            db.Update(task);
                        }
                    }

                    txtCategoryName.Clear();
                    LoadCategoriesData();
                    LoadCategories(); // Cập nhật lại danh sách category ở form chính
                    reloadData(); // Cập nhật lại danh sách task
                    ShowNotification("Category deleted successfully!", MessageDialogIcon.Information);
                };

                // Hiển thị tên danh mục khi click vào một dòng
                gridCategories.SelectionChanged += (s, ev) =>
                {
                    if (gridCategories.SelectedRows.Count > 0)
                    {
                        var selectedRow = gridCategories.SelectedRows[0];
                        txtCategoryName.Text = selectedRow.Cells["CategoryName"].Value.ToString();
                    }
                };

                // Đóng form
                btnClose.Click += (s, ev) => form.Close();

                // Thêm các control vào form
                form.Controls.AddRange(new Control[] {
            gridCategories, txtCategoryName, btnAdd, btnEdit, btnDelete, btnClose
        });

                form.ShowDialog(this);
            }
        }

        private void btnCompleted_Click(object sender, EventArgs e)
        {
            _filter = "Completed";
            reloadData();
        }

        private void btnPending_Click(object sender, EventArgs e)
        {
            _filter = "Pending";
            reloadData();
        }
        private void btnToday_Click(object sender, EventArgs e)
        {
            _filter = "Today";
            reloadData();
        }
        private void Grid_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var db = DbContext.GetInstance();
                var id = (int)grid.Rows[e.RowIndex].Cells["id"].Value;
                var task = db.SingleById<TodoItem>(id);

                if (task != null)
                {
                    try
                    {
                        switch (grid.Columns[e.ColumnIndex].Name)
                        {
                            case "Description":
                                task.Description = grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();
                                break;
                            case "Category":
                                var categoryName = grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString();
                                task.CategoryId = db.Single<Category>(c => c.CategoryName == categoryName).id;
                                break;
                            case "StartDate":
                                if (DateTime.TryParse(grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString(), out DateTime startDate))
                                    task.StartDate = startDate;
                                else
                                    ShowNotification("Invalid start date format!", MessageDialogIcon.Warning);
                                break;
                            case "EndDate":
                                if (DateTime.TryParse(grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value?.ToString(), out DateTime endDate))
                                    task.EndDate = endDate;
                                else
                                    ShowNotification("Invalid end date format!", MessageDialogIcon.Warning);
                                break;
                            case "Status":
                                task.Done = (bool)grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                                break;
                        }
                        db.Update(task);
                        this.BeginInvoke(new Action(() => reloadData()));
                    }
                    catch (Exception ex)
                    {
                        ShowNotification($"Error updating task: {ex.Message}", MessageDialogIcon.Error);
                    }
                }
            }
        }

        private void guna2GradientTileButton1_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button5_Click(object sender, EventArgs e)
        {

        }

        private void btn_category_Click(object sender, EventArgs e)
        {

        }

        private void txtBoxSearch_TextChanged(object sender, EventArgs e)
        {
            // Update the search term whenever the text changes
            _searchTerm = txtBoxSearch.Text.Trim().ToLower();

            // If you want to search as the user types (for immediate feedback)
            // Uncomment the line below:
            // PerformSearch();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            // Perform search when the button is clicked
            PerformSearch();
        }
        private void PerformSearch()
        {
            // Reload data with the current filters and search term
            reloadData();
        }

        private void btnToday_Click_1(object sender, EventArgs e)
        {
            _filter = "Today";
            reloadData();
        }
    }
}
