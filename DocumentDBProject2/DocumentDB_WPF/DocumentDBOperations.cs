using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using System.Windows;

namespace DocumentDB_WPF
{
    class DocumentDBOperations
    {
        private static string EndpointUrl = System.Configuration.ConfigurationSettings.AppSettings["EndpointUrl"];
        private static string AuthorizationKey = System.Configuration.ConfigurationSettings.AppSettings["AuthorizationKey"];
        public async Task InsertDocumentDB(XmlDocument doc, string[] attchmentfiles)
        {
            // Convert Xdocument to Json
            string jsonText = JsonConvert.SerializeXmlNode(doc);
            
            //Convert the JsonText string to Memory Stream
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(jsonText);
            writer.Flush();
            stream.Position = 0;

           // Connect and Insert into DocumentDB
           try
           {
               DocumentClient client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey);

               //Connect to the DB. If DB is not present create it
               var db = client.CreateDatabaseQuery().Where(d => d.Id == "ContosoServiceRequest").AsEnumerable().FirstOrDefault();
               if (db == null)
               {
                   db = client.CreateDatabaseAsync(new Database { Id = "ContosoServiceRequest" }).Result;
               }

               //Connect to the Collection. If DB is not present create it
               var collection = client.CreateDocumentCollectionQuery(db.SelfLink).Where(d => d.Id == "ServiceRequest").AsEnumerable().FirstOrDefault();
               if (collection == null)
               {
                   var collectionSpec = new DocumentCollection { Id = "ServiceRequest" };
                   var requestOptions = new RequestOptions { OfferType = "S2" };
                   collection = client.CreateDocumentCollectionAsync(db.SelfLink, collectionSpec, requestOptions).Result;
               }
               //Insert Document into the Collection using the Stream.
               Document curDoc =  await client.CreateDocumentAsync(collection.SelfLink, Resource.LoadFrom<Document>(stream));

               //Add Attachments to the Document
               //XmlNodeList xlist = doc.GetElementsByTagName("FileUrl");
               foreach (string x in attchmentfiles)
               {
                   //MessageBox.Show(@x.InnerText);
                   if (x != "")
                   {
                       using (FileStream fileStream = new FileStream(x, FileMode.Open))
                       {
                           //Create the attachment
                               string slug = x.Substring(x.LastIndexOf("\\") + 1, (x.Length - x.LastIndexOf("\\")) - 1);
                               string contenttype = GetFileContentType(@x);
                               await client.CreateAttachmentAsync(curDoc.AttachmentsLink, fileStream, new MediaOptions { ContentType = contenttype, Slug = slug}); 
                       }
                   }
               }
               stream.Dispose();
           }
           catch (Exception exp)
           {
               MessageBox.Show(exp.Message);
           }
        }
        public List<string> SearchDocumentDB(string ServiceCategory, string primaryProduct, string AssignedTo)
        {
            DocumentClient client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey);
            //Connect to the DB. If DB is not present create it
            var db = client.CreateDatabaseQuery().Where(d => d.Id == "ContosoServiceRequest").AsEnumerable().FirstOrDefault();
            if (db == null)
            {
                db = client.CreateDatabaseAsync(new Database { Id = "ContosoServiceRequest" }).Result;
            }
            //Connect to the Collection. If DB is not present create it
            var collection = client.CreateDocumentCollectionQuery(db.SelfLink).Where(d => d.Id == "ServiceRequest").AsEnumerable().FirstOrDefault();
            if (collection == null)
            {
                var collectionSpec = new DocumentCollection { Id = "ServiceRequest" };
                var requestOptions = new RequestOptions { OfferType = "S2" };
                collection = client.CreateDocumentCollectionAsync(db.SelfLink, collectionSpec, requestOptions).Result;
            }

            // Build the JSON Query for Retrieving the Service Request Details
            string sqlquery = @"SELECT c.Contoso_Service_Request.ServiceRequest.RequestId FROM ServiceRequest c Where c.Contoso_Service_Request.ServiceRequest.ServicePrimaryProduct = """ 
                              + primaryProduct + 
                              @""" and c.Contoso_Service_Request.ServiceRequest.ServiceCategory = """ 
                              + ServiceCategory +
                              @""" and c.Contoso_Service_Request.ServiceRequest.ServiceAssignedTo = """ 
                              + AssignedTo +
                              @""" and (c.Contoso_Service_Request.Action.ActionStatus != ""Declined"" or c.Contoso_Service_Request.Action.ActionStatus != ""Completed"")" ;

            //MessageBox.Show(sqlquery);
            
            var documents = client.CreateDocumentQuery(collection.DocumentsLink, sqlquery);
            List<string> documentarray = new List<string>();
            foreach(var document in documents)
            {
                documentarray.Add(document.RequestId.ToString());
            }
            return documentarray;
        }
        public XmlDocument RetrieveDocument(string RequestId)
        {
            DocumentClient client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey);
            //Connect to the DB. If DB is not present create it
            var db = client.CreateDatabaseQuery().Where(d => d.Id == "ContosoServiceRequest").AsEnumerable().FirstOrDefault();
            if (db == null)
            {
                db = client.CreateDatabaseAsync(new Database { Id = "ContosoServiceRequest" }).Result;
            }
            //Connect to the Collection. If DB is not present create it
            var collection = client.CreateDocumentCollectionQuery(db.SelfLink).Where(d => d.Id == "ServiceRequest").AsEnumerable().FirstOrDefault();
            if (collection == null)
            {
                var collectionSpec = new DocumentCollection { Id = "ServiceRequest" };
                var requestOptions = new RequestOptions { OfferType = "S2" };
                collection = client.CreateDocumentCollectionAsync(db.SelfLink, collectionSpec, requestOptions).Result;
            }
            
            //MessageBox.Show(RequestId);
            // Build the JSON Query for Retrieving the Service Request Details
            string sqlquery = @"SELECT *  FROM ServiceRequest c Where c.Contoso_Service_Request.ServiceRequest.RequestId = """ + RequestId + @"""";
            var documents = client.CreateDocumentQuery(collection.SelfLink, sqlquery);

            string jsonText = "";

            foreach (var document in documents)
            {
                jsonText = document.Contoso_Service_Request.ToString();
            }
            
            XmlDocument doc = JsonConvert.DeserializeXmlNode(jsonText,"Contoso_Service_Request");
            //string jsonText1 = JsonConvert.SerializeXmlNode(doc);
            return doc;
            
        }
        public async Task UpdateDocumentDB(XmlDocument doc, string RequestId)
        {
            // Convert Xdocument to Json
            string jsonText = JsonConvert.SerializeXmlNode(doc);

            //Convert the JsonText string to Memory Stream
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(jsonText);
            writer.Flush();
            stream.Position = 0;

            // Connect and Insert into DocumentDB
            try
            {
                DocumentClient client = new DocumentClient(new Uri(EndpointUrl), AuthorizationKey);
                //Connect to the DB. If DB is not present create it
                var db = client.CreateDatabaseQuery().Where(d => d.Id == "ContosoServiceRequest").AsEnumerable().FirstOrDefault();
                if (db == null)
                {
                    db =  client.CreateDatabaseAsync(new Database { Id = "ContosoServiceRequest" }).Result;
                }
                //Connect to the Collection. If DB is not present create it
                var collection = client.CreateDocumentCollectionQuery(db.SelfLink).Where(d => d.Id == "ServiceRequest").AsEnumerable().FirstOrDefault();
                if (collection == null)
                {
                    var collectionSpec = new DocumentCollection { Id = "ServiceRequest" };
                    var requestOptions = new RequestOptions { OfferType = "S2" };
                    collection = client.CreateDocumentCollectionAsync(db.SelfLink, collectionSpec, requestOptions).Result;
                }
                //Delete the Original Document 
                string sqlquery = @"SELECT *  FROM ServiceRequest c Where c.Contoso_Service_Request.ServiceRequest.RequestId = """ + RequestId + @"""";
                var documents = client.CreateDocumentQuery(collection.SelfLink, sqlquery);
                foreach (var document in documents)
                {
                    await client.DeleteDocumentAsync(document._self);
                    //MessageBox.Show(result.ToString());
                }
                // Insert the now updated Document to the Database.
                await client.CreateDocumentAsync(collection.SelfLink, Resource.LoadFrom<Document>(stream));
                stream.Dispose();
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
        }
        private string GetFileContentType(string fileName)
        {
            string contentType = "text/plain";
            string ext = System.IO.Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);
            if (registryKey != null && registryKey.GetValue("Content Type") != null)
                contentType = registryKey.GetValue("Content Type").ToString();
            return contentType;
        }

        
    }
}
