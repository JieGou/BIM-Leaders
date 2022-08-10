using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace BIM_Leaders_Core
{
    internal static class DimensionUtils
    {
        internal static void AdjustText(Dimension dimension)
        {
            Document doc = dimension.Document;
            double scale = doc.ActiveView.Scale;

#if VERSION2020
            DisplayUnitType units = doc.GetUnits().GetFormatOptions(UnitType.UT_Length).DisplayUnits;
#else
            ForgeTypeId units = doc.GetUnits().GetFormatOptions(SpecTypeId.Length).GetUnitTypeId();
#endif

            DimensionSegmentArray dimensionSegments = dimension.Segments;

            // Get the bool array that indicates if we need to move dimension segment.
            List<bool> needMove = new List<bool>();
            List<double> moveDistances = new List<double>();

            foreach (DimensionSegment dimensionSegment in dimensionSegments)
            {
                if (!dimensionSegment.IsTextPositionAdjustable())
                {
                    needMove.Add(false);
                    continue;
                }

                double value = UnitUtils.ConvertFromInternalUnits(dimensionSegment.Value.Value, units);

                // Ratio of dimension segment text height to width.
                double ratio = 0.7;
                if (value > 9)
                    ratio = 1.5; // For 2-digit dimensions.
                else if (value > 99)
                    ratio = 2.5; // For 3-digit dimensions.
                else if (value > 999)
                    ratio = 3.5; // For 4-digit dimensions.

                // Size of the dimension along dimension line/
                double textSizeD = dimension.DimensionType.get_Parameter(BuiltInParameter.TEXT_SIZE).AsDouble();
                double textSize = UnitUtils.ConvertFromInternalUnits(textSizeD, units) * ratio;

                // Factor calculated if dimension should be moved to the side.
                double factor = value / (scale * textSize);

                double moveDistance = 0;
                if (factor < 1)
                {
                    needMove.Add(true);

                    // Dimension move distance calculation.
                    double moveDistanceCm = 0;
                    moveDistanceCm += value / 2;            // Move to the edge of the layer.
                    moveDistanceCm += textSize * scale / 2; // Add half of dimension text width.
                    moveDistanceCm += scale * 0.1;          // Add a bit more (1 mm) to make a little gap between dim and edge.
                    moveDistance = UnitUtils.ConvertToInternalUnits(moveDistanceCm, units);
                }  
                else
                    needMove.Add(false);

                moveDistances.Add(moveDistance);
            }

            // Moving the segments with looking on their neighbours.
            for (int i = 0; i < dimensionSegments.Size; i++)
            {
                if (!needMove[i])
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
                if (needMove[i - 1] && needMove[i + 1])
                    continue;

                // Next option - if we need to move to the left, because right segment is too small.
                if (needMove[i + 1])
                {
                    MoveSegmentText(dimensionSegment, dimension, moveDistances[i], MoveDirection.Left);
                    continue;
                }

                // It two other options move to the right.
                MoveSegmentText(dimensionSegment, dimension, moveDistances[i], MoveDirection.Right);
            }
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
