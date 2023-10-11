using System.Collections.Generic;

namespace Collection.DataIntegrator.Outbound.Common.Interface
{
    public interface ISendEmail
    {
        bool SendMail(string To, string From, string Subject, string TextBody, string HtmlBody, List<string> Attachments = null, string CC = null, string BCC = null, string FromName = null);
        //void SendMail(string productionSupportEmailTo, string supportEmail, string v1, string v2, string v3);
    }
}