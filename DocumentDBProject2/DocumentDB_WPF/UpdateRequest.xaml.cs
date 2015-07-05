using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;

namespace DocumentDB_WPF
{
    /// <summary>
    /// Interaction logic for UpdateRequest.xaml
    /// </summary>
    public partial class UpdateRequest : Window
    {
        public string RequestId="";
        public XmlNode Attachments;
        public UpdateRequest()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                DocumentDB_WPF.DocDBProjectDataSet docDBProjectDataSet = ((DocumentDB_WPF.DocDBProjectDataSet)(this.FindResource("docDBProjectDataSet")));
                // Load data into the table ServiceCritacility. You can modify this code as needed.
                DocumentDB_WPF.DocDBProjectDataSetTableAdapters.ServiceCritacilityTableAdapter docDBProjectDataSetServiceCritacilityTableAdapter = new DocumentDB_WPF.DocDBProjectDataSetTableAdapters.ServiceCritacilityTableAdapter();
                docDBProjectDataSetServiceCritacilityTableAdapter.Fill(docDBProjectDataSet.ServiceCritacility);
                System.Windows.Data.CollectionViewSource serviceCritacilityViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("serviceCritacilityViewSource")));
                serviceCritacilityViewSource.View.MoveCurrentToFirst();
                // Load data into the table EmployeeInfo. You can modify this code as needed.
                DocumentDB_WPF.DocDBProjectDataSetTableAdapters.EmployeeInfoTableAdapter docDBProjectDataSetEmployeeInfoTableAdapter = new DocumentDB_WPF.DocDBProjectDataSetTableAdapters.EmployeeInfoTableAdapter();
                docDBProjectDataSetEmployeeInfoTableAdapter.Fill(docDBProjectDataSet.EmployeeInfo);
                System.Windows.Data.CollectionViewSource employeeInfoViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("employeeInfoViewSource")));
                employeeInfoViewSource.View.MoveCurrentToFirst();
                // Load data into the table ActionStatus. You can modify this code as needed.
                DocumentDB_WPF.DocDBProjectDataSetTableAdapters.ActionStatusTableAdapter docDBProjectDataSetActionStatusTableAdapter = new DocumentDB_WPF.DocDBProjectDataSetTableAdapters.ActionStatusTableAdapter();
                docDBProjectDataSetActionStatusTableAdapter.Fill(docDBProjectDataSet.ActionStatus);
                System.Windows.Data.CollectionViewSource actionStatusViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("actionStatusViewSource")));
                actionStatusViewSource.View.MoveCurrentToFirst();
                // Load data into the table ProductInfo. You can modify this code as needed.
                DocumentDB_WPF.DocDBProjectDataSetTableAdapters.ProductInfoTableAdapter docDBProjectDataSetProductInfoTableAdapter = new DocumentDB_WPF.DocDBProjectDataSetTableAdapters.ProductInfoTableAdapter();
                docDBProjectDataSetProductInfoTableAdapter.Fill(docDBProjectDataSet.ProductInfo);
                System.Windows.Data.CollectionViewSource productInfoViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("productInfoViewSource")));
                productInfoViewSource.View.MoveCurrentToFirst();
                // TODO: Add code here to load data into the table ProductInfo1.
                // This code could not be generated, because the docDBProjectDataSetProductInfo1TableAdapter.Fill method is missing, or has unrecognized parameters.
                DocumentDB_WPF.DocDBProjectDataSetTableAdapters.ProductInfo1TableAdapter docDBProjectDataSetProductInfo1TableAdapter = new DocumentDB_WPF.DocDBProjectDataSetTableAdapters.ProductInfo1TableAdapter();
                System.Windows.Data.CollectionViewSource productInfo1ViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("productInfo1ViewSource")));
                productInfo1ViewSource.View.MoveCurrentToFirst();
            }
            catch (Exception exp)
            {
                MessageBox.Show("Error Loading the Service Request Form - Error Was: {0}", exp.Message);
                this.Close();
            }
        }
        private void Update_Request(object sender, RoutedEventArgs e)
        { 
            //Generate the XML From the Forms DataSaveFormDataInXML
            SaveFormDataInXML();
        }
        private async void SaveFormDataInXML()
        {
            //MessageBox.Show(RequestId);
            XElement atchFiles;
            // Generate XML from the Form Data

            XDocument doc = new XDocument(new XElement("Contoso_Service_Request",
                 new XElement("ServiceRequest",
                     new XElement("RequestId", RequestId),
                     new XElement("ServiceRequestorName", txtRequestorName.Text.Trim()),
                     new XElement("ServiceRequestorAlias", txtRequestorAlias.Text.Trim()),
                     new XElement("ServiceRequestDate", txtRequestDate.Text.Trim()),
                     new XElement("ServiceRequestCriticality", txtRequestCriticality.Text.Trim()),
                     new XElement("ServicePrimaryProduct", txtRequestProduct.Text.Trim()),
                     new XElement("ServiceCategory", txtRequestCategory.Text.Trim()),
                     new XElement("ServiceAssignedTo", txtRequestorAssignedTo.Text.Trim()),
                     new XElement("ServiceDescription",
                     new TextRange(rtbServiceDescription.Document.ContentStart, rtbServiceDescription.Document.ContentEnd).Text.Trim()),
                     new XElement("Attachments")),
                 new XElement("Action",
                 new XElement("ActionDate", dpActionDate.SelectedDate),
                 new XElement("ActionStatus", cbActionState.Text.Trim()),
                 new XElement("ActionComments", new TextRange(rtbActionComments.Document.ContentStart, rtbActionComments.Document.ContentEnd).Text.Trim())
                 )));
            if (txtAttachments.Text != "")
            {
                //Create The XML Element for the Attachments.
                string[] attFiles = txtAttachments.Text.Split(';');
                atchFiles = doc.Descendants("Attachments").First();
                atchFiles.Add(new XAttribute("AttachmentCount", attFiles.Count() - 1));
                foreach (string atfiles in attFiles)
                {
                    //Copy the File to Azure Blob and Include the Azure URI of the file to the XElement
                    if (atfiles != "")
                    {
                        atchFiles.Add(new XElement("FileUrl", atfiles));
                    }
                }
            }
            var xmlDocument = new XmlDocument();
            using (var xmlReader = doc.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }

            /// Update the Data to Document DB
            /// 
            var obj = new DocumentDBOperations();
            await obj.UpdateDocumentDB(xmlDocument,RequestId);

            MessageBox.Show("Service Request Updated Successfully - Exitng"); //Change Message As Needed
            this.Close();
        }
    }
}
