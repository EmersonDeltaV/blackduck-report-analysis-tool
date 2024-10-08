﻿using BlackduckReportAnalysis.Models;
using ClosedXML.Excel;
using System.Reflection;

namespace BlackduckReportAnalysis
{
    public static class ExcelService
    {
        private static int currentRow = 8;
        private static XLWorkbook xLWorkbook;
        private static IXLWorksheet worksheet;

        /// <summary>
        /// Initializes the Excel workbook and worksheet for Black Duck Security Risks summary report.
        /// </summary>
        public static void Initialize()
        {
            xLWorkbook = new XLWorkbook();
            worksheet = xLWorkbook.Worksheets.Add("Black Duck Security Risks");

            FormatHeader();
        }

        private static void FormatHeader()
        {
            //format general details detail
            worksheet.Range(1, 1, 1, 11).Merge();
            worksheet.Range(2, 1, 2, 11).Merge();
            worksheet.Range(3, 1, 3, 11).Merge();

            worksheet.Cell(1, 1).Value = "Product Name";
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(2, 1).Value = "Version Number";
            worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(3, 1).Value = "Product Iteration (PI) or Date";
            worksheet.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            var cellBefore = worksheet.Cell(4, 1);
            cellBefore.Value = "To be filled out before the review";
            cellBefore.Style.Fill.BackgroundColor = XLColor.Green;
            cellBefore.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            var cellDuring = worksheet.Cell(5, 1);
            cellDuring.Value = "To be filled out during the review";
            cellDuring.Style.Fill.BackgroundColor = XLColor.Yellow;
            cellDuring.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

            worksheet.Range("A7:K7").SetAutoFilter();

            // Extract headers details and populate
            Type headersType = typeof(Models.Headers);
            FieldInfo[] fields = headersType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            for (int i = 0; i < fields.Length; i++)
            {
                string fieldValue = fields[i].GetValue(null)?.ToString() ?? string.Empty;
                worksheet.Cell(7, i + 1).Value = fieldValue;
            }

            //format cells, add colors and borders
            for (int i = 1; i <= 7; i++)
            {
                var cell = worksheet.Cell(7, i);
                cell.Style.Fill.BackgroundColor = XLColor.Green;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
            for (int i = 8; i <= 11; i++)
            {
                var cell = worksheet.Cell(7, i);
                cell.Style.Fill.BackgroundColor = XLColor.Yellow;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
        }

        /// <summary>
        /// Populates a row in the Black Duck Security Risks summary report with the provided row details.
        /// </summary>
        /// <param name="rowDetails">The details of the row to be populated.</param>
        public static void PopulateRow(RowDetails rowDetails)
        {
            worksheet.Cell(currentRow, 1).Value = rowDetails.ApplicationName;
            worksheet.Cell(currentRow, 2).Value = rowDetails.SoftwareComponent;
            worksheet.Cell(currentRow, 3).Value = rowDetails.SecurityRisk;
            worksheet.Cell(currentRow, 4).Value = rowDetails.VulnerabilityId;
            worksheet.Cell(currentRow, 5).Value = rowDetails.RecommendedFix;
            worksheet.Cell(currentRow, 7).Value = rowDetails.MatchType;

            if (string.IsNullOrEmpty(rowDetails.RecommendedFix))
            {
                SeriLogger.Warning($"Row [{currentRow}] No recommended fix found for {rowDetails.ApplicationName} | {rowDetails.SoftwareComponent} | {rowDetails.VulnerabilityId}");
            }
            else
            {
                SeriLogger.Information($"Row [{currentRow}] {rowDetails.ApplicationName} | {rowDetails.SoftwareComponent} | {rowDetails.VulnerabilityId}");
            }

            currentRow++;
        }

        /// <summary>
        /// Saves the Black Duck Security Risks summary report.
        /// </summary>
        public static void SaveReport()
        {
            xLWorkbook.SaveAs(Path.Combine(ConfigService.Config.OutputFilePath, $"blackduck-summary-{DateTime.Now:yyyy-MM-dd-HHmmss}.xlsx"));
            SeriLogger.Information("Blackduck Analysis is completed and report was generated successfully.");
            xLWorkbook.Dispose();
        }
    }
}
