using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace Pvs2codequality.Converter
{
    internal static class XMLConverter
    {
        public static (int status, string? result, int linesFound) ParseFullDocument(
            string inputXML,
            string? trimFolderName = null
        )
        {
            var xDoc = new XmlDocument();
            xDoc.LoadXml(inputXML);

            if (trimFolderName == null)
            {
                var SolutionPathNode = xDoc
                    .DocumentElement
                    .ChildNodes
                    .OfType<XmlElement>()
                    .First(x => x.Name == "Solution_Path");
                foreach (var t in SolutionPathNode.GetElementsByTagName("SolutionPath").OfType<XmlNode>())
                {
                    if (t.InnerText != null)
                    {
                        var i = t.InnerText.LastIndexOfAny(new[] {'/', '\\'});
                        trimFolderName = t.InnerText.Remove(i);
                        break;
                    }
                }

                if (trimFolderName == null)
                {
                    return (1, null, -1);
                }
            }

            var logRecords = xDoc
                .DocumentElement
                .ChildNodes
                .OfType<XmlElement>()
                .Where(x => x.Name == "PVS-Studio_Analysis_Log")
                .ToArray();

            var result = ParseAllLogNodes(logRecords, trimFolderName);

            return (0, System.Text.Json.JsonSerializer.Serialize(result), result.Count);
        }

        public static ICollection<CodeQualityLogRecord> ParseAllLogNodes(
            ICollection<XmlElement> logRecords,
            string trimFolderName
        )
        {
            return logRecords
                .SelectMany(t => ParseAllLogNodes(t, trimFolderName))
                .ToArray();
        }

        public static ICollection<CodeQualityLogRecord> ParseAllLogNodes(
            XmlElement logRecord,
            string trimFolderName
        )
        {
            var fields = new[]
                {"ErrorCode", "Message", "Line", "FalseAlarm", "File", "CWECode"};
            var values = fields
                .Select(fieldName =>
                {
                    foreach (var t in logRecord.GetElementsByTagName(fieldName).OfType<XmlNode>())
                    {
                        if (t.InnerText != null)
                        {
                            return (fieldName, value: t.InnerText);
                        }
                    }

                    return (fieldName, null);
                })
                .ToDictionary(
                    t => t.fieldName,
                    t => t.value
                );

            var additionalPositionNodes = logRecord
                .GetElementsByTagName("Positions")
                .OfType<XmlElement>()
                .SelectMany(t => t.GetElementsByTagName("Position").OfType<XmlElement>())
                .Select(t =>
                {
                    var lines = t.GetAttribute("lines");
                    var filename = t.InnerText;
                    return lines
                        .Split(',')
                        .Select(line => (filename, lineNumber: int.Parse(line.Trim())));
                })
                .SelectMany(t => t)
                .ToList();
            additionalPositionNodes.Add((values["File"], int.Parse(values["Line"])));
            var results = additionalPositionNodes
                .ToHashSet()
                .Select(t =>
                {
                    var localFile = "";
                    if (t.filename != "")
                    {
                        localFile = t.filename.Substring(trimFolderName.Length).TrimStart('/', '\\');
                    }

                    // ReSharper disable once UseObjectOrCollectionInitializer
                    var result = new CodeQualityLogRecord();
                    result.description = string.Format(
                        "{0}{2}. {1}",
                        values["ErrorCode"],
                        values["Message"],
                        (values["CWECode"] != null) ? ", " + values["CWECode"] : ""
                    );

                    // TODO Нормальный Fingerprint
                    using var sha1 = new SHA1Managed();
                    var hash = sha1.ComputeHash(
                        Encoding.UTF8.GetBytes(localFile + ":" + t.lineNumber + ":" + result.description));
                    result.fingerprint = hash
                        .Select(t1 => t1.ToString("x2"))
                        .Aggregate((a, b) => a + b);

                    // location
                    result.location = new CodeQualityLogRecord.Location()
                    {
                        path = localFile,
                        lines = new CodeQualityLogRecord.LocationLines()
                        {
                            begin = t.lineNumber,
                        },
                    };

                    //
                    return result;
                })
                .ToArray();

            return results;
        }
    }
}