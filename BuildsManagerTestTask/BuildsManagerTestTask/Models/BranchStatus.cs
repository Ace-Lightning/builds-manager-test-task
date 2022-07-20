using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildsManagerTestTask.Models;

internal class BranchStatus
{
    public Branch Branch { get; set; }

    public bool Configured { get; set; }

    public Build LastBuild { get; set; }

    public string Trigger { get; set; }
}
