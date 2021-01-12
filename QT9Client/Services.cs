namespace QT9Client
{
    public static class Services
    {
        public enum wsAppComp
        {
            AppVersionIsCompatible = 0,
            GetNewestCompatibleVersion = 1,
            HelloWorld = 2
        }

        public enum wsAuthenticate
        {
            AuthenticateAppToken = 0,
            AuthenticateUser = 1,
            GetUserExpiration = 2,
            LogUserExpiration = 3,
            LogUserOut = 4,
            ValidateUser = 5
        }

        public enum wsCustomerFeedback
        {
            AddCustomerFeedback = 0,
            AddFeedbackTimeLineEntry = 1,
            GetAllRecords = 2,
            GetAllRecordsByStatus = 3,
            GetCustomerFeedbackResponsiblePartiesBySite = 4,
            GetFeedbackCategories = 5,
            GetFeedbackTypes = 6,
            GetNewCustomerFeedback = 7,
            GetUserCustomerFeedbackSites = 8
        }

        public enum wsCustomers
        {
            AddCustomer = 0,
            AddCustomerWithRemoteID = 1,
            CheckLoginName = 2,
            GetCustomer = 3,
            GetCustomerByName = 4,
            GetCustomersAsList = 5,
            GetCustomersAsTable = 6,
            SaveQT9Customer = 7
        }

        public enum wsDocuments
        {
            AddNewDocument = 0,
            AddNewDocumentCategory = 1,
            AddNewDocumentRevision = 2,
            AddNewDocumentType = 3,
            DocNumInUse = 4,
            GetAllDocumentsAsDataSet = 5,
            GetDocumentByID = 6,
            GetDocumentCategoriesAsDataSet = 7,
            GetDocumentRevisionByID = 8,
            GetDocumentTypeByID = 9,
            GetDocumentTypesAsDataSet = 10,
            GetInvalidFileNameCharacters = 11
        }

        public enum wsFileDownload
        {
            DownloadChunk = 0,
            FileDownloaded = 1,
            GetFileDownload = 2,
            GetFileSize = 3
        }

        public enum wsProducts
        {
            GetProduct = 0,
            GetProductByName = 1,
            GetProductByRemoteID = 2,
            GetProductCategoriesAsList = 3,
            GetProductCategoriesAsTable = 4,
            GetProductCategory = 5,
            GetProductDocsByPartNum = 6,
            GetProductDocsByProductID = 7,
            GetProductPartType = 8,
            GetProductPartTypesAsList = 9,
            GetProductPartTypesAsTable = 10,
            GetProductsAsList = 11,
            GetProductsAsTable = 12,
            SaveQT9Product = 13,
            SaveQT9ProductCategory = 14,
            SaveQT9ProductPartType = 15
        }

        public enum wsISOActions
        {
            AddCar = 0,
            AddNcp = 1,
            GetAllRecords = 2,
            GetCarTypes = 3,
            GetIsoActionDueDate = 4,
            GetIsoActionPriorities = 5,
            GetIsoActionProblemTypes = 6,
            GetIsoActionProductDispositions = 7,
            GetIsoActionResponsiblePartiesBySite = 8,
            GetNewCAR = 9,
            GetNewNCP = 10,
            GetRecordsByStatus = 11,
            GetUserIsoActionSites = 12
        }
    }
}
