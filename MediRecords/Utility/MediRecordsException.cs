using System;

namespace MediRecords.Utility;

public class MediRecordsException : Exception
{
    public MediRecordsException(string errMsg) : base(errMsg)
    {
        
    }

    public MediRecordsException(string? message, Exception? innerException) : base(message, innerException)
    {
        
    }
}
