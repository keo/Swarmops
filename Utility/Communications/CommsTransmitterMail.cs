﻿using System;
using System.Net.Mail;
using System.Reflection;
using System.Text;
using Swarmops.Logic.Communications;
using Swarmops.Logic.Communications.Transmission;
using Swarmops.Logic.Support;
using Swarmops.Logic.Swarm;

namespace Swarmops.Utility.Communications
{
    public class CommsTransmitterMail : ICommsTransmitter
    {
        // To be implemented better

        private static string _smtpServerCache = string.Empty;
        private static DateTime _cacheReloadTime = DateTime.MinValue;

        public void Transmit (PayloadEnvelope envelope, Person person)
        {
            // Create the renderer via reflection of the static FromXml method

            Assembly assembly = typeof(PayloadEnvelope).Assembly;

            Type payloadType = assembly.GetType(envelope.PayloadClass);
            var methodInfo = payloadType.GetMethod("FromXml", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            ICommsRenderer renderer = (ICommsRenderer)(methodInfo.Invoke(null, new object[] { envelope.PayloadXml }));
            RenderedComm comm = renderer.RenderComm (person);

            MailMessage mail = new MailMessage();

            // This is a rather simple mail (no images or stuff like that)

            try
            {
                mail.From = new MailAddress ((string) comm[CommRenderPart.SenderMail], (string) comm[CommRenderPart.SenderName],
                    Encoding.UTF8);
                mail.To.Add (new MailAddress (person.Mail, person.Name));
            }
            catch (ArgumentException e)
            {
                // Address failure -- either sender or recipient

                _cacheReloadTime = DateTime.MinValue;
                throw new OutboundCommTransmitException ("Cannot send mail to " + person.Mail, e);
            }

            mail.Subject = (string) comm[CommRenderPart.Subject];
            mail.Body = (string) comm[CommRenderPart.BodyText];
            mail.SubjectEncoding = Encoding.UTF8;
            mail.BodyEncoding = Encoding.UTF8;

            string smtpServer = _smtpServerCache;

            DateTime now = DateTime.Now;

            if (now > _cacheReloadTime)
            {
                smtpServer = _smtpServerCache = Persistence.Key["SmtpServer"];
                _cacheReloadTime = now.AddMinutes (5);
            }

            if (string.IsNullOrEmpty (smtpServer))
            {
                smtpServer = "192.168.80.204";
                // For development use only - invalidate cache instead of this, forcing re-reload
                _cacheReloadTime = DateTime.MinValue;
            }

            SmtpClient mailClient = new SmtpClient (smtpServer);

            // TODO: SMTP Server login credentials

            try
            {
                mailClient.Send (mail);
            }
            catch (Exception e)
            {
                _cacheReloadTime = DateTime.MinValue;
                throw new OutboundCommTransmitException ("Cannot send mail to " + person.Mail, e);
            }
        }
    }
}