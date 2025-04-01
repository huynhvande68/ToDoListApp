using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.DataAnnotations;
using ServiceStack.OrmLite;

namespace ToDoListApp.DataAccess.Models
{
    [Alias("categories")]
    internal class Category
    {
        [PrimaryKey]
        [AutoIncrement]
        public int id { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        public static Category GetCategoryByName(string name)
        {
            return DbContext.GetInstance()
                .Single<Category>(r => r.CategoryName.ToLower().Trim() == name.ToLower().Trim());
        }
         
    }
}
