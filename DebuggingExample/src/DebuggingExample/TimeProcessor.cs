using System;

namespace DebuggingExample
{
    public class TimeProcessor : ITimeProcessor
    {
        public DateTime CurrentTimeUTC()
        {
            return DateTime.UtcNow;
        }
    }
} 