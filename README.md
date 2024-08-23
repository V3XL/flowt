# flowt
Schedule tasks


```
CREATE TABLE ScheduledTasks (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Url TEXT NOT NULL,
    HttpMethod VARCHAR(10) NOT NULL,
    Payload TEXT,
    ScheduleTime DATETIME NOT NULL,
    IsActive BOOLEAN NOT NULL,
    RetryCount INT DEFAULT 0,
    RetryInterval INT DEFAULT 0,
    Timeout INT DEFAULT 30,
    LastExecutionTime DATETIME,
    Status VARCHAR(50),
    LastErrorMessage TEXT,
    LastResponse TEXT,
    LastResponseCode INT,
    Priority INT DEFAULT 0,
    TaskGuid CHAR(36) NOT NULL,
    Headers JSON
);
```
