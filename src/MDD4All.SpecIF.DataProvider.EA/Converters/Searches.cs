using System;
using System.Collections.Generic;
using System.Text;

namespace MDD4All.SpecIF.DataProvider.EA.Converters
{
    internal class Searches
    {
        internal static string SPECIFICATION_PACKAGES = "<?xml version=\"1.0\" encoding=\"windows-1252\"?><RootSearch><Search Name=\"SpecificationPackages\" GUID=\"{2AC5F8FD-D8EE-4cf5-BC74-04C9E943882A}\" PkgGUID=\"-1\" Type=\"1\" LnksToObj=\"0\" CustomSearch=\"0\" AddinAndMethodName=\"\"><SrchOn><RootTable Type=\"0\"><TableName Display=\"Element\" Name=\"t_object\"/><TableHierarchy Display=\"\" Hierarchy=\"t_object\"/><Field Filter=\"t_object.Stereotype = 'specification'\" Text=\"specification\" IsDateField=\"0\" Type=\"1\" Required=\"1\" Active=\"1\"><TableName Display=\"Element\" Name=\"t_object\"/><TableHierarchy Display=\"Element\" Hierarchy=\"t_object\"/><Condition Display=\"Equal To\" Type=\"=\"/><FieldName Display=\"Stereotype\" Name=\"t_object.Stereotype\"/></Field><Field Filter=\"t_object.Object_Type = 'Package'\" Text=\"Package\" IsDateField=\"0\" Type=\"1\" Required=\"1\" Active=\"1\"><TableName Display=\"Element\" Name=\"t_object\"/><TableHierarchy Display=\"Element\" Hierarchy=\"t_object\"/><Condition Display=\"Equal To\" Type=\"=\"/><FieldName Display=\"ObjectType\" Name=\"t_object.Object_Type\"/></Field></RootTable></SrchOn><LnksTo/></Search></RootSearch>";
        internal static string PACKAGE_ELEMENTS_SEARCH = "<?xml version=\"1.0\" encoding=\"windows-1252\"?><RootSearch><Search Name=\"PackageElementsSearch\" GUID=\"{44FF8B86-D393-4a99-B657-4C4FBFD53ACE}\" PkgGUID=\"-1\" Type=\"0\" LnksToObj=\"0\" CustomSearch=\"1\" AddinAndMethodName=\"\"><SrchOn><RootTable Filter=\"select * from t_object where&#xA;t_object.Package_ID = &lt;Search Term&gt;\" Type=\"-1\"><TableName Display=\"Custom SQL Search\" Name=\"\"/><TableHierarchy Display=\"\" Hierarchy=\"\"/></RootTable></SrchOn><LnksTo/></Search></RootSearch>";
    }
}
