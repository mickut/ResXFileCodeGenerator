namespace VocaDb.ResXFileCodeGenerator;

public sealed record GeneratorOptions(
	string LocalNamespace,
	string? CustomToolNamespace,
	string ClassName,
	bool PublicClass,
	bool NullForgivingOperators,
	bool StaticClass
);

public interface IGenerator
{
	string Generate(Stream resxStream, GeneratorOptions options);
}
