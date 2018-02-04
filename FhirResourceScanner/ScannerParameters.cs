extern alias stu3;
using CommandLine;
using System;

namespace FhirResourceScanner
{
    public class ScannerParameters
    {
        [Option('u', "user", Required = true, HelpText = "This name will be listed in the Test Report output")]
        public string User { get; set; }

        [Option('f', "file", HelpText = "A filename to an xml/json file to load")]
        public string filename { get; set; }

        [Option("url", HelpText = "The fhir query URL to execute to load the bundle/resource to process")]
        public string url { get; set; }

        [Option('a', "auth", HelpText = "A value to populate into the Authorization header of a fhir server (via the --url parameter) when connecting to extract resource content")]
        public string authorization { get; set; }

        [Option('v', "ver", Default = "STU3", HelpText = "This can be either DSTU2, STU3 or R4")]
        public string FhirVersion { get; set; }

        [Option('r', "report", HelpText = "Publish the results to this server location (use 'none' if you don't want to publish the results)", Default = "http://sqlonfhir-stu3.azurewebsites.net/fhir")]
        public string PostResultsToServer { get; set; }

        public static void WriteExmaplesToConsole()
        {
            Console.WriteLine("Example:");

            Console.WriteLine("  > dotnet.exe FhirResourceScanner.dll " + Parser.Default.FormatCommandLine<ScannerParameters>(new ScannerParameters()
            {
                User = "Brian Postlethwaite",
                filename = @"c:\temp\example-bundle.xml",
            }));

            Console.WriteLine("  > dotnet.exe FhirResourceScanner.dll " + Parser.Default.FormatCommandLine<ScannerParameters>(new ScannerParameters()
            {
                User = "David",
                filename = @"c:\temp\example-bundle.json",
            }));

            Console.WriteLine("  > dotnet.exe FhirResourceScanner.dll " + Parser.Default.FormatCommandLine<ScannerParameters>(new ScannerParameters()
            {
                User = "Brian Postlethwaite",
                url = "http://sqlonfhir-stu3.azurewebsites.net/fhir/Encounter",
                PostResultsToServer = "none"
            }));

            Console.WriteLine("  > dotnet.exe FhirResourceScanner.dll " + Parser.Default.FormatCommandLine<ScannerParameters>(new ScannerParameters()
            {
                User = "Brian Postlethwaite",
                url = "http://sqlonfhir-stu3.azurewebsites.net/fhir/Patient?active=true",
                authorization = "Bearer asfdasdfsf=="
            }));

            Console.WriteLine();
        }

    }
}
