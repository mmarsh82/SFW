using System.ComponentModel;

namespace QT9Client
{
    public enum wsDocuments
    {
        [Description(@"<soap:Body>
                        <AddNewDocument xmlns=""QT9.QMS.WebD.WS"">
                          <doc>
                            <ArchiveDate>@p1</ArchiveDate>
                            <ChangeNotes>@p2</ChangeNotes>
                            <DatabaseID>@p3</DatabaseID>
                            <DocCategoryID>@p4</DocCategoryID>
                            <DepartmentID>@p5</DepartmentID>
                            <DocCatTypeID>@p6</DocCatTypeID>
                            <DocRef>@p7</DocRef>
                            <DocumentName>@p8</DocumentName>
                            <DocumentNumber>@p8</DocumentNumber>
                            <DocumentTypeID>@p9</DocumentTypeID>
                            <DocumentsAffected>
                              @p10
                            </DocumentsAffected>
                            <Inactive>@p11</Inactive>
                            <IsElectronic>@p12</IsElectronic>
                            <OriginDate>@p13</OriginDate>
                            <PreviousReviewDate>@p14</PreviousReviewDate>
                            <DocumentOwner>@p15</DocumentOwner>
                            <Revisions>
                             @p16
                            </Revisions>
                            <ScheduledReviewDate>@p17</ScheduledReviewDate>
                            <SiteID>@p18</SiteID>
                            <DescriptionofDoc>@p19</DescriptionofDoc>
                          </doc>
                        </AddNewDocument>
                      </soap:Body>")]
        AddNewDocument = 0,

        [Description(@"<soap:Body>
                        <AddNewDocumentCategory xmlns=""QT9.QMS.WebD.WS"">
                          <DocCategory>@p1</DocCategory>
                        </AddNewDocumentCategory>
                      </soap:Body>")]
        AddNewDocumentCategory = 1,

        [Description(@"<soap:Body>
                        <AddNewDocumentRevision xmlns=""QT9.QMS.WebD.WS"">
                          <DocRev>
                            <Approved>@p1</Approved>
                            <Approver>@p2</Approver>
                            <Approvers>
                              @p3
                            </Approvers>
                            <CollabStarted>@p4</CollabStarted>
                            <Comments>@p5</Comments>
                            <DatabaseID>@p6</DatabaseID>
                            <DateApproved>@p7</DateApproved>
                            <DateStamp>@p8</DateStamp>
                            <DepartmentsEffected>@p8</DepartmentsEffected>
                            <Description>@p9</Description>
                            <DisplayLocation>@p10</DisplayLocation>
                            <DocumentAuthor>@p11</DocumentAuthor>
                            <DocumentID>@p12</DocumentID>
                            <Location>@p13</Location>
                            <ObsoleteDate>@p14</ObsoleteDate>
                            <PublishedByID>@p15</PublishedByID>
                            <PublishDate>@p16</PublishDate>
                            <PhysicalLocation>@p17</PhysicalLocation>
                            <RealRevNum>@p18</RealRevNum>
                            <Rejector>@p19</Rejector>
                            <RejectorID>@p20</RejectorID>
                            <Requestor>@p21</Requestor>
                            <RequestorID>@p22</RequestorID>
                            <RespPartyID>@p23</RespPartyID>
                            <RevisionNumber>@p24</RevisionNumber>
                            <Status>@p25</Status>
                            <TimeLine>@p26</TimeLine>
                            <TrainingRequired>@p27</TrainingRequired>
                          </DocRev>
                        </AddNewDocumentRevision>
                      </soap:Body>")]
        AddNewDocumentRevision = 2,

        [Description(@"<soap:Body>
                        <AddNewDocumentType xmlns=""QT9.QMS.WebD.WS"">
                          <DocType>@p1</DocType>
                        </AddNewDocumentType>
                      </soap:Body>")]
        AddNewDocumentType = 3,

        [Description(@"<soap:Body>
                        <DocNumInUse xmlns=""QT9.QMS.WebD.WS"">
                          <DocNum>@p1</DocNum>
                          <DocTypeID>@p2</DocTypeID>
                          <DepartmentID>@p3</DepartmentID>
                        </DocNumInUse>
                      </soap:Body>")]
        DocNumInUse = 4,

        [Description(@"<soap:Body>
                        <GetAllDocumentsAsDataSet xmlns=""QT9.QMS.WebD.WS"">
                          <IncludeInactive>@p1</IncludeInactive>
                        </GetAllDocumentsAsDataSet>
                      </soap:Body>")]
        GetAllDocumentsAsDataSet = 5,

        [Description(@"<soap:Body>
                        <GetDocumentByID xmlns=""QT9.QMS.WebD.WS"">
                          <DocID>@p1</DocID>
                        </GetDocumentByID>
                      </soap:Body>")]
        GetDocumentByID = 6,

        [Description(@"<soap:Body>
                        <GetDocumentCategoriesAsDataSet xmlns=""QT9.QMS.WebD.WS"" />
                      </soap:Body>")]
        GetDocumentCategoriesAsDataSet = 7,

        [Description(@"<soap:Body>
                        <GetDocumentRevisionByID xmlns=""QT9.QMS.WebD.WS"">
                          <DocRevID>@p1</DocRevID>
                        </GetDocumentRevisionByID>
                      </soap:Body>")]
        GetDocumentRevisionByID = 8,

        [Description(@"<soap:Body>
                        <GetDocumentTypeByID xmlns=""QT9.QMS.WebD.WS"">
                          <DocTypeID>@p1</DocTypeID>
                        </GetDocumentTypeByID>
                      </soap:Body>")]
        GetDocumentTypeByID = 9,

        [Description(@"<soap:Body>
                        <GetDocumentTypesAsDataSet xmlns=""QT9.QMS.WebD.WS"" />
                      </soap:Body>")]
        GetDocumentTypesAsDataSet = 10,

        [Description(@"<soap:Body>
                        <GetInvalidFileNameCharactersResponse xmlns=""QT9.QMS.WebD.WS"">
                          <GetInvalidFileNameCharactersResult>
                            <string>@p1</string>
                            <string>@p2</string>
                          </GetInvalidFileNameCharactersResult>
                        </GetInvalidFileNameCharactersResponse>
                      </soap:Body>")]
        GetInvalidFileNameCharacters = 11
    }
}
