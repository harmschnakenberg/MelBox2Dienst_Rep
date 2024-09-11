using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace MelBox2Email
{
    public partial class MelBox2EmailService : ServiceBase
    {
        const string PR_SMTP_ADDRESS = "http://schemas.microsoft.com/mapi/proptag/0x39FE001E";

        public static int EmailPokeInterval { get; set; } = Properties.Settings.Default.EmailPokeInterval;

        static readonly Application OutlookApp = new Application();
        static readonly NameSpace OutlookNamespace = OutlookApp.GetNamespace("MAPI");
        static MAPIFolder inboxFolder = OutlookNamespace.GetDefaultFolder(OlDefaultFolders.olFolderInbox);        
        static MAPIFolder archiveFolder = OutlookNamespace.GetDefaultFolder(OlDefaultFolders.olFolderJournal);
        

        public MelBox2EmailService()
        {
            InitializeComponent();
            MAPIFolder global = OutlookNamespace.GetDefaultFolder(OlDefaultFolders.olFolderInbox).Parent;
            inboxFolder = SetCustomMailBoxFolder(global, Properties.Settings.Default.InBoxFolder);
            archiveFolder = SetCustomMailBoxFolder(inboxFolder, Properties.Settings.Default.ArchiveFolderName);

            Console.WriteLine($"Posteingang: {inboxFolder.Name}, Archivordner: {archiveFolder.Name}");            
        }

        /// <summary>
        /// Setzt oder erstellt einen Archivierungsordner als Unterordner des Posteingangs in Outlook.
        /// Empfangene E-Mails werden nach Auswertung in diesen Ordner verschoben.
        /// </summary>
        /// <param name="archiveFolderName">Name des Ordners in Outlook</param>
        private static MAPIFolder SetCustomMailBoxFolder(MAPIFolder root, string archiveFolderName)
        {
            //Quelle: https://www.c-sharpcorner.com/article/outlook-integration-in-C-Sharp/
                        
            foreach (MAPIFolder subFolder in root.Folders)
            {
                if (subFolder.Name == archiveFolderName)                
                    return subFolder;                                    
            }
            
            return root.Folders.Add(archiveFolderName, Type.Missing);
        }

        internal void TestStartupAndStop()
        {
            this.OnStart(new string[0]);

            Console.WriteLine($"{this.ServiceName} wurde als Konsolenanwendung gestartet. Beliebige Taste zum beenden..");
            Console.ReadKey();
            Console.WriteLine($"{this.ServiceName} wurde beendet.");

            this.OnStop();
        }

        protected override void OnStart(string[] args)
        {
            Pipe4.StartPipeServer(Pipe4.PipeName.Email, true);
            
            OutlookNamespace.Logon();
            OutlookApp.NewMail += OutlookApp_NewMail;
      
            var exchangeUser = OutlookNamespace.CurrentUser.AddressEntry.GetExchangeUser();
            Pipe4.SendEmailStatus(nameof(exchangeUser.Name), exchangeUser.Name);
            Pipe4.SendEmailStatus(nameof(exchangeUser.PrimarySmtpAddress), exchangeUser.PrimarySmtpAddress);
            
            // Set up a timer that triggers every minute.
            System.Timers.Timer timer = new System.Timers.Timer
            {
                Interval = EmailPokeInterval * 1000
            };
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();
        }


        protected override void OnStop()
        {
            try
            {
                OutlookApp.NewMail -= OutlookApp_NewMail;

                // Log out and release resources
                OutlookNamespace.Logoff();                
                System.Runtime.InteropServices.Marshal.ReleaseComObject(OutlookNamespace);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(OutlookApp);
            }
            catch
            {
                if (Environment.UserInteractive)
                    Console.WriteLine("Die Verbindung zu Outlook konnte nicht sauber geschlossen werden.");
            }
            finally
            {
                // Ensure resources are properly released even in case of exceptions
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.
#if DEBUG
            if (Environment.UserInteractive)
                Console.WriteLine(DateTime.Now);
#endif
            OutlookApp_NewMail();

            // Zum Testen:
            //string[] to = { "harmschnakenberg@gmx.de" };
            //string[] cc = { "harm.schnakenberg@kreutztraeger.de" };
            //OutlookApp_SendMail(to, cc, "Ein Test", "Testweise gesendet. " + DateTime.Now);

        }

  

        private void OutlookApp_NewMail()
        {
            // Retrieve the emails in the Inbox folder
            Items emails = inboxFolder.Items; // .Items[1]; //TEST nur eine Mail
            emails.Sort("[ReceivedTime]", true); //true if you want them descending

            Console.WriteLine($"{emails.Count} in Ordner {inboxFolder.Name}");
            int maxCounter = 3; //TEST

            foreach (MailItem email in emails)
            {
                try
                {
                    // max. Anzahl von E-Mail pro Lesevorgang
                    maxCounter--;
                    Console.WriteLine(maxCounter);
                    if (0 > maxCounter)
                        break;

                    //nur ungelesene Nachrichten einlesen 
                    //if (email.UnRead == false)                   
                    //    continue;
                    
                    email.Recipients.ResolveAll();
                    string from = email.SenderEmailAddress; //.PropertyAccessor.GetProperty(PR_SMTP_ADDRESS) as string;
                    string subject = email.Subject;
                    string body = email.Body;
                    List<string> to = GetSMTPAddressForRecipients(email, OlMailRecipientType.olTo);
                    List<string> cc = GetSMTPAddressForRecipients(email, OlMailRecipientType.olCC);

                    #region in Konsole anzeigen
                    if (Environment.UserInteractive)
                    {
                        Console.WriteLine(new string('_', 32));
                        Console.WriteLine("Sender: " + from);
                        Console.Write("To: ");
                        foreach (var item in to)                        
                            Console.Write(item + ", ");
                        Console.Write("\r\nCc: ");
                        foreach (var item in cc)
                            Console.Write(item + ", ");

                        Console.WriteLine("\r\nSubject: " + subject);
                        Console.WriteLine("Body:\r\n" + (body.Length > 32 ? body.Substring(0,32) : body).Replace("\r\n", " ") );
                        Console.WriteLine(new string('_', 32));
                    }
                    #endregion
                                        
               
                    if (to.Count == 0) //Gürtel + Hosenträger
                        to.Add(email.To);

                    Email pipeEmail = new Email(from, to, cc, subject, body);
                   

                    Pipe4.EmailRecieved(pipeEmail);
                    Thread.Sleep(2000);
//#if !DEBUG
                    email.Move(archiveFolder);
//#endif          
                }
                catch (System.Exception ex)
                {
                    // Handle specific exceptions related to email processing
                    Console.WriteLine("\r\nError processing email:\r\n" + ex);
                    Thread.Sleep(5000);
                }
            }

        }



        /// <summary>
        /// Liest die E-Mail-Adresse aus dem object MailItem.Address
        /// Quelle:https://learn.microsoft.com/en-us/office/client-developer/outlook/pia/how-to-get-the-e-mail-address-of-a-recipient
        /// </summary>
        /// <param name="mail"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private List<string> GetSMTPAddressForRecipients(MailItem mail, OlMailRecipientType type)
        {
            List<string> addresses = new List<string>();

            
            Recipients recips = mail.Recipients;
            foreach (Recipient recip in recips)
            {
                if (recip.Type != (int)type)
                    continue;

                PropertyAccessor pa = recip.PropertyAccessor;
                string smtpAddress =
                    pa.GetProperty(PR_SMTP_ADDRESS).ToString();
#if DEBUG
                Debug.WriteLine(recip.Name + " SMTP=" + smtpAddress);
#endif
                addresses.Add(smtpAddress);
            }

            return addresses;
        }

        internal static void OutlookApp_SendMail(Email email)
        {
            OutlookApp_SendMail(email.To.ToArray(), email.Cc.ToArray(), email.Subject, email.Body);
        }

        private static void OutlookApp_SendMail(string[] to, string[] cc, string subject, string body, bool deleteAfterSubmit = false)
        {
            MailItem mailItem = OutlookApp.CreateItem(OlItemType.olMailItem);

            //mailItem.To = "recipient@example.com";

            if (cc != null)
                foreach (var recipient in to)
                    mailItem.Recipients.Add(recipient).Type = (int)OlMailRecipientType.olTo;

            if (cc != null)
                foreach (var recipient in cc)
                    mailItem.Recipients.Add(recipient).Type = (int)OlMailRecipientType.olCC;

            mailItem.Recipients.ResolveAll();
            mailItem.Subject = subject;
            mailItem.Body = body;
            //mailItem.Save(); //Falls nicht alle EMpfänger aufgelöst werdne können. Quelle: https://stackoverflow.com/questions/28592906/get-recipients-from-outlook-mailitem

            // Attach a file to the email (optional)
            //mailItem.Attachments.Add("C:\\temp\\file.txt");

            //unter anderem Email-Account versenden
            //mailItem.SendUsingAccount = new Application().Session.Accounts["from@mail.com"];

            mailItem.DeleteAfterSubmit = deleteAfterSubmit;

            if (mailItem.Recipients.Count > 0
                && mailItem.Subject.Length > 0
                && mailItem.Body.Length > 0)
            {
                mailItem.Send();
            }
            // Release the COM object
            Marshal.ReleaseComObject(mailItem);
        }

    

        /// <summary>
        /// 
        /// Quelle: https://dotnetcode.medium.com/streamlining-email-management-with-outlook-interop-libraries-in-c-9aaf5d75bd4a
        /// </summary>
        //void Test()
        //{
        //    try
        //    {
        //        Application outlookApp = new Application();

        //        // Log in to Outlook
        //        NameSpace outlookNamespace = outlookApp.GetNamespace("MAPI");
        //        outlookNamespace.Logon();

        //        // Get the Inbox folder
        //        MAPIFolder inboxFolder = outlookNamespace.GetDefaultFolder(OlDefaultFolders.olFolderInbox);
        //        MAPIFolder archiveFolder = outlookNamespace.GetDefaultFolder(OlDefaultFolders.olFolderJournal);

        //        //outlookApp.NewMailEx += OutlookApp_NewMailEx;
        //        outlookApp.NewMail += OutlookApp_NewMail;

        //        // Retrieve the emails in the Inbox folder
        //        Items emails = inboxFolder.Items;

        //        foreach (MailItem email in emails)
        //        {
        //            try
        //            {
        //                Console.WriteLine("Subject: " + email.Subject);
        //                Console.WriteLine("Sender: " + email.SenderName);
        //                Console.WriteLine("Body: " + email.Body);
        //                Console.WriteLine();

        //                //email.Move(archiveFolder);
        //            }
        //            catch (System.Exception ex)
        //            {
        //                // Handle specific exceptions related to email processing
        //                Console.WriteLine("Error processing email: " + ex.Message);
        //            }
        //        }

        //        // Log out and release resources
        //        outlookNamespace.Logoff();
        //        System.Runtime.InteropServices.Marshal.ReleaseComObject(outlookNamespace);
        //        System.Runtime.InteropServices.Marshal.ReleaseComObject(outlookApp);
        //    }
        //    catch (System.Exception ex)
        //    {
        //        // Handle general exceptions related to Outlook connection or login
        //        Console.WriteLine("Error connecting to Outlook: " + ex.Message);
        //    }
        //    finally
        //    {
        //        // Ensure resources are properly released even in case of exceptions
        //        GC.Collect();
        //        GC.WaitForPendingFinalizers();
        //        GC.Collect();
        //        GC.WaitForPendingFinalizers();
        //    }

        //    Console.WriteLine("Press any key to exit...");
        //    Console.ReadKey();
        //}

        //private void OutlookApp_NewMailEx(string EntryIDCollection)
        //{
        //    MailItem mailItem = outlookNamespace.GetItemFromID(EntryIDCollection, folder.StoreID) as Outlook.MailItem;
        //    throw new NotImplementedException();
        //}

        //private void OutlookApp_NewMail()
        //{
        //    throw new NotImplementedException();
        //}



        //private void outlookApp_NewMailEx(string entryID)
        //{
        //    try
        //    {
        //        Outlook.NameSpace outlookNS = outlookApp.GetNamespace("MAPI");
        //        Outlook.MAPIFolder folder = outlookApp.Session.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox);
        //        if (outlookNS != null)
        //        {
        //            Outlook.MailItem mailItem = outlookNS.GetItemFromID(entryID, folder.StoreID) as Outlook.MailItem;

        //            Outlook.PropertyAccessor pa = mailItem.Sender.PropertyAccessor;
        //            string smtpAddress = pa.GetProperty(PR_SMTP_ADDRESS).ToString();

        //            LoggerEventArgs e = new LoggerEventArgs();
        //            e.LogTime = string.Format("[{0:HH:mm:ss.fff}]", DateTime.Now);
        //            e.MessageLevel = LoggerMessageLevel.Level3;
        //            e.MessageType = MessageType.OUTL_RECEIVE_MAIL;
        //            e.MessageText = "SUBJECT=" + mailItem.Subject + "; " +
        //                "RECIPIENTS=" + GetSMTPAddressForRecipients(mailItem) + "; " +
        //                "BODY=" + mailItem.Body;
        //            Observer.LogEvent(e);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        ErrorLogEventArgs err = new ErrorLogEventArgs();
        //        err.LogTime = string.Format("[{0:HH:mm:ss.fff}]", DateTime.Now);
        //        err.MessageText = e.Message;
        //        err.StackTrace = e.StackTrace;

        //        Observer.LogException(err);
        //    }
        //}
    }




}
