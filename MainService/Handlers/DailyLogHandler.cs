using MainService.Interfaces;
using MainService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MainService.Handlers
{
    public class DailyLogHandler : IDailyLogHandler<DailyLog>
    {
        public void Handle(DailyLog dailyLog, string fileName)
        {
            int parsedFilesCount = dailyLog.ParsedFiles;
            int parsedLinesCount = dailyLog.ParsedLines;
            int invalidFilesCount = dailyLog.InvalidLines;
            IList<string> invalidFilePaths = dailyLog.InvalidFileNames;
            if (!invalidFilePaths.Any() || parsedLinesCount == 0)
            {
                return;
            }
            if (Directory.Exists(fileName))
            {
                Directory.CreateDirectory(fileName);
            }
            using (StreamWriter writer = File.CreateText(fileName))
            {
                writer.WriteLine($"parsed_files: {parsedFilesCount}");
                writer.WriteLine($"parsed_lines: {parsedLinesCount}");
                writer.WriteLine($"found_errors: {invalidFilesCount}");
                writer.WriteLine($"invalid_files: [{string.Join(", ", invalidFilePaths)}]");
            }
        }
    }
}
