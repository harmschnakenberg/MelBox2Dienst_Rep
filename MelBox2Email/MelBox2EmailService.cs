using Microsoft.Office.Interop.Outlook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MelBox2Email
{
    public partial class MelBox2EmailService : ServiceBase
    {
        public static int EmailPokeInterval { get; set; } = 60;

        static readonly Application OutlookApp = new Application();
        static readonly NameSpace OutlookNamespace = OutlookApp.GetNamespace("MAPI");
        readonly MAPIFolder inboxFolder = OutlookNamespace.GetDefaultFolder(OlDefaultFolders.olFolderInbox);
        readonly MAPIFolder archiveFolder = OutlookNamespace.GetDefaultFolder(OlDefaultFolders.olFolderJournal);
        

        public MelBox2EmailService()
        {
            InitializeComponent();
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
            OutlookNamespace.Logon();
            OutlookApp.NewMail += OutlookApp_NewMail;

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

            // Log out and release resources
            OutlookNamespace.Logoff();
            System.Runtime.InteropServices.Marshal.ReleaseComObject(OutlookNamespace);
            System.Runtime.InteropServices.Marshal.ReleaseComObject(OutlookApp);

            // Ensure resources are properly released even in case of exceptions
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();

        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            // TODO: Insert monitoring activities here.
#if DEBUG
            if (Environment.UserInteractive)
                Console.WriteLine(DateTime.Now);
#endif
            OutlookApp_NewMail();

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

        private void OutlookApp_NewMail()
        {
            // Retrieve the emails in the Inbox folder
            Items emails = inboxFolder.Items;

            foreach (MailItem email in emails)
            {
                try
                {
                    Console.WriteLine("Subject: " + email.Subject);
                    Console.WriteLine("Sender: " + email.SenderName);
                    Console.WriteLine("Body: " + email.Body);
                    Console.WriteLine();

                    //email.Move(archiveFolder);
                }
                catch (System.Exception ex)
                {
                    // Handle specific exceptions related to email processing
                    Console.WriteLine("Error processing email: " + ex.Message);
                }
            }

        }

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
