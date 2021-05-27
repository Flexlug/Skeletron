﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

using WAV_Bot_DSharp.Database.Models;
using WAV_Bot_DSharp.Services.Interfaces;

namespace WAV_Bot_DSharp.Services
{
    public class SheetGenerator : ISheetGenerator
    {
        public async Task<FileInfo> CompitScoresToFile(List<CompitScore> scores)
        {
            string filePath = $"temp/{DateTime.Now.Ticks}-scores.xlsx";

            //Создаем новый документ
            using (SpreadsheetDocument document = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
            {
                WorkbookPart workbookPart = document.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();
                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();

                FileVersion fv = new FileVersion();
                fv.ApplicationName = "Microsoft Office Excel";
                worksheetPart.Worksheet = new Worksheet(new SheetData());
                WorkbookStylesPart wbsp = workbookPart.AddNewPart<WorkbookStylesPart>();

                // Добавляем в документ набор стилей
                wbsp.Stylesheet = GenerateStyleSheet();
                wbsp.Stylesheet.Save();

                // Задаем колонки и их ширину
                Columns lstColumns = worksheetPart.Worksheet.GetFirstChild<Columns>();
                Boolean needToInsertColumns = false;
                if (lstColumns == null)
                {
                    lstColumns = new Columns();
                    needToInsertColumns = true;
                }

                for (int i = 1; i <= 18; i++) 
                    lstColumns.Append(new Column() { Min = 1, Max = 10, Width = 10, CustomWidth = true });

                if (needToInsertColumns)
                    worksheetPart.Worksheet.InsertAt(lstColumns, 0);


                // Создаем лист в книге
                Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Топ скоры" };
                sheets.Append(sheet);

                SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                // Добавим заголовки в первую строку
                Row row = new Row() { RowIndex = 1 };
                sheetData.Append(row);

                InsertCell(row, 1, "Beginner", CellValues.String);
                InsertCell(row, 2, " ", CellValues.String);
                InsertCell(row, 3, " ", CellValues.String);
                InsertCell(row, 4, "Alpha", CellValues.String);
                InsertCell(row, 5, " ", CellValues.String);
                InsertCell(row, 6, " ", CellValues.String);
                InsertCell(row, 7, "Beta", CellValues.String);
                InsertCell(row, 8, " ", CellValues.String);
                InsertCell(row, 9, " ", CellValues.String);
                InsertCell(row, 10, "Gamma", CellValues.String);
                InsertCell(row, 11, " ", CellValues.String);
                InsertCell(row, 12, " ", CellValues.String);
                InsertCell(row, 13, "Delta", CellValues.String);
                InsertCell(row, 14, " ", CellValues.String);
                InsertCell(row, 15, " ", CellValues.String);
                InsertCell(row, 16, "Epsilon", CellValues.String);
                InsertCell(row, 17, " ", CellValues.String);
                InsertCell(row, 18, " ", CellValues.String);

                // Получаем словарь скоров
                var groupedScores = GroupScoresByCategories(scores);

                // Создаем нужное количество строк
                List<Row> rows = new List<Row>();
                int rowsCount = groupedScores.Select(x => x.Value).Max(x => x.Count);

                // Итератор начинается с 2, чтобы соответствовать номерам строк
                for (int i = 2; i < rowsCount + 2; i++) 
                {
                    Row scoresRow = new Row() { RowIndex = (uint)i };
                    rows.Add(scoresRow);
                    sheetData.Append(scoresRow);
                }

                for (int cat = 0; cat < groupedScores.Count; cat++)
                {
                    List<CompitScore> catScores = groupedScores[(CompitCategories)cat];

                    for (int i = 0; i < catScores.Count; i++)
                    {
                        InsertCell(rows[i], (cat * 3) + 1, catScores[i].Nickname, CellValues.String);
                        InsertCell(rows[i], (cat * 3) + 2, catScores[i].Score.ToString(), CellValues.Number);
                        InsertCell(rows[i], (cat * 3) + 3, catScores[i].ScoreUrl, CellValues.String);
                    }
                }

                workbookPart.Workbook.Save();
                document.Close();
            }

            FileInfo file = new FileInfo(filePath);
            return file;
        }

        private Dictionary<CompitCategories, List<CompitScore>> GroupScoresByCategories(List<CompitScore> rawScores)
        {
            Dictionary<CompitCategories, List<CompitScore>> allScores = new Dictionary<CompitCategories, List<CompitScore>>();

            allScores.Add(CompitCategories.Beginner, GetBestByCategory(rawScores, CompitCategories.Beginner));
            allScores.Add(CompitCategories.Alpha, GetBestByCategory(rawScores, CompitCategories.Alpha));
            allScores.Add(CompitCategories.Beta, GetBestByCategory(rawScores, CompitCategories.Beta));
            allScores.Add(CompitCategories.Gamma, GetBestByCategory(rawScores, CompitCategories.Gamma));
            allScores.Add(CompitCategories.Delta, GetBestByCategory(rawScores, CompitCategories.Delta));
            allScores.Add(CompitCategories.Epsilon, GetBestByCategory(rawScores, CompitCategories.Epsilon));

            return allScores;
        }

        private List<CompitScore> GetBestByCategory(List<CompitScore> rawScores, CompitCategories category)
        {
            List<IGrouping<string, CompitScore>> scoresGroups = rawScores.Select(x => x)
                                                             .Where(x => x.Category == category)
                                                             .GroupBy(x => x.Nickname)
                                                             .ToList();

            List<CompitScore> bestScores = scoresGroups.Select(x => x.Select(x => x)
                                                           .OrderByDescending(x => x.Score)
                                                           .First())
                                                           .ToList();

            return bestScores;
        }

        //Добавление Ячейки в строку (На вход подаем: строку, номер колонки, тип значения, стиль)
        private void InsertCell(Row row, int cell_num, string val, CellValues type, uint styleIndex = 0)
        {
            Cell refCell = null;
            Cell newCell = new Cell() { CellReference = cell_num.ToString() + ":" + row.RowIndex.ToString(), StyleIndex = styleIndex };
            row.InsertBefore(newCell, refCell);

            // Устанавливает тип значения.
            newCell.CellValue = new CellValue(val);
            newCell.DataType = new EnumValue<CellValues>(type);
        }

        private void InsertCell3(Row row, int cell_num, int row_num, string val, WorksheetPart worksheetPart, SheetData sheetData, CellValues type, uint styleIndex = 0)
        {
            Cell refCell = null;
            Cell newCell1 = new Cell() { CellReference = cell_num.ToString() + ":" + row.RowIndex.ToString(), StyleIndex = styleIndex };
            Cell newCell2 = new Cell() { CellReference = (cell_num + 1).ToString() + ":" + row.RowIndex.ToString(), StyleIndex = styleIndex };
            Cell newCell3 = new Cell() { CellReference = (cell_num + 2).ToString() + ":" + row.RowIndex.ToString(), StyleIndex = styleIndex };
            row.InsertBefore(newCell1, refCell);
            row.InsertBefore(newCell2, refCell);
            row.InsertBefore(newCell3, refCell);

            // Устанавливает тип значения.
            newCell1.CellValue = new CellValue(val);
            newCell1.DataType = new EnumValue<CellValues>(type);

            MergeCells mergeCells = new MergeCells();

            MergeCell mergeCell = new MergeCell() { Reference = new StringValue((char)(cell_num + 64) + row_num.ToString() + ':' + (char)(cell_num + 66) + row_num.ToString()) };
            mergeCells.Append(mergeCell);

            worksheetPart.Worksheet.InsertAfter(mergeCells, worksheetPart.Worksheet.Elements<SheetData>().First());
        }

        //Важный метод, при вставки текстовых значений надо использовать.
        //Метод убирает из строки запрещенные спец символы.
        //Если не использовать, то при наличии в строке таких символов, вылетит ошибка.
        private string ReplaceHexadecimalSymbols(string txt)
        {
            string r = "[\x00-\x08\x0B\x0C\x0E-\x1F\x26]";
            return Regex.Replace(txt, r, "", RegexOptions.Compiled);
        }

        //Метод генерирует стили для ячеек (за основу взят код, найденный где-то в интернете)
        private Stylesheet GenerateStyleSheet()
        {
            return new Stylesheet(
                new Fonts(
                    new Font(                                                               // Стиль под номером 0 - Шрифт по умолчанию.
                        new FontSize() { Val = 11 },
                        new Color() { Rgb = new HexBinaryValue() { Value = "000000" } },
                        new FontName() { Val = "Calibri" })
                ),
                new Fills(
                    new Fill(                                                           // Стиль под номером 0 - Заполнение ячейки по умолчанию.
                        new PatternFill() { PatternType = PatternValues.None })
                )
                ,
                new Borders(
                    new Border(                                                         // Стиль под номером 0 - Грани.
                        new LeftBorder(),
                        new RightBorder(),
                        new TopBorder(),
                        new BottomBorder(),
                        new DiagonalBorder())
                ),
                new CellFormats(
                    new CellFormat() { FontId = 0, FillId = 0, BorderId = 0 }                         // Стиль под номером 0 - The default cell style.  (по умолчанию)
                )
            ); // Выход
        }
    }
}
