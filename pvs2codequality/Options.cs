using CommandLine;

namespace Pvs2codequality
{
    public class Options
    {
        [Option('i', "input", Required = true, HelpText = "Input plog file")]
        public string InputFile { get; set; }

        [Option('o', "output", Required = false, HelpText = "Output json file")]
        public string? OutputFile { get; set; }
    }
}