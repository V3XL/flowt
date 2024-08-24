using System;
using System.Collections.Generic;

public class ScheduledTask
{
    //Basic details
    public Guid TaskGuid { get; private set; } // Default to new GUID
    public string Name { get; set; } // Should not be null
    public bool IsActive { get; set; }

    // Request specific
    public string Url { get; set; } // Should not be null
    public string HttpMethod { get; set; } // Should not be null
    public string? Payload { get; set; } // Nullable
    public Dictionary<string, string>? Headers { get; set; } // Nullable
    public int Timeout { get; set; } = 60; // Default to 60 seconds
    public int RetryMaximum { get; set; } = 3; // Default to 3 retries
    public int RetryCount { get; set; } // The current retry count for a task
    public int RetryInterval { get; set; } = 5; // Time in seconds to wait between retries. Defaults to 5 seconds.
    public string? LastResponse { get; set; } // The last response received from the request
    public int? LastResponseCode { get; set; } // The last response code received from the request

    // Scheduling
    public DateTime ScheduleDateTime { get; set; } // The date and time in which this task will next be triggered
    public DateTime? LastExecutionTime { get; set; } // Nullable
    public string? Status { get; set; } // Current status of this task
    public string RecurrenceType { get; set; }
    public int RecurrenceInterval { get; set; }
    public DateTime? NextExecutionTime { get; set; } // Nullable


    public ScheduledTask()
    {
        TaskGuid = Guid.NewGuid(); // Generate new GUID
    }
}