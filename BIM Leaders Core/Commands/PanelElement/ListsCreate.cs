using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using BIM_Leaders_Windows;
using Autodesk.Revit.DB.Architecture;

namespace BIM_Leaders_Core
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class ListsCreate : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Get Document
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            string viewNamesPrefix = "LIST_";
            bool createAluminium = true;
            bool createMetal = true;
            bool createCarpentry = true;
            string markAluminium = "A";
            string markMetal = "M";
            string markCarpentry = "C";
            string viewTypeName = "Lists";

            try
            {
                /*
                ListsCreateForm form = new ListsCreateForm();
                form.ShowDialog();

                if (form.DialogResult == false)
                    return Result.Cancelled;

                // Get user provided information from window
                ListsCreateData data = form.DataContext as ListsCreateData;
                */

                List<Element> elementsAll = GetElements(doc);
                List<Element> elementsAluminium = FilterElements(elementsAll, markAluminium);
                //List<Element> elementsMetal = FilterElements(elementsAll, markMetal);
                //List<Element> elementsCarpentry = FilterElements(elementsAll, markCarpentry);

                using (Transaction trans = new Transaction(doc, "Create Lists"))
                {
                    trans.Start();

                    List<View> viewsAluminium = CreateListViews(doc, viewTypeName, elementsAluminium);

                    ShowResult(viewsAluminium.Count, viewsAluminium.Count, viewsAluminium.Count);

                    trans.Commit();
                }
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
        }

        /// <summary>
        /// Get elements that for lists creating.
        /// </summary>
        /// <returns>List<Element> of unique elements of each type.</returns>
        private static List<Element> GetElements(Document doc)
        {
            List<Element> elements = new List<Element>();

            // Filter.
            List<BuiltInCategory> categories = new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_Doors,
                BuiltInCategory.OST_Windows,
            };
            List<Type> classes = new List<Type>()
            {
                typeof(FamilyInstance),
                typeof(Railing),
                typeof(Wall)
            };
            ElementMulticlassFilter filter = new ElementMulticlassFilter(classes);

            // Get elements.
            IEnumerable<Element> elementsAll = new FilteredElementCollector(doc, doc.ActiveView.Id)
                .WherePasses(filter)
                .WhereElementIsNotElementType()
                .ToElements();

            // Get list of elements with only one element of each type.
            List<ElementId> elementsTypes = new List<ElementId>();
            foreach (Element element in elementsAll)
            {
                ElementId elementType = element.GetTypeId();

                if (elementsTypes.Contains(elementType))
                    continue;

                elementsTypes.Add(elementType);
                elements.Add(element);
            }

            return elements;
        }

        /// <summary>
        /// Filter list of elements that have Type Comments with the given value.
        /// </summary>
        /// <returns>List<Element> of filtered elements.</returns>
        private static List<Element> FilterElements(List<Element> elements, string valueTypeComments)
        {
            List<Element> elementsFiltered = new List<Element>();

            Document doc = elements.First().Document;

            // Filter elements for given list.
            foreach (Element element in elements)
            {
                ElementId elementTypeId = element.GetTypeId();

                if (elementTypeId == ElementId.InvalidElementId)
                    continue;

                Element elementType = doc.GetElement(elementTypeId);

#if VERSION2020 || VERSION2021
                Parameter parameter = elementType.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS);
#else
                Parameter parameter = elementType.GetParameter(ParameterTypeId.AllModelTypeComments);
#endif
                if (parameter == null)
                    continue;

                if (parameter.AsString() == valueTypeComments)
                    elementsFiltered.Add(element);
            }
            return elementsFiltered;
        }

        /// <summary>
        /// Create views for all the given lists. Views will be facing to the front side of the elements.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="viewTypeName">View Type name for new views.</param>
        /// <param name="elements">List of elements that need views.</param>
        /// <returns>List of created views.</returns>
        /// <exception cref="Exception">View type with given name was not found.</exception>
        private static List<View> CreateListViews(Document doc, string viewTypeName, List<Element> elements)
        {
            List<View> viewsCreated = new List<View>(); 

            const double SECTION_SIDES_EXTENSION = 1;

            List<Element> views = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSection))
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();

            ElementId viewType = null;
            foreach (Element view in views)
            {
                ElementId viewTypeI = view.GetTypeId();
                string viewTypeNameI = doc.GetElement(viewTypeI).Name;
                if (viewTypeNameI == viewTypeName)
                {
                    viewType = viewTypeI;
                    break;
                }
            }

            if (viewType == null)
            {
                TaskDialog.Show("Create Lists", "View type with given name was not found.");
                return null;
            }

            foreach (Element element in elements)
            {
                BoundingBoxXYZ sectionBox = new BoundingBoxXYZ(); 

                if (element.GetType() == typeof(FamilyInstance))
                {
                    FamilyInstance familyInstance = element as FamilyInstance;

                    Wall wall = familyInstance.Host as Wall;
                    double wallThickness = wall.WallType.Width;
#if VERSION2020 || VERSION2021
                    double instanceWidth = familyInstance.get_Parameter(BuiltInParameter.CASEWORK_WIDTH).AsDouble();
                    double wallHeight = wall.get_Parameter(BuiltInParameter.WALL_ATTR_HEIGHT_PARAM).AsDouble();
#else
                    double instanceWidth = familyInstance.GetParameter(ParameterTypeId.FamilyWidthParam).AsDouble();
                    double wallHeight = wall.GetParameter(ParameterTypeId.WallAttrHeightParam).AsDouble();
#endif
                    double sectionBoxWidth = instanceWidth + 2 * SECTION_SIDES_EXTENSION;
                    double sectionBoxDepth = wallThickness + 2 * SECTION_SIDES_EXTENSION;
                    double sectionBoxHeight = wallHeight + 2 * SECTION_SIDES_EXTENSION;

                    Transform instanceTransform = familyInstance.GetTransform();

                    XYZ sectionBoxMin = new XYZ(-sectionBoxWidth / 2, -sectionBoxDepth / 2, -SECTION_SIDES_EXTENSION);
                    XYZ sectionBoxMax = new XYZ(sectionBoxWidth / 2, sectionBoxDepth / 2, sectionBoxHeight);

                    sectionBox.Min = sectionBoxMin;
                    sectionBox.Max = sectionBoxMax;
                    sectionBox.Transform = instanceTransform;
                }
                if (element.GetType() == typeof(Wall)) ;
                if (element.GetType() == typeof(Railing)) ;

                ViewSection section = ViewSection.CreateSection(doc, viewType, sectionBox);
                viewsCreated.Add(section);
            }
            return viewsCreated;
        }

        private static void ShowResult(int viewsAluminium, int viewsMetal, int viewsCarpentry)
        {
            // Show result
            string text = "";
            if (viewsAluminium + 0 + 0 == 0)
                text = "No views were created.";
            else
            {
                if (viewsAluminium > 0)
                    text += $"{viewsAluminium} views for aluminium were created.";
                if (viewsMetal > 0)
                {
                    if (text.Length > 0)
                        text += " ";
                    text += $"{viewsMetal} views for metal were created.";
                }
                if (viewsCarpentry > 0)
                {
                    if (text.Length > 0)
                        text += " ";
                    text += $"{viewsCarpentry} views for carpentry were created.";
                }
            }

            TaskDialog.Show("Create Lists", text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(ListsCreate).Namespace + "." + nameof(ListsCreate);
        }
    }
}
