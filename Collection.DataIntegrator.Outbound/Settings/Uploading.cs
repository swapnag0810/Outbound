using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collection.DataIntegrator.Outbound.Settings
{
    public class SFTPServer
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Pass { get; set; }
    }

    public class UploadConfiguration
    {
        public string Name { get; set; }
        public string SourceLocation { get; set; }
        public string TargetLocation { get; set; }
        public string ArchiveLocation { get; set; }
        public string ErrorLocation { get; set; }
    }

    public class EmailConfiguration
    {
        public string ProductionSupportEmailTo { get; set; }
        public string SupportEmail { get; set; }
        public string SmtpPort { get; set; }
        public string SmtpHost { get; set; }
        public string FromMail { get; set; }
        public string EnvironmentEmailSubjectPrefix { get; set; }
        public string SiteName { get; set; }
        public string FromEmail { get; set; }
        public string FromMailPassword { get; set; }
    }

    public class Uploading
    {
        public const string SectionName = "Uploading";

        public Uploading()
        {
            
        }

        public SFTPServer SFTP { get; set; }

        public int PollIntervalSeconds { get; set; }
        public UploadConfiguration UploadConfiguration { get; set; }
        public EmailConfiguration EmailConfiguration { get; set; }

    }
}

