using Microsoft.AspNetCore.Hosting;

namespace DevTools.Application.Services.Impl;

public class TemplateService : ITemplateService
{
    private readonly string _templatesPath;

    public TemplateService(IWebHostEnvironment hostingEnvironment)
    {
        _templatesPath = Path.Combine(hostingEnvironment.ContentRootPath, "Templates");
    }

    public async Task<string> GetTemplateAsync(string templateName)
    {
        using var reader = new StreamReader(Path.Combine(_templatesPath, templateName));

        return await reader.ReadToEndAsync();
    }

    public string ReplaceInTemplate(string input, IDictionary<string, string> replaceWords)
    {
        var response = input;
        foreach (var temp in replaceWords)
            response = response.Replace(temp.Key, temp.Value);
        return response;
    }
}
