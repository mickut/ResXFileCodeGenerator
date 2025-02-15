using System.Text;
using FluentAssertions;
using Xunit;

namespace VocaDb.ResXFileCodeGenerator.Tests.GithubIssues.Issue3;

public class GeneratorTests
{

	[Fact]
	public void Generate_StringBuilder_Name_NotValidIdentifier()
	{
		var text = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<root>
  <xsd:schema id=""root"" xmlns="""" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:msdata=""urn:schemas-microsoft-com:xml-msdata"">
    <xsd:import namespace=""http://www.w3.org/XML/1998/namespace"" />
    <xsd:element name=""root"" msdata:IsDataSet=""true"">
      <xsd:complexType>
        <xsd:choice maxOccurs=""unbounded"">
          <xsd:element name=""metadata"">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" />
              </xsd:sequence>
              <xsd:attribute name=""name"" use=""required"" type=""xsd:string"" />
              <xsd:attribute name=""type"" type=""xsd:string"" />
              <xsd:attribute name=""mimetype"" type=""xsd:string"" />
              <xsd:attribute ref=""xml:space"" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name=""assembly"">
            <xsd:complexType>
              <xsd:attribute name=""alias"" type=""xsd:string"" />
              <xsd:attribute name=""name"" type=""xsd:string"" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name=""data"">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""1"" />
                <xsd:element name=""comment"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""2"" />
              </xsd:sequence>
              <xsd:attribute name=""name"" type=""xsd:string"" use=""required"" msdata:Ordinal=""1"" />
              <xsd:attribute name=""type"" type=""xsd:string"" msdata:Ordinal=""3"" />
              <xsd:attribute name=""mimetype"" type=""xsd:string"" msdata:Ordinal=""4"" />
              <xsd:attribute ref=""xml:space"" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name=""resheader"">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name=""value"" type=""xsd:string"" minOccurs=""0"" msdata:Ordinal=""1"" />
              </xsd:sequence>
              <xsd:attribute name=""name"" type=""xsd:string"" use=""required"" />
            </xsd:complexType>
          </xsd:element>
        </xsd:choice>
      </xsd:complexType>
    </xsd:element>
  </xsd:schema>
  <resheader name=""resmimetype"">
    <value>text/microsoft-resx</value>
  </resheader>
  <resheader name=""version"">
    <value>2.0</value>
  </resheader>
  <resheader name=""reader"">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name=""writer"">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <data name=""Invalid identifier {{0}}"" xml:space=""preserve"">
    <value>String '{{0}}' is not a valid identifier.</value>
  </data>
</root>";

		var expected = $@"// ------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
// ------------------------------------------------------------------------------
#nullable enable
namespace VocaDb.Web.App_GlobalResources
{{
    using System.Globalization;
    using System.Resources;

    public static class CommonMessages
    {{
        private static ResourceManager? s_resourceManager;
        public static ResourceManager ResourceManager => s_resourceManager ??= new ResourceManager(""VocaDb.Web.App_GlobalResources.CommonMessages"", typeof(CommonMessages).Assembly);
        public static CultureInfo? CultureInfo {{ get; set; }}

        /// <summary>
        /// Looks up a localized string similar to String &#39;{{0}}&#39; is not a valid identifier..
        /// </summary>
        public static string? Invalid_identifier__0_ => ResourceManager.GetString(""Invalid identifier {{0}}"", CultureInfo);
    }}
}}";
		var generator = new StringBuilderGenerator();
		using var resxStream = new MemoryStream(Encoding.UTF8.GetBytes(text));
		var source = generator.Generate(
			resxStream: resxStream,
			options: new GeneratorOptions(
				LocalNamespace: "VocaDb.Web.App_GlobalResources",
				CustomToolNamespace: null,
				ClassName: "CommonMessages",
				PublicClass: true,
				NullForgivingOperators: false,
				StaticClass: true
			)
		);
		source.ReplaceLineEndings().Should().Be(expected.ReplaceLineEndings());
	}
}
