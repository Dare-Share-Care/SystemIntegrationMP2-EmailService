using System;
using System.Net;
using System.Net.Mail;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EmailService
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "emailsQueue", durable: false, exclusive: false, autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, eventArgs) =>
            {
                try
                {
                    var body = eventArgs.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    var emailDetail = JsonConvert.DeserializeObject<EmailDetail>(message);

                    if (emailDetail?.Status == null || emailDetail.EmailAddress == null)
                    {
                        Console.WriteLine("Error: Missing Status or EmailAddress in message.");
                        return; // Exit the handler
                    }

                    switch (emailDetail.Status)
                    {
                        case "rejected":
                            SendEmail(emailDetail.EmailAddress, "Your Application Was Rejected",
                                "We regret to inform you that your application was rejected. Please try again later.");
                            break;
                        case "response":
                            SendEmail(emailDetail.EmailAddress, "Thank you for your response",
                                "We have received your response and will get back to you soon.");
                            break;
                    }
                }
                catch (JsonSerializationException jsonEx)
                {
                    Console.WriteLine($"Error deserializing message: {jsonEx.Message}");
                }
                catch (SmtpException smtpEx)
                {
                    Console.WriteLine($"SMTP Error: {smtpEx.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An unexpected error occurred: {ex.Message}");
                }
            };

            channel.BasicConsume(queue: "emailsQueue", autoAck: true, consumer: consumer);
            Console.WriteLine("Email Service started. Press [enter] to exit.");
            Console.ReadLine();
        }

        private static void SendEmail(string toEmail, string subject, string body)
        {
            var emailUser = Environment.GetEnvironmentVariable("EMAIL_SERVICE_USER");
            var emailPass = Environment.GetEnvironmentVariable("EMAIL_SERVICE_PASS");

            // Check if the email user or password is null or empty
            if (string.IsNullOrEmpty(emailUser) || string.IsNullOrEmpty(emailPass))
            {
                Console.WriteLine(
                    "Error: SMTP credentials are missing or incomplete. Ensure the environment variables SMTP_USER and SMTP_PASS are set.");
                return; // Early return from the method
            }

            using var client = new SmtpClient("smtp.office365.com");
            client.Port = 587;
            client.Credentials = new NetworkCredential(emailUser, emailPass);
            client.EnableSsl = true;

            var mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(emailUser); // Assuming the emailUser is also the "from" address
            mailMessage.To.Add(toEmail);
            mailMessage.Body = body;
            mailMessage.Subject = subject;

            try
            {
                client.Send(mailMessage);
            }
            catch (SmtpException smtpEx)
            {
                Console.WriteLine($"SMTP Error: {smtpEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            }
        }

        public class EmailDetail
        {
            public string? Status { get; set; }

            public string? EmailAddress { get; set; } // Assuming each message has the target email address
            // Add other properties as needed
        }
    }
}
