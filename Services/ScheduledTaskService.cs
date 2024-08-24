using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

public class ScheduledTaskService : BackgroundService
{
    private readonly ILogger<ScheduledTaskService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly HttpClient _httpClient;

    public ScheduledTaskService(ILogger<ScheduledTaskService> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _httpClient = new HttpClient();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessTasksAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing tasks.");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Check every 30 seconds
        }
    }

    private async Task ProcessTasksAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        _logger.LogInformation($"[{DateTime.UtcNow}] Checking for tasks to process..");

        try
        {
            var tasks = await dbContext.ScheduledTasks
                                       .Where(t => t.IsActive && t.ScheduleDateTime <= DateTime.UtcNow)
                                       .ToListAsync(cancellationToken);

            foreach (var task in tasks)
            {
                if (ShouldExecuteTask(task))
                {
                    _logger.LogInformation($"Executing task '{task.Name}' at {DateTime.UtcNow} [{task.TaskGuid}]");

                    task.RetryCount = 0; //Reset the retry count
                    var result = await ExecuteHttpRequestWithRetriesAsync(task);

                    // Update task with the result
                    task.LastResponse = result.Content;
                    task.LastResponseCode = result.StatusCode;
                    task.LastExecutionTime = DateTime.UtcNow;

                    // Update the next execution time based on recurrence
                    if (task.RecurrenceType != "None")
                    {
                        task.NextExecutionTime = CalculateNextExecutionTime(task);
                        task.ScheduleDateTime = task.NextExecutionTime ?? DateTime.UtcNow.AddYears(1);
                    }
                    else
                    {
                        task.IsActive = false; // Mark as inactive if no recurrence
                    }

                    await dbContext.SaveChangesAsync(cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching tasks from the database.");
            throw; // Re-throw to let the background service handle it
        }
    }

    private async Task<(string Content, int StatusCode)> ExecuteHttpRequestWithRetriesAsync(ScheduledTask task)
    {
        int currentRetryCount = 0;

        while (currentRetryCount <= task.RetryMaximum)
        {
            try
            {
                var request = new HttpRequestMessage(new HttpMethod(task.HttpMethod), task.Url);

                // Add headers
                if (task.Headers != null)
                {
                    foreach (var header in task.Headers)
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }

                // Add payload if it's a POST/PUT request
                if (task.HttpMethod == "POST" || task.HttpMethod == "PUT")
                {
                    request.Content = new StringContent(task.Payload ?? "", System.Text.Encoding.UTF8, "application/json");
                }

                // Send the request
                var response = await _httpClient.SendAsync(request);

                return (await response.Content.ReadAsStringAsync(), (int)response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Attempt {currentRetryCount + 1} failed for task '{task.Name}'. Retrying in {task.RetryInterval} seconds...");
                currentRetryCount++;
                
                // If we've reached the maximum retries, return an error response
                if (currentRetryCount > task.RetryMaximum)
                {
                    _logger.LogError($"Task '{task.Name}' failed after {task.RetryMaximum} attempts.");
                    return ($"Error: {ex.Message}", 500);
                }

                task.RetryCount = currentRetryCount;
                await Task.Delay(TimeSpan.FromSeconds(task.RetryInterval));
            }
        }

        return ("Unexpected error", 500); // Should not reach here
    }

    private bool ShouldExecuteTask(ScheduledTask task)
    {
        return !task.NextExecutionTime.HasValue || task.NextExecutionTime.Value <= DateTime.UtcNow;
    }

    private DateTime? CalculateNextExecutionTime(ScheduledTask task)
    {
        return task.RecurrenceType switch
        {
            "Minute" => DateTime.UtcNow.AddMinutes(task.RecurrenceInterval),
            "Hour" => DateTime.UtcNow.AddHours(task.RecurrenceInterval),
            "Day" => DateTime.UtcNow.AddDays(task.RecurrenceInterval),
            "Month" => DateTime.UtcNow.AddMonths(task.RecurrenceInterval),
            "Year" => DateTime.UtcNow.AddYears(task.RecurrenceInterval),
            _ => null
        };
    }
}
