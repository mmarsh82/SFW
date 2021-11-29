using System.ComponentModel;

namespace QT9Client
{
    public enum wsAuthenticate
    {
        AuthenticateAppToken = 0,

        [Description(@"<soap:Body>
                        <AuthenticateUser xmlns=""QT9.QMS.WebD.WS"">
                          <UserName>@p1</UserName>
                          <Password>@p2</Password>
                        </AuthenticateUser>
                      </soap:Body>")]
        AuthenticateUser = 1,

        GetUserExpiration = 2,
        LogUserExpiration = 3,

        [Description(@"<soap:Body>
                        <LogUserOut xmlns=""QT9.QMS.WebD.WS"">
                          <UserName>@p1</UserName>
                        </LogUserOut>
                      </soap:Body>")]
        LogUserOut = 4,

        ValidateUser = 5
    }
}
