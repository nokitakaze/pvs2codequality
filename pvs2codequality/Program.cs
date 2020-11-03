using System;
using System.IO;
using CommandLine;
using Pvs2codequality.Converter;

namespace Pvs2codequality
{
    internal static class Program
    {
        static int Main(string[] args)
        {
            int returnCode = 0;
            Parser
                .Default
                .ParseArguments<Options>(args)
                .WithParsed(o => { returnCode = RunOptionsAndReturnExitCode(o); });

            return returnCode;
        }

        public static int RunOptionsAndReturnExitCode(Options options)
        {
            var outputFilename = options.OutputFile;
            if (outputFilename == null)
            {
                var i = options.InputFile.LastIndexOf('.');
                outputFilename = options.InputFile.Remove(i) + ".json";
            }

            if (!File.Exists(options.InputFile))
            {
                Console.WriteLine(
                    "File {0} does not exist",
                    outputFilename
                );

                return 100;
            }

            var inputXML = File.ReadAllText(options.InputFile);
            var outputJson = XMLConverter.ParseFullDocument(
                inputXML,
                trimFolderName: null,
                reportFilenamePrefix: options.ReportFilenamePrefix
            );
            File.WriteAllText(outputFilename, outputJson.result!);
            Console.WriteLine(
                "File {0} created. {1} lines found",
                outputFilename,
                outputJson.linesFound
            );

            return outputJson.status;
        }
    }
}