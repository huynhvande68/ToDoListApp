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

namespace ToDoListApp
{
    public partial class Form1 : Form
    {
        private Guna2Button selectedButton;
        private DateTime _date = DateTime.Today;
        private string _category = "All";

        public Form1()
        {
            InitializeComponent();
            LoadCategories();
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

            //MessageBox.Show($"Bạn đã chọn danh mục: {categoryName}");
        }
        void reloadData()
        {
            var db = DbContext.GetInstance();
            var data = db.Select<TodoItem>(r => r.StartDate >= this._date && r.EndDate <= this._date);

            if (this._category != "All")
                data = data.Where(r =>
                r.CategoryId == Category.GetCategoryByName(this._category).id).ToList();

            
            // Gán dữ liệu vào Guna2DataGridView
            grid.DataSource = null;
            grid.DataSource = data;

            // Sự kiện chỉnh sửa (giả lập OnEdit từ Bunifu)
            //grid.CellEndEdit += (sender, e) =>
            //{
            //    var row = grid.Rows[e.RowIndex];
            //    if (row.DataBoundItem is TodoItem item)
            //    {
            //        db.Save(item);
            //        ShowNotification("Saved successfully!", MessageDialogIcon.Information);
            //    }
            //};

            
        }

        private void guna2DateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
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


        private void bunifuSeparator1_Click(object sender, EventArgs e)
        {

        }

        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void btn_category_Click(object sender, EventArgs e)
        {

        }   

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            reloadData();
        }

        private void guna2DataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
