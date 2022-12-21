using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

namespace BIM_Leaders_Logic
{
    [Transaction(TransactionMode.Manual)]
    public class ListsCreateModel : BaseModel
    {
        private int _countViewsAluminium;
        private int _countViewsMetal;
        private int _countViewsCarpentry;

        #region PROPERTIES

        // Input
        public int SelectedViewType { get; set; }
        public int SelectedViewTemplate { get; set; }
        public string ViewNamePrefix { get; set; }

        public bool SortIsNeeded { get; set; }

        public bool CreateAluminium { get; set; }
        public string TypeCommentsAluminium { get; set; }
        public int SelectedTagGenAluminium { get; set; }
        public string TypeNamePrefixAluminiumWalls { get; set; }

        public bool CreateMetal { get; set; }
        public string TypeCommentsMetal { get; set; }
        public int SelectedTagGenMetal { get; set; }

        public bool CreateCarpentry { get; set; }
        public string TypeCommentsCarpentry { get; set; }
        public int SelectedTagGenCarpentry { get; set; }

        public int SelectedTagRailing { get; set; }

        public int SelectedTitleBlock { get; set; }
        public double TagPlacementOffsetXmm { get; set; }
        public double TagPlacementOffsetYmm { get; set; }

        // Converted
        public double TagPlacementOffsetX { get; private set; }
        public double TagPlacementOffsetY { get; private set; }

        #endregion

        #region METHODS

        private protected override void TryExecute()
        {
            ConvertUserInput();

            using (Transaction trans = new Transaction(Doc, TransactionName))
            {
                trans.Start();

                List<Element> elementsAll = GetElements();

                if (CreateAluminium)
                    CreateLists(ListType.Aluminium, elementsAll);
                if (CreateMetal)
                    CreateLists(ListType.Metal, elementsAll);
                if (CreateCarpentry)
                    CreateLists(ListType.Carpentry, elementsAll);

                /*
                while (CreateAluminium)
                {
                    List<Element> elements = FilterElements(ListType.Aluminium, elementsAll);

                    if (elements.Count == 0)
                        break;

                    if (SortIsNeeded)
                        elements = SortElements(elements);

                    MarkElements(elements);
                    List<View> views = CreateListViews(elements, new ElementId(SelectedTagGenAluminium));

                    CreateListSheets(ListType.Aluminium, views);

                    _countViewsAluminium += views.Count;

                    break;
                }
                while (CreateMetal)
                {
                    List<Element> elements = FilterElements(elementsAll, TypeCommentsMetal);

                    if (elements.Count == 0)
                        break;

                    if (SortIsNeeded)
                        elements = SortElements(elements);

                    MarkElements(elements, TypeNamePrefixAluminiumWalls);
                    List<View> views = CreateListViews(elements, new ElementId(SelectedTagGenMetal));

                    CreateListSheets(ListType.Metal, views);

                    _countViewsMetal += views.Count;

                    break;
                }
                while (CreateCarpentry)
                {
                    List<Element> elements = FilterElements(elementsAll, TypeCommentsCarpentry);

                    if (elements.Count == 0)
                        break;

                    if (SortIsNeeded)
                        elements = SortElements(elements);

                    MarkElements(elements, TypeNamePrefixAluminiumWalls);
                    List<View> views = CreateListViews(elements, new ElementId(SelectedTagGenCarpentry));

                    CreateListSheets(ListType.Carpentry, views);

                    _countViewsCarpentry += views.Count;

                    break;
                }
                */
                // !!! Create sheets
                // !!! Place views
                // !!! Place legend components

                trans.Commit();
            }

            Result.Result = GetRunResult();
        }

        public SortedDictionary<string, int> GetViewTypeList()
        {
            IEnumerable<ViewFamilyType> views = new FilteredElementCollector(Doc)
                .OfClass(typeof(ViewFamilyType))
                .ToElements()
                .Cast<ViewFamilyType>()
                .Where(x => x.ViewFamily == ViewFamily.Section);

            if (views.Count() == 0)
                throw new InvalidOperationException("Cannot retrieve section view types!");

            return new SortedDictionary<string, int>(views.ToDictionary(x => x.Name, x => x.Id.IntegerValue));
        }

        public SortedDictionary<string, int> GetViewTemplateList()
        {
            SortedDictionary<string, int> viewTemplatesList = new SortedDictionary<string, int>();

            List<View> views = new FilteredElementCollector(Doc)
                .OfClass(typeof(ViewSection))
                .ToElements()
                .Cast<View>()
                .ToList();

            foreach (View view in views)
            {
                if (view.IsTemplate && !viewTemplatesList.ContainsKey(view.Name))
                    viewTemplatesList.Add(view.Name, view.Id.IntegerValue);
            }

            if (viewTemplatesList.Count == 0)
                throw new Exception("Cannot retrieve section view templates!");

            return viewTemplatesList;
        }

        public SortedDictionary<string, int> GetTitleBlockList()
        {
            SortedDictionary<string, int> titleBlockList = new SortedDictionary<string, int>();

            IList<Element> titleBlocks = new FilteredElementCollector(Doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .ToElements();

            if (titleBlocks.Count == 0)
                throw new InvalidOperationException("Cannot retrieve title blocks!");

            return new SortedDictionary<string, int>(titleBlocks.ToDictionary(x => x.Name, x => x.Id.IntegerValue));
        }

        /// <summary>
        /// Get list of generic tag types.
        /// </summary>
        /// <returns>Sorted dictionary with tag type name as key and tag element id as value.</returns>
        /// <exception cref="System.InvalidOperationException">No multicategory tags found.</exception>
        public SortedDictionary<string, int> GetTagGenList()
        {
            IList<Element> tags = new FilteredElementCollector(Doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_MultiCategoryTags)
                .ToElements();

            if (tags.Count == 0)
                throw new InvalidOperationException("Cannot retrieve multicategory tags!");

            return new SortedDictionary<string, int>(tags.ToDictionary(x => x.Name, x => x.Id.IntegerValue));
        }

        /// <summary>
        /// Get list of railing tag types.
        /// </summary>
        /// <returns>Sorted dictionary with tag type name as key and tag element id as value.</returns>
        /// <exception cref="System.InvalidOperationException">No railing tags found.</exception>
        public SortedDictionary<string, int> GetTagRailingList()
        {
            IList<Element> tags = new FilteredElementCollector(Doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_StairsRailingTags)
                .ToElements();

            if (tags.Count == 0)
                throw new InvalidOperationException("Cannot retrieve railing tags!");

            return new SortedDictionary<string, int>(tags.ToDictionary(x => x.Name, x => x.Id.IntegerValue));
        }

        private void ConvertUserInput()
        {
#if VERSION2020
			TagPlacementOffsetX = UnitUtils.ConvertToInternalUnits(TagPlacementOffsetXmm, DisplayUnitType.DUT_MILLIMETERS);
			TagPlacementOffsetY = UnitUtils.ConvertToInternalUnits(TagPlacementOffsetYmm, DisplayUnitType.DUT_MILLIMETERS);
#else
            TagPlacementOffsetX = UnitUtils.ConvertToInternalUnits(TagPlacementOffsetXmm, UnitTypeId.Millimeters);
            TagPlacementOffsetY = UnitUtils.ConvertToInternalUnits(TagPlacementOffsetYmm, UnitTypeId.Millimeters);
#endif
        }

        /// <summary>
        /// Get elements that for lists creating.
        /// </summary>
        /// <returns>List<Element> of unique elements of each type.</returns>
        private List<Element> GetElements()
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
            IEnumerable<Element> elementsAll = new FilteredElementCollector(Doc)
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
                if (element.GetType() != typeof(Wall))
                    elementsTypesCollected.Add(elementType);
            }
            return elements;
        }

        private void CreateLists(ListType listType, List<Element> elementsAll)
        {
            List<Element> elements = FilterElements(listType, elementsAll);

            if (elements.Count == 0)
                return;

            if (SortIsNeeded)
                elements = SortElements(elements);

            MarkElements(elements);
            List<View> views = CreateListViews(listType, elements);

            CreateListSheets(listType, views);

            _countViewsAluminium += views.Count;
        }

        /// <summary>
        /// Filter list of elements that have Type Comments with the given value.
        /// </summary>
        /// <returns>List<Element> of filtered elements.</returns>
        private List<Element> FilterElements(ListType listType, List<Element> elements)
        {
            string valueTypeComments = "";

            switch (listType)
            {
                case ListType.Aluminium: valueTypeComments = TypeCommentsAluminium; break;
                case ListType.Metal: valueTypeComments = TypeCommentsMetal; break;
                case ListType.Carpentry: valueTypeComments = TypeCommentsCarpentry; break;
            }

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
        private List<Element> SortElements(List<Element> elements)
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
        /// Get the Type Mark value from given element.
        /// </summary>
        /// <returns>Type Mark value.</returns>
        private string GetTypeMark(Element element)
        {
            string typeMark = "";

            ElementId elementTypeId = element.GetTypeId();

            if (elementTypeId == ElementId.InvalidElementId)
                return typeMark;

            Element elementType = Doc.GetElement(elementTypeId);

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
        private string GetTypeComments(Element element)
        {
            string typeComments = "";

            ElementId elementTypeId = element.GetTypeId();

            if (elementTypeId == ElementId.InvalidElementId)
                return typeComments;

            Element elementType = Doc.GetElement(elementTypeId);

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
        private void MarkElements(List<Element> elements)
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

                    string elementNewTypeName = TypeNamePrefixAluminiumWalls + " " + counter;

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
                            ElementType wallTypeNew = wallType.Duplicate(TypeNamePrefixAluminiumWalls + counter);
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
        private List<View> CreateListViews(ListType listType, List<Element> elements)
        {
            if (elements == null || elements.Count == 0)
                return null;

            ElementId viewType = new ElementId(SelectedViewType);
            ElementId viewTemplate = new ElementId(SelectedViewTemplate);
            int tagTypeId = 0;
            switch(listType)
            {
                case ListType.Aluminium: tagTypeId = SelectedTagGenAluminium; break;
                case ListType.Metal: tagTypeId = SelectedTagGenMetal; break;
                case ListType.Carpentry: tagTypeId = SelectedTagGenCarpentry; break;
            }
            ElementId tagType = new ElementId(tagTypeId);

            List<View> viewsCreated = new List<View>();

            View view = Doc.GetElement(viewTemplate) as View;
            double scale = view.Scale;

            foreach (Element element in elements)
            {
                BoundingBoxXYZ sectionBox = GetSectionBox(element);

                ViewSection section = ViewSection.CreateSection(Doc, viewType, sectionBox);

                string typeMark = GetTypeMark(element);
                if (typeMark == "")
                {
                    TaskDialog.Show("Create Lists", $"Error creating views. Element {element.Id} has empty Type Mark.");
                    return null;
                }
                try
                {
                    section.Name = ViewNamePrefix + typeMark;
                }
                catch
                {
                    TaskDialog.Show("Create Lists", "Error creating views. Element with duplicate Type Marks exist.");
                    return null;
                }

                section.ViewTemplateId = viewTemplate;

                Reference reference = new Reference(element);

                // Move tag on the view because it's on the family point now.
                XYZ tagLocation = sectionBox.Transform.Origin;

                double moveTagX = -(section.RightDirection.X * TagPlacementOffsetX * scale);
                double moveTagY = -(section.RightDirection.Y * TagPlacementOffsetX * scale);
                double moveTagZ = (sectionBox.Max.Y - sectionBox.Min.Y) / 2 - TagPlacementOffsetY * scale;
                XYZ moveTag = new XYZ(moveTagX, moveTagY, moveTagZ);

                XYZ tagLocationMoved = tagLocation.Add(moveTag);

                IndependentTag.Create(Doc, tagType, section.Id, reference, false, TagOrientation.Horizontal, tagLocationMoved);

                viewsCreated.Add(section);
            }
            return viewsCreated;
        }

        /// <summary>
        /// Get section box for section creating.
        /// </summary>
        /// <param name="element">Element that needs a section.</param>
        private BoundingBoxXYZ GetSectionBox(Element element)
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
        private BoundingBoxXYZ GetSectionBoxInstance(FamilyInstance familyInstance, double SECTION_SIDES_EXTENSION)
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
        private BoundingBoxXYZ GetSectionBoxWall(Wall wall, double SECTION_SIDES_EXTENSION)
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
        private XYZ GetElementLocationInstance(FamilyInstance familyInstance)
        {
            Transform instanceTransform = familyInstance.GetTransform();
            XYZ instanceLocation = instanceTransform.Origin;

            return instanceLocation;
        }

        private void CreateListSheets(ListType listType, List<View> views)
        {
            ElementId titleblockType = new ElementId(SelectedTitleBlock);

            string sheetNumberType = "";
            switch (listType)
            {
                case ListType.Aluminium:
                    sheetNumberType = "01";
                    break;
                case ListType.Metal:
                    sheetNumberType = "02";
                    break;
                case ListType.Carpentry:
                    sheetNumberType = "03";
                    break;
            }
            int sheetNumberCount = 1;

            XYZ placementPoint = new XYZ(TagPlacementOffsetXmm, TagPlacementOffsetYmm, 0);
            ElementId viewportType = GetViewportType();

            foreach (View view in views)
            {
                ViewSheet sheet = ViewSheet.Create(Doc, titleblockType);
                sheet.SheetNumber = $"A90.{sheetNumberType}.{sheetNumberCount:D2}";
                sheet.Name = "LIST";

                sheetNumberCount++;

                Viewport viewport = Viewport.Create(Doc, sheet.Id, view.Id, placementPoint);
                viewport.ChangeTypeId(viewportType);
            }
        }

        /// <summary>
        /// Get viewport type that has no labels.
        /// </summary>
        /// <returns>Viewport type with no labels, or fisrt default if all viewport types are labeled.</returns>
        private ElementId GetViewportType()
        {
            IEnumerable<ElementType> viewportTypes = new FilteredElementCollector(Doc)
                .OfClass(typeof(ElementType))
                .Cast<ElementType>()
                .Where(q => q.FamilyName == "Viewport");

            IEnumerable<ElementType> viewportTypesEmpty = viewportTypes.Where(x => x.get_Parameter(BuiltInParameter.VIEWPORT_ATTR_SHOW_LABEL).AsInteger() == 0);

            if (viewportTypesEmpty.Count() > 0)
                return viewportTypesEmpty.First().Id;

            return viewportTypes.FirstOrDefault().Id;
        }

        private enum ListType
        {
            Aluminium,
            Metal,
            Carpentry
        }

        private protected override string GetRunResult()
        {
            if (_countViewsAluminium == 0 &&
                _countViewsMetal == 0 &&
                _countViewsCarpentry == 0
                )
                return "No lists created.";

            string text = "";

            if (_countViewsAluminium > 0)
                text += $"{_countViewsAluminium} alumimium";
            if (_countViewsMetal > 0)
            {
                if (text.Length > 0)
                    text += $", ";
                text += $"{_countViewsAluminium} metal";
            }  
            if (_countViewsCarpentry > 0)
            {
                if (text.Length > 0)
                    text += $", ";
                text += $"{_countViewsCarpentry} carpentry";
            }

            text += " lists were created.";

            return text;
        }

        #endregion
    }
}