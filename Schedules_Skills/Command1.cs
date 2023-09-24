#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

#endregion

namespace Schedules_Skills
{
    [Transaction(TransactionMode.Manual)]
    public class Command1 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // this is a variable for the Revit application
            UIApplication uiapp = commandData.Application;

            // this is a variable for the current Revit model
            Document doc = uiapp.ActiveUIDocument.Document;

            // Your code goes here
            using (Transaction t = new Transaction(doc))
            {
                t.Start("Create schedule");
                ElementId catId = new ElementId(BuiltInCategory.OST_Doors);
                ViewSchedule newSchedule = ViewSchedule.CreateSchedule(doc, catId);
                newSchedule.Name = "My Door Schedule";

                FilteredElementCollector doorCollector = new FilteredElementCollector(doc);
                doorCollector.OfCategory(BuiltInCategory.OST_Doors);
                doorCollector.WhereElementIsNotElementType();

                Element doorInst = doorCollector.FirstElement();

                Parameter doorNumParam = doorInst.LookupParameter("Mark");
                Parameter doorLevelParam = doorInst.LookupParameter("Level");


                Parameter doorWidthParam = doorInst.get_Parameter(BuiltInParameter.DOOR_WIDTH);
                Parameter doorHeightParam = doorInst.get_Parameter(BuiltInParameter.DOOR_HEIGHT);
                Parameter doorTypeParam = doorInst.get_Parameter(BuiltInParameter.ELEM_FAMILY_AND_TYPE_PARAM);

                // 02b. Create fields
                ScheduleField doorNumField = newSchedule.Definition.AddField(ScheduleFieldType.Instance, doorNumParam.Id);
                ScheduleField doorLevelField = newSchedule.Definition.AddField(ScheduleFieldType.Instance, doorLevelParam.Id);
                ScheduleField doorWidthField = newSchedule.Definition.AddField(ScheduleFieldType.ElementType, doorWidthParam.Id);
                ScheduleField doorHeightField = newSchedule.Definition.AddField(ScheduleFieldType.ElementType, doorHeightParam.Id);
                ScheduleField doorTypeField = newSchedule.Definition.AddField(ScheduleFieldType.Instance, doorTypeParam.Id);

                doorLevelField.IsHidden = true;
                doorWidthField.DisplayType = ScheduleFieldDisplayType.Totals;

                // 03. Filter by level
                //Level filterLevel = GetLevelByName(doc, "01 - Entry Level");
                Level filterLevel = GetLevelByName(doc, "ACCO - Level 1");

                ScheduleFilter levelFilter = new ScheduleFilter(doorLevelField.FieldId, ScheduleFilterType.Equal, filterLevel.Id);
                newSchedule.Definition.AddFilter(levelFilter);

                //04a. Group schedule data
                ScheduleSortGroupField typeSort = new ScheduleSortGroupField(doorTypeField.FieldId);
                typeSort.ShowHeader = true;
                typeSort.ShowFooter = true;
                typeSort.ShowBlankLine = true;
                newSchedule.Definition.AddSortGroupField(typeSort);

                //04b. Sort Schedule Data
                ScheduleSortGroupField markSort = new ScheduleSortGroupField(doorTypeField.FieldId);
                newSchedule.Definition.AddSortGroupField(markSort);


                //05. Set totals
                newSchedule.Definition.IsItemized = true;
                newSchedule.Definition.ShowGrandTotal = true;
                newSchedule.Definition.ShowGrandTotalTitle = true;
                newSchedule.Definition.ShowGrandTotalCount = true;


                t.Commit();

            }

            //. Filter a list for unique items
            List<string> rawString = new List<string>() { "a", "a", "d", "c", "d", "b", "d" };
            List<string> uniqueStrings = rawString.Distinct().ToList();
            return Result.Succeeded;
        }

        private Level GetLevelByName(Document doc, string levelName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfCategory(BuiltInCategory.OST_Levels);
            collector.WhereElementIsNotElementType();

            foreach (Level curLevel in collector)
            {
                if (curLevel.Name == levelName)
                {
                    return curLevel;
                }
            }
            return null;
        }

        internal static PushButtonData GetButtonData()
        {
            // use this method to define the properties for this command in the Revit ribbon
            string buttonInternalName = "btnCommand1";
            string buttonTitle = "Button 1";

            ButtonDataClass myButtonData1 = new ButtonDataClass(
                buttonInternalName,
                buttonTitle,
                MethodBase.GetCurrentMethod().DeclaringType?.FullName,
                Properties.Resources.Blue_32,
                Properties.Resources.Blue_16,
                "This is a tooltip for Button 1");

            return myButtonData1.Data;
        }
    }
}
