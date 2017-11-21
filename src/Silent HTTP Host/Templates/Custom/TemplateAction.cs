using Silent_HTTP_Host.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Silent_HTTP_Host.Templates.Custom
{
    class TemplateAction
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

            ret.AppendContent("Login");

            return ret;
        }
    }
}
