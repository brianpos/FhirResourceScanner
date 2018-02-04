extern alias stu3;
using System;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using stu3::Hl7.Fhir.Model;
using stu3::Hl7.Fhir.Rest;

namespace FhirResourceScanner
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<ScannerParameters>(args)
               .WithParsed<ScannerParameters>(opts => ScanResources(opts))
               .WithNotParsed<ScannerParameters>((errs) => ScannerParameters.WriteExmaplesToConsole());
        }

        private static void ScanResources(ScannerParameters args)
        {
            FhirClient server = new FhirClient("http://sqlonfhir-stu3.azurewebsites.net/fhir");
            try
            {
                CustomFluentPathFunctions.PrepareSqlonfhirSybolTableFunctions();

                Console.WriteLine(CommandLine.Text.HeadingInfo.Default.ToString());
                Console.WriteLine(CommandLine.Text.CopyrightInfo.Default.ToString());
                Console.WriteLine($"------------------------------------");

                // server to receive the results
                server.PreferredFormat = ResourceFormat.Json;
                server.OnBeforeRequest += (object sender, stu3::Hl7.Fhir.Rest.BeforeRequestEventArgs e) =>
                {
                    if (!string.IsNullOrEmpty(args.authorization))
                        e.RawRequest.Headers.Add("Authorization", args.authorization);
                };
                ResourceScanner.Scanner scanner = new ResourceScanner.Scanner();
                if (!string.IsNullOrEmpty(args.url))
                {
                    ScanResourcesFromServer(args, server, scanner);
                }
                else if (System.IO.File.Exists(args.filename))
                {
                    Console.WriteLine($"Scanning {args.filename}");
                    Resource resource = null;
                    var fi = new System.IO.FileInfo(args.filename);
                    if (fi.Extension == ".json")
                    {
                        resource = new stu3::Hl7.Fhir.Serialization.FhirJsonParser().Parse<Resource>(System.IO.File.ReadAllText(args.filename));
                    }
                    else if (fi.Extension == ".xml")
                    {
                        resource = new stu3::Hl7.Fhir.Serialization.FhirXmlParser().Parse<Resource>(System.IO.File.ReadAllText(args.filename));
                    }
                    else
                    {
                        Console.WriteLine($"File is not xml or json: {args.filename}");
                        return;
                    }
                    if (resource != null)
                    {
                        if (resource is Bundle bundle)
                        {
                            // TODO: scan the bundle
                            foreach (var entry in bundle.Entry.Select(e => e.Resource))
                            {
                                if (entry != null)
                                    scanner.ScanResourceInstance(entry, args.filename);
                            }
                        }
                        else
                        {
                            scanner.ScanResourceInstance(resource, args.filename);
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"File not found: {args.filename}");
                    return;
                }
                var testReport = scanner.ToReport3(args.User);
                DebugDumpResourceXml(testReport);

                // Send the results to the requested server
                if (!string.IsNullOrEmpty(args.PostResultsToServer) && args.PostResultsToServer != "none")
                {
                    Console.WriteLine($"Sunmitting usage report to {args.PostResultsToServer} ...");
                    server = new FhirClient(args.PostResultsToServer);
                    var newReport = server.Create(testReport);
                    Console.WriteLine($"Usage Report submitted to {newReport.ResourceIdentity()?.OriginalString}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static void ScanResourcesFromServer(ScannerParameters args, stu3::Hl7.Fhir.Rest.FhirClient server, ResourceScanner.Scanner scanner)
        {
            Console.WriteLine($"Scanning {args.url}");
            Resource resource = server.Get(args.url);
            if (resource is Bundle bundle)
            {
                if (bundle.Total.HasValue)
                    Console.WriteLine($"    Total: {bundle.Total.Value}");
                foreach (var entry in bundle.Entry.Select(e => e.Resource))
                {
                    if (entry != null)
                        scanner.ScanResourceInstance(entry, args.url);
                }
                while (bundle?.NextLink != null)
                {
                    Console.WriteLine($"Continuing {bundle.NextLink.OriginalString}");
                    bundle = server.Get(bundle.NextLink.OriginalString) as Bundle;
                    foreach (var entry in bundle?.Entry.Select(e => e.Resource))
                    {
                        if (entry != null)
                            scanner.ScanResourceInstance(entry, args.url);
                    }
                }
            }
            else
            {
                scanner.ScanResourceInstance(resource, args.url);
            }
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
