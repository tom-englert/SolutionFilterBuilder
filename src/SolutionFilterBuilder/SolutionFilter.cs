// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

internal sealed class SolutionFilter
{
    public SolutionFilter(string solutionPath, IList<string> projects)
    {
        Solution = new Solution(solutionPath, projects);
    }

    public string Description { get; } = "This filter was auto-generated using 'https://github.com/tom-englert/SolutionFilterBuilder'";

    public Solution Solution { get; }

    public void AddProject(string projectPath)
    {
        Solution.Projects.Add(projectPath);
    }
}

internal sealed class Solution
{
    public Solution(string path, IList<string> projects)
    {
        Path = path;
        Projects = projects;
    }

    public string Path { get; }

    public IList<string> Projects { get; }
}
