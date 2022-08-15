using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace BIM_Leaders_Core
{
    internal static class DimensionUtils
    {
        internal static void AdjustText(Dimension dimension)
        {
            List<double> moveDistances = GetMovingData(dimension);

            // Moving the segments with looking on their neighbours.
            DimensionSegmentArray dimensionSegments = dimension.Segments;
            for (int i = 0; i < dimensionSegments.Size; i++)
            {
                if (moveDistances[i] == 0)
                    continue;

                DimensionSegment dimensionSegment = dimensionSegments.get_Item(i);

                // Apply smart moving to minimize interfere with adjasent segments.
                // Before all add exception to those smart moving - first and last segments.
                // They even don't have 2 neightbours.
                if (i == 0)
                {
                    MoveSegmentText(dimensionSegment, dimension, moveDistances[i], MoveDirection.Left);
                    continue;
                }
                if (i == dimensionSegments.Size - 1)
                {
                    MoveSegmentText(dimensionSegment, dimension, moveDistances[i], MoveDirection.Right);
                    continue;
                }

                // Now we have 4 variants of combinations:
                // [i - 1] [i] [i + 1] [action]
                //  true        true    pass
                //  true        false   move right
                //  false       false   move right
                //  false       true    move left

                // First one pass because we neighbour segments are also too small.
                if (moveDistances[i - 1] > 0 && moveDistances[i + 1] > 0)
                    continue;

                // Next option - if we need to move to the left, because right segment is too small.
                if (moveDistances[i + 1] > 0)
                {
                    MoveSegmentText(dimensionSegment, dimension, moveDistances[i], MoveDirection.Left);
                    continue;
                }

                // It two other options move to the right.
                MoveSegmentText(dimensionSegment, dimension, moveDistances[i], MoveDirection.Right);
            }
        }

        /// <summary>
        /// Get list of distances to which dimension segment text need to be moved.
        /// </summary>
        /// <param name="dimension">Dimension to calculate the distances.</param>
        /// <returns>List of distances. If dimension segment text don't need or cannot be moved, distance will be 0.</returns>
        internal static List<double> GetMovingData(Dimension dimension)
        {
            // Get the bool array that indicates if we need to move dimension segment.
            List<double> moveDistances = new List<double>();

            Document doc = dimension.Document;
            double scale = doc.ActiveView.Scale;

#if VERSION2020
            DisplayUnitType units = doc.GetUnits().GetFormatOptions(UnitType.UT_Length).DisplayUnits;
#else
            ForgeTypeId units = doc.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId();
#endif

            DimensionSegmentArray dimensionSegments = dimension.Segments;
            foreach (DimensionSegment dimensionSegment in dimensionSegments)
            {
                double moveDistance = 0;

                if (!dimensionSegment.IsTextPositionAdjustable())
                {
                    moveDistances.Add(moveDistance);
                    continue;
                }

                double value = UnitUtils.ConvertFromInternalUnits(dimensionSegment.Value.Value, units);

                // Length of the dimension text along dimension line/
                double textLengthD = dimension.DimensionType.get_Parameter(BuiltInParameter.TEXT_SIZE).AsDouble();
                double textLength = UnitUtils.ConvertFromInternalUnits(textLengthD, units) * GetRatio(value);

                // Factor calculated if dimension should be moved to the side.
                double factor = value / (scale * textLength);

                if (factor < 1)
                {
                    // Dimension move distance calculation.
                    double moveDistanceCm = 0;
                    moveDistanceCm += value / 2;              // Move to the edge of the layer.
                    moveDistanceCm += textLength * scale / 2; // Add half of dimension text width.
                    moveDistanceCm += scale * 0.1;            // Add a bit more (1 mm) to make a little gap between dim and edge.
                    moveDistance = UnitUtils.ConvertToInternalUnits(moveDistanceCm, units);
                }
                moveDistances.Add(moveDistance);
            }
            return moveDistances;
        }

        /// <summary>
        /// Ratio of dimension segment text height to width.
        /// </summary>
        /// <param name="value">Dimension segment value.</param>
        /// <returns>Ratio of dimension segment text height to width.</returns>
        internal static double GetRatio(double value)
        {
            double ratio = 0.7;

            if (value > 9)
                ratio = 1.5; // For 2-digit dimensions.
            else if (value > 99)
                ratio = 2.5; // For 3-digit dimensions.
            else if (value > 999)
                ratio = 3.5; // For 4-digit dimensions.

            return ratio;
        }

        /// <summary>
        /// Move the scpecified dimension segment text to the given distance on the given side.
        /// </summary>
        /// <param name="dimensionSegment">Dimension segment, which text need to be moved.</param>
        /// <param name="dimension">Dimension to which segment belongs.</param>
        /// <param name="moveDistance">Distance to move.</param>
        /// <param name="moveDirection">Direction to move (left/right).</param>
        internal static void MoveSegmentText(DimensionSegment dimensionSegment, Dimension dimension, double moveDistance, MoveDirection moveDirection)
        {
            // Get moving direction
            Line line = dimension.Curve as Line;
            XYZ direction = line.Direction;

            XYZ vector = (moveDirection == MoveDirection.Right)
                ? direction * moveDistance
                : direction.Negate() * moveDistance;

            // Get the current text XYZ position.
            XYZ currentTextPosition = dimensionSegment.TextPosition;
            XYZ newTextPosition = Transform.CreateTranslation(vector).OfPoint(currentTextPosition);
            
            // Set the new text position for the segment's text.
            dimensionSegment.TextPosition = newTextPosition;
        }

        internal enum MoveDirection
        {
            Left,
            Right
        }
    }
}
