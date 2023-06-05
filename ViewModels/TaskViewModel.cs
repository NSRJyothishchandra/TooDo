using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Prism.Commands;
using TooDo.Models;
using System.Data.SQLite;
using Dapper;
using System.Linq;

namespace TooDo.ViewModels
{
    public class TaskViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Task> Tasks { get; set; }
        public ObservableCollection<Task> CompletedTasks { get; set; }

        private Task selectedTask;
        public Task SelectedTask
        {
            get { return selectedTask; }
            set
            {
                if (selectedTask != value)
                {
                    selectedTask = value;
                    RaisePropertyChanged(nameof(SelectedTask));
                }
            }
        }

        private string newTaskTitle;
        public string NewTaskTitle
        {
            get { return newTaskTitle; }
            set
            {
                if (newTaskTitle != value)
                {
                    newTaskTitle = value;
                    RaisePropertyChanged(nameof(NewTaskTitle));
                }
            }
        }

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

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
                var tasks = connection.Query<Task>("SELECT * FROM Tasks").ToList();
                Tasks = new ObservableCollection<Task>(tasks);
                CompletedTasks = new ObservableCollection<Task>(tasks.Where(t => t.IsCompleted));
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
                RaisePropertyChanged(nameof(SelectedTask));

                if (Tasks.Contains(SelectedTask))
                {
                    Tasks.Remove(SelectedTask);
                    CompletedTasks.Add(SelectedTask);
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

                if (Tasks.Contains(SelectedTask))
                {
                    Tasks.Remove(SelectedTask);
                }
                else if (CompletedTasks.Contains(SelectedTask))
                {
                    CompletedTasks.Remove(SelectedTask);
                }

                SelectedTask = null;
            }
        }
    }
}
