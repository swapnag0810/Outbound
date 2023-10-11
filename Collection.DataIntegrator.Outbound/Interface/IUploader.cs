using Collection.DataIntegrator.Outbound.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Collection.DataIntegrator.Outbound.Interface
{
    public interface IUploader
    {
        bool FetchOutboundQueue();
        List<EventType> FetchOutboundEventTypes();
        bool FileUpload(string srcFileName, string targetSubFolderName, string srcJSONFile);
        bool SendEmail(string subject, string textBody, string htmlBody);
    }
}

