using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PlacementOfHeaters
{
    public partial class MainWindowModelService
    {
        internal ObservableCollection<KeyValuePair<string, Element>> PopulateMechEquipTypes()
        {
            ObservableCollection<KeyValuePair<string, Element>> result = new ObservableCollection<KeyValuePair<string, Element>>();
            var elements = new FilteredElementCollector(doc)
                               .OfCategory(BuiltInCategory.OST_MechanicalEquipment)
                               .WhereElementIsElementType();

            elements.ToList().Sort((x, y) => string.Compare(((ElementType)x).FamilyName, ((ElementType)y).FamilyName));

            foreach (Element element in elements)
            {
                string FamilyAndType = ((ElementType)element).FamilyName + ": " + element.Name;
                result.Add(new KeyValuePair<string, Element>(FamilyAndType, element));
            }

            return result;
        }

        internal ObservableCollection<Parameter> PopulateMechEquipTypeProperties(Element selectedMechEquipType)
        {
            ObservableCollection<Parameter> result = new ObservableCollection<Parameter>();
            List<Parameter> parameters = new List<Parameter>();
            using (Transaction t = new Transaction(doc, "Получение параметров"))
            {
                t.Start();

                if (selectedMechEquipType is FamilySymbol familySymbol)
                {
                    familySymbol.Activate();
                    FamilyInstance familyInstance = doc.Create.NewFamilyInstance(XYZ.Zero, familySymbol, StructuralType.NonStructural);
                    foreach (Parameter parameter in familyInstance.Parameters) if (parameter.Definition.ParameterType == ParameterType.Length) parameters.Add(parameter);
                }

                t.RollBack();
            }

            parameters.Sort((x, y) => string.Compare(x.Definition.Name, y.Definition.Name));
            foreach (Parameter parameter in parameters) result.Add(parameter);

            return result;
        }
    }
}
