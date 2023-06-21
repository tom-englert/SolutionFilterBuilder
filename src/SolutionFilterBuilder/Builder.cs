using System.Text.Json;

using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;

using TomsToolbox.Essentials;

using static Constants;

internal sealed class Builder
{
    private const string SolutionFilterExtension = @".slnf";

    private readonly ProjectInfo[] _projects;
    private readonly HashSet<string> _includedProjects = new();
    private readonly string _solutionPath;
    private readonly string _solutionFilterFilePath;

    public Builder(FileInfo input, string output)
    {
        _solutionPath = input.FullName;

        var solutionDirectory = input.DirectoryName ?? ".";
        var solutionFilterName = Path.GetFileName(output);

        if (string.Equals(SolutionFilterExtension, Path.GetExtension(output), StringComparison.OrdinalIgnoreCase))
        {
            solutionFilterName = Path.ChangeExtension(solutionFilterName, null);
            _solutionFilterFilePath = output;
        }
        else
        {
            _solutionFilterFilePath = output + SolutionFilterExtension;
        }

        if (string.IsNullOrEmpty(Path.GetDirectoryName(_solutionFilterFilePath)))
        {
            _solutionFilterFilePath = Path.Combine(solutionDirectory, _solutionFilterFilePath);
            _solutionPath = Path.GetFileName(_solutionPath);
        }

        Output.WriteLine($"Solution: '{input}'");
        Output.WriteLine($"Filter:   '{solutionFilterName}'");
        Output.WriteLine();

        var solution = SolutionFile.Parse(input.FullName);

        _projects = LoadProjects(solution, solutionFilterName)
            .ExceptNullItems()
            .ToArray();

        Output.WriteLine();
    }

    public int Build()
    {
        foreach (var project in _projects)
        {
            if (!ShouldInclude(project))
                continue;

            Output.WriteLine($"Include: {project.ProjectReference.RelativePath}");

            AddProject(project);
        }

        if (!_includedProjects.Any())
        {
            Output.WriteError("No projects to include, filter not generated");
            return 1;
        }

        Output.WriteLine();
        Output.WriteLine($"Create: '{_solutionFilterFilePath}'");

        var solutionFilter = new SolutionFilter(_solutionPath, _includedProjects.OrderBy(item => item, StringComparer.Ordinal).ToArray());

        var filterText = JsonSerializer.Serialize(solutionFilter, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true });

        File.WriteAllText(_solutionFilterFilePath, filterText);

        return 0;
    }

    private void AddProject(ProjectInfo projectInfo, int level = 0)
    {
        if (!_includedProjects.Add(projectInfo.ProjectReference.RelativePath))
            return;

        if (level > 0)
        {
            Output.WriteLine($"{new string(' ', 2 * level)}- {projectInfo.ProjectReference.RelativePath}");
        }

        ProjectInfo? FindProject(string referencePath)
        {
            var fullPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(projectInfo.ProjectReference.AbsolutePath) ?? ".", referencePath));

            return _projects.FirstOrDefault(project => string.Equals(project.ProjectReference.AbsolutePath, fullPath, StringComparison.OrdinalIgnoreCase));
        }

        var projectReferences = projectInfo.Project
            .GetItems("ProjectReference")
            .Select(item => item.EvaluatedInclude)
            .Select(FindProject)
            .ExceptNullItems()
            .ToArray();

        foreach (var reference in projectReferences)
        {
            AddProject(reference, level + 1);
        }
    }

    private bool ShouldInclude(ProjectInfo projectInfo)
    {
        if (_includedProjects.Contains(projectInfo.ProjectReference.RelativePath))
            return false;

        var project = projectInfo.Project;

        var property = project.GetProperty(IncludeInSolutionFilter);

        return bool.TryParse(property?.EvaluatedValue, out var include) && include;
    }

    private static IEnumerable<ProjectInfo?> LoadProjects(SolutionFile solution, string solutionFilterName)
    {
        var globalProperties = new Dictionary<string, string>
        {
            { SolutionFilterName, solutionFilterName }
        };

        foreach (var projectReference in solution.ProjectsInOrder)
        {
            if (projectReference.ProjectType == SolutionProjectType.SolutionFolder)
                continue;

            var projectInfo = default(ProjectInfo);

            try
            {
                Output.WriteLine($"Load: {projectReference.RelativePath}");

                var project = new Project(projectReference.AbsolutePath, globalProperties, null);

                var filterName = project.GetProperty(SolutionFilterName)?.EvaluatedValue;

                if (filterName != solutionFilterName)
                    throw new InvalidOperationException($"The property {SolutionFilterName} is not set for the project");

                Output.WriteLine($"  - {IncludeInSolutionFilter}: {project.GetProperty(IncludeInSolutionFilter)?.EvaluatedValue}");

                projectInfo = new ProjectInfo(projectReference, project);
            }
            catch (Exception ex)
            {
                Output.WriteWarning($"Loading failed: {ex.Message}");
            }

            yield return projectInfo;
        }
    }

    private sealed record ProjectInfo(ProjectInSolution ProjectReference, Project Project);
}
