using ClosedXML.Excel;
using VeritoolCollector.Models;

namespace VeritoolCollector.Writers;

internal static class ChecklistUpdater
{
    public static void TryUpdate(RunContext context, IReadOnlyDictionary<string, string> data, Logger logger)
    {
        if (context.ChecklistTemplatePath is null)
        {
            logger.Warn("No checklist template provided. Skipping workbook update.");
            return;
        }

        if (!File.Exists(context.ChecklistTemplatePath))
        {
            logger.Warn($"Checklist template not found: {context.ChecklistTemplatePath}");
            return;
        }

        var targetFileName = $"Checklist_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        var outputPath = Path.Combine(context.DestinationRoot, targetFileName);
        File.Copy(context.ChecklistTemplatePath, outputPath, overwrite: true);
        context.ChecklistOutputPath = outputPath;

        using var workbook = new XLWorkbook(outputPath);
        foreach (var sheetBinding in context.Config.ChecklistBindings)
        {
            if (!workbook.TryGetWorksheet(sheetBinding.Key, out var worksheet))
            {
                logger.Warn($"Worksheet '{sheetBinding.Key}' not found in checklist template.");
                continue;
            }

            foreach (var (dataKey, cellAddress) in sheetBinding.Value)
            {
                if (!data.TryGetValue(dataKey, out var value))
                {
                    continue;
                }

                worksheet.Cell(cellAddress).Value = value;
            }
        }

        workbook.Save();
        logger.Info($"Checklist updated: {outputPath}");
    }
}

