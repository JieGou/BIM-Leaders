using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

namespace BIM_Leaders_Core
{
    /// <summary>
    /// Class to converting list of bools (from selected checkboxes list) to list of types.
    /// </summary>
    public static class Categories
    {
        private static readonly List<List<Type>> _categoriesList = new List<List<Type>>(){
            new List<Type>(){ typeof(AreaScheme) },
            new List<Type>(){ typeof(BrowserOrganization) },
            new List<Type>(){ typeof(BuildingPadType) },
            new List<Type>(){ typeof(CeilingType) },
            new List<Type>(){ typeof(CurtainSystemType) },
            new List<Type>(){ typeof(DimensionType) },
            new List<Type>(){ typeof(Family), typeof(FamilySymbol) },
            new List<Type>(){ typeof(FilledRegionType) },
            new List<Type>(){ typeof(GridType) },
            new List<Type>(){ typeof(GroupType) },
            new List<Type>(){ typeof(LevelType) },
            new List<Type>(){ typeof(LinePatternElement) },
            new List<Type>(){ typeof(Material) },
            new List<Type>(){ typeof(PanelType) },
            new List<Type>(){ typeof(ContinuousRailType), typeof(RailingType) },
            new List<Type>(){ typeof(FasciaType), typeof(GutterType), typeof(RoofType) },
            new List<Type>(){ typeof(SpotDimensionType) },
            new List<Type>(){ typeof(StairsType) },
            new List<Type>(){ typeof(StairsLandingType) },
            new List<Type>(){ typeof(StairsRunType) },
            new List<Type>(){ typeof(TextNoteType) },
            new List<Type>(){ typeof(ViewDrafting), typeof(ViewPlan), typeof(ViewSchedule), typeof(ViewSection) },
            new List<Type>(){ typeof(WallType) },
            new List<Type>(){ typeof(WallFoundationType) }
        };

        public static List<Type> GetTypesList(List<bool> categoriesBool)
        {
            List<Type> CategoriesList = new List<Type>();
            for (int i = 0; i < categoriesBool.Count; i++)
            {
                if (categoriesBool[i])
                    CategoriesList.AddRange(_categoriesList[i]);
            }
            return CategoriesList;
        }
    }
}
