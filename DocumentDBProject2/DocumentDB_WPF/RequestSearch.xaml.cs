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
    /// Interaction logic for RequestSearch.xaml
    /// </summary>
    public partial class RequestSearch : Window
    {
        public RequestSearch()
        {
            InitializeComponent();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            DocumentDB_WPF.DocDBProjectDataSet docDBProjectDataSet = ((DocumentDB_WPF.DocDBProjectDataSet)(this.FindResource("docDBProjectDataSet")));
            // Load data into the table EmployeeInfo. You can modify this code as needed.
            DocumentDB_WPF.DocDBProjectDataSetTableAdapters.EmployeeInfoTableAdapter docDBProjectDataSetEmployeeInfoTableAdapter = new DocumentDB_WPF.DocDBProjectDataSetTableAdapters.EmployeeInfoTableAdapter();
            docDBProjectDataSetEmployeeInfoTableAdapter.Fill(docDBProjectDataSet.EmployeeInfo);
            System.Windows.Data.CollectionViewSource employeeInfoViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("employeeInfoViewSource")));
            employeeInfoViewSource.View.MoveCurrentToFirst();
            // Load data into the table ProductInfo1. You can modify this code as needed.
            DocumentDB_WPF.DocDBProjectDataSetTableAdapters.ProductInfo1TableAdapter docDBProjectDataSetProductInfo1TableAdapter = new DocumentDB_WPF.DocDBProjectDataSetTableAdapters.ProductInfo1TableAdapter();
            docDBProjectDataSetProductInfo1TableAdapter.Fill(docDBProjectDataSet.ProductInfo1);
            System.Windows.Data.CollectionViewSource productInfo1ViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("productInfo1ViewSource")));
            productInfo1ViewSource.View.MoveCurrentToFirst();
            // Load data into the table ProductInfo. You can modify this code as needed.
            DocumentDB_WPF.DocDBProjectDataSetTableAdapters.ProductInfoTableAdapter docDBProjectDataSetProductInfoTableAdapter = new DocumentDB_WPF.DocDBProjectDataSetTableAdapters.ProductInfoTableAdapter();
            docDBProjectDataSetProductInfoTableAdapter.Fill(docDBProjectDataSet.ProductInfo);
            System.Windows.Data.CollectionViewSource productInfoViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("productInfoViewSource")));
            productInfoViewSource.View.MoveCurrentToFirst();
        }
        private void SearchRequest(object sender, RoutedEventArgs e)
        {
            cbServiceCategory.IsEnabled = false;
            cbPrimaryProductName.IsEnabled = false;
            cbAssignedTo.IsEnabled = false;
            btSearchAction.IsEnabled = false;
            
            var docDBOps = new DocumentDBOperations();
            //MessageBox.Show(cbServiceCategory.SelectedValue.ToString());
            List<string> docs = docDBOps.SearchDocumentDB(cbServiceCategory.Text, 
                                        cbPrimaryProductName.Text, 
                                        cbAssignedTo.Text);
            foreach (string document in docs)
            {
                cbSRToAction.Items.Add(document);
            }
            btActionRequest.IsEnabled = true;
            if (cbSRToAction.Items.Count > 0)
            { cbSRToAction.SelectedValue = docs[0]; }

        }
        private void ActionRequest(object sender, RoutedEventArgs e)
        {
            string srToAction = cbSRToAction.SelectedItem.ToString();
            var docDBOps = new DocumentDBOperations();
            XmlDocument doc = docDBOps.RetrieveDocument(srToAction);
            //MessageBox.Show();
            
             //Create and Populate the Update Request Form From XML.
            UpdateRequest obj = new UpdateRequest();
            obj.RequestId = doc.GetElementsByTagName("RequestId")[0].InnerText;
            obj.txtRequestorName.Text = doc.GetElementsByTagName("ServiceRequestorName")[0].InnerText;
            obj.txtRequestorName.IsEnabled = false;

            obj.txtRequestorAlias.Text = doc.GetElementsByTagName("ServiceRequestorAlias")[0].InnerText;
            obj.txtRequestorAlias.IsEnabled = false;

            obj.txtRequestDate.Text = doc.GetElementsByTagName("ServiceRequestDate")[0].InnerText;
            obj.txtRequestDate.IsEnabled = false;

            obj.txtRequestCriticality.Text = doc.GetElementsByTagName("ServiceRequestCriticality")[0].InnerText;
            obj.txtRequestCriticality.IsEnabled = false;

            obj.txtRequestProduct.Text = doc.GetElementsByTagName("ServicePrimaryProduct")[0].InnerText;
            obj.txtRequestProduct.IsEnabled = false;

            obj.txtRequestCategory.Text = doc.GetElementsByTagName("ServiceCategory")[0].InnerText;
            obj.txtRequestCategory.IsEnabled = false;
            
            obj.txtRequestorAssignedTo.Text = doc.GetElementsByTagName("ServiceAssignedTo")[0].InnerText;
            obj.txtRequestorAssignedTo.IsEnabled = false;

            XmlNodeList xlist = doc.GetElementsByTagName("FileUrl");
            foreach (XmlNode x in xlist)
            {
                obj.txtAttachments.AppendText(x.InnerText + ";");
            }
            obj.txtAttachments.IsEnabled = false;

            obj.rtbServiceDescription.AppendText(doc.GetElementsByTagName("ServiceDescription")[0].InnerText);
            obj.rtbServiceDescription.IsEnabled = false;

            obj.dpActionDate.IsEnabled = true;
            obj.cbActionState.IsEnabled = true;
            obj.rtbActionComments.IsReadOnly = false;
            obj.Attachments = doc.GetElementsByTagName("Attachments")[0];
            obj.Show();

            this.Close();
        }
    }
}
