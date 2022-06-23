using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PlacementOfHeaters
{
    public partial class MainWindowViewModel : ModelBase
    {
        private ObservableCollection<KeyValuePair<string, Element>> mechEquipTypes;
        public ObservableCollection<KeyValuePair<string, Element>>  MechEquipTypes
        {
            get
            {
                return mechEquipTypes;
            }
            set
            {
                mechEquipTypes = value;
                OnPropertyChanged("MechEquipTypes");
            }
        }

        private Element selectedMechEquipType;
        public Element SelectedMechEquipType
        {
            get
            {
                return selectedMechEquipType;
            }
            set
            {
                selectedMechEquipType = value;
                OnPropertyChanged("SelectedMechEquipType");
            }
        }

        private ObservableCollection<Parameter> mechEquipTypeProperties;
        public ObservableCollection<Parameter> MechEquipTypeProperties
        {
            get
            {
                return mechEquipTypeProperties;
            }
            set
            {
                mechEquipTypeProperties = value;
                OnPropertyChanged("MechEquipTypeProperties");
            }
        }

        private Parameter selectedMechEquipTypeProperty;
        public Parameter SelectedMechEquipTypeProperty
        {
            get
            {
                return selectedMechEquipTypeProperty;
            }
            set
            {
                selectedMechEquipTypeProperty = value;
                OnPropertyChanged("SelectedMechEquipTypeProperty");
            }
        }

        private int distanceFromWall;
        public int DistanceFromWall
        {
            get
            {
                return distanceFromWall;
            }
            set
            {
                distanceFromWall = value;
                OnPropertyChanged("DistanceFromWall");
            }
        }

        private int elevParam;
        public int ElevParam
        {
            get
            {
                return elevParam;
            }
            set
            {
                elevParam = value;
                OnPropertyChanged("ElevParam");
            }
        }

        private bool mechEquipTypeIsSelected;
        public bool MechEquipTypeIsSelected
        {
            get
            {
                return mechEquipTypeIsSelected;
            }
            set
            {
                mechEquipTypeIsSelected = value;
                OnPropertyChanged("MechEquipTypeIsSelected");
            }
        }

        public ICommand cmbMechEquipTypeChanged => new RelayCommandWithoutParameter(OncmbMechEquipTypeChanged);
        private void OncmbMechEquipTypeChanged()
        {
            MechEquipTypeIsSelected = true;
            MechEquipTypeProperties = MainWindowModelService.PopulateMechEquipTypeProperties(SelectedMechEquipType);
        }
    }
}
