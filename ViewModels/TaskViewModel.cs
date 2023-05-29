using System.Collections.ObjectModel;
using System.Windows.Input;
using Prism.Commands;
using TooDo.Models;
using System.Data.SQLite;
using Dapper;

namespace TooDo.ViewModels
{
    public class TaskViewModel
    {
        public ObservableCollection<Task> Tasks { get; set; }
        public Task SelectedTask { get; set; }
        public string NewTaskTitle { get; set; }

        public ICommand AddTaskCommand { get; set; }
        public ICommand CompleteTaskCommand { get; set; }
        public ICommand DeleteTaskCommand { get; set; }

        private readonly string connectionString = "Data Source=TooDo.db;Version=3;";

        public TaskViewModel()
        {
            InitializeDatabase();
            LoadTasksFromDatabase();

            AddTaskCommand = new DelegateCommand(AddTask);
            CompleteTaskCommand = new DelegateCommand(CompleteTask);
            DeleteTaskCommand = new DelegateCommand(DeleteTask);
        }

        private void InitializeDatabase()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                connection.Execute(@"CREATE TABLE IF NOT EXISTS Tasks (
                                        Title TEXT,
                                        IsCompleted INTEGER)");
            }
        }

        private void LoadTasksFromDatabase()
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                Tasks = new ObservableCollection<Task>(connection.Query<Task>("SELECT * FROM Tasks"));
            }
        }

        private void AddTask()
        {
            if (!string.IsNullOrEmpty(NewTaskTitle))
            {
                var task = new Task { Title = NewTaskTitle };
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    connection.Execute("INSERT INTO Tasks (Title, IsCompleted) VALUES (@Title, @IsCompleted)", task);
                }
                Tasks.Add(task);
                NewTaskTitle = string.Empty;
            }
        }

        private void CompleteTask()
        {
            if (SelectedTask != null)
            {
                SelectedTask.IsCompleted = true;
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    connection.Execute("UPDATE Tasks SET IsCompleted = 1 WHERE Title = @Title", SelectedTask);
                }
            }
        }

        private void DeleteTask()
        {
            if (SelectedTask != null)
            {
                using (var connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    connection.Execute("DELETE FROM Tasks WHERE Title = @Title", SelectedTask);
                }
                Tasks.Remove(SelectedTask);
                SelectedTask = null;
            }
        }
    }
}
