using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildsManagerTestTask.Models;

internal class Build
{
    public int Id { get; set; }

    public string BuildNumber { get; set; }

    public DateTime QueueTime { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime FinishTime { get; set; }

    public DateTime LastChangedDate { get; set; }

    public string Status { get; set; }

    public string Result { get; set; }

    public string Reason { get; set; }

    public string SourceBranch { get; set; }

    public string SourceVersion { get; set; }

    public string[] Tags { get; set; }

    public int MyProperty { get; set; }
}
