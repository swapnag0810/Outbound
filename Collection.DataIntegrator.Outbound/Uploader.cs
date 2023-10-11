using ChoETL;
using Collection.DataIntegrator.Outbound.Common.Interface;
using Collection.DataIntegrator.Outbound.Interface;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Collection.DataIntegrator.Outbound.Models;
using Collection.DataIntegrator.Outbound.Common;

namespace Collection.DataIntegrator.Outbound
{
    public class Uploader : BackgroundService, IUploader
    {
        private readonly ILogger<Uploader> _logger;
        private readonly IServiceProvider _svcProvider;
        private readonly Settings.Uploading _uploadSettings;

        public Uploader(ILogger<Uploader> logger, IServiceProvider provider, Microsoft.Extensions.Options.IOptions<Settings.Uploading> settings)//
        {
            _logger = logger;
            _svcProvider = provider;
            _uploadSettings = settings.Value;
        }

        public bool FetchOutboundQueue()
        {
            try
            {
                Business.OutboundDataQueueBL bl = null;
                var queueFound = new List<OutboundDataQueue>();

                var outboundEventTypesList = new List<EventType>();
                outboundEventTypesList = FetchOutboundEventTypes();

                if(outboundEventTypesList == null || outboundEventTypesList.Count() == 0)
                {
                    _logger.LogError("No Outbound Event Types records found in DB table.");
                    return false;
                }

                outboundEventTypesList.ForEach(oe =>
                {
                    int outboundEventTypeId = oe.EventTypeId;
                    _logger.LogInformation("Outbound Event Type found to process." + oe.EventName);

                    #region Fetch Pending Outbound Data Queue
                    try
                    {
                        using (var svcscope = _svcProvider.CreateScope())
                        {
                            bl = svcscope.ServiceProvider.GetRequiredService<Business.OutboundDataQueueBL>();
                            queueFound = bl.FetchPendingOutboundQueue(outboundEventTypeId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Fetching Outbound Queue from Database table failed.");
                        // todo : send notification, eventhough it fails would be fetched in next attempt
                    }

                    if (queueFound == null || queueFound.Count() == 0)
                    {
                        _logger.LogInformation("No Outbound Queue records found to process.");
                        //return true;
                    }
                    else
                    {
                        int counter = 0;
                        string eventTypeFile = oe.FileName;
                        string eventDestSubFolderName = oe.SFTPOutboundFolderName;
                        string srcFile = _uploadSettings.UploadConfiguration.SourceLocation + eventTypeFile.Replace("timestamp", DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                        string srcJSONFile = Path.GetDirectoryName(srcFile) + "\\" + Path.GetFileNameWithoutExtension(srcFile) + ".json";

                        queueFound.ForEach(f =>
                        {
                            File.WriteAllText(srcJSONFile, f.Content);
                            if (ConvertJSONToCSV(srcJSONFile, srcFile, counter))
                            {
                                _logger.LogInformation("Outbound process - conversion of json to csv done for Outbound Queue Id"+ f.OutboundDataQueueId);
                            }
                            else
                            {
                                _logger.LogError("Outbound queue - process failed with json to csv conversion for Outbound Queue Id" + f.OutboundDataQueueId);
                                // todo : send notification
                                //return false;
                            }

                            counter = counter + 1;
                        });

                        if (FileUpload(srcFile, eventDestSubFolderName, srcJSONFile))
                        {
                            _logger.LogInformation("Outbound queue process successful with file upload to SFTP");
                            // Update all queue records with processed status
                            queueFound.ForEach(upe =>
                            {
                                using (var svcscope = _svcProvider.CreateScope())
                                {
                                    bl = svcscope.ServiceProvider.GetRequiredService<Business.OutboundDataQueueBL>();
                                    //Fetch Outbound queue by Id
                                    var queueData = new OutboundDataQueue();
                                    queueData = bl.FetchOutboundQueueById(upe.OutboundDataQueueId);
                                    //update status as 1
                                    queueData.Status = true;
                                    queueData.UpdatedAt = DateTime.UtcNow;
                                    bl.UpdateOutboundQueueStatus(queueData);

                                    _logger.LogInformation("Outbound Queue status update succesful at: {time} and Id is is {upe.OutboundDataQueueId}", DateTimeOffset.Now, upe.OutboundDataQueueId);
                                    CreateOutboundQueueLog("Outbound Queue status update succesful at: {time} " + DateTimeOffset.Now, true, upe.OutboundDataQueueId, oe.EventTypeId);

                                }
                            });
                        }
                        else
                        {
                            _logger.LogError("Outbound queue process failed with file upload at {time} " + DateTimeOffset.Now);
                            CreateOutboundQueueLog("Outbound Queue process failed with file upload at: {time} " + DateTimeOffset.Now , true, 0, oe.EventTypeId);
                            // todo : send notification
                        }

                    }
                    #endregion of fetch Pending Outbound Data Queue
                });
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fetching Pending Outbound queue to process failed ");
                //send notification
                SendEmail($"ERROR : Communication Outbound Queue Process - Fetching Pending Outbound queue to process failed ",
                                    "",
                                    $" Error : Communication Outbound Queue Process - Fetching Pending Outbound queue to process failed - {ex}");
                return false;
            }
        }

        public bool ConvertJSONToCSV(string srcFile, string targetFile, int counter)
        {
            try
            {
                FileMode fileMode = counter > 0 ? FileMode.Append : FileMode.Create;
                using (var r = new ChoJSONReader(srcFile))
                {
                    using (var fs = new FileStream(targetFile, fileMode, FileAccess.Write))
                    {
                        using (var w = new ChoCSVWriter(fs)
                            .Configure(c => c.MaxScanRows = 10000)
                            .Configure(c => c.ThrowAndStopOnMissingField = false)
                          )
                        {
                            if (fs.Position == 0) // we don't need header if file already existed before
                            {
                                w.WithFirstLineHeader();
                            }
                            w.Write(r);
                        }
                        fs.Write(Environment.NewLine);
                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Outbound Queue Process - converting json to csv failed ");
                //send notification
                SendEmail($"ERROR : Communication Outbound Queue Process - converting json to csv failed ",
                                    "",
                                    $" Error : Communication Outbound Queue Process - converting json to csv failed - {ex}");
                return false;
            }
        }
        public bool FileUpload(string srcFileToUpload, string targetSubFolderName, string srcJSONFile)
        {
            var uploadConfig = _uploadSettings.UploadConfiguration;
            try
            {
                if (uploadConfig == null)
                {
                    _logger.LogInformation("Uploader running at: {time}, Upload configuration is not defined", DateTimeOffset.Now);
                    return false;
                }
                else
                {
                    SFTPConfig cfg = new SFTPConfig { Host = _uploadSettings.SFTP.Host, Port = _uploadSettings.SFTP.Port, UserName = _uploadSettings.SFTP.User, Password = _uploadSettings.SFTP.Pass };
                    IFileStorage ftpclient = new SFTPClient(_logger, cfg);
                    if (!ftpclient.CanConnect())
                    {
                        _logger.LogError("Failed to connect to SFTP {host}", _uploadSettings.SFTP.Host);
                        return false;
                    }

                    _logger.LogInformation("Uploader running at: {time}, uploading :{filetype} from {source} to {target}", DateTimeOffset.Now, uploadConfig.Name, uploadConfig.SourceLocation, uploadConfig.TargetLocation);

                    if (string.IsNullOrEmpty(uploadConfig.SourceLocation))
                    {
                        _logger.LogError("Source location is not defined");
                        return false;
                    }

                    if (string.IsNullOrEmpty(uploadConfig.TargetLocation))
                    {
                        _logger.LogError("Target location is not defined");
                        return false;
                    }

                    bool uploaded = false;
                    var sourceFile = srcFileToUpload;

                    if (File.Exists(sourceFile))
                    {
                        _logger.LogInformation("source file to upload exists");
                    }
                    else
                    {
                        _logger.LogInformation("source file to upload doesnt exist");
                        return false;
                    }
                    uploaded = ftpclient.Upload(sourceFile, uploadConfig.TargetLocation + targetSubFolderName + "/" + Path.GetFileName(sourceFile));
                    _logger.LogInformation("Target Path and file {file}", uploadConfig.TargetLocation + targetSubFolderName + "/" + Path.GetFileName(sourceFile));

                    if (uploaded)
                    {
                        _logger.LogInformation("Succesfully uploaded {file}", sourceFile);
                        // delete temp json from source temp location in file share
                        if ((File.Exists(srcJSONFile)))
                        {
                            File.Delete(srcJSONFile);
                            _logger.LogInformation("Deleted {sourceFile} from temp location", srcJSONFile);
                        }

                        // move temp csv file to archived folder 
                        var archiveFile = Path.Combine(uploadConfig.ArchiveLocation, Path.GetFileName(sourceFile));
                        if (File.Exists(archiveFile))
                        {
                            _logger.LogInformation("Found duplicate file {file}.Renamed.", archiveFile);
                            File.Move(archiveFile, archiveFile + "." + DateTime.Now.ToString("yyyyMMddHHmmssSSS"));
                        }
                        File.Move(sourceFile, archiveFile);
                        return true;
                    }
                    else
                    {
                        _logger.LogError("Failed to upload {file}", sourceFile);
                        var errfile = Path.Combine(uploadConfig.ErrorLocation, Path.GetFileName(sourceFile));
                        if (File.Exists(errfile))
                        {
                            _logger.LogInformation("Found duplicate file {file}.Renamed.", errfile);
                            File.Move(errfile, errfile + "." + DateTime.Now.ToString("yyyyMMddHHmmssSSS"));
                        }
                        File.Move(sourceFile, errfile);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "File upload to SFTP target location failed.");
                SendEmail($"ERROR : Communication Outbound Queue Process - File upload to SFTP target location failed ",
                                    "",
                                    $" Error : Communication Outbound Queue Process - File upload to SFTP target location failed - {ex}");
                return false;
            }
        }

        public List<EventType> FetchOutboundEventTypes()
        {
            var eventTypes = new EventType(); 
            try
            {
                Business.EventTypeBL etBL = null;
                
                using (var svcscope = _svcProvider.CreateScope())
                {
                    etBL = svcscope.ServiceProvider.GetRequiredService<Business.EventTypeBL>();

                    eventTypes = etBL.FetchOutboundEventTypes();

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fetching Outbound Event Types from Database table failed.");
                SendEmail($"ERROR : Communication Outbound Queue Process - Fetching Outbound Event Types from Database table failed ",
                                    "",
                                    $" Error : Communication Outbound Queue Process - Fetching Outbound Event Types from Database table failed - {ex}");
            }
            return eventTypes;
        }

        public bool CreateOutboundQueueLog(string logDescrption, bool status, int id, int eventTypeId)
        {
            _logger.LogInformation("Create Outbound Data Queue Log started");
            try
            {
                _logger.LogInformation("Create Outbound Data Queue Log Try Block");
                Business.OutboundDataQueueBL bl = null;
                var logToSave = new OutboundDataQueueLog()
                {
                    OutboundDataQueueId = id,
                    Description = logDescrption,
                    EventTypeId = eventTypeId,
                    Status = status,
                    CreatedAt = DateTime.UtcNow
                };
                using (var svcscope = _svcProvider.CreateScope())
                {
                    bl = svcscope.ServiceProvider.GetRequiredService<Business.OutboundDataQueueBL>();
                    OutboundDataQueueLog logAdded = bl.CreateOutboundDataQueueLog(logToSave);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create Outbound Data Queue Log failed.");
                SendEmail($"ERROR : Communication Outbound Queue Process - Create Outbound Data Queue Log failed ",
                            "",
                            $" Error : Communication Outbound Queue Process - Create Outbound Data Queue Log failed - {ex}");
                return false;
            }
        }

        public bool SendEmail(string subject, string textBody, string htmlBody)
        {
            _logger.LogInformation("SendEmail Method");
            SendEmailConfig emailConfig = new SendEmailConfig
            {
                SupportEmail = _uploadSettings.EmailConfiguration.SupportEmail,
                SmtpPort = _uploadSettings.EmailConfiguration.SmtpPort,
                SmtpHost = _uploadSettings.EmailConfiguration.SmtpHost,
                FromMail = _uploadSettings.EmailConfiguration.FromMail,
                EnvironmentEmailSubjectPrefix = _uploadSettings.EmailConfiguration.EnvironmentEmailSubjectPrefix,
                SiteName = _uploadSettings.EmailConfiguration.SiteName,
                FromEmail = _uploadSettings.EmailConfiguration.FromEmail,
                FromMailPassword = _uploadSettings.EmailConfiguration.FromMailPassword,
            };
            ISendEmail _sendEmail = new SendEmail(_logger, emailConfig); 
            if(_sendEmail.SendMail(_uploadSettings.EmailConfiguration.ProductionSupportEmailTo,
                                    _uploadSettings.EmailConfiguration.SupportEmail,
                                    subject,
                                    textBody,
                                    htmlBody))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                FetchOutboundQueue();

                int waitMilliseconds = (_uploadSettings.PollIntervalSeconds <= 1 ? 1 : _uploadSettings.PollIntervalSeconds) * 1000;

                await Task.Delay(waitMilliseconds, stoppingToken);
            }
        }
    }
}

