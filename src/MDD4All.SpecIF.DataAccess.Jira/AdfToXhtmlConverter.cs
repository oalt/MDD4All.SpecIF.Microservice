using MDD4All.Jira.DataModels.V3.ADF;
using System;
using System.Linq;

namespace MDD4All.SpecIF.DataAccess.Jira
{
    public class AdfToXhtmlConverter
    {

        public string ConvertAdfToXhtml(AtlassianDocumentFormat atlassianDocument)
        {
            string result = "";

            if (atlassianDocument != null)
            {
                foreach (Content content in atlassianDocument.Content)
                {
                    result += ConvertContent(content);
                }
            }

            return result;
        }

        private string ConvertContent(Content content)
        {
            string result = "";

            switch(content.Type)
            {
                case AdfTypes.PARAGRAPH:
                    result += ConvertParagraph(content);
                    break;

                case AdfTypes.BULLET_LIST:
                    result += ConvertBulletList(content);
                    break;

                case AdfTypes.ORDERED_LIST:
                    result += ConvertOrderedList(content);
                    break;
            }

            return result;
        }

        

        private string ConvertParagraph(Content paragraph, bool addNewLine = true)
        {

            string result = "";

            result += "<p>";

            bool italic = false;
            bool bold = false;
            bool underline = false;

            foreach(Content paragraphContent in paragraph.Contents)
            {
                if(paragraphContent.Marks != null) // inline formating
                {
                    if(paragraphContent.Marks.Any(mark => mark.Type != null && mark.Type == AdfMarks.EM))
                    {
                        italic = true;
                        result += "<i>";
                    }
                    if (paragraphContent.Marks.Any(mark => mark.Type != null && mark.Type == AdfMarks.STRONG))
                    {
                        bold = true;
                        result += "<b>";
                    }
                    if (paragraphContent.Marks.Any(mark => mark.Type != null && mark.Type == AdfMarks.UNDERLINE))
                    {
                        underline = true;
                        result += "<u>";
                    }


                    result += paragraphContent.Text;

                    // add closing tags in reverse order to opened tags
                    if(underline)
                    {
                        result += "</u>";
                    }
                    if(bold)
                    {
                        result += "</b>";
                    }
                    if(italic)
                    {
                        result += "</i>";
                    }
                }
                else
                {
                    result += paragraphContent.Text;
                }
            }

            result += "</p>";

            if (addNewLine)
            {
                result += Environment.NewLine;
            }

            return result;
        }

        private string ConvertBulletList(Content bulletList)
        {
            string result = "";

            result += "<ul>";

            foreach(Content listItem in bulletList.Contents)
            {
                if(listItem.Type == AdfTypes.LIST_ITEM)
                {
                    result += ConvertListItem(listItem);
                }
                else if (listItem.Type == AdfTypes.ORDERED_LIST)
                {
                    result = ConvertOrderedList(listItem);
                }
                else if (listItem.Type == AdfTypes.BULLET_LIST)
                {
                    result = ConvertBulletList(listItem);
                }
            }

            result += "</ul>";

            return result;
        }

        private string ConvertOrderedList(Content orderedList)
        {
            string result = "";

            result += "<ol>";

            foreach (Content listItem in orderedList.Contents)
            {
                if (listItem.Type == AdfTypes.LIST_ITEM)
                {
                    result += ConvertListItem(listItem);
                }
                else if(listItem.Type == AdfTypes.ORDERED_LIST)
                {
                    result = ConvertOrderedList(listItem);
                }
                else if (listItem.Type == AdfTypes.BULLET_LIST)
                {
                    result = ConvertBulletList(listItem);
                }
            }

            result += "</ol>";

            return result;
        }

        private string ConvertListItem(Content listItem)
        {
            string result = "";

            result += "<li>";

            foreach(Content listItemContent in listItem.Contents)
            {
                if(listItemContent.Type == AdfTypes.PARAGRAPH)
                {
                    result += ConvertParagraph(listItemContent, false);
                }
            }

            result += "</li>" + Environment.NewLine;

            return result;
        }


    }
}
