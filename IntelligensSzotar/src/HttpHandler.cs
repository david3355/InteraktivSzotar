using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace IntelligensSzotar
{
    class HttpHandler
    {
        public HttpHandler()
        {
            string url = "https://www.googleapis.com/language/translate/v2/languages?key=AIzaSyBgV6pHxefhhl7DHjovxoK243KNKZM6bc8";
            WebRequest request = HttpWebRequest.Create(url);
            WebResponse response =  request.GetResponse();
        }
    }
}
