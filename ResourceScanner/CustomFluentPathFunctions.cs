extern alias dstu2;
extern alias stu3;
extern alias r4;

using Hl7.Fhir.ElementModel;
using Hl7.Fhir.Model.Primitives;
using Hl7.FhirPath;
using Hl7.FhirPath.Expressions;
using stu3::Hl7.Fhir.FhirPath;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FhirResourceScanner
{
    public class CustomFluentPathFunctions
    {
        static bool _added = false;
        static Regex _replace = new Regex(@"\[[0-9]*\]", RegexOptions.Compiled);

        public CustomFluentPathFunctions(Regex replace)
        {
            _replace = replace;
        }

        public static void PrepareSqlonfhirSybolTableFunctions()
        {
            if (!_added)
            {
                _added = true;

                // Custom function that returns the name of the property, rather than its value
                Hl7.FhirPath.FhirPathCompiler.DefaultSymbolTable.Add("element_def_path", (object f) =>
                {
                    if (f is IEnumerable<IElementNavigator>)
                    {
                        object[] bits = (f as IEnumerable<IElementNavigator>).Select(i =>
                        {
                            string name;
                            if (i is r4.Hl7.Fhir.ElementModel.PocoNavigator)
                            {
                                name = (i as r4.Hl7.Fhir.ElementModel.PocoNavigator).Name;
                            }
                            else if (i is stu3.Hl7.Fhir.ElementModel.PocoNavigator)
                            {
                                name = (i as stu3.Hl7.Fhir.ElementModel.PocoNavigator).Name;
                            }
                            else if (i is dstu2.Hl7.Fhir.ElementModel.PocoNavigator)
                            {
                                name = (i as dstu2.Hl7.Fhir.ElementModel.PocoNavigator).Name;
                            }
                            else
                            {
                                name = "?";
                            }
                            return _replace.Replace(name, "");
                        }).ToArray();
                        return FhirValueList.Create(bits);
                    }
                    return FhirValueList.Create(new object[] { "?" });
                });
                Hl7.FhirPath.FhirPathCompiler.DefaultSymbolTable.Add("propname", (object f) =>
                {
                    if (f is IEnumerable<IElementNavigator>)
                    {
                        object[] bits = (f as IEnumerable<IElementNavigator>).Select(i =>
                        {
                            if (i is stu3.Hl7.Fhir.ElementModel.PocoNavigator)
                            {
                                return (i as stu3.Hl7.Fhir.ElementModel.PocoNavigator).Name;
                            }
                            if (i is dstu2.Hl7.Fhir.ElementModel.PocoNavigator)
                            {
                                return (i as dstu2.Hl7.Fhir.ElementModel.PocoNavigator).Name;
                            }
                            return "?";
                        }).ToArray();
                        return FhirValueList.Create(bits);
                    }
                    return FhirValueList.Create(new object[] { "?" });
                });
                Hl7.FhirPath.FhirPathCompiler.DefaultSymbolTable.Add("pathname", (object f) =>
                {
                    if (f is IEnumerable<IElementNavigator>)
                    {
                        object[] bits = (f as IEnumerable<IElementNavigator>).Select(i =>
                        {
                            if (i is stu3.Hl7.Fhir.ElementModel.PocoNavigator)
                            {
                                return (i as stu3.Hl7.Fhir.ElementModel.PocoNavigator).Location;
                            }
                            if (i is dstu2.Hl7.Fhir.ElementModel.PocoNavigator)
                            {
                                return (i as dstu2.Hl7.Fhir.ElementModel.PocoNavigator).Location;
                            }
                            return "?";
                        }).ToArray();
                        return FhirValueList.Create(bits);
                    }
                    return FhirValueList.Create(new object[] { "?" });
                });

                FhirPathCompiler.DefaultSymbolTable.Add("commonpathname", (object f) =>
                {
                    if (f is IEnumerable<IElementNavigator>)
                    {
                        object[] bits = (f as IEnumerable<IElementNavigator>).Select(i =>
                        {
                            if (i is stu3.Hl7.Fhir.ElementModel.PocoNavigator)
                            {
                                return (i as stu3.Hl7.Fhir.ElementModel.PocoNavigator).CommonPath;
                            }
                            if (i is dstu2.Hl7.Fhir.ElementModel.PocoNavigator)
                            {
                                return (i as dstu2.Hl7.Fhir.ElementModel.PocoNavigator).CommonPath;
                            }
                            return "?";
                        }).ToArray();
                        return FhirValueList.Create(bits);
                    }
                    return FhirValueList.Create(new object[] { "?" });
                });

                FhirPathCompiler.DefaultSymbolTable.Add("shortpathname", (object f) =>
                {
                    if (f is IEnumerable<IElementNavigator>)
                    {
                        object[] bits = (f as IEnumerable<IElementNavigator>).Select(i =>
                        {
                            if (i is stu3.Hl7.Fhir.ElementModel.PocoNavigator)
                            {
                                return (i as stu3.Hl7.Fhir.ElementModel.PocoNavigator).ShortPath;
                            }
                            if (i is dstu2.Hl7.Fhir.ElementModel.PocoNavigator)
                            {
                                return (i as dstu2.Hl7.Fhir.ElementModel.PocoNavigator).ShortPath;
                            }
                            return "?";
                        }).ToArray();
                        return FhirValueList.Create(bits);
                    }
                    return FhirValueList.Create(new object[] { "?" });
                });

                Hl7.FhirPath.FhirPathCompiler.DefaultSymbolTable.AddFhirExtensions();
            }
        }
    }
}
