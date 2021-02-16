
using goodmaji;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace GoodMajiPrescoShopCollection.Core.Implement
{
    class EmailService
    {
        public  void SendEmailAysnc(string recipient , string content , string subject )
        {
            try
            {
                string fromEmail = "testhawooo@gmail.com";
                MailMessage message = new MailMessage(recipient, fromEmail, subject, content);
                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.UseDefaultCredentials = false;

                client.Credentials = new System.Net.NetworkCredential(fromEmail, "h@Wo0o!@#$%");
                client.Send(message);
            }
            catch (Exception ex)
            {
                SqlDbmanager.AddErrorLog("", ex.Message);
            }
        }

    
    }
}
