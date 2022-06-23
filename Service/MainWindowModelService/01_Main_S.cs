using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.ObjectModel;

namespace PlacementOfHeaters
{
    public partial class MainWindowModelService
    {
        private UIApplication app;
        private UIDocument uidoc;
        private Document doc;
        private RevitEvent revitEvent;

        public MainWindowModelService(UIApplication app)
        {
            this.app = app;
            uidoc = app.ActiveUIDocument;
            doc = uidoc.Document;
            revitEvent = new RevitEvent();
        }

        public void TransactionCreate(string transactionName, Delegate method, params object[] args)
        {
            using (Transaction t = new Transaction(doc, transactionName))
            {
                t.Start();
                method?.DynamicInvoke(args);
                t.Commit();
            }
        }

        internal ObservableCollection<Document> PopulateDocuments()
        {
            ObservableCollection<Document> result = new ObservableCollection<Document>();
            foreach (Document document in app.Application.Documents) result.Add(document);
            return result;
        }
    }
}
