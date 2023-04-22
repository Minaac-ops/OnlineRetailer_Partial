using System.Runtime.CompilerServices;
using Serilog;

namespace Monitoring
{
    public static class LoggerExtensions
    {
        public static ILogger Here(this ILogger logger,
            [CallerMemberName] string memberName = "", // Automatic inserts the name of the class that calls the method
            [CallerFilePath] string sourceFilePath = "", // Filepath is the path to the file containing the class
            [CallerLineNumber] int sourceLineNumber = 0) // The linenumber that was executed
        {
            return logger
                .ForContext("MemberName", memberName)
                .ForContext("FilePath", sourceFilePath)
                .ForContext("LineNumber", sourceLineNumber); // Embedded into the serilogs
        }
    }
}