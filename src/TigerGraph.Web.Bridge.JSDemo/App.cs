using Bridge;
using Bridge.Html5;
using Bridge.jQuery2;
using Newtonsoft.Json;
using System;

namespace TigerGraph.Web.Bridge.JSDemo
{
    public class App
    {
        public static void Main()
        {

            var config = new AjaxOptions()
            {
                Url = "https://localhost:5001/p/echo",
                // Set the contentType of the request
                ContentType = "application/json; charset=utf-8",
                // On response, call custom success method
                Success = (data, textStatus, jqXHR) =>
                {
                    // Output the whole response object.
                    Console.WriteLine(data);

                    // or, output just the message using
                    // the "d" property string indexer.
                    // Console.WriteLine(data["d"]);
                }
            };

            // Make the Ajax request
            jQuery.Ajax(config);

        }


    }
}