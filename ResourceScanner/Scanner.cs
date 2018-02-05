extern alias stu3;
extern alias dstu2;
extern alias r4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fm2 = dstu2.Hl7.Fhir.Model;
using fm3 = stu3.Hl7.Fhir.Model;
using fm4 = r4.Hl7.Fhir.Model;
using System.IO;

namespace ResourceScanner
{
    public class Scanner
    {
        public string ResourceName { get; set; }

        public class SourceDetails
        {
            public string Source { get; set; }
            public string ResourceName { get; set; }
            public int InstanceCount { get; set; }
        }

        public class ResultItem
        {
            public string PropertyName { get; set; }

            public bool FromStructureDef { get; set; }

            public int UsageCount { get; set; }
        }

        Dictionary<string, SourceDetails> Sources = new Dictionary<string, SourceDetails>();

        Dictionary<string, ResultItem> _properties = new Dictionary<string, ResultItem>();

        public void ScanResourceInstance(fm3.Resource resource, string source)
        {
            SourceDetails sourceDetail;
            if (!Sources.ContainsKey(source))
            {
                sourceDetail = new SourceDetails()
                {
                    Source = source,
                    ResourceName = resource.TypeName,
                    InstanceCount = 1
                };
                Sources.Add(source, sourceDetail);
            }
            else
            {
                sourceDetail = Sources[source];
                sourceDetail.InstanceCount++;
            }
            this.ResourceName = resource.TypeName;

            // Prepare the property list from the Structure Definition
            if (!_properties.ContainsKey(ResourceName))
            {
                var sourceSD = new Hl7.Fhir.Specification.Source.ZipSource(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "specification.zip"));
                var instSD = sourceSD.ResolveByUri("http://hl7.org/fhir/StructureDefinition/" + resource.TypeName) as fm3.StructureDefinition;
                if (instSD == null)
                {
                    Console.WriteLine($"Unable to load the StructureDefinition for {resource.TypeName}");
                    return;
                }

                foreach (var ed in instSD.Differential.Element)
                {
                    if (!_properties.ContainsKey(ed.Path))
                        _properties.Add(ed.Path, new ResultItem() { PropertyName = ed.Path, UsageCount = 0, FromStructureDef = true });
                }
            }
            if (_properties.ContainsKey(ResourceName))
            {
                _properties[ResourceName].UsageCount++;
            }

            // Now process this actual instance
            var results = stu3.Hl7.Fhir.FhirPath.ElementNavFhirExtensions.Select(resource, "descendants().element_def_path()");
            foreach (var item in results)
            {
                if (item is fm3.FhirString str)
                {
                    string pname = $"{str.Value}";
                    if (_properties.ContainsKey(pname))
                    {
                        _properties[pname].UsageCount++;
                    }
                    else
                    {
                        // don't count props not in the differential
                        // _properties.Add(pname, new ResultItem() { PropertyName = pname, UsageCount = 1 });
                    }
                }
            }
        }

        public fm3.TestReport ToReport3(string testerName)
        {
            fm3.TestReport result = new fm3.TestReport();
            result.Name = "Property Usage Report: " + ResourceName;
            result.Status = fm3.TestReport.TestReportStatus.Completed;
            result.Result = fm3.TestReport.TestReportResult.Pass;
            result.Tester = testerName;
            result.TestScript = new fm3.ResourceReference(null, "FHIR Resource Property Usage Tester");
            result.Setup = new fm3.TestReport.SetupComponent();

            // Calculate a score (the percentage of properties from the SD that have a value)
            decimal propertiesInSd = _properties.Where(p => p.Value.FromStructureDef).Count();
            decimal propertiesWithValue = _properties.Where(p => p.Value.FromStructureDef && p.Value.UsageCount > 0).Count();
            if (propertiesInSd > 0)
                result.Score = propertiesWithValue / propertiesInSd * 100;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<div xmlns=\"http://www.w3.org/1999/xhtml\">");
            sb.AppendLine($"<h2>Property Usage Evaluation Report: {ResourceName}</h2>");
            sb.AppendLine($"<p>Tested by: {testerName}</p>");
            sb.AppendLine($"<p>Score: {result.Score:0} ({propertiesWithValue}/{propertiesInSd})</p>");

            foreach (var item in Sources)
            {
                result.Setup.Action.Add(new fm3.TestReport.SetupActionComponent()
                {
                    Operation = new fm3.TestReport.OperationComponent()
                    {
                        Detail = item.Value.Source,
                        Message = new fm3.Markdown($"Contained {item.Value.InstanceCount} instances"),
                        Result = fm3.TestReport.TestReportActionResult.Pass
                    }
                });
                sb.AppendLine($"<p>{item.Value.Source} contained {item.Value.InstanceCount} instances</p>");
            }

            var tc = new fm3.TestReport.TestComponent();
            tc.Name = ResourceName;
            tc.Description = $"Property usage evaluation for {ResourceName}";
            sb.AppendLine("<table>");
            sb.AppendLine($"<tr><th style='font-family: italic;'>Property Name</th><th style='font-family: italic;'>Usage Count</th></tr>");
            foreach (var item in this._properties.Values.OrderBy(s => s.PropertyName))
            {
                sb.AppendLine($"<tr><td>{item.PropertyName}{(item.FromStructureDef?"" : " *")}</td><td>{item.UsageCount}</td></tr>");
                tc.Action.Add(new fm3.TestReport.TestActionComponent()
                {
                    Operation = new fm3.TestReport.OperationComponent()
                    {
                        Result = fm3.TestReport.TestReportActionResult.Pass,
                        Detail = $"{item.PropertyName}",
                        Message = new fm3.Markdown($"{item.PropertyName}: {item.UsageCount}")
                    }
                });
            }
            sb.AppendLine("</table>");
            result.Test.Add(tc);

            // Complete the narrative
            sb.AppendLine("</div>");
            fm3.Narrative text = new fm3.Narrative();
            text.Status = fm3.Narrative.NarrativeStatus.Generated;
            text.Div = sb.ToString();
            result.Text = text;

            return result;
        }
    }
}
