using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PlacementOfHeaters
{
    public partial class MainWindowModelService
    {
        internal ObservableCollection<Element> PopulateWindowFamilies(Document selectedDocument)
        {
            ObservableCollection<Element> result = new ObservableCollection<Element>();
            var elements = new FilteredElementCollector(selectedDocument)
                               .OfCategory(BuiltInCategory.OST_Windows)
                               .WhereElementIsNotElementType()
                               .Cast<FamilyInstance>()
                               .Select(x => x.Symbol.Family)
                               .GroupBy(x => x.Name)
                               .Select(x => x.First());

            elements.ToList().Sort((x, y) => string.Compare(x.Name, y.Name));
            foreach (Element element in elements) result.Add(element);

            return result;
        }

        internal ObservableCollection<Parameter> PopulateWindowProperties(Element selectedWindowFamily)
        {
            if (selectedWindowFamily == null) return null;

            ObservableCollection<Parameter> result = new ObservableCollection<Parameter>();
            Document selectedDocument = selectedWindowFamily.Document;
            ParameterSet instanceParameterSet = new FilteredElementCollector(selectedDocument)
                                                    .OfCategory(BuiltInCategory.OST_Windows)
                                                    .WhereElementIsNotElementType()
                                                    .Where(x => ((ElementType)selectedDocument.GetElement(x.GetTypeId())).FamilyName == selectedWindowFamily.Name)
                                                    .FirstOrDefault()
                                                    .Parameters;

            ParameterSet symbolParameterSet = new FilteredElementCollector(selectedDocument)
                                                  .WherePasses(new FamilySymbolFilter(selectedWindowFamily.Id))
                                                  .FirstOrDefault()
                                                  .Parameters;

            List<Parameter> parameters = new List<Parameter>();
            foreach (Parameter parameter in instanceParameterSet) if (parameter.Definition.ParameterType == ParameterType.Length) parameters.Add(parameter);
            foreach (Parameter parameter in symbolParameterSet) if (parameter.Definition.ParameterType == ParameterType.Length) parameters.Add(parameter);
            parameters.Sort((x, y) => string.Compare(x.Definition.Name, y.Definition.Name));
            foreach (Parameter parameter in parameters) result.Add(parameter);

            return result;
        }

        internal void PlaceHeaters(Element selectedMechEquipType, 
                                   Parameter selectedMechEquipTypeProperty, 
                                   int elevParam, 
                                   int distanceFromWall,
                                   Element selectedWindowFamily, 
                                   Parameter selectedWindowProperty)
        {
            if (selectedMechEquipType == null
                || selectedWindowFamily == null) return;

            using (Transaction t= new Transaction(doc, "Размещение приборов под окнами"))
            {
                t.Start();

                Document selectedDocument = selectedWindowFamily.Document;
                var elements = new FilteredElementCollector(selectedDocument)
                                   .OfCategory(BuiltInCategory.OST_Windows)
                                   .WhereElementIsNotElementType()
                                   .Where(x => ((ElementType)selectedDocument.GetElement(x.GetTypeId())).FamilyName == selectedWindowFamily.Name);

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
                    XYZ windowLocationPoint = ((LocationPoint)element.Location).Point;
                    Wall hostWall = ((FamilyInstance)element).Host as Wall;
                    if (hostWall == null) continue;

                    XYZ point = GetInternalPoint(windowLocationPoint, hostWall, view3D, distanceFromWall);
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
                    RotateParallelToWall(point, familyInstance, hostWall);

                    if (selectedMechEquipTypeProperty != null 
                        && selectedWindowProperty != null)
                    {
                        var parameterValue = element.get_Parameter(selectedWindowProperty.Definition).AsDouble();
                        if (parameterValue == 0) parameterValue = ((FamilyInstance)element).Symbol.get_Parameter(selectedWindowProperty.Definition).AsDouble();
                        familyInstance.get_Parameter(selectedMechEquipTypeProperty.Definition).Set(parameterValue);                    
                    }
                    familyInstance.get_Parameter(BuiltInParameter.INSTANCE_ELEVATION_PARAM).Set(UnitUtils.ConvertToInternalUnits(elevParam, DisplayUnitType.DUT_MILLIMETERS));
                }

                if (!(doc.ActiveView is View3D)) doc.Delete(view3D.Id);

                t.Commit();
            }
        }

        private XYZ GetInternalPoint(XYZ point, Wall wall, View3D view3D, int distanceFromWall)
        {
            ICollection<BuiltInCategory> categoryFilter = new Collection<BuiltInCategory>
            { 
                BuiltInCategory.OST_Walls, 
                BuiltInCategory.OST_Windows, 
                BuiltInCategory.OST_Doors 
            };
            ReferenceIntersector intersector = new ReferenceIntersector(new ElementMulticategoryFilter(categoryFilter), FindReferenceTarget.All, view3D);
            intersector.FindReferencesInRevitLinks = true;

            double halfWidth = wall.Width / 2;

            XYZ vector = null;
            if (intersector.FindNearest(point, wall.Orientation.Normalize()) != null)
                vector = wall.Orientation.Normalize();
            else if (intersector.FindNearest(point, wall.Orientation.Negate().Normalize()) != null)
                vector = wall.Orientation.Negate().Normalize();
            else return null;

            return point + vector * (halfWidth + UnitUtils.ConvertToInternalUnits(distanceFromWall, DisplayUnitType.DUT_MILLIMETERS));
        }

        private void RotateParallelToWall(XYZ point, Element element, Wall wall)
        {
            Line axis = Line.CreateUnbound(point, XYZ.BasisZ);
            Location elementLocation = element.Location;
            XYZ facingOrientation = ((FamilyInstance)element).FacingOrientation;
            XYZ wallOrientation = wall.Orientation;
            double angle = facingOrientation.AngleTo(wallOrientation);

            while (!wallOrientation.IsAlmostEqualTo(((FamilyInstance)element).FacingOrientation))
                elementLocation.Rotate(axis, angle);
        }
    }
}