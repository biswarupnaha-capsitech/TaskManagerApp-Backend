using Capsitech;
using Capsitech.Extensions;
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using TaskManager.Util;

namespace TaskManager.Services
{
    /// <summary>
    /// Email sender, This class is used by the application to send email for account confirmation and password reset.
    /// </summary>
    public class EmailSender : IEmailSender
    {
        private static SendGridSettings Settings;
        public static void Init(IConfiguration configuration)
        {
            Settings = new SendGridSettings
            {
                ApiKey = configuration["SendGrid:ApiKey"],
                SenderEmail = configuration["SendGrid:SenderEmail"],
                SenderName = configuration["SendGrid:SenderName"],
            };
        }
        public static void Init(SendGridSettings sendGridSettings) => Settings = sendGridSettings;
        public async Task<Response> SendEmailAsync(string email, string subject, string message) =>
            await SendEmailInternal(new EmailAddress(email), null, null, subject, message, "");


        public async Task<Response> SendEmailAsync(string email, string subject, string message, params Attachment[] attachments) =>
            await SendEmailInternal(new EmailAddress(email), null, null, subject, message, "", attachments);

        public async Task<Response> SendEmailAsync(EmailAddress email, string subject, string message) =>
            await SendEmailInternal(email, null, null, subject, message, "");

        public async Task<Response> SendEmailAsync(EmailAddress email, List<EmailAddress> ccs, string subject, string message) =>
            await SendEmailInternal(email, ccs, null, subject, message, "");

        public async Task<Response> SendEmailAsync(EmailAddress email, string subject, string message, params Attachment[] attachments) =>
            await SendEmailInternal(email, null, null, subject, message, "", attachments);

        public async Task<Response> SendEmailAsync(EmailAddress email, List<EmailAddress> ccs, string subject, string message, params Attachment[] attachments) =>
            await SendEmailInternal(email, ccs, null, subject, message, "", attachments);

        public async Task<Response> SendEmailBccAsync(EmailAddress email, List<EmailAddress> bccs, string subject, string message, string senderName, params Attachment[] attachments) =>
         await SendEmailBccInternal(email, bccs, null, subject, message, senderName, attachments);
        public async Task<Response> SendEmailAsync(List<EmailAddress> emails, string subject, string message) =>
            await SendEmailInternal(emails, null, null, subject, message);

        public async Task<Response> SendEmailAsync(List<EmailAddress> emails, List<EmailAddress> ccs, string subject, string message) =>
            await SendEmailInternal(emails, ccs, null, subject, message);

        public async Task<Response> SendEmailAsync(List<EmailAddress> emails, string subject, string message, params Attachment[] attachments) =>
            await SendEmailInternal(emails, null, null, subject, message, attachments);

        public async Task<Response> SendEmailAsync(List<EmailAddress> emails, List<EmailAddress> ccs, string subject, string message, params Attachment[] attachments) =>
            await SendEmailInternal(emails, ccs, null, subject, message, attachments);

        public async Task<Response> SendEmailWithReplyAsync(string email, string replyTo, string subject, string message, string senderName = null) =>
            await SendEmailInternal(new EmailAddress(email), null, replyTo.IsEmpty() ? null : new EmailAddress(replyTo), subject, message, senderName);

        public async Task<Response> SendEmailWithReplyAsync(string email, string replyTo, string subject, string message, string senderName, params Attachment[] attachments) =>
            await SendEmailInternal(new EmailAddress(email), null, replyTo.IsEmpty() ? null : new EmailAddress(replyTo), subject, message, senderName, attachments);

        async Task<Response> SendEmailBccInternal(EmailAddress email, List<EmailAddress> bccs, EmailAddress replyTo, string subject, string message, string senderName, params Attachment[] attachments)
        {
            try
            {
                var client = new SendGridClient(Settings.ApiKey);
                var msg = new SendGridMessage()
                {
                    From = Settings.GetSender(senderName),
                    Subject = subject,
                    HtmlContent = message
                };
                msg.AddTo(email);

                if (bccs?.Count > 0)
                    msg.AddBccs(bccs);

                if (replyTo != null)
                    msg.ReplyTo = replyTo;

                if (attachments?.Length > 0)
                    msg.AddAttachments(new List<Attachment>(attachments));

                var resp = await client.SendEmailAsync(msg);
                return resp;
            }
            catch (Exception ex)
            {
                await SendException(ex);
            }

            return null;
        }
        async Task<Response> SendEmailInternal(EmailAddress email, List<EmailAddress> ccs, EmailAddress replyTo, string subject, string message, string senderName, params Attachment[] attachments)
        {
            try
            {
                var client = new SendGridClient(Settings.ApiKey);
                var msg = new SendGridMessage()
                {
                    From = Settings.GetSender(senderName),
                    Subject = subject,
                    HtmlContent = message
                };
                msg.AddTo(email);

                if (ccs?.Count > 0)
                    msg.AddCcs(ccs);

                if (replyTo != null)
                    msg.ReplyTo = replyTo;

                if (attachments?.Length > 0)
                    msg.AddAttachments(new List<Attachment>(attachments));

                return await client.SendEmailAsync(msg);
            }
            catch (Exception ex)
            {
                await SendException(ex);
            }

            return null;
        }
        async Task<Response> SendEmailInternal(List<EmailAddress> emails, List<EmailAddress> ccs, EmailAddress replyTo, string subject, string message, params Attachment[] attachments)
        {
            try
            {
                var tos = emails.Where(e => !e.Email.IsEmpty()).Distinct(new EmailAddressComparer()).ToList();
                if (tos?.Count == 0) return null;

                var client = new SendGridClient(Settings.ApiKey);
                var msg = new SendGridMessage()
                {
                    From = Settings.GetSender(),
                    Subject = subject,
                    HtmlContent = message,
                };
                msg.AddTos(tos);

                if (ccs?.Count > 0)
                {
                    var ccAddresses = ccs.Intersect(emails, new EmailAddressComparer())?.Where(e => !e.Email.IsEmpty());
                    if (ccAddresses?.Count() > 0)
                        msg.AddCcs(ccAddresses.ToList());
                }

                if (replyTo != null)
                    msg.ReplyTo = replyTo;

                if (attachments?.Length > 0)
                    msg.AddAttachments(new List<Attachment>(attachments));

                var response = await client.SendEmailAsync(msg);
                return response;
            }
            catch (Exception ex)
            {
                await SendException(ex);
            }

            return null;
        }
        public static async Task<Response> SendException(Exception exception, HttpContext context, string moreInfo = null, string exceptionPathUrl = null)
        {
            try
            {
                //skip exception to sending to the developer
                if (CanSkipException(exception))
                    return null;

                var em = new EmailSender();
                var strBody = "";
                try
                {
                    if (context.Request.Body?.Length > 0)
                        using (StreamReader reader = new StreamReader(context.Request.Body))
                            strBody = await reader.ReadToEndAsync();
                }
                catch
                {
                    try
                    {
                        if (context.Request.Form?.Count > 0)
                            strBody = string.Join(", ", context.Request.Form.Keys.Select(k => $"{k}: {context.Request.Form[k]}"));
                    }
                    catch { }
                }

                return await em.SendEmailAsync(GetDeveloperEmails(), $"Error in application - {AppConfig.Current.Version}", $@"
An error was caught in application<br />
User Name: {context.User.Identity.GetUserName()} - {context.User.GetUserId()}<br />
Time: {DateTime.Now}<br />
Version: {AppConfig.Current.Version}<br />
Request URL: {exceptionPathUrl ?? context.Request.Path.ToString()}<br />
Request IP: {context.Request.Host.ToString()}<br />
Query: {(context.Request.QueryString != null ? context.Request.QueryString.Value : "")}<br />
Body: {strBody}{(!moreInfo.IsEmpty() ? "<br />Description: " + moreInfo : "")}<br /><br />

Exception:<br />{(exception.ToString() + (exception.InnerException != null ? "<br /><br />InnerException:<br />" + exception.InnerException.ToString() : "") + "<br /><br />StackTrace:<br/>" + exception.StackTrace)}");
            }
            catch
            {
                //throw;
            }

            return null;
        }
        public static async Task<Response> SendException(Exception exception, string desc = null)
        {
            try
            {
                //skip exception to sending to the developer
                if (CanSkipException(exception))
                    return null;

                var em = new EmailSender();
                return await em.SendEmailAsync(GetDeveloperEmails(), $"Error in application - {AppConfig.Current.Version}", $@"
An error was caught in application<br /><br />{(!desc.IsEmpty() ? "Desc: " + desc + "<br />" : "")}
Time: {DateTime.Now}<br />
Version: {AppConfig.Current.Version}<br /><br />
Exception:<br />{(exception.ToString() + (exception.InnerException != null ? "<br /><br />InnerException:<br />" + exception.InnerException.ToString() : "") + "<br /><br />StackTrace:<br/>" + exception.StackTrace)}");
            }
            catch
            {
                //throw;
            }

            return null;
        }
        public static async Task<Response> SendException(string messsage, string level = "info")
        {
            try
            {
                var em = new EmailSender();
                return await em.SendEmailAsync(GetDeveloperEmails(), $"{level} - {AppConfig.Current.Version}", $@"
An message was sent by application<br /><br />{(!level.IsEmpty() ? "Level: " + level + "<br />" : "")}
Time: {DateTime.Now}<br />
Version: {AppConfig.Current.Version}<br /><br />
Message:<br />{messsage}");
            }
            catch
            {
                //throw;
            }

            return null;
        }

        static List<EmailAddress> GetDeveloperEmails()
        {
            List<EmailAddress> tos = new List<EmailAddress>();
            if (AppConfig.Current.DeveloperEmail?.Split(",")?.Length > 0)
            {
                tos.AddRange(AppConfig.Current.DeveloperEmail.Split(",").Where(e => !e.IsEmpty()).Select(e => new EmailAddress(e.Trim())));
            }
            else
                tos.Add(new EmailAddress(AppConfig.Current.DeveloperEmail));
            return tos;
        }
        static bool CanSkipException(Exception exception)
        {
            //skip AppModelException to sending to the developer
            //skip CheckDuplicateException to sending to the developer
            if (exception is Capsitech.Data.MongoDB.Exceptions.DuplicateRecordException ||
                exception is AppModelException ||
                (exception is FormatException && exception.Message?.StartsWith("'null'") == true))
                return true;

            //mongodb duplicate key
            if (exception is MongoDB.Driver.MongoWriteException &&
                exception.Message?.Contains("duplicate key") == true)
                return true;

            //check for ObjectId format exception
            if (exception is FormatException &&
                exception.Message?.Contains("not a valid 24 digit hex string") == true)
                return true;


            //Microsoft.AspNetCore.Connections.ConnectionResetException: An existing connection was forcibly closed by the remote host
            if (exception is Microsoft.AspNetCore.Connections.ConnectionResetException)
                return true;

            //Microsoft.AspNetCore.Server.Kestrel.Core.BadHttpRequestException: Reading the request body timed out due to data arriving too slowly.
            if (exception is Microsoft.AspNetCore.Server.Kestrel.Core.BadHttpRequestException)
                return true;

            return false;
        }
    }

    public class SendGridSettings
    {
        public string ApiKey { get; set; }
        public string SenderEmail { get; set; } = "noreply@emails.capsitech.com";
        public string SenderName { get; set; } = "Capsitech";
        public EmailAddress GetSender(string senderName = null)//bool isForTCF)
        {
            return new EmailAddress(SenderEmail, senderName ?? SenderName);
        }
    }

    public interface IEmailSender
    {
        Task<Response> SendEmailAsync(string email, string subject, string message);
        Task<Response> SendEmailAsync(string email, string subject, string message, params Attachment[] attachments);
        Task<Response> SendEmailAsync(EmailAddress email, string subject, string message);
        Task<Response> SendEmailAsync(EmailAddress email, List<EmailAddress> ccs, string subject, string message);
        Task<Response> SendEmailAsync(EmailAddress email, string subject, string message, params Attachment[] attachments);
        Task<Response> SendEmailAsync(EmailAddress email, List<EmailAddress> ccs, string subject, string message, params Attachment[] attachments);
        Task<Response> SendEmailBccAsync(EmailAddress email, List<EmailAddress> bccs, string subject, string message, string senderName, params Attachment[] attachments);
        Task<Response> SendEmailAsync(List<EmailAddress> emails, string subject, string message);
        Task<Response> SendEmailAsync(List<EmailAddress> emails, string subject, string message, params Attachment[] attachments);

        Task<Response> SendEmailAsync(List<EmailAddress> emails, List<EmailAddress> ccs, string subject, string message);
        Task<Response> SendEmailAsync(List<EmailAddress> emails, List<EmailAddress> ccs, string subject, string message, params Attachment[] attachments);

        Task<Response> SendEmailWithReplyAsync(string email, string replyTo, string subject, string message, string senderName);
        Task<Response> SendEmailWithReplyAsync(string email, string replyTo, string subject, string message, string senderName, params Attachment[] attachments);
    }

    /// <summary>
    /// Equality comparer for <see cref="EmailAddress"/>
    /// </summary>
    public class EmailAddressComparer : IEqualityComparer<EmailAddress>
    {
        /// <summary>
        /// Method to compare to objects
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals(EmailAddress x, EmailAddress y)
        {
            return x.Email.Equals(y.Email, StringComparison.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// Get hashcode
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode(EmailAddress obj)
        {
            return obj.Email.GetHashCode();
        }
    }
}
