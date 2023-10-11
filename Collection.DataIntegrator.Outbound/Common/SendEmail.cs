using Collection.DataIntegrator.Outbound.Common.Interface;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using Microsoft.Extensions.Logging;


namespace Collection.DataIntegrator.Outbound.Common
{
    public class SendEmailConfig
    {
        public string SupportEmail { get; set; }
        public string SmtpPort { get; set; }
        public string SmtpHost { get; set; }
        public string FromMail { get; set; }
        public string EnvironmentEmailSubjectPrefix { get; set; }
        public string SiteName { get; set; }
        public string FromEmail { get; set; }
        public string FromMailPassword { get; set; }
    }
    
    public class SendEmail: ISendEmail
    {
        private readonly ILogger _logger;
        private readonly SendEmailConfig _config;

        public SendEmail(ILogger logger, SendEmailConfig emailConfig)
        {
            _logger = logger;
            _config = emailConfig;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="To"></param>
        /// <param name="From"></param>
        /// <param name="Subject"></param>
        /// <param name="TextBody"></param>
        /// <param name="HtmlBody"></param>
        /// <param name="Attachments"></param>
        /// <param name="CC"></param>
        /// <param name="BCC"></param>
        /// <param name="FromName"></param>
        /// <returns></returns>
        public bool SendMail(string To, string From, string Subject, string TextBody, string HtmlBody = null, List<string> Attachments = null, string CC = null, string BCC = null, string FromName = "")
        {
            string QueuefileName = string.Format("Email logs at {0:yyyyMMdd}", DateTime.Now);

            try
            {
                bool Status = false;
                _logger.LogError(QueuefileName, string.Format("{0}: {1}", DateTime.Now, " SendEmail method started"));
                _logger.LogError(QueuefileName, string.Format("{0}: {1}: {2}", DateTime.Now, " Sending Mail To: ", To));

                string supportEmail = _config.SupportEmail.ToString();
                SmtpClient Smtp = new SmtpClient
                {
                    Port = Convert.ToInt32(_config.SmtpPort),
                    Host = _config.SmtpHost
                };
                MailMessage Mail = new MailMessage();
                if (FromName == "" || FromName == null || FromName == string.Empty)
                {
                    FromName = _config.SiteName.ToString();
                }
                if (From != "" && From != null && From != string.Empty)
                    Mail.From = new MailAddress(From, FromName);
                else
                    Mail.From = new MailAddress(supportEmail);

                Mail.IsBodyHtml = true;

                if (HtmlBody == "" || HtmlBody == string.Empty || HtmlBody == null)
                    Mail.Body = "<html><body><p>" + TextBody + "</p></body></html>";
                else
                    Mail.Body = HtmlBody;

                if (Attachments != null)
                {
                    foreach (string Attachment in Attachments)
                    {
                        _logger.LogError(QueuefileName, string.Format("{0}: {1}: {2}", DateTime.Now, " adding attachment : ", Attachment));
                        Mail.Attachments.Add(new Attachment(Attachment));
                    }
                }
                if (!string.IsNullOrEmpty(To))
                {
                    To = To.Trim().Trim(';').Trim();
                    string[] ToList = To.Split(';');
                    foreach (string ToPerson in ToList)
                    {
                        _logger.LogError(QueuefileName, string.Format("{0}: {1}: {2}", DateTime.Now, " adding email_to : ", ToPerson));
                        Mail.To.Add(ToPerson);
                    }
                }
                if (!string.IsNullOrEmpty(CC))
                {
                    CC = CC.Trim().Trim(';').Trim();
                    string[] CCList = CC.Split(';');
                    foreach (string CCPerson in CCList)
                    {
                        _logger.LogError(QueuefileName, string.Format("{0}: {1}: {2}", DateTime.Now, " adding cc_to : ", CCPerson));
                        Mail.CC.Add(CCPerson);
                    }
                }
                if (!string.IsNullOrEmpty(BCC))
                {
                    BCC = BCC.Trim().Trim(';').Trim();
                    string[] BCCList = BCC.Split(';');
                    foreach (string BCCPerson in BCCList)
                    {
                        _logger.LogError(QueuefileName, string.Format("{0}: {1}: {2}", DateTime.Now, " adding bcc_to : ", BCCPerson));
                        Mail.Bcc.Add(BCCPerson);
                    }
                }
                Mail.Subject = _config.EnvironmentEmailSubjectPrefix.ToString() + Subject;
                _logger.LogError(QueuefileName, string.Format("{0}: {1}: {2}", DateTime.Now, " Subject is : ", Mail.Subject));
                _logger.LogError(QueuefileName, string.Format("{0}: {1}: {2}", DateTime.Now, " Email Body is : ", Mail.Body));
                Smtp.UseDefaultCredentials = true;//using local host to send emails hence true, false if using SMTP credentials
                Smtp.EnableSsl = false;//using local host to send emails hence false, true if using SMTP credentials
                //uncomment below if using SMTP credentials
                //Smtp.Credentials = new NetworkCredential(_config.FromEmail, _config.FromMailPassword);
                //Smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                Smtp.Send(Mail);
                Status = true;
                Mail.Dispose();
                _logger.LogError(QueuefileName, string.Format("{0}: {1}: {2}", DateTime.Now, " Mail Sent Successfully to ", To));
                return Status;
            }
            catch (Exception ex)
            {
                _logger.LogError(QueuefileName, string.Format("{0}: {1}: {2}: {3}", DateTime.Now, " Error in sending email to :  ", To, ex));
                return false;
            }
        }

    }
}

