using System.IO;
using CommandLine;
using Pvs2codequality.Converter;

namespace Pvs2codequality
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            Parser
                .Default
                .ParseArguments<Options>(args)
                .WithParsed(RunOptionsAndReturnExitCode);
        }

        public static void RunOptionsAndReturnExitCode(object rawOptions)
        {
            var options = (Options) rawOptions;
            var outputFilename = options.OutputFile;
            if (outputFilename == null)
            {
                var i = options.InputFile.LastIndexOf('.');
                outputFilename = options.InputFile.Remove(i) + ".json";
            }

            var inputXML = File.ReadAllText(options.InputFile);
            var outputJson = XMLConverter.ParseFullDocument(inputXML);
            File.WriteAllText(outputFilename, outputJson.result!);
        }
    }
}