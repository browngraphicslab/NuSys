using System;
using WinRTXamlToolkit.Controls.DataVisualization;

namespace NuSysApp
{
    public class ToolUtilities
    {
        public static string GetLabel(string title, ToolModel.ToolFilterTypeTitle filter)
        {
            switch (filter)
            {
                case ToolModel.ToolFilterTypeTitle.LastEditedDate:
                case ToolModel.ToolFilterTypeTitle.Date :
                    DateTime dt = DateTime.Parse(title);
                    if (dt.Hour == 0)
                    {
                        return dt.Month + "/" + dt.Day + "/" + dt.Year;
                    }
                    else
                    {
                        return dt.Hour + ":00";
                    }
                case ToolModel.ToolFilterTypeTitle.Creator:
                //case ToolModel.ToolFilterTypeTitle.MetadataKeys:
                case ToolModel.ToolFilterTypeTitle.Title:
                    return "";
                default:
                    return title;
            }
        }
    }
}