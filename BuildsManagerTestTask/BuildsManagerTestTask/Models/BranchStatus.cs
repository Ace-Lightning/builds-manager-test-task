namespace BuildsManagerTestTask.Models;

internal class BranchStatus
{
    public Branch Branch { get; set; }

    public bool Configured { get; set; }

    public Build LastBuild { get; set; }

    public string Trigger { get; set; }
}
