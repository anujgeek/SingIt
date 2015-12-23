using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace SingItService
{
    [DataContract]
    public class Rating
    {
        [DataMember]
        public string username;

        [DataMember]
        public int rating;
    }
}