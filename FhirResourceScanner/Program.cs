extern alias stu3;
using System;
using System.Linq;
using stu3::Hl7.Fhir.Model;
using stu3::Hl7.Fhir.Rest;

namespace FhirResourceScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CustomFluentPathFunctions.PrepareSqlonfhirSybolTableFunctions();

                Console.WriteLine($"Scanning FHIR Resource Usage");
                Console.WriteLine($"----------------------------");
                if (args.Length < 2)
                {
                    DisplayUsage();
                    return;
                }
                Console.WriteLine($"===> {String.Join(", ", args)}");

                // server to receive the results
                FhirClient server = new FhirClient("http://sqlonfhir-stu3.azurewebsites.net/fhir");
                ResourceScanner.Scanner scanner = new ResourceScanner.Scanner();
                if (args[0].StartsWith("http://") || args[0].StartsWith("https://"))
                {
                    Resource resource = server.Get(args[0]);
                    if (resource is Bundle bundle)
                    {
                        // TODO: scan the bundle
                        foreach (var entry in bundle.Entry.Select(e => e.Resource))
                        {
                            if (entry != null)
                                scanner.ScanResourceInstance(entry, args[0]);
                        }
                    }
                    else
                    {
                        scanner.ScanResourceInstance(resource, args[0]);
                    }
                }
                else if (System.IO.File.Exists(args[0]))
                {
                    Resource resource = null;
                    var fi = new System.IO.FileInfo(args[0]);
                    if (fi.Extension == ".json")
                    {
                        resource = new stu3::Hl7.Fhir.Serialization.FhirJsonParser().Parse<Resource>(System.IO.File.ReadAllText(args[0]));
                    }
                    else if (fi.Extension == ".xml")
                    {
                        resource = new stu3::Hl7.Fhir.Serialization.FhirXmlParser().Parse<Resource>(System.IO.File.ReadAllText(args[0]));
                    }
                    else
                    {
                        Console.WriteLine($"File is not xml or json: {args[0]}");
                        DisplayUsage();
                        return;
                    }
                    if (resource != null)
                        scanner.ScanResourceInstance(resource, args[0]);
                }
                else
                {
                    Console.WriteLine($"File not found: {args[0]}");
                    DisplayUsage();
                    return;
                }
                var testReport = scanner.ToReport3(args[1]);
                DebugDumpResourceXml(testReport);
                var newReport = server.Create(testReport);
                Console.WriteLine($"{newReport.ResourceIdentity()?.OriginalString}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static void DisplayUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("   >dotnet.exe FhirResourceScanner.dll encounter-example.xml \"Brian Pos\"");
            Console.WriteLine("   >dotnet.exe FhirResourceScanner.dll encounter-example.json \"David\"");
            Console.WriteLine("   >dotnet.exe FhirResourceScanner.dll https://sqlonfhir.azurewebsites.net/fhir/Encounter?active=true \"Grahame\"");
        }

        private static void DebugDumpResourceXml(stu3::Hl7.Fhir.Model.Base results)
        {
            var xs = new stu3::Hl7.Fhir.Serialization.FhirXmlSerializer();
            var output = xs.SerializeToString(results);
            var doc = System.Xml.Linq.XDocument.Parse(output);
            System.Console.WriteLine(doc.ToString(System.Xml.Linq.SaveOptions.None));
        }

    }
}
