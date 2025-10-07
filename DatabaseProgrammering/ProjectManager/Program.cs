using ProjectManager.Models;

namespace ProjectManager
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            seedTasks();

            Application.Run(new MainForm());
        }

        private static void seedTasks()
        {
            DBContext db = new DBContext();
            if (!db.Tasks.Any())
            {
                Models.Task produceSoftware = new Models.Task();
                produceSoftware.Name = "Produce Software";
                produceSoftware.Todos = new List<Todo>();
                produceSoftware.Todos.Add(new Todo() { Name = "Write code" });
                produceSoftware.Todos.Add(new Todo() { Name = "Compile source" });
                produceSoftware.Todos.Add(new Todo() { Name = "Test Program" });

                Models.Task brewCoffee = new Models.Task();
                brewCoffee.Name = "Brew Coffee";
                brewCoffee.Todos = new List<Todo>();
                brewCoffee.Todos.Add(new Todo() { Name = "Pour water" });
                brewCoffee.Todos.Add(new Todo() { Name = "Pour coffee" });
                brewCoffee.Todos.Add(new Todo() { Name = "Turn on" });

                db.Tasks.Add(produceSoftware);
                db.Tasks.Add(brewCoffee);

                db.SaveChanges();
            }
        }
    }
}