using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Web;
using System.Web.Configuration;

namespace SendEmailWithTemplate.helper
{
    public class EmailHelper
    {
        private static string EMAIL_TEMPLATE = "template";
        private static String EMAIL_VERIFY = "verify";
        private static String EMAIL_FOLDER = "templates//";

        public bool sendMail(String content, String title, String emailReceiver)
        {
            try
            {
                String body = createBody(content);
                body = HttpUtility.HtmlDecode(body);


                System.Net.Mail.MailAddress[] mailTo = { new System.Net.Mail.MailAddress(emailReceiver) };
                return Mail(mailTo, title, body, null);
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        private String createBody(String content)
        {
            var path = HttpRuntime.AppDomainAppPath;
            String pathFileTemplate = String.Format("{0}{1}{2}.html", path, EMAIL_FOLDER, EMAIL_TEMPLATE);
            String template = File.ReadAllText(pathFileTemplate);
            return template.Replace("{0}", content);
        }

        public String createContent(String template, object[] param)
        {
            for (int i = 0; i < param.Length; i++)
            {
                String position = "{" + i + "}";
                template = template.Replace(position, param[i] == null ? "" : param[i].ToString());
            }
            return template;
        }



        private bool Mail(MailAddress[] mailTo, string subject, string body, Attachment[] attachments)
        {
            try
            {
                string path = HttpContext.Current.Request.ApplicationPath == "/" ? "" : HttpContext.Current.Request.ApplicationPath;
                Configuration configurationFile = WebConfigurationManager.OpenWebConfiguration(path + "/Web.config");

                MailSettingsSectionGroup mailSettings = configurationFile.GetSectionGroup("system.net/mailSettings") as MailSettingsSectionGroup;

                if (mailSettings != null)
                {
                    int port = mailSettings.Smtp.Network.Port;
                    string host = mailSettings.Smtp.Network.Host;
                    string password = mailSettings.Smtp.Network.Password;
                    string username = mailSettings.Smtp.Network.UserName;
                    string displayName = "Email from server";

                    MailAddress senderAuthenticated = new MailAddress(username, displayName);
                    MailMessage mess = new MailMessage();
                    mess.From = senderAuthenticated;
                    for (int i = 0; i < mailTo.Length; i++)
                    {
                        mess.To.Add(mailTo[i]);
                    }
                    mess.IsBodyHtml = true;
                    mess.Sender = senderAuthenticated;
                    mess.Subject = subject;
                    mess.Body = HttpUtility.HtmlDecode(body);
                    mess.Priority = MailPriority.High;
                    if (attachments != null)
                    {
                        for (int i = 0; i < attachments.Length; i++)
                        {
                            mess.Attachments.Add(attachments[i]);
                        }

                    }
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = host;
                    smtp.Port = port;
                    smtp.UseDefaultCredentials = false;
                    //smtp.EnableSsl = mailSettings.Smtp.Network.EnableSsl;

                    smtp.Credentials = new NetworkCredential(username, password);

                    smtp.Send(mess);

                    return true;
                }

                return false;
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.ProtocolError)
                {
                    return false;
                }
                return false;
            }
        }



        public bool sendVerifyLink(String email, String code, String message)
        {
            var path = HttpRuntime.AppDomainAppPath;
            String pathFileTemplate = String.Format("{0}{1}{2}.html", path, EMAIL_FOLDER, EMAIL_VERIFY);
            String template = File.ReadAllText(pathFileTemplate);
            String content = createContent(template, new object[] { email, code, message });
            return sendMail(content, "Verify Link", email);
        }
    }
}