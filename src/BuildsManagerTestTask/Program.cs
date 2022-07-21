using BuildsManagerTestTask.Clients;
using BuildsManagerTestTask.Models;

Console.WriteLine($"========= Build Manager for AppCenter ({DateTime.Now:g}) =========");

if (args.Length < 3)
{
    Console.WriteLine("You missed one or more required parameters when started the programm. Please check it and try again.");
    Environment.Exit(1);
}

string ownerName = args[0];
string appName = args[1];
string token = args[2];
string baseUri = "https://api.appcenter.ms";

Console.WriteLine($"Started for application: {appName} ({ownerName})");

AppCenterClient client = new AppCenterClient(token, baseUri);

var branchesStatuses = await client.GetBranchesAsync(ownerName, appName);
var configuredBranches = branchesStatuses.Where((branch) => branch.Configured);

if (configuredBranches.Count() == 0)
{
    Console.WriteLine("There are no configured branches");
    return;
}

Console.WriteLine("Found branches:");

foreach (BranchStatus branchStatus in configuredBranches)
{
    Console.WriteLine($" - {branchStatus.Branch.Name} ({branchStatus.Branch.Commit.Sha})");
}

Console.WriteLine("Starting builds for the branches...");

var startedBuilds = new List<Build>();

foreach (BranchStatus branchStatus in configuredBranches)
{
    var startedBuild = await client.StartBuildAsync(ownerName, appName, branchStatus.Branch.Name, branchStatus.Branch.Commit.Sha);
    startedBuilds.Add(startedBuild);
}

Console.WriteLine($"Stared {startedBuilds.Count} build(s)");

if (startedBuilds.Count == 0)
{
    return;
}

Console.WriteLine("Waiting for started builds to complete...");

do
{
    await Task.Delay(1000);
    for (int i = 0; i < startedBuilds.Count(); i++)
    {
        if (!startedBuilds[i].Status.Equals("completed", StringComparison.OrdinalIgnoreCase))
        {
            startedBuilds[i] = await client.GetBuildAsync(ownerName, appName, startedBuilds[i].Id);
        }
    }
} while (!startedBuilds.All((build) => build.Status.Equals("completed", StringComparison.OrdinalIgnoreCase)));

Console.WriteLine("Results:");

foreach (Build build in startedBuilds)
{
    TimeSpan buildDuration = build.FinishTime - build.StartTime;
    string logsLink = $"{baseUri}/download?url=/v0.1/apps/{ownerName}/{appName}/builds/{build.Id}/downloads/logs";
    Console.WriteLine($" - {build.SourceBranch} build {build.Result} in {buildDuration.Seconds} seconds. Link to build logs: {logsLink}");
}