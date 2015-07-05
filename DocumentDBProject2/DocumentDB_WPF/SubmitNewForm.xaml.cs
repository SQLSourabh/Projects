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
using System.IO;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;

namespace DocumentDB_WPF
{
    /// <summary>
    /// Interaction logic for SubmitNewForm.xaml
    /// </summary>
    public partial class SubmitNewForm : Window
    {       
        public SubmitNewForm()
        {
            InitializeComponent();
        }
        private void Submit_Request(object sender, RoutedEventArgs e)
        {
            //Mark all the Submit Fields As ReadOnly
            btAttachments.IsEnabled = false;
            txtAttachments.IsEnabled = false;
            txtRequestorName.IsEnabled = false;
            txtRequestorAlias.IsEnabled = false;
            dpRequestDate.IsEnabled = false;
            cbRequestCriticality.IsEnabled = false;
            cbPrimaryProductName.IsEnabled = false;
            cbServiceCategory.IsEnabled = false;
            rtbServiceDescription.IsEnabled = false;
            cbAssignedTo.IsEnabled = false;
            cbActionState.IsEnabled = true;
            dpActionDate.IsEnabled = true;
            rtbActionComments.IsEnabled = true;

            //Generate the XML
             SaveFormDataInXML();
        }
        private void Add_Attachments(object sender, RoutedEventArgs e)
        { 
            // Use the Window Open File Dialouge to Select Some Files
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Multiselect = true;
            Nullable<bool> result = dlg.ShowDialog();
            if (result != null)
            {
                foreach(string files in dlg.FileNames)
                {
                    txtAttachments.Text = txtAttachments.Text + files +";";
                }
            }
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
        private async void SaveFormDataInXML()
        {
            //Setup the Cloud Azure Blob Container
            //byte[] key = Encoding.ASCII.GetBytes(keystring);
            string requestid = "SRQ_" + DateTime.Now.ToString("dd_MMM_yyyy_HH_mm_ss");
            XElement atchFiles; 
            string[] attFiles = {""};

            // Copy the attachment files to Azure Blob Storage and generate the XML tab for all the File URL
            XDocument doc = new XDocument(new XElement("Contoso_Service_Request",
                      new XElement("ServiceRequest", 
                              new XElement("RequestId", requestid),
                              new XElement("ServiceRequestorName", txtRequestorName.Text.Trim()),
                              new XElement("ServiceRequestorAlias", txtRequestorAlias.Text.Trim()),
                              new XElement("ServiceRequestDate", dpRequestDate.SelectedDate),
                              new XElement("ServiceRequestCriticality", cbRequestCriticality.Text.Trim()),
                              new XElement("ServicePrimaryProduct", cbPrimaryProductName.Text.Trim()),
                              new XElement("ServiceCategory", cbServiceCategory.Text.Trim()),
                              new XElement("ServiceAssignedTo", cbAssignedTo.Text.Trim()),
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
                attFiles = txtAttachments.Text.Split(';');
                atchFiles = doc.Descendants("Attachments").First();
                atchFiles.Add(new XAttribute("AttachmentCount",attFiles.Count()-1));                
                foreach (string atfiles in attFiles)
                {
                    if (atfiles != "")
                    {
                        try
                        {
                            atchFiles.Add(new XElement("FileUrl",@atfiles));
                        }
                        catch (Exception exp)
                        {
                            MessageBox.Show(exp.Message);
                        }
                    }
                }              
            }
            var xmlDocument = new XmlDocument();
            using (var xmlReader = doc.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            /// Also Insert the Data to Document DB
            var obj = new DocumentDBOperations();
            await obj.InsertDocumentDB(xmlDocument, attFiles);

            MessageBox.Show("Service Request Submitted Successfully - Exitng"); //Change Message As Needed
            this.Close();
        }
     }
}
