using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;

namespace ToDoListApp.DataAccess.Models
{
    [Alias("todo_list")]
    internal class TodoItem
    {
        [PrimaryKey]
        [AutoIncrement]
        public int id { get; set; }

        public string Description { get; set; } = string.Empty;

        [References(typeof(Category))]
        public int CategoryId { get; set; }

        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(1); // Make it expire tomorrow by default

        public bool Done { get; set; } = false;

        public Category GetCategory() => DbContext.GetInstance().SingleById<Category>(this.CategoryId);
    }
}
