using LogAnalyzer.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogAnalyzer.Util {
    public class FileHelper {
        public static IEnumerable<LineBlock> SegmentText(RegexFilter start, RegexFilter end, List<LogLine> allLines) {
            List<LogLine> toReturn = new List<LogLine>();
            bool started = false;
            int lineNumber = 0;
            foreach (var l in allLines) {
                if (started) {
                    toReturn.Add(l);
                }
                if (start.Val(l) as bool? == true) {
                    started = true;
                }
                if (end.Val(l) as bool? == true) {
                    yield return new LineBlock() {
                        Lines = toReturn,
                        EndLineNumber = lineNumber
                    };
                    toReturn = new List<LogLine>();
                    started = false;
                }
                lineNumber++;

            }
        }


        public static string GetFilename(int abosluteLineNumber, List<int> lineCountsPerFile, List<string> fileNames) {
            int currentSum = 0;
            for (int i = 0; i < lineCountsPerFile.Count(); i++) {
                int inspectionSum = currentSum + lineCountsPerFile[i];
                if (inspectionSum > abosluteLineNumber) {
                    return fileNames[i] + " line: " + (abosluteLineNumber - currentSum); ;
                }
                currentSum += inspectionSum;
            }
            throw new Exception("Line number too big");
        }

        public static List<LogLine> ReadAllLines(string path) {
            if (!System.IO.File.Exists(path)) {
                return new List<LogLine>();
            }
            return System.IO.File.ReadAllLines(path).Select(i => new LogLine(i)).ToList();
        }
    }
}
