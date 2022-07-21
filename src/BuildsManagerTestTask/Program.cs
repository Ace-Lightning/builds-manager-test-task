using BuildsManagerTestTask.Clients;
using BuildsManagerTestTask.Models;

const string completeBuildStatus = "completed";
const string baseUri = "https://api.appcenter.ms";

Console.WriteLine($"========= Build Manager for AppCenter ({DateTime.Now:g}) =========");

if (args.Length < 3)
{
    Console.WriteLine("You missed one or more required parameters when started the programm. Please check it and try again.");
    Environment.Exit(1);
}

string ownerName = args[0];
string appName = args[1];
string token = args[2];
Console.WriteLine($"Started for application: {appName} ({ownerName})");

AppCenterClient client = new AppCenterClient(token, baseUri);

IEnumerable<BranchStatus> branchesStatuses = Enumerable.Empty<BranchStatus>();

try
{
    branchesStatuses = await client.GetBranchesAsync(ownerName, appName);
}
catch (Exception ex)
{
    Console.WriteLine($"Error has occurred while getting list of branches: {ex.Message}");
    Environment.Exit(1);
}
 
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
    try
    {
        var startedBuild = await client.StartBuildAsync(ownerName, appName, branchStatus.Branch.Name, branchStatus.Branch.Commit.Sha);
        startedBuilds.Add(startedBuild);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error has occurred while starting build for branch {branchStatus.Branch.Name}: {ex.Message}");
    }
}

Console.WriteLine($"Started {startedBuilds.Count} build(s)");

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
        if (!startedBuilds[i].Status.Equals(completeBuildStatus, StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                startedBuilds[i] = await client.GetBuildAsync(ownerName, appName, startedBuilds[i].Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error has occurred while updating status of build {startedBuilds[i].Id}: {ex.Message}");
                // TODO: implement a cancelation for checking of build status by timeout to avoid endless loop
            }
        }
    }
} while (!startedBuilds.All((build) => build.Status.Equals(completeBuildStatus, StringComparison.OrdinalIgnoreCase)));

Console.WriteLine("Results:");

foreach (Build build in startedBuilds)
{
    TimeSpan buildDuration = build.FinishTime - build.StartTime;
    string logsLink = $"{baseUri}/download?url=/v0.1/apps/{ownerName}/{appName}/builds/{build.Id}/downloads/logs";
    Console.WriteLine($" - {build.SourceBranch} build {build.Result} in {buildDuration.TotalSeconds} seconds. Link to build logs: {logsLink}");
}