using System;
using System.Collections.Generic;
using System.Text;

namespace MDD4All.SpecIF.Generators.Vocabulary.DataModels
{
    public class Table
    {
        /// <summary>
        /// List of table rows
        /// </summary>
        public List<List<TableCell>> TableCells { get; set; } = new List<List<TableCell>>();

        public int GetRequiredColumnContentWidth(int columnIndex)
        {
            int result = 0;

            foreach(List<TableCell> row in TableCells)
            {
                int width = row[columnIndex].MaximumWidth;
                if(width > result)
                {
                    result = width;
                }
            }

            return result;
        }

        public string GenerateGridTable()
        {
            string result = "";

            if(TableCells.Count > 0)
            {
                List<TableCell> headerRow = TableCells[0];

                List<int> columnWidths = CalculateRequiredColumnWidths();

                result += GenerateRowSeparator(columnWidths) + Environment.NewLine;

                int rowConter = 0;

                foreach(List<TableCell> row in TableCells)
                {
                    int contentLines = CalculateNecessaryLines(row);

                    for (int linesCounter = 0; linesCounter < contentLines; linesCounter++)
                    {
                        for (int columnCounter = 0; columnCounter < headerRow.Count; columnCounter++)
                        {
                            TableCell currentCell = row[columnCounter];


                            string lineContent = "";
                            

                            if (currentCell.Content.Count > linesCounter)
                            {
                                 lineContent = currentCell.Content[linesCounter];

                                 lineContent += GetWhiteSpace(columnWidths[columnCounter] - lineContent.Length);


                            }
                            else
                            {
                                lineContent += GetWhiteSpace(columnWidths[columnCounter]);
                            }
                            result += "| " + lineContent + " ";
                        }

                        result += "|" + Environment.NewLine;
                    }

                    if (rowConter == 0)
                    {
                        result += GenerateHeaderSeparator(columnWidths) + Environment.NewLine;
                    }
                    else
                    {
                        result += GenerateRowSeparator(columnWidths) + Environment.NewLine;
                    }

                    rowConter++;
                }
                
            }

            result += Environment.NewLine;

            return result;
        }

        private int CalculateNecessaryLines(List<TableCell> row)
        {
            int result = 0;

            foreach(TableCell tableCell in row)
            {
                if(tableCell.Content.Count > result)
                {
                    result = tableCell.Content.Count;
                }
            }

            return result;
        }

        private string GetWhiteSpace(int length)
        {
            string result = "";

            for (int counter = 0; counter < length; counter++)
            {
                result += " ";
            }

            return result;
        }

        private List<int> CalculateRequiredColumnWidths()
        {
            List<int> result = new List<int>();

            List<TableCell> headerRow = TableCells[0];

            for (int columnCounter = 0; columnCounter < headerRow.Count; columnCounter++)
            {
                int columnWidth = 0;
                foreach(List<TableCell> row in TableCells)
                {
                    if(row[columnCounter].MaximumWidth > columnWidth)
                    {
                        columnWidth = row[columnCounter].MaximumWidth;
                    }
                }

                result.Add(columnWidth);
            }

            return result;
        }

        private string GenerateRowSeparator(List<int> columnWidths)
        {
            string result = "";

            foreach(int width in columnWidths)
            {
                result += "+";
                for (int counter = 0; counter < width + 2; counter++)
                {
                    result += "-";
                }
            }

            result += "+";

            return result;
        }

        private string GenerateHeaderSeparator(List<int> columnWidths)
        {
            string result = "";

            foreach (int width in columnWidths)
            {
                result += "+";
                for (int counter = 0; counter < width + 2; counter++)
                {
                    result += "=";
                }
            }

            result += "+";

            return result;
        }
    }
}
