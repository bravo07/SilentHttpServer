using Silent_HTTP_Host.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silent_HTTP_Host.Templates.Custom
{
    class TemplateLogin
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

            ret.SetHeader("Content-Type", "text/html");


            // Generating a simple HTML page, if you are to use this
            // in a real world application (which I wouldn't recommend,
            // this is very incomplete) you would want to get a
            // templating engine

            ret.AppendContent("<html>");
            ret.AppendContent("<head>");
            ret.AppendContent("<title>Login</title>");
            ret.AppendContent("</head>");

            ret.AppendContent("<body>");
            ret.AppendContent("<form action=\"/action\" method=\"POST\">");
            ret.AppendContent("<input type=\"text\" highlight=\"Username\"></input>");
            ret.AppendContent("</form>");
            ret.AppendContent("</body>");
            ret.AppendContent("</html>");


            return ret;
        }
    }
}
