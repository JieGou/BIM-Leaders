﻿using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;

namespace BIM_Leaders_Logic
{
    [Transaction(TransactionMode.Manual)]
    public class DimensionSectionFloorsModel : BaseModel
    {
        private int _countSpots;
        private int _countSegments;

        #region PROPERTIES

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

        private bool _placeSpots;
        public bool PlaceSpots
        {
            get { return _placeSpots; }
            set
            {
                _placeSpots = value;
                OnPropertyChanged(nameof(PlaceSpots));
            }
        }

        private int _selectedElement;
        public int SelectedElement
        {
            get { return _selectedElement; }
            set
            {
                _selectedElement = value;
                OnPropertyChanged(nameof(SelectedElement));
            }
        }

        #endregion

        #region METHODS

        private protected override void TryExecute()
        {
            ConvertUserInput();

            GetDividedFloors(out List<Floor> floorsThin, out List<Floor> floorsThick);
            DetailLine detailLine = Doc.GetElement(new ElementId(SelectedElement)) as DetailLine;
            Line line = detailLine.GeometryCurve as Line;
            List<Face> intersectionFacesAll = GetIntersectionFaces(line, floorsThin, floorsThick);

            // Check if no intersections
            if (intersectionFacesAll.Count == 0)
                Result.Result = "No intersections were found";

            // Create annotations
            using (Transaction trans = new Transaction(Doc, TransactionName))
            {
                trans.Start();

                if (PlaceSpots)
                    CreateSpots(line, intersectionFacesAll);
                else
                    CreateDimensions(line, intersectionFacesAll);

                trans.Commit();
            }

            Result.Result = GetRunResult();
        }

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
            IEnumerable<Element> floorsAll = new FilteredElementCollector(Doc, Doc.ActiveView.Id)
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
                View = Doc.ActiveView
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


            View view = Doc.ActiveView; 
            XYZ zero = new XYZ(0, 0, 0);

            for (int i = 0; i < intersectionFaces.Count; i++)
            {
                double x = line.GetEndPoint(0).X;
                double y = line.GetEndPoint(0).Y;
                double z = intersectionFaces[i].EdgeLoops.get_Item(0).get_Item(0).AsCurve().GetEndPoint(0).Z;
                XYZ origin = new XYZ(x, y, z);

                try
                {
                    SpotDimension sd = Doc.Create.NewSpotElevation(view, intersectionFaces[i].Reference, origin, zero, zero, origin, false);
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

            Dimension dimension = Doc.Create.NewDimension(Doc.ActiveView, line, references);
            DimensionUtils.AdjustText(dimension);

#if !VERSION2020
            dimension.HasLeader = false;
#endif

            _countSegments += references.Size - 1;
        }

        private protected override string GetRunResult()
        {
            string text = "";

            if (_countSpots == 0 && _countSegments == 0)
            {
                text = "No annotations created.";
            }
            else
            {
                text = (PlaceSpots)
                    ? $"{_countSpots} spot elevations created."
                    : $"Dimension with {_countSegments} segments created.";
            }

            return text;
        }

        #endregion
    }
}