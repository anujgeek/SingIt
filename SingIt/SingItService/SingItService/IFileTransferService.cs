using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace SingItService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IFileTransferService" in both code and config file together.
    [ServiceContract]
    public interface IFileTransferService
    {
        [OperationContract]
        [WebInvoke(Method = "POST", BodyStyle = WebMessageBodyStyle.Wrapped, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [return: MessageParameter(Name = "Output")]
        string UploadSong([MessageParameter(Name = "stream")]Stream stream);
    }

    [DataContract]
    public class UploadedFile
    {
        [DataMember]
        public string FilePath { get; set; }

        [DataMember]
        public string FileName { get; set; }
    }
}
