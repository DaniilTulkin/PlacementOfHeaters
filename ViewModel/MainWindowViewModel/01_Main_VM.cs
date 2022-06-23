using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;

namespace PlacementOfHeaters
{
    public partial class MainWindowViewModel : ModelBase
    {
        public Action CloseAction { get; set; }

        private MainWindowModelService MainWindowModelService;

        private ObservableCollection<Document> documents;
        public ObservableCollection<Document> Documents
        {
            get
            {
                return documents;
            }
            set
            {
                documents = value;
                OnPropertyChanged("Documents");
            }
        }

        private Document selectedDocument;
        public Document SelectedDocument
        {
            get
            {
                return selectedDocument;
            }
            set
            {
                selectedDocument = value;
                OnPropertyChanged("SelectedDocument");
            }
        }

        public MainWindowViewModel(UIApplication app)
        {
            MainWindowModelService = new MainWindowModelService(app);
            Documents = MainWindowModelService.PopulateDocuments();
            MechEquipTypes = MainWindowModelService.PopulateMechEquipTypes();
        }

        public ICommand cmbDocumentChanged => new RelayCommandWithoutParameter(OncmbDocumentChanged);
        private void OncmbDocumentChanged()
        {
            WindowFamilies = MainWindowModelService.PopulateWindowFamilies(SelectedDocument);
            CurtainWallTypes = MainWindowModelService.PopulateCurtainWallTypes(SelectedDocument);
        }

        public ICommand btnOK => new RelayCommandWithoutParameter(OnbtnOK);
        private void OnbtnOK()
        {
            if (IsWindow)
            {
                MainWindowModelService.PlaceHeaters(SelectedMechEquipType, 
                                                    SelectedMechEquipTypeProperty, 
                                                    ElevParam,
                                                    DistanceFromWall,
                                                    SelectedWindowFamily, 
                                                    SelectedWindowProperty);
            }
            else
            {
                MainWindowModelService.PlaceHeaters(SelectedMechEquipType, 
                                                    SelectedMechEquipTypeProperty, 
                                                    ElevParam,
                                                    DistanceFromWall,
                                                    SelectedCurtainWallType, 
                                                    PlacementStep);
            }
            CloseAction();
        }

        public ICommand btnCancel => new RelayCommandWithoutParameter(OnbtnCancel);
        private void OnbtnCancel()
        {
            CloseAction();
        }
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
