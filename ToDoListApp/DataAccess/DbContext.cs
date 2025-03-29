using System;
using System.Collections.Generic;
using System.Data;
using ServiceStack.OrmLite;
using ToDoListApp.DataAccess.Models;


namespace ToDoListApp.DataAccess
{
    internal class DbContext
    {
        private static IDbConnection _db;
        public static Exception Exception;

        public static IDbConnection GetInstance()
        {
            var dbFactory = new OrmLiteConnectionFactory(
                "Data Source=Database/todolist.db;Version=3;",
                SqliteDialect.Provider);

            try
            {
                if (_db == null)
                {
                    _db = dbFactory.Open();
                    Migrate();
                }

                if (_db.State == ConnectionState.Broken || _db.State == ConnectionState.Closed)
                    _db = dbFactory.Open();

                return _db;
            }
            catch (Exception err)
            {
                Exception = err;
                return null;
            }
        }

        private static void Migrate()
        {
            var db = GetInstance();

            if (db.CreateTableIfNotExists<Category>())
            {
                db.Save(new Category() { CategoryName = "Personal" });
                db.Save(new Category() { CategoryName = "Shopping" });
                db.Save(new Category() { CategoryName = "Work" });
                db.Save(new Category() { CategoryName = "Errands" });
                db.Save(new Category() { CategoryName = "Projects" });
               
            }
            if (db.CreateTableIfNotExists<TodoItem>())
            {
                var todoItems = new List<TodoItem>
                    {
                        new TodoItem { CategoryId = 1, Description = "Check the todo item to mark it complete" },
                        new TodoItem { CategoryId = 2, Description = "Filter using the calendar date" },
                        new TodoItem { CategoryId = 3, Description = "Group tasks using categories" }
                    };
                db.SaveAll(todoItems);
            }
        }
    }
}
