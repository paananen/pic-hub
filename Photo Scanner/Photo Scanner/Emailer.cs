using System;
using System.Net.Mail;
using System.Net;
using Newtonsoft.Json;

namespace Photo_Scanner
{
    internal static class Emailer
    {
        public static void Send(PhotoData pData)
        {
            try
            {
                var to = new MailAddress("to_address@gmail.com");
                var from = new MailAddress("from_address@gmail.com");
                var mail = new MailMessage(from, to)
                {
                    Subject = pData.FullPath,
                    Body = JsonConvert.SerializeObject(pData, Formatting.Indented)
                };
            
                var attachment = new Attachment(pData.FullPath);
                mail.Attachments.Add(attachment);
            
                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    // You might need to do this to enable emails https://support.google.com/accounts/answer/6010255
                    Credentials = new NetworkCredential("from_address@gmail.com", "from_address_password"),
                    EnableSsl = true
                };
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\t [e] - ...working...");
                smtp.Send(mail);
                Console.WriteLine("\t [e] - complete");
                Console.ForegroundColor = ConsoleColor.White;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
    }
}
