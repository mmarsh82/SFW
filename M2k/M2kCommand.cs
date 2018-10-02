using IBMU2.UODOTNET;
using M2k;
using System;

namespace M2kClient
{
    public class M2kCommand
    { 

        /// <summary>
        /// Default Constructor
        /// </summary>
        public M2kCommand()
        { }

        /// <summary>
        /// Will edit any record in Manage 2000
        /// ***WARNING***
        /// DO NOT attempt to edit any record value that has business logic associated with it
        /// Contact your ERP administrator if you need help verifing
        /// </summary>
        /// <param name="file">Manage 2000 file to be edited</param>
        /// <param name="recordID">Record ID value to be edited</param>
        /// <param name="attribute">The attribute number that the new value will be associated with, see the warning</param>
        /// <param name="newValue">New value to be writted into the record</param>
        /// <param name="connection">UniConnection to use for the edit</param>
        /// <returns>Pass\Fail for the editing of the record</returns>
        public bool EditRecord(string file, string recordID, int attribute, string newValue, M2kConnection connection)
        {
            try
            {
                using (UniSession uSession = UniObjects.OpenSession(connection.HostName, connection.UserName, connection.Password, connection.Database, connection.Service))
                {
                    using (UniFile uFile = uSession.CreateUniFile(file))
                    {
                        using (UniDynArray udArray = uFile.Read(recordID))
                        {
                            udArray.Insert(attribute, newValue);
                            uFile.Write(recordID, udArray);
                        }
                    }
                    UniObjects.CloseSession(uSession);
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
