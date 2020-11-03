using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace Pvs2codequality
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
#pragma warning disable 8618
    public class Options
    {
        [Option('i', "input", Required = true, HelpText = "Input plog file")]
        public string InputFile { get; set; }

        [Option('o', "output", Required = false, HelpText = "Output json file")]
        public string? OutputFile { get; set; }

        [Option("report-filename-prefix", Required = false, Default = null,
            HelpText = "Prefixes for filename in output report. If null or empty, then no prefixes attach")]
        public string? ReportFilenamePrefix { get; set; }

        // TODO TrimFolderName
    }
#pragma warning restore 8618
}