using System.ComponentModel;
using MCP.Shared.Models;
using ModelContextProtocol.Server;

namespace MCP.Server.Tools;

/// <summary>
/// MCP Tool providing todo/task management functionality.
/// This demonstrates a stateful tool that maintains data across calls.
/// </summary>
[McpServerToolType]
public static class TodoTool
{
    private static readonly List<TodoItem> _todos = [];
    private static readonly object _lock = new();

    /// <summary>
    /// Creates a new todo item.
    /// </summary>
    [McpServerTool, Description("Creates a new todo item with the specified title, description, and priority.")]
    public static TodoItem CreateTodo(
        [Description("The title of the todo item")] string title,
        [Description("Optional description for the todo item")] string description = "",
        [Description("Priority level: 'Low', 'Medium', or 'High'. Defaults to 'Medium'.")] string priority = "Medium")
    {
        var validPriorities = new[] { "Low", "Medium", "High" };
        priority = validPriorities.Contains(priority, StringComparer.OrdinalIgnoreCase) 
            ? priority 
            : "Medium";

        var todo = new TodoItem
        {
            Title = title,
            Description = description,
            Priority = priority,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        lock (_lock)
        {
            _todos.Add(todo);
        }

        return todo;
    }

    /// <summary>
    /// Gets all todo items, optionally filtered by completion status.
    /// </summary>
    [McpServerTool, Description("Gets all todo items. Can optionally filter by completion status or priority.")]
    public static List<TodoItem> GetTodos(
        [Description("Filter by completion status: 'all', 'completed', or 'pending'. Defaults to 'all'.")] string filter = "all",
        [Description("Filter by priority: 'Low', 'Medium', 'High', or 'all'. Defaults to 'all'.")] string priority = "all")
    {
        lock (_lock)
        {
            var result = _todos.AsEnumerable();

            result = filter.ToLowerInvariant() switch
            {
                "completed" => result.Where(t => t.IsCompleted),
                "pending" => result.Where(t => !t.IsCompleted),
                _ => result
            };

            if (priority.ToLowerInvariant() != "all")
            {
                result = result.Where(t => t.Priority.Equals(priority, StringComparison.OrdinalIgnoreCase));
            }

            return [.. result.OrderByDescending(t => t.CreatedAt)];
        }
    }

    /// <summary>
    /// Marks a todo item as completed.
    /// </summary>
    [McpServerTool, Description("Marks a todo item as completed by its ID.")]
    public static TodoItem? CompleteTodo(
        [Description("The ID of the todo item to mark as completed")] string id)
    {
        lock (_lock)
        {
            var todo = _todos.FirstOrDefault(t => t.Id == id);
            if (todo != null)
            {
                todo.IsCompleted = true;
                todo.CompletedAt = DateTime.UtcNow;
            }
            return todo;
        }
    }

    /// <summary>
    /// Updates a todo item.
    /// </summary>
    [McpServerTool, Description("Updates an existing todo item's title, description, or priority.")]
    public static TodoItem? UpdateTodo(
        [Description("The ID of the todo item to update")] string id,
        [Description("New title (leave empty to keep current)")] string? title = null,
        [Description("New description (leave empty to keep current)")] string? description = null,
        [Description("New priority: 'Low', 'Medium', or 'High' (leave empty to keep current)")] string? priority = null)
    {
        lock (_lock)
        {
            var todo = _todos.FirstOrDefault(t => t.Id == id);
            if (todo != null)
            {
                if (!string.IsNullOrWhiteSpace(title))
                    todo.Title = title;
                if (description != null)
                    todo.Description = description;
                if (!string.IsNullOrWhiteSpace(priority))
                {
                    var validPriorities = new[] { "Low", "Medium", "High" };
                    if (validPriorities.Contains(priority, StringComparer.OrdinalIgnoreCase))
                        todo.Priority = priority;
                }
            }
            return todo;
        }
    }

    /// <summary>
    /// Deletes a todo item.
    /// </summary>
    [McpServerTool, Description("Deletes a todo item by its ID.")]
    public static bool DeleteTodo(
        [Description("The ID of the todo item to delete")] string id)
    {
        lock (_lock)
        {
            var todo = _todos.FirstOrDefault(t => t.Id == id);
            if (todo != null)
            {
                _todos.Remove(todo);
                return true;
            }
            return false;
        }
    }

    /// <summary>
    /// Gets summary statistics about the todo list.
    /// </summary>
    [McpServerTool, Description("Gets summary statistics about the todo list including total count, completed count, and breakdown by priority.")]
    public static object GetTodoStats()
    {
        lock (_lock)
        {
            return new
            {
                Total = _todos.Count,
                Completed = _todos.Count(t => t.IsCompleted),
                Pending = _todos.Count(t => !t.IsCompleted),
                ByPriority = new
                {
                    High = _todos.Count(t => t.Priority == "High" && !t.IsCompleted),
                    Medium = _todos.Count(t => t.Priority == "Medium" && !t.IsCompleted),
                    Low = _todos.Count(t => t.Priority == "Low" && !t.IsCompleted)
                }
            };
        }
    }

    /// <summary>
    /// Clears all completed todos.
    /// </summary>
    [McpServerTool, Description("Removes all completed todo items from the list.")]
    public static int ClearCompleted()
    {
        lock (_lock)
        {
            var completedCount = _todos.RemoveAll(t => t.IsCompleted);
            return completedCount;
        }
    }
}
