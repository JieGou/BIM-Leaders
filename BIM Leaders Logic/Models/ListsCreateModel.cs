﻿using System;
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
        const double SECTION_SIDES_EXTENSION = 1;

        private int _countSheetsAluminium;
        private int _countSheetsMetal;
        private int _countSheetsCarpentry;

        #region PROPERTIES

        // Input
        public bool CreateAluminium { get; set; }
        public bool CreateMetal { get; set; }
        public bool CreateCarpentry { get; set; }
        public string TypeNamePrefixAluminiumWalls { get; set; }
        public string TypeCommentsAluminium { get; set; }
        public string TypeCommentsMetal { get; set; }
        public string TypeCommentsCarpentry { get; set; }
        public bool SortIsNeeded { get; set; }

        public string ViewNamePrefix { get; set; }
        public int SelectedViewTypeSection { get; set; }
        public int SelectedViewTemplateSection { get; set; }
        public int SelectedViewTypePlan { get; set; }
        public int SelectedViewTemplatePlan { get; set; }
        
        public int SelectedTagGenAluminium { get; set; }
        public int SelectedTagGenMetal { get; set; }
        public int SelectedTagGenCarpentry { get; set; }
        public int SelectedTagRailing { get; set; }
        public double TagPlacementOffsetXmm { get; set; }
        public double TagPlacementOffsetYmm { get; set; }
        public double TagPlacementOffsetX { get; private set; }
        public double TagPlacementOffsetY { get; private set; }

        public int SelectedTitleBlock { get; set; }
        public double FacadePlacementOffsetXmm { get; set; }
        public double FacadePlacementOffsetYmm { get; set; }
        public double FacadePlacementOffsetX { get; private set; }
        public double FacadePlacementOffsetY { get; private set; }
        public double SectionPlacementOffsetXmm { get; set; }
        public double SectionPlacementOffsetX { get; private set; }
        public double PlanPlacementOffsetYmm { get; set; }
        public double PlanPlacementOffsetY { get; private set; }

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

                trans.Commit();
            }

            Result.Result = GetRunResult();
        }

        public SortedDictionary<string, int> GetViewTypeListSection()
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

        public SortedDictionary<string, int> GetViewTemplateListSection()
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

        public SortedDictionary<string, int> GetViewTypeListPlan()
        {
            IEnumerable<ViewFamilyType> views = new FilteredElementCollector(Doc)
                .OfClass(typeof(ViewFamilyType))
                .ToElements()
                .Cast<ViewFamilyType>()
                .Where(x => x.ViewFamily == ViewFamily.FloorPlan);

            if (views.Count() == 0)
                throw new InvalidOperationException("Cannot retrieve plan view types!");

            return new SortedDictionary<string, int>(views.ToDictionary(x => x.Name, x => x.Id.IntegerValue));
        }

        public SortedDictionary<string, int> GetViewTemplateListPlan()
        {
            SortedDictionary<string, int> viewTemplatesList = new SortedDictionary<string, int>();

            List<View> views = new FilteredElementCollector(Doc)
                .OfClass(typeof(ViewPlan))
                .ToElements()
                .Cast<View>()
                .ToList();

            foreach (View view in views)
            {
                if (view.IsTemplate && !viewTemplatesList.ContainsKey(view.Name))
                    viewTemplatesList.Add(view.Name, view.Id.IntegerValue);
            }

            if (viewTemplatesList.Count == 0)
                throw new Exception("Cannot retrieve plan view templates!");

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
            FacadePlacementOffsetX = UnitUtils.ConvertToInternalUnits(FacadePlacementOffsetXmm, DisplayUnitType.DUT_MILLIMETERS);
			FacadePlacementOffsetY = UnitUtils.ConvertToInternalUnits(FacadePlacementOffsetYmm, DisplayUnitType.DUT_MILLIMETERS);
            SectionPlacementOffsetX = UnitUtils.ConvertToInternalUnits(SectionPlacementOffsetXmm, DisplayUnitType.DUT_MILLIMETERS);
			PlanPlacementOffsetY = UnitUtils.ConvertToInternalUnits(PlanPlacementOffsetYmm, DisplayUnitType.DUT_MILLIMETERS);
#else
            TagPlacementOffsetX = UnitUtils.ConvertToInternalUnits(TagPlacementOffsetXmm, UnitTypeId.Millimeters);
            TagPlacementOffsetY = UnitUtils.ConvertToInternalUnits(TagPlacementOffsetYmm, UnitTypeId.Millimeters);
            FacadePlacementOffsetX = UnitUtils.ConvertToInternalUnits(FacadePlacementOffsetXmm, UnitTypeId.Millimeters);
            FacadePlacementOffsetY = UnitUtils.ConvertToInternalUnits(FacadePlacementOffsetYmm, UnitTypeId.Millimeters);
            SectionPlacementOffsetX = UnitUtils.ConvertToInternalUnits(SectionPlacementOffsetXmm, UnitTypeId.Millimeters);
            PlanPlacementOffsetY = UnitUtils.ConvertToInternalUnits(PlanPlacementOffsetYmm, UnitTypeId.Millimeters);
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
            var views = CreateListViews(listType, elements);

            CreateListSheets(listType, views);
            PlaceTags(listType, views);
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
        /// <returns>Dictionary of elements and views created for them.</returns>
        private Dictionary<Element, Tuple<ViewSection, ViewSection, ViewSection>> CreateListViews(ListType listType, List<Element> elements)
        {
            var viewsData = new Dictionary<Element, Tuple<ViewSection, ViewSection, ViewSection>>();

            if (elements == null || elements.Count == 0)
                return null;

            ElementId viewTypeSection = new ElementId(SelectedViewTypeSection);
            ElementId viewTemplateSection = new ElementId(SelectedViewTemplateSection);
            ElementId viewTypePlan = new ElementId(SelectedViewTypePlan);
            ElementId viewTemplatePlan = new ElementId(SelectedViewTemplatePlan);

            foreach (Element element in elements)
            {
                (BoundingBoxXYZ Section, BoundingBoxXYZ Facade, BoundingBoxXYZ Plan) viewsBoxes = GetSectionBoxes(element);

                ViewSection facade = ViewSection.CreateSection(Doc, viewTypeSection, viewsBoxes.Facade);
                ViewSection section = ViewSection.CreateSection(Doc, viewTypeSection, viewsBoxes.Section);
                ViewSection plan = ViewSection.CreateSection(Doc, viewTypeSection, viewsBoxes.Plan);
                //ViewPlan.Create(Doc, viewTypePlan, element.LevelId);

                string typeMark = GetTypeMark(element);
                if (typeMark == "")
                    throw new InvalidOperationException($"Error creating views. Element {element.Id} has empty Type Mark.");

                try { facade.Name = ViewNamePrefix + typeMark + "_F"; }
                catch
                {
                    throw new InvalidOperationException($"Error naming views. View with name {facade.Name} exist.");
                }
                try { section.Name = ViewNamePrefix + typeMark + "_S"; }
                catch
                {
                    throw new InvalidOperationException($"Error naming views. View with name {section.Name} exist.");
                }
                try { plan.Name = ViewNamePrefix + typeMark + "_P"; }
                catch
                {
                    throw new InvalidOperationException($"Error naming views. View with name {plan.Name} exist.");
                }

                facade.ViewTemplateId = viewTemplateSection;
                section.ViewTemplateId = viewTemplateSection;
                plan.ViewTemplateId = viewTemplateSection;
                //plan.ViewTemplateId = viewTemplatePlan;

                viewsData.Add(element, new Tuple<ViewSection, ViewSection, ViewSection>(facade, section, plan));
            }
            return viewsData;
        }

        /// <summary>
        /// Get section box for section creating.
        /// </summary>
        /// <param name="element">Element that needs a section.</param>
        private BoundingBoxXYZ GetSectionBox(Element element)
        {
            BoundingBoxXYZ sectionBox = new BoundingBoxXYZ();

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
        /// Get section box for section creating.
        /// </summary>
        /// <param name="element">Element that needs a section.</param>
        private (BoundingBoxXYZ, BoundingBoxXYZ, BoundingBoxXYZ) GetSectionBoxes(Element element)
        {
            BoundingBoxXYZ sectionBox = new BoundingBoxXYZ();
            BoundingBoxXYZ facadeBox = new BoundingBoxXYZ();
            BoundingBoxXYZ planBox = new BoundingBoxXYZ();

            if (element.GetType() == typeof(FamilyInstance))
                (sectionBox, facadeBox, planBox) = GetSectionBoxesInstance(element as FamilyInstance, SECTION_SIDES_EXTENSION);
            else if (element.GetType() == typeof(Wall))
                (sectionBox, facadeBox) = GetSectionBoxesWall(element as Wall, SECTION_SIDES_EXTENSION);
            else if (element.GetType() == typeof(Railing))
            {

            }

            return (sectionBox, facadeBox, planBox);
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
        /// Get bounding box for creating a section view that cuts the given <paramref name="familyInstance"/>.
        /// </summary>
        /// <param name="doc">Document.</param>
        /// <param name="familyInstance">Family instance to cut.</param>
        /// <param name="SECTION_SIDES_EXTENSION">Extension of the bounding box (will apply to left, right and far side).</param>
        /// <returns>Bounding box with right coordinates for section creating (Z is looking to the section direction, etc).</returns>
        private (BoundingBoxXYZ, BoundingBoxXYZ, BoundingBoxXYZ) GetSectionBoxesInstance(FamilyInstance familyInstance, double SECTION_SIDES_EXTENSION)
        {
            BoundingBoxXYZ facadeBox = new BoundingBoxXYZ();
            BoundingBoxXYZ sectionBox = new BoundingBoxXYZ();
            BoundingBoxXYZ planBox = new BoundingBoxXYZ();

            ElementId elementTypeId = familyInstance.GetTypeId();
            if (elementTypeId == ElementId.InvalidElementId)
                return (null, null, null);
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
            // Get cropping dimensions, cropping will cover size of the element + extension.
            // For section cropping box, depth will be only half of the element + extenxion.
            double croppingBoxDepth = wallThickness + 2 * SECTION_SIDES_EXTENSION;
            double croppingBoxWidth = instanceWidth + 2 * SECTION_SIDES_EXTENSION;
            double croppingBoxWidthHalf = instanceWidth / 2 + SECTION_SIDES_EXTENSION;
            double croppingBoxHeight = wallHeight + 2 * SECTION_SIDES_EXTENSION;

            double facadeBoxWidth = croppingBoxWidth;
            double facadeBoxDepth = croppingBoxHeight;
            double facadeBoxHeight = croppingBoxDepth;

            // Box will be rotated, so width and height will interchange.
            double sectionBoxWidth = croppingBoxDepth;
            double sectionBoxDepth = croppingBoxHeight;
            double sectionBoxHeight = croppingBoxWidthHalf;

            // Plan will be created with section view.
            // Depth here is absolute Z. Height here is plan height on sheet.
            double planBoxWidth = croppingBoxWidth;
            double planBoxDepth = croppingBoxHeight;
            double planBoxHeight = croppingBoxDepth;

            Transform instanceTransform = familyInstance.GetTransform();

            // Change the dimensions of the section boxes.
            XYZ facadeBoxMin = new XYZ(-facadeBoxWidth / 2, -facadeBoxDepth / 2, 0);
            XYZ facadeBoxMax = new XYZ(facadeBoxWidth / 2, facadeBoxDepth / 2, facadeBoxHeight);
            facadeBox.Min = facadeBoxMin;
            facadeBox.Max = facadeBoxMax;
            XYZ sectionBoxMin = new XYZ(-sectionBoxWidth / 2, -sectionBoxDepth / 2, 0);
            XYZ sectionBoxMax = new XYZ(sectionBoxWidth / 2, sectionBoxDepth / 2, sectionBoxHeight);
            sectionBox.Min = sectionBoxMin;
            sectionBox.Max = sectionBoxMax;
            XYZ planBoxMin = new XYZ(-planBoxWidth / 2, -planBoxHeight / 2, instanceTransform.Origin.Z - SECTION_SIDES_EXTENSION);
            XYZ planBoxMax = new XYZ(planBoxWidth / 2, planBoxHeight / 2, instanceTransform.Origin.Z + 5 * SECTION_SIDES_EXTENSION);
            planBox.Min = planBoxMin;
            planBox.Max = planBoxMax;

            // Move the sexion box to the needed coordinates via Transform.

            // Get element transform (location, rotation, etc.).
            // Change it via Z rotation because we need to see not front of element but side of it
            // Change it via X rotation because for section creating Z is looking from section front.

            Transform rotationZ = Transform.CreateRotation(new XYZ(0, 0, 1), -Math.PI / 2);
            Transform rotationX = Transform.CreateRotation(new XYZ(1, 0, 0), Math.PI / 2);
            Transform rotation2Z = Transform.CreateRotation(new XYZ(0, 0, 1), -Math.PI);
            Transform rotation2X = Transform.CreateRotation(new XYZ(1, 0, 0), Math.PI);

            Transform rotationSection = rotationZ.Multiply(rotationX);
            Transform rotationFacade = rotationX;
            Transform rotationPlan = rotation2Z.Multiply(rotation2X);

            Transform instanceTransformSectionRotated = instanceTransform.Multiply(rotationSection);
            Transform instanceTransformFacadeRotated = instanceTransform.Multiply(rotationFacade);
            Transform instanceTransformPlanRotated = instanceTransform.Multiply(rotationPlan);

            Transform moveUp = Transform.CreateTranslation(new XYZ(0, wallHeight / 2, 0));
            Transform moveFromWall = Transform.CreateTranslation(new XYZ(0, 0, -croppingBoxDepth / 2));
            Transform moveUpPlan = Transform.CreateTranslation(new XYZ(0, 0, -5 * SECTION_SIDES_EXTENSION));

            Transform instanceTransformSectionRaised = instanceTransformSectionRotated.Multiply(moveUp);
            Transform instanceTransformFacadeRaised = instanceTransformFacadeRotated.Multiply(moveUp);
            Transform instanceTransformFacadeFromWall = instanceTransformFacadeRaised.Multiply(moveFromWall);
            Transform instanceTransformPlanRaised = instanceTransformPlanRotated.Multiply(moveUpPlan);

            sectionBox.Transform = instanceTransformSectionRaised;
            facadeBox.Transform = instanceTransformFacadeFromWall;
            planBox.Transform = instanceTransformPlanRaised;

            return (sectionBox, facadeBox, planBox);
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
        /// Get bounding box for creating a section view that cuts the given <paramref name="wall"/>.
        /// </summary>
        /// <param name="doc">Document.</param>
        /// <param name="wall">Wall to cut.</param>
        /// <param name="SECTION_SIDES_EXTENSION">Extension of the bounding box (will apply to left, right and far side).</param>
        /// <returns>Bounding box with right coordinates for section creating (Z is looking to the section direction, etc).</returns>
        private (BoundingBoxXYZ, BoundingBoxXYZ) GetSectionBoxesWall(Wall wall, double SECTION_SIDES_EXTENSION)
        {
            BoundingBoxXYZ sectionBox = new BoundingBoxXYZ();
            BoundingBoxXYZ facadeBox = new BoundingBoxXYZ();

            XYZ location = new XYZ();

            ElementId elementTypeId = wall.GetTypeId();
            if (elementTypeId == ElementId.InvalidElementId)
                return (null, null);

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
            Transform transformSection = Transform.Identity;
            Transform transformFacade = Transform.Identity;
            transformSection.Origin = wallCenter;
            transformSection.BasisY = upDirection;
            transformSection.BasisZ = wallDirection;
            transformSection.BasisX = upDirection.CrossProduct(wallDirection);

            transformFacade.Origin = wallCenter;
            transformFacade.BasisY = upDirection;
            transformFacade.BasisZ = upDirection.CrossProduct(wallDirection);
            transformFacade.BasisX = -wallDirection;

            //XYZ wallOrientation = wall.Orientation;

            sectionBox.Transform = transformSection;
            facadeBox.Transform = transformFacade;

            return (sectionBox, facadeBox);
        }

        private void CreateListSheets(ListType listType, Dictionary<Element, Tuple<ViewSection, ViewSection, ViewSection>> viewsData)
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

            XYZ viewportPointFacade = new XYZ(FacadePlacementOffsetX, FacadePlacementOffsetY, 0);
            XYZ viewportPointSection = new XYZ(SectionPlacementOffsetX, FacadePlacementOffsetY, 0);
            XYZ viewportPointPlan = new XYZ(FacadePlacementOffsetX, PlanPlacementOffsetY, 0);

            ElementId viewportType = GetViewportType();

            foreach (KeyValuePair<Element, Tuple<ViewSection, ViewSection, ViewSection>> viewData in viewsData)
            {
                ViewSheet sheet = ViewSheet.Create(Doc, titleblockType);
                sheet.SheetNumber = $"A90.{sheetNumberType}.{sheetNumberCount:D2}";
                sheet.Name = "LIST";

                sheetNumberCount++;

                Viewport viewportFacade = Viewport.Create(Doc, sheet.Id, viewData.Value.Item1.Id, viewportPointFacade);
                Viewport viewportSection = Viewport.Create(Doc, sheet.Id, viewData.Value.Item2.Id, viewportPointSection);
                Viewport viewportPlan = Viewport.Create(Doc, sheet.Id, viewData.Value.Item3.Id, viewportPointPlan);
                viewportFacade.ChangeTypeId(viewportType);
                viewportSection.ChangeTypeId(viewportType);
                viewportPlan.ChangeTypeId(viewportType);

                if (listType == ListType.Aluminium)
                    _countSheetsAluminium++;
                else if (listType == ListType.Metal)
                    _countSheetsMetal++;
                else if (listType == ListType.Carpentry)
                    _countSheetsCarpentry++;
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

        /// <summary>
        /// Place tags on views.
        /// </summary>
        private void PlaceTags(ListType listType, Dictionary<Element, Tuple<ViewSection, ViewSection, ViewSection>> viewsData)
        {
            int tagTypeId = 0;
            switch (listType)
            {
                case ListType.Aluminium: tagTypeId = SelectedTagGenAluminium; break;
                case ListType.Metal: tagTypeId = SelectedTagGenMetal; break;
                case ListType.Carpentry: tagTypeId = SelectedTagGenCarpentry; break;
            }
            ElementId tagType = new ElementId(tagTypeId);

            foreach (KeyValuePair<Element, Tuple<ViewSection, ViewSection, ViewSection>> viewData in viewsData)
            {
                ViewSection view = viewData.Value.Item1;
                double scale = view.Scale;

                // Move tag on the view because it's on the family point now.
                //XYZ tagLocation = viewsBoxes.Facade.Transform.Origin;
                XYZ tagLocation = view.Origin;

                double moveTagX = -(view.RightDirection.X * TagPlacementOffsetX * scale);
                double moveTagY = -(view.RightDirection.Y * TagPlacementOffsetX * scale);
                double moveTagZ = (view.CropBox.Max.Z - view.CropBox.Min.Z) / 2 - TagPlacementOffsetY * scale;
                XYZ moveTag = new XYZ(moveTagX, moveTagY, moveTagZ);

                XYZ tagLocationMoved = tagLocation.Add(moveTag);

                Reference reference = new Reference(viewData.Key);

                IndependentTag.Create(Doc, tagType, view.Id, reference, false, TagOrientation.Horizontal, tagLocationMoved);
            }
        }

        private enum ListType
        {
            Aluminium,
            Metal,
            Carpentry
        }

        private protected override string GetRunResult()
        {
            if (_countSheetsAluminium == 0 &&
                _countSheetsMetal == 0 &&
                _countSheetsCarpentry == 0
                )
                return "No lists created.";

            string text = "";

            if (_countSheetsAluminium > 0)
                text += $"{_countSheetsAluminium} alumimium";
            if (_countSheetsMetal > 0)
            {
                if (text.Length > 0)
                    text += $", ";
                text += $"{_countSheetsAluminium} metal";
            }  
            if (_countSheetsCarpentry > 0)
            {
                if (text.Length > 0)
                    text += $", ";
                text += $"{_countSheetsCarpentry} carpentry";
            }

            text += " lists were created.";

            return text;
        }

        #endregion
    }
}