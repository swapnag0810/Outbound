using Collection.DataIntegrator.Outbound.Common.Interface;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Collection.DataIntegrator.Outbound.Common
{
    public class SFTPConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

    }
    public class SFTPClient : IFileStorage
    {
        private readonly ILogger _logger;

        private readonly SFTPConfig _config;

        public SFTPClient(ILogger logger, SFTPConfig sftpConfig)
        {
            _logger = logger;
            _config = sftpConfig;
        }

        public bool CanConnect()
        {
            using var client = new Renci.SshNet.SftpClient(_config.Host, _config.Port == 0 ? 22 : _config.Port, _config.UserName, _config.Password);
            try
            {
                client.Connect();
                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Failed to connect");
                return false;
            }
            finally
            {
                client.Disconnect();
            }
        }

        public IEnumerable<string> List(string remoteDirectory = ".")
        {
            using var client = new Renci.SshNet.SftpClient(_config.Host, _config.Port == 0 ? 22 : _config.Port, _config.UserName, _config.Password);
            try
            {
                client.Connect();
                return client.ListDirectory(remoteDirectory).Where(f => f.IsRegularFile).Select(f => f.FullName);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Failed in listing files under [{remoteDirectory}]");
                return null;
            }
            finally
            {
                client.Disconnect();
            }
        }

        public bool Delete(string remoteFilePath)
        {
            using var client = new Renci.SshNet.SftpClient(_config.Host, _config.Port == 0 ? 22 : _config.Port, _config.UserName, _config.Password);
            try
            {
                client.Connect();
                client.DeleteFile(remoteFilePath);
                _logger.LogInformation($"File [{remoteFilePath}] deleted.");
                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Failed in deleting file [{remoteFilePath}]");
            }
            finally
            {
                client.Disconnect();
            }

            return false;
        }

        public bool Download(string remoteFilePath, string localFilePath)
        {
            using var client = new Renci.SshNet.SftpClient(_config.Host, _config.Port == 0 ? 22 : _config.Port, _config.UserName, _config.Password);
            try
            {
                client.Connect();
                using var s = File.Create(localFilePath);
                client.DownloadFile(remoteFilePath, s);
                _logger.LogInformation($"Finished downloading file [{localFilePath}] from [{remoteFilePath}]");
                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Failed in downloading file [{localFilePath}] from [{remoteFilePath}]");
            }
            finally
            {
                client.Disconnect();
            }
            return false;
        }


        public bool Upload(string localFilePath, string remoteFilePath)
        {
            using var client = new Renci.SshNet.SftpClient(_config.Host, _config.Port == 0 ? 22 : _config.Port, _config.UserName, _config.Password);
            try
            {
                client.Connect();
                using var s = File.OpenRead(localFilePath);
                client.UploadFile(s, remoteFilePath);
                _logger.LogInformation($"Finished uploading file [{localFilePath}] to [{remoteFilePath}]");
                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Failed in uploading file [{localFilePath}] to [{remoteFilePath}]");
            }
            finally
            {
                client.Disconnect();
            }
            return false;
        }

    }
}
