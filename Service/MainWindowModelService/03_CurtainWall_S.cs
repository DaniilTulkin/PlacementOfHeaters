using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PlacementOfHeaters
{
    public partial class MainWindowModelService
    {
        internal ObservableCollection<Element> PopulateCurtainWallTypes(Document selectedDocument)
        {
            ObservableCollection<Element> result = new ObservableCollection<Element>();
            var elements = new FilteredElementCollector(selectedDocument)
                               .OfClass(typeof(Wall))
                               .Select(x => selectedDocument.GetElement(x.GetTypeId()))
                               .Where(x => ((WallType)x).Kind == WallKind.Curtain)
                               .GroupBy(x => x.Name)
                               .Select(x => x.First());

            elements.ToList().Sort((x, y) => string.Compare(x.Name, y.Name));
            foreach (Element element in elements) result.Add(element);

            return result;
        }

        internal void PlaceHeaters(Element selectedMechEquipType, 
                                   Parameter selectedMechEquipTypeProperty, 
                                   int elevParam,
                                   int distanceFromWall,
                                   Element selectedCurtainWallType, 
                                   int placementStep)
        {
            if (selectedMechEquipType == null
                || selectedCurtainWallType == null
                || placementStep == 0) return;

            using (Transaction t = new Transaction(doc, "Размещение приборов под витражами"))
            {
                t.Start();

                Document selectedDocument = selectedCurtainWallType.Document;
                var elements = new FilteredElementCollector(selectedDocument)
                                   .OfCategory(BuiltInCategory.OST_Walls)
                                   .WhereElementIsNotElementType()
                                   .Where(x => selectedDocument.GetElement(x.GetTypeId()).Name == selectedCurtainWallType.Name);

                View3D view3D = null;
                if (doc.ActiveView is View3D)
                {
                    view3D = doc.ActiveView as View3D;
                }
                else
                {
                    ViewFamilyType viewType = new FilteredElementCollector(doc)
                                                  .OfClass(typeof(ViewFamilyType))
                                                  .Cast<ViewFamilyType>()
                                                  .Where(x => x.ViewFamily == ViewFamily.ThreeDimensional)
                                                  .FirstOrDefault();
                    view3D = View3D.CreateIsometric(doc, viewType.Id);
                }

                foreach (Element element in elements.ToList())
                {
                    Wall curtain = element as Wall;
                    Curve curtainLocation = ((LocationCurve)curtain.Location).Curve;

                    double curtainLength = curtainLocation.Length;
                    double step = UnitUtils.ConvertToInternalUnits(placementStep, DisplayUnitType.DUT_MILLIMETERS);
                    for (double i = step; i < curtainLength - curtainLength % step; i += step)
                    {
                        XYZ pointOnCurve = curtainLocation.Evaluate(i, false);
                        XYZ point = GetInternalPoint(pointOnCurve, curtain, view3D, distanceFromWall);
                        if (point == null) continue;

                        Level levelOfSelectedDoc = selectedDocument.GetElement(element.LevelId) as Level;
                        Level level = null;
                        foreach (Level levelOfDoc in new FilteredElementCollector(doc).OfClass(typeof(Level)).WhereElementIsNotElementType())
                        {
                            if (levelOfSelectedDoc.Elevation == levelOfDoc.Elevation)
                            {
                                level = levelOfDoc;
                                break;
                            }
                        }
                        if (level == null) continue;

                        FamilySymbol familySymbol = selectedMechEquipType as FamilySymbol;
                        familySymbol.Activate();

                        FamilyInstance familyInstance = doc.Create.NewFamilyInstance(point, familySymbol, level, StructuralType.NonStructural);
                        RotateParallelToWall(point, familyInstance, curtain);

                        familyInstance.get_Parameter(BuiltInParameter.INSTANCE_ELEVATION_PARAM).Set(UnitUtils.ConvertToInternalUnits(elevParam, DisplayUnitType.DUT_MILLIMETERS));
                    }

                }

                if (!(doc.ActiveView is View3D)) doc.Delete(view3D.Id);

                t.Commit();
            }
        }
    }
}
