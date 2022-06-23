using Autodesk.Revit.DB;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PlacementOfHeaters
{
    public partial class MainWindowViewModel : ModelBase
    {
        private ObservableCollection<Element> curtainWallTypes;
        public ObservableCollection<Element> CurtainWallTypes
        {
            get
            {
                return curtainWallTypes;
            }
            set
            {
                curtainWallTypes = value;
                OnPropertyChanged("CurtainWallTypes");
            }
        }

        private Element selectedCurtainWallType;
        public Element SelectedCurtainWallType
        {
            get
            {
                return selectedCurtainWallType;
            }
            set
            {
                selectedCurtainWallType = value;
                OnPropertyChanged("SelectedCurtainWallType");
            }
        }

        private bool curtainWallTypeIsSelected;
        public bool CurtainWallTypeIsSelected
        {
            get
            {
                return curtainWallTypeIsSelected;
            }
            set
            {
                curtainWallTypeIsSelected = value;
                OnPropertyChanged("CurtainWallTypeIsSelected");
            }
        }

        private int placementStep;
        public int PlacementStep
        {
            get
            {
                return placementStep;
            }
            set
            {
                placementStep = value;
                OnPropertyChanged("PlacementStep");
            }
        }

        public ICommand rdbIsCurtainWall => new RelayCommandWithoutParameter(OnrdbIsCurtainWall);
        private void OnrdbIsCurtainWall()
        {
            IsWindow = false;
            SelectedWindowFamily = null;
            WindowFamilyIsSelected = false;
            SelectedWindowProperty = null;
        }

        public ICommand cmbCurtainWallTypeChanged => new RelayCommandWithoutParameter(OncmbCurtainWallTypeChanged);
        private void OncmbCurtainWallTypeChanged()
        {
            CurtainWallTypeIsSelected = true;
        }
    }
}
