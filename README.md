# flowt
API for scheduling and executing HTTP requests.


Create the table
```
CREATE TABLE ScheduledTasks (
    TaskGuid CHAR(36) PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    IsActive BOOLEAN NOT NULL,
    Url TEXT NOT NULL,
    HttpMethod VARCHAR(10) NOT NULL,
    Payload TEXT NULL,
    Headers JSON NULL,
    Timeout INT DEFAULT 60,
    RetryMaximum INT DEFAULT 3,
    RetryCount INT DEFAULT 0,
    RetryInterval INT DEFAULT 5,
    LastResponse TEXT NULL,
    LastResponseCode INT NULL,
    ScheduleDateTime DATETIME NOT NULL,
    LastExecutionTime DATETIME NULL,
    Status VARCHAR(50) NULL,
    RecurrenceType VARCHAR(10) NOT NULL DEFAULT 'None',
    RecurrenceInterval INT DEFAULT 0,
    NextExecutionTime DATETIME NULL
);
```


Docker image
```
v3xl/flowt
```
