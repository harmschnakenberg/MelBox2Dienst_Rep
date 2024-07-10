using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MelBox2Dienst
{
    public static partial class Pipes
    {
        #region Senden
 

        //internal static void SendEmail(Email email)
        //{
        //    Log.Info(
        //        "NamedPipe SendEmail(): " +
        //        Send(Name.Email, Verb.EmailSend, Status.New, JsonSerializer.Serialize(email))
        //        );
        //}

        //internal async static void SendSms(List<string> guards, Sms sms, uint messageId)
        //{
        //    foreach (string phone in guards)
        //    {
        //        if (IsPhoneNumber(phone))
        //        {
                  
        //            sms.Phone = phone;
        //            string result = await Send( Pipes.Name.Gsm, Pipes.Verb.SmsSend, Status.New, JsonSerializer.Serialize<Sms>(sms) );

        //            // von GSM:  $"{Verb.SmsSend}|{Status.Success}|{sms2}";
        //            //TODO: Senden von SMS von GSM-Modem bestätigen lassen und SMS-Id in DB eintragen.

        //            string[] args = result.Split('|');
        //            Sms smsAnswer = JsonSerializer.Deserialize<Sms>(args[2]);

        //            if (Status.Success == args[1])
        //                Sql.CreateSent(messageId, smsAnswer);
        //            else
        //                Log.Error($"Sms-Versand an '{phone}' konnte nicht bestätigt werden; '{sms.Content}'");
                   
        //        }
        //    }

        //}

        //public static bool IsPhoneNumber(string number)
        //{
        //    return Regex.Match(number, @"(\+[0-9]+)").Success;
        //}

        #endregion

        #region Empfangen
//        private static string ParseAnswer(string[] args)
//        {
//            if (args.Length < 3) return args.ToString();
           
//            string verb = args[0];
//            string status = args[1];
//            string arg = args[2];

//            try
//            {
//                switch (verb)
//                {
//                    case Verb.SmsRecieved:
//                        try
//                        {
//                            // SENDER:
//                            // Pipes.Send(Pipes.Name.MelBox2Service, Pipes.Verb.SmsRecieved, Pipes.Status.New, JsonSerializer.Serialize(simSms)).RunSynchronously();

//                            if (status != Pipes.Status.New) //nicht vorgesehener Fall
//                                return $"{Verb.SmsRecieved}|{Status.Error}|{arg}";

//                            Sms sms = JsonSerializer.Deserialize<Sms>(arg);
//#if DEBUG
//                            Log.Info($"Empfangen:\r\n" +
//                                $"\t{verb}|{status}|{arg}\r\n\r\n" +
//                                $"SMS:\r\n" +
//                                $"Index:\t{sms.Index}\r\n" +
//                                $"von:\t{sms.Phone}\r\n" +
//                                $"Inhalt:\t{sms.Content}");
//#endif
//                            if (Sql.MessageRecieveAndRelay(sms))
//                                return $"{verb}|{Pipes.Status.Success}|{arg}";
//                            else
//                            {
//                                Log.Error($"SMS konnte nicht gelesen werden: '{args}'");
//                                return $"{verb}|{Pipes.Status.Error}|{arg}";
//                            }
//                        }
//                        catch
//                        {
//                            return $"{verb}|{Pipes.Status.Error}|{arg}";
//                        }

//                    default:
//                        string x = $"Unerwartete Anfrage NamedPipe: {args}";
//                        Log.Info(x);
//                        return x;
//                }
//            }
//            catch (Exception ex)
//            {
//                return ex.ToString();
//            }

//        }

        #endregion
    }
}
