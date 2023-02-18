using System.CommandLine;

using Microsoft.Build.Locator;

using static Constants;

#pragma warning disable CA1852 // Seal internal types (Program is auto generated)
var returnValue = 0;
#pragma warning restore CA1852 // Seal internal types

const string description = $"""
    Creates a solution filter form a Visual Studio solution, controlled by MSBuild project properties.
    All projects containing a property '{IncludeInSolutionFilter}' set to 'true' are included in the generated filter, along with all their referenced projects.
    A property '{SolutionFilterName}' is provided to be able to write MSBuild conditions based on the filter name.

    You can mark individual projects by adding the '{IncludeInSolutionFilter}' property to the project file:
    
    - Include the project and all references in any filter:
      <{IncludeInSolutionFilter}>true</{IncludeInSolutionFilter}>
    
    - Include the project only if the filter name is 'Setup.slnf':
      <{IncludeInSolutionFilter} Condition="'$(SolutionFilterName)'=='Setup'">true</{IncludeInSolutionFilter}>

    You can include several projects by convention by adding conditional properties in the Directory.Build.targets file:
    
    - Include all test projects in 'Test.slnf':
      <{IncludeInSolutionFilter} Condition="'$({IncludeInSolutionFilter})'=='' AND $(IsTestProject) AND '$({SolutionFilterName})'=='Test'">true</{IncludeInSolutionFilter}>
    
    - Include all projects ending with 'Something' in 'Something.slnf':
      <_IsSomeProject>$(MSBuildProjectName.ToUpperInvariant().EndsWith("SOMETHING"))</_IsSomeProject>
      <{IncludeInSolutionFilter} Condition="'$({IncludeInSolutionFilter})'=='' AND $(_IsSomeProject) AND '$({SolutionFilterName})'=='Something'">true</{IncludeInSolutionFilter}>
    
    - Include all executables in 'Apps.slnf':
      <{IncludeInSolutionFilter} Condition="'$({IncludeInSolutionFilter})'=='' AND '$(OutputType)'=='Exe' AND '$(IsTestProject)'!='true' AND '$({SolutionFilterName})'=='Apps'">true</{IncludeInSolutionFilter}>
    """;

var rootCommand = new RootCommand
{
    Name = "build-slnf",
    Description = description
};

var inputOption = new Option<FileInfo>(new[] { "--input", "-i" }, """
    The path to the solution file that is the source for the filter. 
    If no solution is specified, it tries to find a single one in the current directory.
    """) { IsRequired = false };

var outputOption = new Option<string>(new[] { "--output", "-o" }, """
    The name of the solution filter that is created.
    An existing file will be overwritten without confirmation.
    """) { IsRequired = true };

rootCommand.AddOption(inputOption);
rootCommand.AddOption(outputOption);
rootCommand.SetHandler((input, output) => returnValue = Run(input, output), inputOption, outputOption);

await rootCommand.InvokeAsync(args);

return returnValue;

static int Run(FileInfo? input, string output)
{
    var visualStudioInstance = MSBuildLocator.QueryVisualStudioInstances().MaxBy(instance => instance.Version);
    MSBuildLocator.RegisterInstance(visualStudioInstance);

    try
    {
        return new Builder(input ?? FindSolution(), output).Build();
    }
    catch (Exception ex)
    {
        Output.WriteError($"Execution failed: {ex.Message}");
        return 1;
    }
}

static FileInfo FindSolution()
{
    var candidates = Directory.GetFiles(".", "*.sln");

    return candidates.Length switch
    {
        0 => throw new InvalidOperationException("No solution found in current folder."),
        1 => new FileInfo(candidates[0]),
        _ => throw new InvalidOperationException("Multiple solutions found in current folder. Please specify input explicit")
    };
}


