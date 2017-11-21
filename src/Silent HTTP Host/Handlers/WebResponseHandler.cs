using JSON;
using Silent_HTTP_Host.Templates;
using Silent_HTTP_Host.Templates.Custom;
using Silent_HTTP_Host.Templates.Default;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Silent_HTTP_Host.Handlers
{
    class WebResponseHandler
    {
        #region Private Variables
        private Template m_template;
        private JsonObject requestData;
        private object requestObject;

        #endregion

        #region Public Methods
        public WebResponseHandler(string scriptPath, ref WebRequestHandler request)
        {
            #region Generating objects for this request
            requestData = new JsonObject();
            m_template = Template.GetRawTemplate();

            #endregion

            #region Checking request parameters.
            //
            // Just checking some request parameters. This HTTP server
            // doesn't support a lot of things, so we need to send
            // error responses for the things it doesn't support.
            //

            if (request.path == null || request.path.Length <= 0)
            {
                m_template = Templates.Default.BadRequest.GetTemplate(
                    ref request,
                    ref requestData);

                return;
            }

            if (request.connectingHost == null || request.connectingHost.Length <= 0)
            {
                m_template = Templates.Default.BadRequest.GetTemplate(
                    ref request,
                    ref requestData);

                return;
            }

            if (request.mode != "GET" && request.mode != "POST")
            {
                m_template = Templates.Default.BadRequest.GetTemplate(
                    ref request,
                    ref requestData);
                m_template.AppendContent("Unsupported request method");

                return;
            }

            if (request.protocolVersion != "HTTP/1.1" && request.protocolVersion != "HTTP/1.0")
            {
                m_template = Templates.Default.HTTPVersionNotSupported.GetTemplate(
                    ref request,
                    ref requestData);

                return;
            }
            #endregion

            try
            {
                #region Prepend
                m_template = Template.MergeTemplates(
                    m_template,
                    Templates.Custom.Special.Prepend.GetTemplate(
                        ref request,
                        ref requestData));
                #endregion


                switch (scriptPath)
                {
                    #region Action
                    case "/action":
                        {
                            if (request.mode == "POST")
                            {
                                m_template = Template.MergeTemplates(
                                    m_template,
                                    Templates.Custom.TemplateAction.GetTemplate(
                                        ref request,
                                        ref requestData));
                            }
                            else
                            {
                                // Method must be POST. Send an error page.
                                m_template.SetHeader("Content-Type", "text/plain");
                                m_template.AppendContent("Content must be uploaded using the HTTP method POST.");
                                m_template.SetStatus(500, "Internal Server Error");
                            }

                            break;
                        }
                    #endregion

                    #region Login
                    case "/login":
                        {
                            m_template = Template.MergeTemplates(
                                m_template,
                                Templates.Custom.TemplateLogin.GetTemplate(
                                    ref request,
                                    ref requestData));

                            break;
                        }
                    #endregion

                    #region Register
                    case "/register":
                        {
                            m_template = Template.MergeTemplates(
                                m_template,
                                Templates.Custom.TemplateRegister.GetTemplate(
                                    ref request,
                                    ref requestData));
                            break;
                        }
                    #endregion

                    #region 404
                    default:
                        {
                            m_template = Template.MergeTemplates(
                                m_template,
                                NotFound.GetTemplate(
                                    ref request,
                                    ref requestData));
                            break;
                        }
                    #endregion
                }


                #region Append
                m_template = Template.MergeTemplates(
                    m_template,
                    Templates.Custom.Special.Append.GetTemplate(
                        ref request,
                        ref requestData));
                #endregion
            }
            catch (Exception ex)
            {
                Logs.LogException(ex.ToString());

                // Don't merge templates for stuff like this
                m_template = InternalServerError.GetTemplate(
                    ref request,
                    ref requestData);
            }
        }

        public void Clear()
        {
            if (m_template != null)
                m_template.ClearContent();
            m_template = null;
        }
                #endregion
        #region Public Properties
        public string Content
        {
            get
            {
                return m_template.Content;
            }
        }

        public int StatusCode
        {
            get
            {
                return m_template.StatusCode;
            }
        }

        public string StatusCodeDescriptor
        {
            get
            {
                return m_template.StatusCodeDescriptor;
            }
        }

        public HttpHeader[] Headers
        {
            get
            {
                return m_template.Headers;
            }
        }
        #endregion
    }
}