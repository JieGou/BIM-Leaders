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

            bool createAluminium = true;
            bool createMetal = true;
            bool createCarpentry = true;
            bool sortElements = true;
            string markAluminium = "A";
            string markMetal = "M";
            string markCarpentry = "C";
            string viewTypeName = "Lists";
            string viewTemplateName = "Lists";
            string viewNamePrefix = "LIST_";

            string tagFamilyName = "Lists tag GF A4";
            string tagTypeNameAluminium = "Aluminium";
            string tagTypeNameMetal = "Metal";
            string tagTypeNameCarpentry = "Carpentry";
            double tagPlacementOffsetX = 177.5; // In mm.
            double tagPlacementOffsetY = 208.7;

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

                List<Element> elementsAluminium = (createAluminium)
                    ? FilterElements(elementsAll, markAluminium)
                    : null;
                List<Element> elementsMetal = (createMetal)
                    ? FilterElements(elementsAll, markMetal)
                    : null;
                List<Element> elementsCarpentry = (createCarpentry)
                    ? FilterElements(elementsAll, markCarpentry)
                    : null;

                if (sortElements)
                {
                    elementsAluminium = SortElements(elementsAluminium);
                    elementsMetal = SortElements(elementsMetal);
                    elementsCarpentry = SortElements(elementsCarpentry);
                }

                ElementId viewType = GetViewType(doc, viewTypeName);
                ElementId viewTemplate = GetViewTemplate(doc, viewTemplateName);

                ElementId tagTypeAluminium = GetTagType(doc, tagFamilyName, tagTypeNameAluminium);
                ElementId tagTypeMetal = GetTagType(doc, tagFamilyName, tagTypeNameMetal);
                ElementId tagTypeCarpentry = GetTagType(doc, tagFamilyName, tagTypeNameCarpentry);

                using (Transaction trans = new Transaction(doc, "Create Lists"))
                {
                    trans.Start();

                    MarkElements(elementsAluminium);
                    MarkElements(elementsMetal);
                    MarkElements(elementsCarpentry);

                    List<View> viewsAluminium = CreateListViews(doc, elementsAluminium, viewType, viewTemplate, viewNamePrefix, tagTypeAluminium, tagPlacementOffsetX, tagPlacementOffsetY);
                    List<View> viewsMetal = CreateListViews(doc, elementsMetal, viewType, viewTemplate, viewNamePrefix, tagTypeMetal, tagPlacementOffsetX, tagPlacementOffsetY);
                    List<View> viewsCarpentry = CreateListViews(doc, elementsCarpentry, viewType, viewTemplate, viewNamePrefix, tagTypeCarpentry, tagPlacementOffsetX, tagPlacementOffsetY);

                    ShowResult(viewsAluminium.Count, viewsMetal.Count, viewsCarpentry.Count);

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
            IEnumerable<Element> elementsAll = new FilteredElementCollector(doc)
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
                string typeMark = GetTypeMark(doc, element);

                if (typeMark == valueTypeComments)
                    elementsFiltered.Add(element);
            }
            return elementsFiltered;
        }

        /// <summary>
        /// Sort elements lists and type Tape Mark numbers.
        /// </summary>
        /// <param name="elements">List of elements to sort</param>
        /// <returns>Sorted list of elements.</returns>
        private static List<Element> SortElements(List<Element> elements)
        {
            if (elements == null)
                return null;

            List<Element> elementsSorted = new List<Element>();
            
            Dictionary<Element, double> elementsHeight = new Dictionary<Element, double>();
            Dictionary<Element, double> elementsCompar = new Dictionary<Element, double>();

            Document doc = elements[0].Document;

            foreach (Element element in elements)
            {
                Level level = doc.GetElement(element.LevelId) as Level;
                elementsHeight.Add(element, level.Elevation);
            }

            foreach (Element element in elements)
            {
                elementsCompar.Add(element, 0); // COMPARE ELEMENTS ON PLAN (TOP-BOTTOM, CLOCKWISE, ETC.)!
            }

            elementsSorted = elements.OrderBy(x => elementsHeight[x]).ThenBy(x => elementsCompar[x]).ToList();

            return elementsSorted;
        }

        /// <summary>
        /// Get view type ElementId by given view type name.
        /// </summary>
        /// <param name="viewTypeName">Name of the view type.</param>
        /// <returns>View Type.</returns>
        private static ElementId GetViewType(Document doc, string viewTypeName)
        {
            ElementId viewType = null;

            List<Element> views = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSection))
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();

            foreach (Element view in views)
            {
                ElementId viewTypeI = view.GetTypeId();

                if (viewTypeI == ElementId.InvalidElementId)
                    continue;

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

            return viewType;
        }

        /// <summary>
        /// Get view template ElementId by given view type name.
        /// </summary>
        /// <param name="viewTemplateName">Name of the view template.</param>
        /// <returns>View Template.</returns>
        private static ElementId GetViewTemplate(Document doc, string viewTemplateName)
        {
            ElementId viewTemplate = null;

            List<View> views = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .ToElements()
                .Cast<View>()
                .ToList();

            foreach (View view in views)
            {
                if (view.IsTemplate && view.Name == viewTemplateName)
                {
                    viewTemplate = view.Id;
                    break;
                }
            }

            if (viewTemplate == null)
                TaskDialog.Show("Create Lists", "View template with given name was not found.");

            return viewTemplate;
        }

        /// <summary>
        /// Get tag type ElementId by given tag type name.
        /// </summary>
        /// <param name="tagTypeName">Name of the tag type.</param>
        /// <returns>Tag type.</returns>
        private static ElementId GetTagType(Document doc, string tagFamilyName, string tagTypeName)
        {
            ElementId tagType = null;

            IEnumerable<FamilySymbol> tag = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_MultiCategoryTags)
                .Cast<FamilySymbol>()
                .Where(x => x.FamilyName == tagFamilyName)
                .Where(x => x.Name == tagTypeName);

            if (tag.Count() == 0)
            {
                TaskDialog.Show("Create Lists", "Tag type with given name was not found.");
                return null;
            }

            tagType = tag.First().Id;
            return tagType;
        }

        /// <summary>
        /// Set Tape Mark numbers.
        /// </summary>
        /// <param name="elements">List of elements to mark.</param>
        private static void MarkElements(List<Element> elements)
        {
            if (elements == null)
                return;

            Document doc = elements.First().Document;

            //SortElements(elements);

            int counter = 1;
            foreach (Element element in elements)
            {
                ElementId elementTypeId = element.GetTypeId();

                if (elementTypeId == ElementId.InvalidElementId)
                    continue;

                Element elementType = doc.GetElement(elementTypeId);

#if VERSION2020 || VERSION2021
                Parameter parameter = elementType.get_Parameter(BuiltInParameter.WINDOW_TYPE_ID);
#else
                Parameter parameter = elementType.GetParameter(ParameterTypeId.WindowTypeId);
#endif
                parameter.Set(counter.ToString());
                counter++;
            }
        }

        private static List<View> CreateListViews(Document doc, List<Element> elements, ElementId viewType, ElementId viewTemplate, string viewNamePrefix, ElementId tagType, double tagPlacementOffsetX, double tagPlacementOffsetY)
        {
            if (elements == null)
                return null;

            List<View> viewsCreated = new List<View>(); 

            const double SECTION_SIDES_EXTENSION = 1;

            View view = doc.GetElement(viewTemplate) as View;
            double scale = view.Scale;

            foreach (Element element in elements)
            {
                BoundingBoxXYZ sectionBox = new BoundingBoxXYZ();
                XYZ instanceLocation = new XYZ();

                if (element.GetType() == typeof(FamilyInstance))
                {
                    sectionBox = GetSectionBoxInstance(doc, element, SECTION_SIDES_EXTENSION);
                    instanceLocation = GetLocationInstance(element);
                }
                if (element.GetType() == typeof(Wall))
                {

                }
                if (element.GetType() == typeof(Railing))
                {

                }

                ViewSection section = ViewSection.CreateSection(doc, viewType, sectionBox);

                string typeMark = GetTypeMark(doc, element);
                try
                {
                    section.Name = viewNamePrefix + typeMark;
                }
                catch
                {
                    TaskDialog.Show("Create Lists", "Error creating views. Element with empty and/or duplicate Type Marks exist.");
                    return null;
                }

                section.ViewTemplateId = viewTemplate;

                Reference reference = new Reference(element);

                // Move tag on the view because it's on the family point now.
#if VERSION2020 || VERSION2021
                double tagPlacementOffsetXconverted = UnitUtils.ConvertToInternalUnits(tagPlacementOffsetX, DisplayUnitType.DUT_MILLIMETERS);
                double tagPlacementOffsetYconverted = UnitUtils.ConvertToInternalUnits(tagPlacementOffsetY, DisplayUnitType.DUT_MILLIMETERS);
#else
                double tagPlacementOffsetXconverted = UnitUtils.ConvertToInternalUnits(tagPlacementOffsetX, UnitTypeId.Millimeters);
                double tagPlacementOffsetYconverted = UnitUtils.ConvertToInternalUnits(tagPlacementOffsetY, UnitTypeId.Millimeters);
#endif
                double moveTagX = -(section.RightDirection.X * tagPlacementOffsetXconverted * scale);
                double moveTagY = -(section.RightDirection.Y * tagPlacementOffsetXconverted * scale);
                double moveTagZ = (sectionBox.Max.Y - sectionBox.Min.Y) / 2 - tagPlacementOffsetYconverted * scale;
                XYZ moveTag = new XYZ(moveTagX, moveTagY, moveTagZ);
                XYZ tagLocation = instanceLocation.Add(moveTag);

                IndependentTag.Create(doc, tagType, section.Id, reference, false, TagOrientation.Horizontal, tagLocation);

                viewsCreated.Add(section);
            }
            return viewsCreated;
        }

        /// <summary>
        /// Get the Type Mark value from given element.
        /// </summary>
        /// <returns>Type Mark value.</returns>
        private static string GetTypeMark(Document doc, Element element)
        {
            string typeMark = "";

            ElementId elementTypeId = element.GetTypeId();

            if (elementTypeId == ElementId.InvalidElementId)
                return typeMark;

            Element elementType = doc.GetElement(elementTypeId);

#if VERSION2020 || VERSION2021
            Parameter parameter = elementType.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS);
#else
                Parameter parameter = elementType.GetParameter(ParameterTypeId.AllModelTypeComments);
#endif

            return typeMark;
        }

        private static BoundingBoxXYZ GetSectionBoxInstance(Document doc, Element element, double SECTION_SIDES_EXTENSION)
        {
            BoundingBoxXYZ sectionBox = new BoundingBoxXYZ();

            FamilyInstance familyInstance = element as FamilyInstance;

            ElementId elementTypeId = element.GetTypeId();
            if (elementTypeId == ElementId.InvalidElementId)
                return null;
            Element elementType = doc.GetElement(elementTypeId);

            Wall wall = familyInstance.Host as Wall;
            double wallThickness = wall.WallType.Width;

#if VERSION2020 || VERSION2021
            double instanceWidth = elementType.get_Parameter(BuiltInParameter.CASEWORK_WIDTH).AsDouble();
            double wallHeight = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble();
#else
            double instanceWidth = elementType.GetParameter(ParameterTypeId.FamilyWidthParam).AsDouble();
            double wallHeight = wall.GetParameter(ParameterTypeId.WallUserHeightParam).AsDouble();
#endif

            // Get box dimensions, box will cover one half of the element + extension.
            double sectionBoxOriginalWidth = wallThickness + 2 * SECTION_SIDES_EXTENSION;
            double sectionBoxOriginalDepth = instanceWidth / 2 + SECTION_SIDES_EXTENSION;
            double sectionBoxOriginalHeight = wallHeight + 2 * SECTION_SIDES_EXTENSION;
            // Box will be rotated, so depth and height will interchange.
            double sectionBoxWidth = sectionBoxOriginalWidth;
            double sectionBoxDepth = sectionBoxOriginalHeight;
            double sectionBoxHeight = sectionBoxOriginalDepth;

            // Get element transform (location, rotation, etc.).
            // Change it via Z rotation because we need to see not front of element but side of it
            // Change it via X rotation because for section creating Z is looking fro section front.
            Transform instanceTransform = familyInstance.GetTransform();

            Transform rotationZ = Transform.CreateRotation(new XYZ(0, 0, 1), -Math.PI / 2);
            Transform rotationX = Transform.CreateRotation(new XYZ(1, 0, 0), Math.PI / 2);
            Transform rotation = rotationZ.Multiply(rotationX);
            Transform instanceTransformRotated = instanceTransform.Multiply(rotation);

            Transform moveUp = Transform.CreateTranslation(new XYZ(0, sectionBoxOriginalHeight / 2 - SECTION_SIDES_EXTENSION, 0));
            Transform instanceTransformRaised = instanceTransformRotated.Multiply(moveUp);

            XYZ sectionBoxMin = new XYZ(-sectionBoxWidth / 2, -sectionBoxDepth / 2, 0);
            XYZ sectionBoxMax = new XYZ(sectionBoxWidth / 2, sectionBoxDepth / 2, sectionBoxHeight);

            sectionBox.Min = sectionBoxMin;
            sectionBox.Max = sectionBoxMax;
            sectionBox.Transform = instanceTransformRaised;

            return sectionBox;
        }
        
        private static XYZ GetLocationInstance(Element element)
        {
            XYZ instanceLocation = new XYZ();

            FamilyInstance familyInstance = element as FamilyInstance;

            Transform instanceTransform = familyInstance.GetTransform();
            instanceLocation = instanceTransform.Origin;

            return instanceLocation;
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