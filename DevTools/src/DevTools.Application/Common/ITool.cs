using DevTools.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace DevTools.Application.Common
{
    public interface ITool
    {
        string Name { get; }

        string Description { get; }

        ToolGroup Group{ get; }

        FormFile Body { get; set; }
    }
}