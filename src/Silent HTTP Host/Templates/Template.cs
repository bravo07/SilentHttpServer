using Silent_HTTP_Host.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Silent_HTTP_Host.Templates
{
    class Template
    {
        #region Public Static methods

        /// <summary>
        /// Merges two templates into one. This is good for merging
        /// prepend and the user script.
        /// </summary>
        /// <param name="template1">Template one</param>
        /// <param name="template2">Template two, this ones data will be favored.</param>
        /// <returns>A merged array</returns>
        public static Template MergeTemplates(Template template1, Template template2)
        {
            if (template1 == null && template2 == null)
                throw new ArgumentNullException();
            if (template1 == null)
                return template2;
            if (template2 == null)
                return template1;

            Template ret = GetRawTemplate();

            ret.AppendContent(template1.Content);
            ret.SetStatus(template1.StatusCode, template1.StatusCodeDescriptor);
            foreach(HttpHeader h in template1.Headers)
                ret.SetHeader(h.name, h.value);

            ret.AppendContent(template2.Content);
            ret.SetStatus(template2.StatusCode, template2.StatusCodeDescriptor);
            foreach (HttpHeader h in template2.Headers)
                ret.SetHeader(h.name, h.value);

            return ret;
        }

        public static Template RenderTemplate(Type type, ref WebRequestHandler request)
        {
            // Getting the rendering method
            MethodInfo mi = type.GetMethod("GetTemplate");

            // Generating parameters
            object[] param = { request };

            // Invoking the member
            object result = mi.Invoke(null, param);

            // Nulllifying the parameters that no longer are needed
            param = null;

            // casing the result to template and returning
            return result as Template;
        }

        public static Template GetRawTemplate()
        {
            return new Template();
        }
        #endregion
        #region Private variables
        private List<HttpHeader> m_headers;
        private StringBuilder m_content;
        private int m_statusCode;
        private string m_statusCodeDescriptor;

        #endregion
        #region Methods for this class
        public void AppendContent(string s, bool htmlSanitize = false)
        {
            if (htmlSanitize)
                s = Misc.HtmlEscape(s);

            m_content.Append(s);
        }

        public void ClearContent()
        {
            m_content.Clear();
        }

        public void SetStatus(int code, string descriptor)
        {
            m_statusCode = code;
            m_statusCodeDescriptor = descriptor;
        }

        public void SetHeader(string name, string value, bool overwrite = true)
        {
            bool hasOverwritten = false;

            if (overwrite)
            {
                // Checking if the header exists
                for (int i = 0; !hasOverwritten && i < m_headers.Count; i++)
                {
                    // Comparing the header names
                    if (m_headers[i].name == name)
                    {
                        // Header exist's, let's overwrite it.
                        m_headers[i] = new HttpHeader()
                        {
                            name = name,
                            value = value
                        };

                        // Setting this so that we don't add the header again
                        hasOverwritten = true;
                    }
                }
            }

            if (!hasOverwritten)
            {
                // Checking if this is a redirect header, if it is, we
                // want to set the status code to a 30x status code if
                // it's not already.
                if (name.ToLower() == "location")
                {
                    // Check if we have already set the redirect header.
                    if (m_statusCode < 300 || m_statusCode > 300)
                    {
                        // User hasn't set redirect status, so let's do
                        // it for him/her
                        SetStatus(307, "Temporary Redirect");
                    }
                }

                m_headers.Add(new HttpHeader()
                {
                    name = name,
                    value = value
                });
            }
        }
        #endregion
        #region Initialize
        public Template()
        {
            m_headers = new List<HttpHeader>();
            m_content = new StringBuilder();
            m_statusCode = 200;
            m_statusCodeDescriptor = "OK";
        }

        #endregion
        #region Public Properties
        public string Content
        {
            get
            {
                return m_content.ToString();
            }
        }

        public int StatusCode
        {
            get
            {
                return m_statusCode;
            }
        }

        public string StatusCodeDescriptor
        {
            get
            {
                return m_statusCodeDescriptor;
            }
        }

        public HttpHeader[] Headers
        {
            get
            {
                return m_headers.ToArray();
            }
        }
        #endregion
    }
}
