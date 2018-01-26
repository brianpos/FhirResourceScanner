extern alias stu3;

using FhirResourceScanner;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ResourceScanner.Test
{
    [TestClass]
    public class BasicTests
    {
        [TestInitialize]
        public void Initialize()
        {
            CustomFluentPathFunctions.PrepareSqlonfhirSybolTableFunctions();
        }

        [TestMethod, Ignore]
        public void TestServer()
        {
            stu3::Hl7.Fhir.Rest.FhirClient server3 = new stu3::Hl7.Fhir.Rest.FhirClient("http://sqlonfhir-stu3.azurewebsites.net/fhir");
            var encounters = server3.Search<stu3::Hl7.Fhir.Model.Encounter>();
            DebugDumpResourceXml(encounters);

            ResourceScanner.Scanner model = new ResourceScanner.Scanner();
            foreach (var enc in encounters.Entry.Select(s => s.Resource as stu3::Hl7.Fhir.Model.Encounter))
            {
                // Test this encounter
                model.ScanResourceInstance(enc, "unit-test");
            }
            var results = model.ToReport3("Brian Postlethwaite (Telstra Health)");
            DebugDumpResourceXml(results);
            Assert.AreEqual("Encounter", results.Test.FirstOrDefault()?.Name);
        }

        [TestMethod]
        public void TestLocal()
        {
            var encounters = new stu3::Hl7.Fhir.Model.Bundle();
            var encounter = new stu3::Hl7.Fhir.Model.Encounter()
            {
                Id = "enc",
                Status = stu3::Hl7.Fhir.Model.Encounter.EncounterStatus.Arrived,
                Period = new stu3::Hl7.Fhir.Model.Period() { Start = "2017-12", End = "2018-01" }
            };
            encounter.Location.Add(new stu3::Hl7.Fhir.Model.Encounter.LocationComponent()
            {
                Location = new stu3::Hl7.Fhir.Model.ResourceReference("Location/example", "Example Org")
            });
            encounter.Location.Add(new stu3::Hl7.Fhir.Model.Encounter.LocationComponent()
            {
                Location = new stu3::Hl7.Fhir.Model.ResourceReference("Location/other", "Other Example Org")
            });
            encounters.Entry.Add(new stu3::Hl7.Fhir.Model.Bundle.EntryComponent()
            {
                Resource = encounter
            });

            ResourceScanner.Scanner model = new ResourceScanner.Scanner();
            foreach (var enc in encounters.Entry.Select(s => s.Resource as stu3::Hl7.Fhir.Model.Encounter))
            {
                // Test this encounter
                model.ScanResourceInstance(enc, "unit-test");
            }
            var results = model.ToReport3("Brian Postlethwaite (Telstra Health)");
            DebugDumpResourceXml(results);
            Assert.AreEqual("Encounter", results.Test.FirstOrDefault()?.Name);
            var tr = results.Test.FirstOrDefault();

//            Assert.AreEqual("Encounter.Id: 1", results.Test.FirstOrDefault()?.Action.Where(a => a.Operation.Detail == "Encounter.Id").Select(a => a.Operation.Message).FirstOrDefault().Value);
        }

        private void DebugDumpResourceXml(stu3::Hl7.Fhir.Model.Base results)
        {
            var xs = new stu3::Hl7.Fhir.Serialization.FhirXmlSerializer();
            var output = xs.SerializeToString(results);
            var doc = System.Xml.Linq.XDocument.Parse(output);
            System.Console.WriteLine(doc.ToString(System.Xml.Linq.SaveOptions.None));
        }
    }
}
