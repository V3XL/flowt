using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.None);

// Add services to the container
builder.Services.AddDbContext<ApplicationDbContext>();
builder.Services.AddHostedService<ScheduledTaskService>();

var app = builder.Build();

// Configure global error handling
app.UseExceptionHandler(options =>
{
    options.Run(async context =>
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = 500; // Internal Server Error
        await context.Response.WriteAsync(JsonConvert.SerializeObject(new
        {
            Error = "An error occurred while processing your request."
        }));
    });
});

app.MapGet("/", () => "");

// Create a new task
app.MapPost("/tasks", async (ScheduledTask task, ApplicationDbContext db) =>
{
    db.ScheduledTasks.Add(task);
    await db.SaveChangesAsync();
    return Results.Created($"/tasks/{task.TaskGuid}", task);
});

// Get all tasks
app.MapGet("/tasks", async (ApplicationDbContext db) =>
    await db.ScheduledTasks.ToListAsync());

// Get a task by TaskGuid
app.MapGet("/tasks/{taskGuid}", async (Guid taskGuid, ApplicationDbContext db) =>
{
    var task = await db.ScheduledTasks.FindAsync(taskGuid);
    return task is not null ? Results.Ok(task) : Results.NotFound();
});

// Update a task by TaskGuid
// Update a task by TaskGuid
app.MapPut("/tasks/{taskGuid}", async (Guid taskGuid, ScheduledTask updatedTask, ApplicationDbContext db) =>
{
    var task = await db.ScheduledTasks.FindAsync(taskGuid);
    if (task is null) return Results.NotFound();

    // Update only the provided fields
    if (updatedTask.Name != null) task.Name = updatedTask.Name;
    if (updatedTask.Url != null) task.Url = updatedTask.Url;
    if (updatedTask.HttpMethod != null) task.HttpMethod = updatedTask.HttpMethod;
    if (updatedTask.Payload != null) task.Payload = updatedTask.Payload;
    if (updatedTask.Headers != null) task.Headers = updatedTask.Headers;
    if (updatedTask.ScheduleDateTime != DateTime.MinValue) task.ScheduleDateTime = updatedTask.ScheduleDateTime;
    if (updatedTask.IsActive != task.IsActive) task.IsActive = updatedTask.IsActive;
    if (updatedTask.RecurrenceType != null) task.RecurrenceType = updatedTask.RecurrenceType;
    if (updatedTask.RecurrenceInterval != null) task.RecurrenceInterval = updatedTask.RecurrenceInterval;
    if (updatedTask.NextExecutionTime != DateTime.MinValue) task.NextExecutionTime = updatedTask.NextExecutionTime;

    await db.SaveChangesAsync();
    return Results.Ok(task);
});


// Delete a task by TaskGuid
app.MapDelete("/tasks/{taskGuid}", async (Guid taskGuid, ApplicationDbContext db) =>
{
    var task = await db.ScheduledTasks.FindAsync(taskGuid);
    if (task is null) return Results.NotFound();

    db.ScheduledTasks.Remove(task);
    await db.SaveChangesAsync();
    return Results.NoContent();
});
app.Urls.Add("http://*:80");
app.Run();
