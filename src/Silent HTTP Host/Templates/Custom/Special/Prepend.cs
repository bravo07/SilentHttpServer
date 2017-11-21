using MySql.Data.MySqlClient;
using Silent_HTTP_Host.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silent_HTTP_Host.Templates.Custom.Special
{
    class Prepend
    {
        /// <summary>
        /// Generates a template
        /// </summary>
        /// <param name="request">The request from the client</param>
        /// <param name="requestData">A Json object that can be globally accessed from this reuqest</param>
        /// <returns>A template</returns>
        public static Template GetTemplate(ref WebRequestHandler request, ref JSON.JsonObject requestData)
        {
            Template ret = Template.GetRawTemplate();

            // Prepend file is a good place to handle sql connections,
            // opening file handles, and other things that you want to
            // do at initial runtime of user connection

            return ret;
        }
    }
}
