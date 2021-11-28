using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using ChatSystem.Models;
using ChatSystem.BL;
using ChatSystem.Helping_Clesses;

namespace ChatSystem.Helping_Clesses
{
    public class MailSender
    {
        public static bool SendForgotPasswordEmail(int id, string email, string BaseUrl = "")
        {
            try
            {
                MailMessage msg = new MailMessage();
                string text = "<link href='https://fonts.googleapis.com/css?family=Bree+Serif' rel='stylesheet'><style>  * {";
                text += "  font-family: 'Bree Serif', serif; }";
                text += " .list-group-item {       border: none;  }    .hor {      border-bottom: 5px solid black;   }";
                text += " .line {       margin-bottom: 20px; }";
                msg.From = new MailAddress("no.replay.nodlays@gmail.com");
                msg.To.Add(email);
                msg.Subject = "Chat System | Password Reset";
                msg.IsBodyHtml = true;
                string temp = "<html><head></head><body><nav class='navbar navbar-default'><div class='container-fluid'>" +
                    "</div> </nav><center><div><h1 class='text-center'>Password Reset!</h1>" +
                    "<p class='text-center'> Simply click the button showing below to reset your password (Link will expire after date change): </p><br>" +
                    "<button style='background-color: rgb(0,174,239);'>" +
                        "<a href='" + BaseUrl + "Auth/ResetPassword?id=" + StringCipher.Base64Encode(id.ToString()) + "&time=" + StringCipher.Base64Encode(DateTime.Now.ToString("MM/dd/yyyy")) + "' style='text-decoration:none;font-size:15px;color:white;'>Reset Password</a>" +
                    "</button>" +
                    "<p style='color:red;'>Link will not work in spam. Please move this mail into your inbox.</p></div></center>";

                temp += "<script src = 'https://ajax.googleapis.com/ajax/libs/jquery/3.2.1/jquery.min.js' ></ script ></ body ></ html >";
                //string link = "https://localhost:44363/Auth/ResetPassword?email=" + StringCipher.Encrypt(email, "test") + "&time=" + StringCipher.Encrypt(DateTime.Now.ToString(), "test");
                //link = link.Replace("+", "%20");
                //temp = temp.Replace("LINKFORFORGOTPASSWORD", link);
                msg.Body = temp;

                //Following method is used when other servers then gmail
                //using (SmtpClient client = new SmtpClient())
                //{
                //    client.EnableSsl = false;
                //    client.UseDefaultCredentials = false;
                //    client.Credentials = new NetworkCredential("info@nodlays.com", "delta@O27");
                //    client.Host = "webmail.nodlays.com";
                //    client.Port = 25;
                //    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                //    client.Send(msg);
                //}

                //following method is used on gmail server 
                using (SmtpClient smt = new SmtpClient())
                {
                    smt.Host = "smtp.gmail.com";
                    NetworkCredential ntwd = new NetworkCredential();
                    ntwd.UserName = "no-reply@zuptu.com"; //Your Email ID
                    ntwd.Password = "fWPFL7DseQKp6Di12"; // Your Password
                    smt.UseDefaultCredentials = false;
                    smt.Credentials = ntwd;
                    smt.Port = 587;
                    smt.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smt.EnableSsl = true;
                    smt.Send(msg);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }


        public static bool SendChatStatus(int chatId)
        {
            try
            {
                DatabaseEntities de = new DatabaseEntities();
                
                ChatRoom chat = new ChatRoomBL().GetChatRoomById(chatId, de);

                List<ChatMessage> msglist = new ChatMessageBL().GetActiveChatMessageList(chatId, de).OrderBy(x=> x.CreatedAt).ToList();

                MailMessage msg = new MailMessage();
                msg.From = new MailAddress("no-reply@zuptu.com");
                msg.To.Add(chat.User1.Email);
                //msg.To.Add("waqaxahmed786@gmail.com");
                msg.Subject = chat.Company.Name + " | Chat Summary";
                msg.IsBodyHtml = true;
                
                string MailBody = "<html>" +
                "<head>" +
                    "<link rel='stylesheet' href='https://maxcdn.bootstrapcdn.com/bootstrap/3.4.1/css/bootstrap.min.css'>" +
                    "<script src='https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js'></script>" +
                    "<script src='https://maxcdn.bootstrapcdn.com/bootstrap/3.4.1/js/bootstrap.min.js'></script>" +
                "</head>" +
                "<body>" +
                    "<div class='container' style='padding:10px; margin:10px; border:1px solid lightgrey; border-radius:5px;'>" +
                        "<center>" +
                            "<h1>Agent # " + chat.User.Name + "</h1>" +
                            "<b>Started At #</b> " + Convert.ToDateTime(chat.StartedAt).ToString("MM/dd/yyyy HH:mm:ss") + "<br>" +
                            "<b>Closed At #</b> " + Convert.ToDateTime(chat.ClosedAt).ToString("MM/dd/yyyy HH:mm:ss") + "<br>" +
                            "<b>Total Messages #</b> " + msglist.Count + "<br>" +
                            "<hr>" +
                            "<h2>Chat Summary</h2>" +
                        "</center>" +
                        "<br>";


                if(msglist.Count > 0)
                {
                    foreach(ChatMessage c in msglist)
                    {
                        if(c.SenderId == chat.ClientId)
                        {
                            string name = c.ChatRoom.User1.Name;
                            if(name.Length > 20)
                            {
                                name = name.Substring(0, 20) + "..,";
                            }
                            MailBody += "<div class='row'>" +
                                "<div style='margin-left: auto;margin-right: 0; background-color:lightblue; padding:5px; width:250px; border-radius:5px 0 5px 5px;'> " +
                                      "<b style='float:right;' title='"+ c.ChatRoom.User1.Name + "'>"+name+"</b><br>" +
                                      "<hr/>" +
                                      "<p style='word-wrap: break-word;'>"+c.Message+"</p>" +

                                      "<small style='float:right; margin-top:5px;'> " +
                                          Convert.ToDateTime(c.CreatedAt).ToString("dddd, dd MMMM yyyy") +
                                          "<br>" +
                                          "<small style='float:right;'>" + Convert.ToDateTime(c.CreatedAt).ToString("HH:mm:ss") + "</small>"+
                                      "</small>" +
                                      "<br>" +
                                      "<br>" +

                                "</div>" +
                            "</div>" +
                            "<br>" +
                            "<br>";
                        }
                        else
                        {
                            string name = c.ChatRoom.User.Name;
                            if (name.Length > 20)
                            {
                                name = name.Substring(0, 20) + "..,";
                            }
                            MailBody += "<div class='row'>" +
                                "<div style='margin-right: auto; margin-left: 0; background-color:whitesmoke; padding:5px; width:250px; border-radius:0 5px 5px 5px;'>" +
                                      "<b style='float:left;' title='"+ c.ChatRoom.User.Name + "'>" + name + "</b><br>" +
                                      "<hr/>" +
                                      "<p style='word-wrap: break-word;'>" + c.Message + "</p>" +

                                      "<small style=''margin-right: auto; margin-left: 0; margin-top:5px;'> " +
                                          Convert.ToDateTime(c.CreatedAt).ToString("dddd, dd MMMM yyyy") +
                                          "<br>" +
                                          "<small style='margin-right: auto; margin-left: 0;'>" + Convert.ToDateTime(c.CreatedAt).ToString("HH:mm:ss") + "</smal>" +
                                      "</small>" +

                                "</div>" +
                            "</div>" +
                            "<br>" +
                            "<br>";
                        }
                    }
                }
                else
                {
                    MailBody += "<center style='color:red'><h3>No Record Found</h3></center>";
                }

                MailBody += "</div>" +
                            "<br>" +
                            "<br>" +
                    "</body>"+
                "</html>";

                
                msg.Body = MailBody;

                using (SmtpClient smt = new SmtpClient())
                {
                    smt.Host = "smtp.gmail.com";
                    NetworkCredential ntwd = new NetworkCredential();
                    ntwd.UserName = "no-reply@zuptu.com"; //Your Email ID
                    ntwd.Password = "fWPFL7DseQKp6Di12"; // Your Password
                    smt.UseDefaultCredentials = false;
                    smt.Credentials = ntwd;
                    smt.Port = 587;
                    smt.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smt.EnableSsl = true;
                    smt.Send(msg);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}