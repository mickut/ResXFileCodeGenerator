using System.Globalization;
using Microsoft.CodeAnalysis;

namespace VocaDb.ResXFileCodeGenerator;

[Generator]
public class SourceGenerator : ISourceGenerator
{
	private static readonly IGenerator s_generator = new StringBuilderGenerator();

	// Code from: https://github.com/dotnet/ResXResourceManager/blob/0ec11bae232151400a5a8ca7b9835ac063c516d0/src/ResXManager.Model/ResourceManager.cs#L267
	private static bool IsValidLanguageName(string? languageName)
	{
		try
		{
			if (languageName.IsNullOrEmpty())
				return false;

			if (languageName.StartsWith("qps-", StringComparison.Ordinal))
				return true;

			var culture = new CultureInfo(languageName);

			while (!culture.IsNeutralCulture)
				culture = culture.Parent;

			return culture.LCID != 4096;
		}
		catch
		{
			return false;
		}
	}

	// Code from: https://github.com/dotnet/ResXResourceManager/blob/0ec11bae232151400a5a8ca7b9835ac063c516d0/src/ResXManager.Model/ProjectFileExtensions.cs#L77
	private static string GetBaseName(string filePath)
	{
		var name = Path.GetFileNameWithoutExtension(filePath);
		var innerExtension = Path.GetExtension(name);
		var languageName = innerExtension.TrimStart('.');

		return IsValidLanguageName(languageName) ? Path.GetFileNameWithoutExtension(name) : name;
	}

	// Code from: https://github.com/dotnet/ResXResourceManager/blob/c8b5798d760f202a1842a74191e6010c6e8bbbc0/src/ResXManager.VSIX/Visuals/MoveToResourceViewModel.cs#L120
	private static string GetLocalNamespace(string? resxPath, string? targetPath, string? projectPath, string? rootNamespace)
	{
		try
		{
			if (resxPath is null)
				return string.Empty;

			var resxFolder = Path.GetDirectoryName(resxPath);
			var projectFolder = Path.GetDirectoryName(projectPath);
			rootNamespace ??= string.Empty;

			if (resxFolder is null || projectFolder is null)
				return string.Empty;

			var localNamespace = rootNamespace;

			if (!string.IsNullOrWhiteSpace(targetPath))
			{
				localNamespace += ".";
				localNamespace += Path.GetDirectoryName(targetPath).Replace(Path.DirectorySeparatorChar, '.')
					.Replace(Path.AltDirectorySeparatorChar, '.')
					.Replace(" ", "");
			}
			else if (resxFolder.StartsWith(projectFolder, StringComparison.OrdinalIgnoreCase))
			{
				localNamespace += resxFolder.Substring(projectFolder.Length)
					.Replace(Path.DirectorySeparatorChar, '.')
					.Replace(Path.AltDirectorySeparatorChar, '.')
					.Replace(" ", "");
			}
			return localNamespace;
		}
		catch (Exception)
		{
			return string.Empty;
		}
	}

	public void Execute(GeneratorExecutionContext context)
	{
		if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.MSBuildProjectFullPath", out var projectFullPath))
			return;

		if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace))
			return;

		// Code from: https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md#consume-msbuild-properties-and-metadata
		var publicClassGlobal = false;
		if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.ResXFileCodeGenerator_PublicClass", out var publicClassSwitch))
			publicClassGlobal = publicClassSwitch.Equals("true", StringComparison.OrdinalIgnoreCase);

		var nullForgivingOperators =
			context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.ResXFileCodeGenerator_NullForgivingOperators", out var nullForgivingOperatorsSwitch) &&
			nullForgivingOperatorsSwitch is { Length: > 0 } &&
			nullForgivingOperatorsSwitch.Equals("true", StringComparison.OrdinalIgnoreCase);
		
		var staticClass = !(context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.ResXFileCodeGenerator_StaticClass", out var staticClassSwitch) &&
			staticClassSwitch is { Length: > 0 } &&
			staticClassSwitch.Equals("false", StringComparison.OrdinalIgnoreCase));

		var resxFiles = context.AdditionalFiles
			.Where(af => af.Path.EndsWith(".resx"))
			.Where(af => Path.GetFileNameWithoutExtension(af.Path) == GetBaseName(af.Path));

		foreach (var resxFile in resxFiles)
		{
			using var resxStream = File.OpenRead(resxFile.Path);

			var options = new GeneratorOptions(
				LocalNamespace:
					GetLocalNamespace(
						resxFile.Path,
						context.AnalyzerConfigOptions.GetOptions(resxFile).TryGetValue("build_metadata.EmbeddedResource.TargetPath", out var targetPath) && targetPath is { Length: > 0 }
							? targetPath
							: null,
						projectFullPath,
						rootNamespace),
				CustomToolNamespace:
					context.AnalyzerConfigOptions.GetOptions(resxFile).TryGetValue("build_metadata.EmbeddedResource.CustomToolNamespace", out var customToolNamespace) && customToolNamespace is { Length: > 0 }
						? customToolNamespace
						: null,
				ClassName: Path.GetFileNameWithoutExtension(resxFile.Path),
				PublicClass:
					context.AnalyzerConfigOptions.GetOptions(resxFile).TryGetValue("build_metadata.EmbeddedResource.PublicClass", out var perFilePublicClassSwitch) && perFilePublicClassSwitch is { Length: > 0 }
						? perFilePublicClassSwitch.Equals("true", StringComparison.OrdinalIgnoreCase)
						: publicClassGlobal,
				NullForgivingOperators: nullForgivingOperators,
				StaticClass:
					context.AnalyzerConfigOptions.GetOptions(resxFile).TryGetValue("build_metadata.EmbeddedResource.StaticClass", out var perFileStaticClassSwitch) && perFileStaticClassSwitch is { Length: > 0 }
						? !perFileStaticClassSwitch.Equals("false", StringComparison.OrdinalIgnoreCase)
			 			: staticClass
			);

			var source = s_generator.Generate(resxStream, options);

			context.AddSource($"{options.LocalNamespace}.{options.ClassName}.g.cs", source);
		}
	}

	public void Initialize(GeneratorInitializationContext context) { }
}
