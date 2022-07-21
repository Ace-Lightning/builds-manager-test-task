namespace BuildsManagerTestTask.Models;

internal class Branch
{
    public string Name { get; set; }

    public Commit Commit { get; set; }

    public bool Protected { get; set; }
}
