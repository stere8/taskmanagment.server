using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManagment.Server.Models;
using Task = TaskManagment.Server.Models.Task;

namespace TaskManagment.Server.Data
{
    public class TaskManagmentServerContext : DbContext
    {
        public TaskManagmentServerContext (DbContextOptions<TaskManagmentServerContext> options)
            : base(options)
        {
        }

        public DbSet<Task> Tasks { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
