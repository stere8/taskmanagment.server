using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TaskManagment.Server.Data;
using TaskManagment.Server.DTOs;
using TaskManagment.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TaskManagment.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly TaskManagmentServerContext _context;
        private readonly ILogger<TasksController> _logger;

        public TasksController(TaskManagmentServerContext context, ILogger<TasksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Models.Task>>> GetTasks()
        {
            try
            {
                _logger.LogInformation("Retrieving all tasks...");
                var tasks = await _context.Tasks.ToListAsync();
                _logger.LogInformation("Successfully retrieved all tasks.");
                return tasks;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve all tasks.");
                return StatusCode(500, "An error occurred while retrieving all tasks.");
            }
        }

        // GET: api/Tasks/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Models.Task>> GetTask(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving task with ID {TaskId}...", id);
                var task = await _context.Tasks.FindAsync(id);

                if (task == null)
                {
                    _logger.LogWarning("Task with ID {TaskId} not found.", id);
                    return NotFound();
                }

                _logger.LogInformation("Successfully retrieved task with ID {TaskId}.", id);
                return task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve task with ID {TaskId}.", id);
                return StatusCode(500, "An error occurred while retrieving task.");
            }
        }

        // PUT: api/Tasks/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTask(int id, Models.Task task)
        {
            try
            {
                _logger.LogInformation("Updating task with ID {TaskId}...", id);

                if (id != task.Id)
                {
                    _logger.LogWarning("Invalid request: Provided ID ({ProvidedId}) does not match task ID.", id);
                    return BadRequest("Invalid ID provided.");
                }

                _context.Entry(task).State = EntityState.Modified;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Task with ID {TaskId} updated successfully.", id);
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaskExists(id))
                {
                    _logger.LogWarning("Task with ID {TaskId} not found.", id);
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating task with ID {TaskId}.", id);
                return StatusCode(500, "An error occurred while updating task.");
            }
        }

        // POST: api/Tasks
        [HttpPost]
        public async Task<ActionResult<Models.Task>> PostTask(AddTaskDto addTaskDto)
        {
            try
            {
                _logger.LogInformation("Creating a new task...");

                // Map the DTO to the Task model
                var task = new Models.Task
                {
                    Title = addTaskDto.Title,
                    Description = addTaskDto.Description,
                    DueDate = addTaskDto.DueDate,
                    Completed = addTaskDto.Completed,
                    UserId = addTaskDto.UserId
                };

                // Add the task to the context and save changes
                _context.Tasks.Add(task);
                await _context.SaveChangesAsync();

                // Return the created task with the generated ID
                _logger.LogInformation("Task created successfully.");
                return CreatedAtAction("GetTask", new { id = task.Id }, task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the task.");
                return StatusCode(500, "An error occurred while creating task.");
            }
        }

        // DELETE: api/Tasks/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                _logger.LogInformation("Deleting task with ID {TaskId}...", id);

                var task = await _context.Tasks.FindAsync(id);
                if (task == null)
                {
                    _logger.LogWarning("Task with ID {TaskId} not found.", id);
                    return NotFound();
                }

                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Task with ID {TaskId} deleted successfully.", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting task with ID {TaskId}.", id);
                return StatusCode(500, "An error occurred while deleting task.");
            }
        }

        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
    }
}
