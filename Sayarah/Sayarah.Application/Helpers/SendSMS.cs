//using System;
//using System.IO;
//using System.Net;


//namespace Sayarah.Helpers
//{
//    public class SMSHelper
//    {
//        public SendingResult SendMessage(SendMessageInput input)
//        {

//            try
//            {


//                HttpWebRequest req = (HttpWebRequest)
//                WebRequest.Create("http://www.alfa-cell.com/api/msgSend.php");
//                req.Method = "POST";
//                req.ContentType = "application/x-www-form-urlencoded";
//                string postData = "mobile=" + input.UserName
//                    + "&password=" + input.Password
//                    + "&numbers=" + input.PhoneNumbers
//                    + "&sender=" + input.SenderName
//                    + "&msg=" + ConvertToUnicode(input.MessageText) + "&applicationType=24";
//                req.ContentLength = postData.Length;

//                StreamWriter stOut = new
//                StreamWriter(req.GetRequestStream(),
//                System.Text.Encoding.ASCII);
//                stOut.Write(postData);
//                stOut.Close();
//                string strResponse;
//                StreamReader stIn = new StreamReader(req.GetResponse().GetResponseStream());
//                strResponse = stIn.ReadToEnd();
//                stIn.Close();

//                var status = Int32.Parse(strResponse);

//                return new SendingResult { Success = status == 1 ? true : false, Status = status, Message = TextError(strResponse) };
//            }
//            catch (Exception ex)
//            {
//                return new SendingResult { Message = ex.Message };
//            }

//        }

//        private string ConvertToUnicode(string val)
//        {
//            string msg2 = string.Empty;

//            for (int i = 0; i < val.Length; i++)
//            {
//                msg2 += convertToUnicode(System.Convert.ToChar(val.Substring(i, 1)));
//            }

//            return msg2;
//        }
//        private string convertToUnicode(char ch)
//        {
//            System.Text.UnicodeEncoding class1 = new System.Text.UnicodeEncoding();
//            byte[] msg = class1.GetBytes(System.Convert.ToString(ch));
//            return fourDigits(msg[1] + msg[0].ToString("X"));
//        }

//        private string fourDigits(string val)
//        {
//            string result = string.Empty;

//            switch (val.Length)
//            {
//                case 1: result = "000" + val; break;
//                case 2: result = "00" + val; break;
//                case 3: result = "0" + val; break;
//                case 4: result = val; break;
//            }
//            return result;
//        }

//        private string TextError(string val)
//        {
//            switch (val)
//            {
//                case "1":
//                    //SMS Have Been Send
//                    return "تمت العملية بنجاح";
//                case "2":
//                    //balabce is zero (SMS Not Send)
//                    return "إن رصيدك لدى ألفا سيل قد إنتهى ولم يعد به أي رسائل. (لحل المشكلة قم بشحن رصيدك من الرسائل لدى ألفا سيل. لشحن رصيدك إتبع تعليمات شحن الرصيد)";
//                case "3":
//                    //balance not enough (SMS Not Send)
//                    return "إن رصيدك الحالي لا يكفي لإتمام عملية الإرسال. (لحل المشكلة قم بشحن رصيدك من الرسائل لدى ألفا سيل. لشحن رصيدك إتبع تعليمات شحن الرصيد).";
//                case "4":
//                    //error in user name (SMS Not Send)
//                    return "إن إسم المستخدم الذي إستخدمته للدخول إلى حساب الرسائل غير صحيح (تأكد من أن إسم المستخدم الذي إستخدمته هو نفسه الذي تستخدمه عند دخولك إلى موقع ألفا سيل).";
//                case "5":
//                    //error in password (SMS Not Send)
//                    return "هناك خطأ في كلمة المرور (تأكد من أن كلمة المرور التي تم إستخدامها هي نفسها التي تستخدمها عند دخولك موقع ألفا سيل,إذا نسيت كلمة المرور إضغط على رابط نسيت كلمة المرور لتصلك رسالة على جوالك برقم المرور الخاص بك)";
//                case "6":
//                    //there is a problem in sending, try again later  (SMS Not Send)
//                    return "إن صفحة الإرسال لاتجيب في الوقت الحالي (قد يكون هناك طلب كبير على الصفحة أو توقف مؤقت للصفحة فقط حاول مرة أخرى أو تواصل مع الدعم الفني إذا إستمر الخطأ)";
//                case "12":
//                    return "إن حسابك بحاجة إلى تحديث يرجى مراجعة الدعم الفني.";
//                case "13":
//                    return "إن إسم المرسل الذي إستخدمته في هذه الرسالة لم يتم قبوله. (يرجى إرسال الرسالة بإسم مرسل آخر أو تعريف إسم المرسل لدى ألفا سيل)";
//                case "14":
//                    return "إن إسم المرسل الذي إستخدمته غير معرف لدى ألفا سيل. (يمكنك تعريف إسم المرسل من خلال صفحة إضافة إسم مرسل)";
//                case "15":
//                    return "يوجد رقم جوال خاطئ في الأرقام التي قمت بالإرسال لها. (تأكد من صحة الأرقام التي تريد الإرسال لها وأنها بالصيغة الدولية)";
//                case "16":
//                    return "الرسالة التي قمت بإرسالها لا تحتوي على إسم مرسل. (أدخل إسم مرسل عند إرسالك الرسالة)";
//                case "17":
//                    return "م يتم ارسال نص الرسالة. الرجاء التأكد من ارسال نص الرسالة والتأكد من تحويل الرسالة الى يوني كود (الرجاء التأكد من استخدام الدالة ConvertToUnicode)";
//                case "18":
//                    return "الارسال متوقف حاليا";
//                case "19":
//                    return "applicationType غير موجود في الرابط";
//                case "-1":
//                    return "لم يتم التواصل مع خادم (Server) الإرسال ألفا سيل بنجاح. (قد يكون هناك محاولات إرسال كثيرة تمت معا , أو قد يكون هناك عطل مؤقت طرأ على الخادم إذا إستمرت المشكلة يرجى التواصل مع الدعم الفني)";
//                case "-2":
//                    return "لم يتم التواصل مع خادملم يتم الربط مع قاعدة البيانات (Database) التي تحتوي على حسابك وبياناتك لدى ألفا سيل. (قد يكون هناك محاولات إرسال كثيرة تمت معا , أو قد يكون هناك عطل مؤقت طرأ على الخادم إذا إستمرت المشكلة يرجى التواصل مع الدعم الفني)";
//                default:
//                    return "";
//            }
//        }
//    }

//    /////////////////////////MessageInput///////////////////////////

//    public class SendMessageBase
//    {
//        public string UserName { set; get; }
//        public string Password { set; get; }
//        public string SenderName { set; get; }

//        public SendMessageBase()
//        {
//            UserName = "";
//            Password = "";
//            SenderName = "";
//        }
//    }
//    public class SendMessageInput : SendMessageBase
//    {
//        public string MessageText { set; get; }
//        public string PhoneNumbers { set; get; }
//    }
//    public class SendingResult
//    {
//        public bool Success { get; set; }
//        public int Status { get; set; }
//        public string Message { get; set; }
//    }
//}
