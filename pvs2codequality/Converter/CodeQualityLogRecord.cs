namespace Pvs2codequality.Converter
{
    /// <summary>
    /// 
    /// </summary>
    /// https://docs.gitlab.com/ee/user/project/merge_requests/code_quality.html#implementing-a-custom-tool
    public class CodeQualityLogRecord
    {
        public string? description { get; set; }
        public string? fingerprint { get; set; }
        public Location? location { get; set; }

        public class Location
        {
            public string? path { get; set; }
            public LocationLines? lines { get; set; }
        }

        public class LocationLines
        {
            public int begin { get; set; }
        }
    }
}