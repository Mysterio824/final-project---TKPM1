using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.ToolContracts
{
    public interface ITool
    {
        string Name { get; }
        string Description { get; }

        ToolType Type { get; }
        string Execute(string input);
    }
}

