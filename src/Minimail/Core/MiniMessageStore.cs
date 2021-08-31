using Microsoft.Extensions.Logging;
using Minimail.Core;
using SmtpServer;
using SmtpServer.Protocol;
using SmtpServer.Storage;
using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Minimail
{
    public class MiniMessageStore : MessageStore
    {
        private ILogger _logger;
        private string _tmpDir;
        private string _newDir;

        public MiniMessageStore(PathsOptions pathsOptions, ILogger logger)
        {
            _logger = logger;

            _tmpDir = Path.Combine(pathsOptions.Maildir, "tmp");
            Directory.CreateDirectory(_tmpDir);

            _newDir = Path.Combine(pathsOptions.Maildir, "new");
            Directory.CreateDirectory(_newDir);

            Directory.CreateDirectory(Path.Combine(pathsOptions.Maildir, "cur"));
        }

        public override async Task<SmtpResponse> SaveAsync(ISessionContext context, IMessageTransaction transaction, ReadOnlySequence<byte> buffer, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Store mail.");

            try
            {
                var fileName = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid()}";
                var tmpFilePath = Path.Combine(_tmpDir, fileName);

                using (var stream = File.OpenWrite(tmpFilePath))
                {
                    var position = buffer.GetPosition(0);

                    while (buffer.TryGet(ref position, out var memory))
                    {
                        await stream.WriteAsync(memory, cancellationToken);
                    }
                }

                try
                {
                    File.Move(tmpFilePath, Path.Combine(_newDir, fileName));
                }
                catch (Exception)
                {
                    try
                    {
                        if (File.Exists(tmpFilePath))
                            File.Delete(tmpFilePath);
                    }
                    catch (Exception)
                    {
                        //
                    }

                    throw;
                }
                
                return SmtpResponse.Ok;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to store mail.");
                return SmtpResponse.MailboxUnavailable;
            }
        }
    }
}
