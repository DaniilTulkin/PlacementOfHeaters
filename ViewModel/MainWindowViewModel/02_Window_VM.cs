using Autodesk.Revit.DB;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PlacementOfHeaters
{
    public partial class MainWindowViewModel : ModelBase
    {
        private bool isWindow = true;
        public bool IsWindow
        {
            get
            {
                return isWindow;
            }
            set
            {
                isWindow = value;
                OnPropertyChanged("IsWindow");
            }
        }

        private ObservableCollection<Element> windowFamilies;
        public ObservableCollection<Element> WindowFamilies
        {
            get
            {
                return windowFamilies;
            }
            set
            {
                windowFamilies = value;
                OnPropertyChanged("WindowFamilies");
            }
        }

        private Element selectedWindowFamily;
        public Element SelectedWindowFamily
        {
            get
            {
                return selectedWindowFamily;
            }
            set
            {
                selectedWindowFamily = value;
                OnPropertyChanged("SelectedWindowFamily");
            }
        }

        private bool windowFamilyIsSelected;
        public bool WindowFamilyIsSelected
        {
            get
            {
                return windowFamilyIsSelected;
            }
            set
            {
                windowFamilyIsSelected = value;
                OnPropertyChanged("WindowFamilyIsSelected");
            }
        }

        private ObservableCollection<Parameter> windowProperties;
        public ObservableCollection<Parameter> WindowProperties
        {
            get
            {
                return windowProperties;
            }
            set
            {
                windowProperties = value;
                OnPropertyChanged("WindowProperties");
            }
        }

        private Parameter selectedWindowProperty;
        public Parameter SelectedWindowProperty
        {
            get
            {
                return selectedWindowProperty;
            }
            set
            {
                selectedWindowProperty = value;
                OnPropertyChanged("SelectedWindowProperty");
            }
        }

        public ICommand rdbIsWindow => new RelayCommandWithoutParameter(OnrdbIsWindow);
        private void OnrdbIsWindow()
        {
            IsWindow = true;
            SelectedCurtainWallType = null;
            CurtainWallTypeIsSelected = false;
            PlacementStep = 0;
        }

        public ICommand cmbWindowFamilyChanged => new RelayCommandWithoutParameter(OncmbWindowFamilyChanged);
        private void OncmbWindowFamilyChanged()
        {
            WindowFamilyIsSelected = true;
            WindowProperties = MainWindowModelService.PopulateWindowProperties(SelectedWindowFamily);
        }
    }
}
