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
    [Transaction(TransactionMode.Manual)]
    public class ListsCreate : IExternalCommand
    {
        private static Document _doc;
        private static bool _createAluminium = true;
        private static bool _createMetal = true;
        private static bool _createCarpentry = true;
        private static bool _sortElements = true;

        private static string _typeCommentsAluminium = "A";
        private static string _typeCommentsMetal = "M";
        private static string _typeCommentsCarpentry = "C";

        private static string _typeNamePrefixAluminiumWalls = "Aluminium Walls";

        private static string _viewTypeName = "Lists";
        private static string _viewTemplateName = "Lists";
        private static string _viewNamePrefix = "LIST_";

        private static string _tagGenFamilyName = "Lists tag GF A4";
        private static string _tagGenTypeNameAluminium = "Aluminium";
        private static string _tagGenTypeNameMetal = "Metal";
        private static string _tagGenTypeNameCarpentry = "Wood";

        private static string _tagRailingFamilyName = "Lists tag GF A4 Railing";
        private static string _tagRailingTypeName = "Aluminium";

        private static double _tagPlacementOffsetX = 177.5; // In mm.
        private static double _tagPlacementOffsetY = 208.7;

        private const string TRANSACTION_NAME = "Create Lists";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            _doc = commandData.Application.ActiveUIDocument.Document;

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

                List<Element> elementsAll = GetElements();

                List<Element> elementsAluminium = (_createAluminium)
                    ? FilterElements(elementsAll, _typeCommentsAluminium)
                    : null;
                List<Element> elementsMetal = (_createMetal)
                    ? FilterElements(elementsAll, _typeCommentsMetal)
                    : null;
                List<Element> elementsCarpentry = (_createCarpentry)
                    ? FilterElements(elementsAll, _typeCommentsCarpentry)
                    : null;
                
                if (_sortElements)
                {
                    elementsAluminium = SortElements(elementsAluminium);
                    elementsMetal = SortElements(elementsMetal);
                    elementsCarpentry = SortElements(elementsCarpentry);
                }

                ElementId viewType = GetViewType(_viewTypeName);
                ElementId viewTemplate = GetViewTemplate(_viewTemplateName);

                ElementId tagTypeAluminium = GetTagType(BuiltInCategory.OST_MultiCategoryTags, _tagGenFamilyName, _tagGenTypeNameAluminium);
                ElementId tagTypeMetal = GetTagType(BuiltInCategory.OST_MultiCategoryTags, _tagGenFamilyName, _tagGenTypeNameMetal);
                ElementId tagTypeCarpentry = GetTagType(BuiltInCategory.OST_MultiCategoryTags, _tagGenFamilyName, _tagGenTypeNameCarpentry);
                ElementId tagTypeRailing = GetTagType(BuiltInCategory.OST_StairsRailingTags, _tagRailingFamilyName, _tagRailingTypeName);

                using (Transaction trans = new Transaction(_doc, TRANSACTION_NAME))
                {
                    trans.Start();

                    MarkElements(elementsAluminium, _typeNamePrefixAluminiumWalls);
                    MarkElements(elementsMetal, _typeNamePrefixAluminiumWalls);
                    MarkElements(elementsCarpentry, _typeNamePrefixAluminiumWalls);

                    List<View> viewsAluminium = CreateListViews(elementsAluminium, viewType, viewTemplate, _viewNamePrefix, tagTypeAluminium, _tagPlacementOffsetX, _tagPlacementOffsetY);
                    List<View> viewsMetal = CreateListViews(elementsMetal, viewType, viewTemplate, _viewNamePrefix, tagTypeMetal, _tagPlacementOffsetX, _tagPlacementOffsetY);
                    List<View> viewsCarpentry = CreateListViews(elementsCarpentry, viewType, viewTemplate, _viewNamePrefix, tagTypeCarpentry, _tagPlacementOffsetX, _tagPlacementOffsetY);

                    // !!! Create sheets
                    // !!! Place views
                    // !!! Place legend components

                    ShowResult(viewsAluminium?.Count, viewsMetal?.Count, viewsCarpentry?.Count);

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
        private static List<Element> GetElements()
        {
            List<Element> elements = new List<Element>();

            // Filter.
            List<Type> classes = new List<Type>()
            {
                typeof(FamilyInstance),
                typeof(Railing),
                typeof(Wall)
            };
            ElementMulticlassFilter filter = new ElementMulticlassFilter(classes);

            // Get elements.
            IEnumerable<Element> elementsAll = new FilteredElementCollector(_doc)
                .WherePasses(filter)
                .WhereElementIsNotElementType()
                .ToElements();

            // Get list of elements with only one element of each type.
            List<ElementId> elementsTypesCollected = new List<ElementId>();
            foreach (Element element in elementsAll)
            {
                ElementId elementType = element.GetTypeId();

                if (elementsTypesCollected.Contains(elementType))
                    continue;

                elements.Add(element);

                // Curtain walls need to be added of the same type also, so don't count them in Collected Element Types list.
                if (element.GetType() != typeof (Wall))
                    elementsTypesCollected.Add(elementType);
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
                string typeComments = GetTypeComments(element);

                if (typeComments == valueTypeComments)
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
            if (elements == null || elements.Count == 0)
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
        private static ElementId GetViewType(string viewTypeName)
        {
            ElementId viewType = null;

            List<Element> views = new FilteredElementCollector(_doc)
                .OfClass(typeof(ViewSection))
                .WhereElementIsNotElementType()
                .ToElements()
                .ToList();

            foreach (Element view in views)
            {
                ElementId viewTypeI = view.GetTypeId();

                if (viewTypeI == ElementId.InvalidElementId)
                    continue;

                string viewTypeNameI = _doc.GetElement(viewTypeI).Name;
                if (viewTypeNameI == viewTypeName)
                {
                    viewType = viewTypeI;
                    break;
                }
            }

            if (viewType == null)
            {
                TaskDialog.Show(TRANSACTION_NAME, "View type with given name was not found.");
                return null;
            }

            return viewType;
        }

        /// <summary>
        /// Get view template ElementId by given view type name.
        /// </summary>
        /// <param name="viewTemplateName">Name of the view template.</param>
        /// <returns>View Template.</returns>
        private static ElementId GetViewTemplate(string viewTemplateName)
        {
            ElementId viewTemplate = null;

            List<View> views = new FilteredElementCollector(_doc)
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
                TaskDialog.Show(TRANSACTION_NAME, "View template with given name was not found.");

            return viewTemplate;
        }

        /// <summary>
        /// Get tag type ElementId by given tag type name.
        /// </summary>
        /// <param name="tagTypeName">Name of the tag type.</param>
        /// <returns>Tag type.</returns>
        private static ElementId GetTagType(BuiltInCategory category, string tagFamilyName, string tagTypeName)
        {
            IEnumerable<Element> tags = new FilteredElementCollector(_doc)
                .OfClass(typeof(IndependentTag))
                .OfCategory(category)
                .ToElements();

            foreach (Element tag in tags)
            {
                FamilySymbol tagTypeI = _doc.GetElement(tag.GetTypeId()) as FamilySymbol;
                if (tagTypeI.Name == tagTypeName && tagTypeI.FamilyName == tagFamilyName)
                    return tagTypeI.Id;
            }

            TaskDialog.Show(TRANSACTION_NAME, $"Tag type with given name {tagFamilyName} / {tagTypeName} was not found.");
            return null;
        }

        /// <summary>
        /// Get the Type Mark value from given element.
        /// </summary>
        /// <returns>Type Mark value.</returns>
        private static string GetTypeMark(Element element)
        {
            string typeMark = "";

            ElementId elementTypeId = element.GetTypeId();

            if (elementTypeId == ElementId.InvalidElementId)
                return typeMark;

            Element elementType = _doc.GetElement(elementTypeId);

#if VERSION2020 || VERSION2021
            Parameter parameter = elementType.get_Parameter(BuiltInParameter.WINDOW_TYPE_ID);
#else
            Parameter parameter = elementType.GetParameter(ParameterTypeId.WindowTypeId);
#endif
            if (parameter != null)
                typeMark = parameter.AsString();

            return typeMark;
        }

        /// <summary>
        /// Get the Type Comments value from given element.
        /// </summary>
        /// <returns>Type Comments value.</returns>
        private static string GetTypeComments(Element element)
        {
            string typeComments = "";

            ElementId elementTypeId = element.GetTypeId();

            if (elementTypeId == ElementId.InvalidElementId)
                return typeComments;

            Element elementType = _doc.GetElement(elementTypeId);

#if VERSION2020 || VERSION2021
            Parameter parameter = elementType.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_COMMENTS);
#else
            Parameter parameter = elementType.GetParameter(ParameterTypeId.AllModelTypeComments);
#endif
            if (parameter != null)
                typeComments = parameter.AsString();

            return typeComments;
        }

        /// <summary>
        /// Set Tape Mark numbers.
        /// </summary>
        /// <param name="elements">List of elements to mark.</param>
        private static void MarkElements(List<Element> elements, string typeNamePrefixAluminiumWalls)
        {
            if (elements == null || elements.Count == 0)
                return;

            Document doc = elements.First().Document;

            int counter = 1;
            foreach (Element element in elements)
            {
                ElementId elementTypeId = element.GetTypeId();

                if (elementTypeId == ElementId.InvalidElementId)
                    continue;

                Element elementType = doc.GetElement(elementTypeId);

                // Arrange Curtain Wall types.
                if (element.GetType() == typeof(Wall))
                {
                    ElementClassFilter wallsFilter = new ElementClassFilter(typeof(Wall));
                    IList<ElementId> wallsSameType = elementType.GetDependentElements(wallsFilter);

                    string elementNewTypeName = typeNamePrefixAluminiumWalls + " " + counter;

                    if (wallsSameType.Count == 1)
                    {
                        try
                        {
                            elementType.Name = elementNewTypeName;
                        }
                        catch
                        {
                            TaskDialog.Show("Create Lists", $"Error renaming aluminium wall type. Name {elementNewTypeName} may be unavailable.");
                        }
                    }
                    else
                    {
                        WallType wallType = elementType as WallType;
                        try
                        {
                            ElementType wallTypeNew = wallType.Duplicate(typeNamePrefixAluminiumWalls + counter);
                            element.ChangeTypeId(wallTypeNew.Id);
                        }
                        catch
                        {
                            TaskDialog.Show("Create Lists", $"Error creating new aluminium wall type. Name {elementNewTypeName} may be unavailable.");
                        }
                    }
                }

#if VERSION2020 || VERSION2021
                Parameter parameter = elementType.get_Parameter(BuiltInParameter.WINDOW_TYPE_ID);
#else
                Parameter parameter = elementType.GetParameter(ParameterTypeId.WindowTypeId);
#endif
                if (parameter != null)
                {
                    parameter.Set(counter.ToString());
                    counter++;
                }
            }
        }

        /// <summary>
        /// Create views for given list elements.
        /// </summary>
        /// <returns>List of created views.</returns>
        private static List<View> CreateListViews(List<Element> elements, ElementId viewType, ElementId viewTemplate, string viewNamePrefix, ElementId tagType, double tagPlacementOffsetX, double tagPlacementOffsetY)
        {
            if (elements == null || elements.Count == 0)
                return null;

            List<View> viewsCreated = new List<View>(); 

            View view = _doc.GetElement(viewTemplate) as View;
            double scale = view.Scale;

            foreach (Element element in elements)
            {
                BoundingBoxXYZ sectionBox = GetSectionBox(element);

                ViewSection section = ViewSection.CreateSection(_doc, viewType, sectionBox);

                string typeMark = GetTypeMark(element);
                if (typeMark == "")
                {
                    TaskDialog.Show("Create Lists", $"Error creating views. Element {element.Id.ToString()} has empty Type Mark.");
                    return null;
                }
                try
                {
                    section.Name = viewNamePrefix + typeMark;
                }
                catch
                {
                    TaskDialog.Show("Create Lists", "Error creating views. Element with duplicate Type Marks exist.");
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
                XYZ tagLocation = sectionBox.Transform.Origin;

                double moveTagX = -(section.RightDirection.X * tagPlacementOffsetXconverted * scale);
                double moveTagY = -(section.RightDirection.Y * tagPlacementOffsetXconverted * scale);
                double moveTagZ = (sectionBox.Max.Y - sectionBox.Min.Y) / 2 - tagPlacementOffsetYconverted * scale;
                XYZ moveTag = new XYZ(moveTagX, moveTagY, moveTagZ);

                XYZ tagLocationMoved = tagLocation.Add(moveTag);

                IndependentTag.Create(_doc, tagType, section.Id, reference, false, TagOrientation.Horizontal, tagLocationMoved);

                viewsCreated.Add(section);
            }
            return viewsCreated;
        }

        /// <summary>
        /// Get section box for section creating.
        /// </summary>
        /// <param name="element">Element that needs a section.</param>
        private static BoundingBoxXYZ GetSectionBox(Element element)
        {
            BoundingBoxXYZ sectionBox = new BoundingBoxXYZ();

            const double SECTION_SIDES_EXTENSION = 1;

            if (element.GetType() == typeof(FamilyInstance))
                sectionBox = GetSectionBoxInstance(element as FamilyInstance, SECTION_SIDES_EXTENSION);
            else if (element.GetType() == typeof(Wall))
                sectionBox = GetSectionBoxWall(element as Wall, SECTION_SIDES_EXTENSION);
            else if (element.GetType() == typeof(Railing))
            {

            }

            return sectionBox;
        }

        /// <summary>
        /// Get bounding box for creating a section view that cuts the given <paramref name="familyInstance"/>.
        /// </summary>
        /// <param name="doc">Document.</param>
        /// <param name="familyInstance">Family instance to cut.</param>
        /// <param name="SECTION_SIDES_EXTENSION">Extension of the bounding box (will apply to left, right and far side).</param>
        /// <returns>Bounding box with right coordinates for section creating (Z is looking to the section direction, etc).</returns>
        private static BoundingBoxXYZ GetSectionBoxInstance(FamilyInstance familyInstance, double SECTION_SIDES_EXTENSION)
        {
            BoundingBoxXYZ sectionBox = new BoundingBoxXYZ();

            ElementId elementTypeId = familyInstance.GetTypeId();
            if (elementTypeId == ElementId.InvalidElementId)
                return null;
            Element elementType = familyInstance.Document.GetElement(elementTypeId);

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

            // Change the dimensions of the section box.
            XYZ sectionBoxMin = new XYZ(-sectionBoxWidth / 2, -sectionBoxDepth / 2, 0);
            XYZ sectionBoxMax = new XYZ(sectionBoxWidth / 2, sectionBoxDepth / 2, sectionBoxHeight);
            sectionBox.Min = sectionBoxMin;
            sectionBox.Max = sectionBoxMax;

            // Move the sexion box to the needed coordinates via Transform.

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

            sectionBox.Transform = instanceTransformRaised;

            return sectionBox;
        }

        /// <summary>
        /// Get bounding box for creating a section view that cuts the given <paramref name="wall"/>.
        /// </summary>
        /// <param name="doc">Document.</param>
        /// <param name="wall">Wall to cut.</param>
        /// <param name="SECTION_SIDES_EXTENSION">Extension of the bounding box (will apply to left, right and far side).</param>
        /// <returns>Bounding box with right coordinates for section creating (Z is looking to the section direction, etc).</returns>
        private static BoundingBoxXYZ GetSectionBoxWall(Wall wall, double SECTION_SIDES_EXTENSION)
        {
            BoundingBoxXYZ sectionBox = new BoundingBoxXYZ();
            XYZ location = new XYZ();

            ElementId elementTypeId = wall.GetTypeId();
            if (elementTypeId == ElementId.InvalidElementId)
                return null;

            Element elementType = wall.Document.GetElement(elementTypeId);

            double wallThickness = wall.WallType.Width;

#if VERSION2020 || VERSION2021
            double wallLength = wall.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH).AsDouble();
            double wallHeight = wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM).AsDouble();
#else
            double wallLength = elementType.GetParameter(ParameterTypeId.CurveElemLength).AsDouble();
            double wallHeight = wall.GetParameter(ParameterTypeId.WallUserHeightParam).AsDouble();
#endif

            // Get box dimensions, box will cover one half of the wall + extension.
            double sectionBoxOriginalWidth = wallThickness + 2 * SECTION_SIDES_EXTENSION;
            double sectionBoxOriginalDepth = wallLength / 2 + SECTION_SIDES_EXTENSION;
            double sectionBoxOriginalHeight = wallHeight + 2 * SECTION_SIDES_EXTENSION;
            // Box will be rotated, so depth and height will interchange.
            double sectionBoxWidth = sectionBoxOriginalWidth;
            double sectionBoxDepth = sectionBoxOriginalHeight;
            double sectionBoxHeight = sectionBoxOriginalDepth;

            // Change the dimensions of the section box.
            XYZ sectionBoxMin = new XYZ(-sectionBoxWidth / 2, -sectionBoxDepth / 2, 0);
            XYZ sectionBoxMax = new XYZ(sectionBoxWidth / 2, sectionBoxDepth / 2, sectionBoxHeight);
            sectionBox.Min = sectionBoxMin;
            sectionBox.Max = sectionBoxMax;

            // Move the sexion box to the needed coordinates via Transform.

            // Get wall location line
            LocationCurve wallLocationCurve = wall.Location as LocationCurve;
            XYZ wallPoint0 = wallLocationCurve.Curve.GetEndPoint(0);
            XYZ wallPoint1 = wallLocationCurve.Curve.GetEndPoint(1);

            XYZ wallCenter = wallPoint0.Add(wallPoint1.Subtract(wallPoint0) / 2);

            XYZ wallDirection = wallPoint1.Subtract(wallPoint0).Normalize();

            XYZ upDirection = new XYZ(0, 0, 1);
            Transform transform = Transform.Identity;
            transform.Origin = wallCenter;
            transform.BasisY = upDirection;
            transform.BasisZ = wallDirection;
            transform.BasisX = upDirection.CrossProduct(wallDirection);

            //XYZ wallOrientation = wall.Orientation;

            sectionBox.Transform = transform;

            return sectionBox;
        }

        /// <summary>
        /// Get point of the location for Family Instance element.
        /// </summary>
        /// <param name="element">Element to calculate the location point.</param>
        /// <returns>XYZ location point.</returns>
        private static XYZ GetElementLocationInstance(FamilyInstance familyInstance)
        {
            XYZ instanceLocation = new XYZ();

            Transform instanceTransform = familyInstance.GetTransform();
            instanceLocation = instanceTransform.Origin;

            return instanceLocation;
        }

        private static void ShowResult(int? viewsAluminium, int? viewsMetal, int? viewsCarpentry)
        {
            // Show result
            string text = "";
            if (viewsAluminium + viewsMetal + viewsCarpentry == 0)
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

            TaskDialog.Show(TRANSACTION_NAME, text);
        }

        public static string GetPath()
        {
            // Return constructed namespace path
            return typeof(ListsCreate).Namespace + "." + nameof(ListsCreate);
        }
    }
}