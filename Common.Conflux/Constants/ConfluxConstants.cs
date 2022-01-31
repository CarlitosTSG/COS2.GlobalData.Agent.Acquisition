using System;
using System.Collections.Generic;
using System.Text;

namespace Conflux.Constants
{
    public enum WebRequestType { Login, GetSpecial, GetSingle, GetList, Create, Update, Delete, Command };
    public enum WebResponseType { SystemError, AppError, DataSingle, DataList, Message, Undefined = 9999 };
    public enum EntityRelationship { None, IsChildOf, IsParentOf, IsPartOf };
    public enum EntityLinkType { None, IsReal, IsVirtual };
    public enum MonitorLogType { Debug, Info, Warning, Critical, Statistics};
    public enum EntityHistoryRecordType { Added, Updated, Deleted, ParentDeleted };
}
