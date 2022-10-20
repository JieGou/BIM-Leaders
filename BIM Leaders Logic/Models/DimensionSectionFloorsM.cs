using System;
using System.Collections.Generic;
using System.ComponentModel;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace BIM_Leaders_Logic
{
    [Transaction(TransactionMode.Manual)]
    public class DimensionSectionFloorsM : INotifyPropertyChanged, IExternalEventHandler
    {
        private UIDocument _uidoc;
        private Document _doc;

        private int _countSpots;
        private int _countSegments;

        private const string TRANSACTION_NAME = "Annotate Section";

        #region PROPERTIES

        /// <summary>
        /// ExternalEvent needed for Revit to run transaction in API context.
        /// So we must call not the main method but raise the event.
        /// </summary>
        public ExternalEvent ExternalEvent { get; set; }

        private double _minThickThicknessCm;
        public double MinThickThicknessCm
        {
            get { return _minThickThicknessCm; }
            set
            {
                _minThickThicknessCm = value;
                OnPropertyChanged(nameof(MinThickThicknessCm));
            }
        }

        private double _minThickThickness;
        public double MinThickThickness
        {
            get { return _minThickThickness; }
            set
            {
                _minThickThickness = value;
                OnPropertyChanged(nameof(MinThickThickness));
            }
        }

        private bool _placementThinTop;
        public bool PlacementThinTop
        {
            get { return _placementThinTop; }
            set
            {
                _placementThinTop = value;
                OnPropertyChanged(nameof(PlacementThinTop));
            }
        }

        private bool _placementThickTop;
        public bool PlacementThickTop
        {
            get { return _placementThickTop; }
            set
            {
                _placementThickTop = value;
                OnPropertyChanged(nameof(PlacementThickTop));
            }
        }

        private bool _placementThickBot;
        public bool PlacementThickBot
        {
            get { return _placementThickBot; }
            set
            {
                _placementThickBot = value;
                OnPropertyChanged(nameof(PlacementThickBot));
            }
        }

        private bool _inputPlaceSpots;
        public bool PlaceSpots
        {
            get { return _inputPlaceSpots; }
            set
            {
                _inputPlaceSpots = value;
                OnPropertyChanged(nameof(PlaceSpots));
            }
        }

        private int _selectElements;
        public int SelectElements
        {
            get { return _selectElements; }
            set
            {
                _selectElements = value;
                OnPropertyChanged(nameof(SelectElements));
            }
        }

        private string _runResult;
        public string RunResult
        {
            get { return _runResult; }
            set
            { 
                _runResult = value;
                OnPropertyChanged(nameof(RunResult));
            }
        }

        #endregion

        public DimensionSectionFloorsM(ExternalCommandData commandData)
        {
            _uidoc = commandData.Application.ActiveUIDocument;
            _doc = _uidoc.Document;
        }

        public void Run()
        {
            ExternalEvent.Raise();
        }

        #region IEXTERNALEVENTHANDLER

        public string GetName()
        {
            return TRANSACTION_NAME;
        }

        public void Execute(UIApplication app)
        {
            RunResult = "";

            try
            {
                ConvertUserInput();

                GetDividedFloors(out List<Floor> floorsThin, out List<Floor> floorsThick);
                DetailLine detailLine = _doc.GetElement(new ElementId(SelectElements)) as DetailLine;
                Line line = detailLine.GeometryCurve as Line;
                List<Face> intersectionFacesAll = GetIntersectionFaces(line, floorsThin, floorsThick);

                // Check if no intersections
                if (intersectionFacesAll.Count == 0)
                    RunResult = "No intersections were found";

                // Create annotations
                using (Transaction trans = new Transaction(_doc, TRANSACTION_NAME))
                {
                    trans.Start();
                    
                    if (PlaceSpots)
                        CreateSpots(line, intersectionFacesAll);
                    else
                        CreateDimensions(line, intersectionFacesAll);
                    
                    trans.Commit();
                }
            }
            catch (Exception e)
            {
                RunResult = e.Message;
            }

            ShowResult();
        }

        #endregion

        #region METHODS

        private void ConvertUserInput()
        {
#if VERSION2020
            MinThickThickness = UnitUtils.ConvertToInternalUnits(MinThickThicknessCm, DisplayUnitType.DUT_CENTIMETERS);
#else
            MinThickThickness = UnitUtils.ConvertToInternalUnits(MinThickThicknessCm, UnitTypeId.Centimeters);
#endif
        }

        /// <summary>
        /// Get floors in the current document, divided by the given thickness.
        /// </summary>
        /// <param name="thickness">Thickness value to divide floors into 2 lists.</param>
        /// <param name="floorsThin">Floors with thickness less than the given value.</param>
        /// <param name="floorsThick">Floors with thickness greater than the given value.</param>
        private void GetDividedFloors(out List<Floor> floorsThin, out List<Floor> floorsThick)
        {
            floorsThin = new List<Floor>();
            floorsThick = new List<Floor>();

            // Get Floors
            IEnumerable<Element> floorsAll = new FilteredElementCollector(_doc, _doc.ActiveView.Id)
                .OfClass(typeof(Floor))
                .WhereElementIsNotElementType()
                .ToElements();

            // Divide Floors to thick and thin tor spot elevations arrangement
            foreach (Floor floor in floorsAll)
            {
                FloorType floorType = floor.FloorType;

                double floorThickness = floorType.get_Parameter(BuiltInParameter.FLOOR_ATTR_DEFAULT_THICKNESS_PARAM).AsDouble();

                if (floorThickness > MinThickThickness)
                    floorsThick.Add(floor);
                else
                    floorsThin.Add(floor);
            }
        }

        /// <summary>
        /// Get intersection faces of given floors lists for future dimensioning.
        /// </summary>
        /// <param name="floorsThin">First list of floors.</param>
        /// <param name="floorsThick">Second list of floors.</param>
        /// <param name="line">Line to find intersections.</param>
        /// <param name="inputPlacementThinTop">True if need to find intersections on top of the first floors list.</param>
        /// <param name="inputPlacementThickTop">True if need to find intersections on top of the second floors list.</param>
        /// <param name="inputPlacementThickBot">True if need to find intersections on bottom of the second floors list.</param>
        /// <returns></returns>
        private List<Face> GetIntersectionFaces(Line line, List<Floor> floorsThin, List<Floor> floorsThick)
        {
            List<Face> intersectionFacesAll = new List<Face>();

            if (PlacementThinTop)
            {
                List<Face> intersectionFacesThinTop = GetIntersections(line, floorsThin)[0];
                intersectionFacesAll.AddRange(intersectionFacesThinTop);
            }
            if (PlacementThickTop || PlacementThickBot)
            {
                List<List<Face>> intersectionFacesThick = GetIntersections(line, floorsThick);

                if (PlacementThickTop)
                {
                    List<Face> intersectionFacesThickTop = intersectionFacesThick[0];
                    intersectionFacesAll.AddRange(intersectionFacesThickTop);
                }
                if (PlacementThickBot)
                {
                    List<Face> intersectionFacesThickBot = intersectionFacesThick[1];
                    intersectionFacesAll.AddRange(intersectionFacesThickBot);
                }
            }

            return intersectionFacesAll;
        }

        /// <summary>
        /// Get intersections from line and list of floors.
        /// </summary>
        /// <returns>List of floor faces that intersect with the given line.</returns>
        private List<List<Face>> GetIntersections(Line line, List<Floor> floors)
        {
            Options options = new Options
            {
                ComputeReferences = true,
                IncludeNonVisibleObjects = false,
                View = _doc.ActiveView
            };

            IntersectionResultArray ira = new IntersectionResultArray();

            // List for intersection points. Top[0], Bottom[1].
            List<Face> intersectionFaces0 = new List<Face>();
            List<Face> intersectionFaces1 = new List<Face>();
            List<List<Face>> intersectionFaces = new List<List<Face>>();
            intersectionFaces.Add(intersectionFaces0);
            intersectionFaces.Add(intersectionFaces1);

            // Iterate through floors solids and get faces
            foreach (Floor floor in floors)
            {
                foreach (Solid solid in floor.get_Geometry(options))
                {
                    foreach (Face face in solid.Faces)
                    {
                        // Some faces are cylidrical so pass them
                        try
                        {
                            // Check if faces are horizontal
                            UV p = new UV(0, 0);
                            if (Math.Round(face.ComputeNormal(p).X) == 0)
                            {
                                if (Math.Round(face.ComputeNormal(p).Y) == 0)
                                {
                                    if (Math.Round(face.ComputeNormal(p).Z) == 1)
                                    {
                                        SetComparisonResult intersection = face.Intersect(line, out ira);
                                        if (intersection == SetComparisonResult.Overlap)
                                            intersectionFaces[0].Add(face);
                                    }
                                    if (Math.Round(face.ComputeNormal(p).Z) == -1)
                                    {
                                        SetComparisonResult intersection = face.Intersect(line, out ira);
                                        if (intersection == SetComparisonResult.Overlap)
                                            intersectionFaces[1].Add(face);
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }
            }

            return intersectionFaces;
        }

        /// <summary>
        /// Create spot elevations on a faces through a given line.
        /// </summary>
        private void CreateSpots(Line line, List<Face> intersectionFaces)
        {
            View view = _doc.ActiveView; 
            XYZ zero = new XYZ(0, 0, 0);

            for (int i = 0; i < intersectionFaces.Count; i++)
            {
                double x = line.GetEndPoint(0).X;
                double y = line.GetEndPoint(0).Y;
                double z = intersectionFaces[i].EdgeLoops.get_Item(0).get_Item(0).AsCurve().GetEndPoint(0).Z;
                XYZ origin = new XYZ(x, y, z);

                try
                {
                    SpotDimension sd = _doc.Create.NewSpotElevation(view, intersectionFaces[i].Reference, origin, zero, zero, origin, false);
                    _countSpots++;
                }
                catch { }
            }
        }

        /// <summary>
        /// Create dimension on a faces through a given line.
        /// </summary>
        private void CreateDimensions(Line line, List<Face> intersectionFaces)
        {
            // Convert List<Face> to ReferenceArray
            ReferenceArray references = new ReferenceArray();
            foreach (Face face in intersectionFaces)
                references.Append(face.Reference);

            Dimension dimension = _doc.Create.NewDimension(_doc.ActiveView, line, references);
            DimensionUtils.AdjustText(dimension);

#if !VERSION2020
            dimension.HasLeader = false;
#endif

            _countSegments += references.Size - 1;
        }

        private void ShowResult()
        {
            if (RunResult.Length == 0)
            {
                if (_countSpots == 0 && _countSegments == 0)
                    RunResult = "No annotations created.";
                else
                    RunResult = (PlaceSpots)
                        ? $"{_countSpots} spot elevations created."
                        : $"Dimension with {_countSegments} segments created.";
            }

            TaskDialog.Show(TRANSACTION_NAME, RunResult);
        }

        #endregion

        #region INOTIFYPROPERTYCHANGED

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler CanExecuteChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
