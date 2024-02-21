using SmtpServer;
using SmtpServer.Mail;
using SmtpServer.Storage;
using System.Collections.Concurrent;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Minimail
{
    class MiniMailboxFilter : IMailboxFilter
    {
        private ConcurrentDictionary<string, object?> _whitelist;
        private ILogger _logger;

        public MiniMailboxFilter(ConcurrentDictionary<string, object?> whitelist, ILogger logger)
        {
            _whitelist = whitelist;
            _logger = logger;
        }

        public Task<MailboxFilterResult> CanAcceptFromAsync(ISessionContext context, IMailbox from, int size, CancellationToken cancellationToken)
        {
            return Task.FromResult(MailboxFilterResult.Yes);
        }

        public Task<MailboxFilterResult> CanDeliverToAsync(ISessionContext context, IMailbox to, IMailbox from, CancellationToken cancellationToken)
        {
            try
            {
                var toAddress = to.AsAddress();

                if (_whitelist.ContainsKey(toAddress))
                {
                    _logger.LogInformation("Accept to deliver mail from {From} to {To}.", from.AsAddress(), toAddress);
                    return Task.FromResult(MailboxFilterResult.Yes);
                }

                else
                {
                    _logger.LogInformation("Reject to deliver mail from {From} to {To}.", from.AsAddress(), toAddress);
                    return Task.FromResult(MailboxFilterResult.NoPermanently);
                }
            }
            catch (Exception)
            {
                return Task.FromResult(MailboxFilterResult.NoPermanently);
            }
        }
    }
}
