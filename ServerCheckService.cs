using Cipher;
using ConnectionServer;
using System;
using System.Configuration;
using System.ServiceProcess;
using System.Timers;

namespace ServerCheckService
{
    public partial class ServerCheckService : ServiceBase
    {
        private Timer _timer;
        private int _intervalInMinutes;

        private string _ipServer1;
        private string _ipServer2;
        private int _serverPort1;
        private int _serverPort2;

        private string _loginSms;
        private string _passSms;
        private string _phoneNumber;

        private StringCipher _stringCipher = new StringCipher("804B8AA7-4ABA-4957-AB78-4C36EEBE0431");

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();


        public ServerCheckService()
        {
            InitializeComponent();

            try
            {
                _ipServer1 = ConfigurationManager.AppSettings["IpServer1"];
                _ipServer2 = ConfigurationManager.AppSettings["IpServer2"];
                _serverPort1 = Convert.ToInt32(ConfigurationManager.AppSettings["ServerPort1"]);
                _serverPort2 = Convert.ToInt32(ConfigurationManager.AppSettings["ServerPort2"]);

                _loginSms = ConfigurationManager.AppSettings["LoginSms"];
                _passSms = DecryptSenderSmsPassword();
                _phoneNumber = ConfigurationManager.AppSettings["PhoneNumber"];

                _intervalInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["IntervalInMinutes"]);
                _timer = new Timer(_intervalInMinutes * 60000);

            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw new Exception(ex.Message);
            }
        }

        protected override void OnStart(string[] args)
        {
            _timer.Elapsed += DoWork;
            _timer.Start();
            Logger.Info("Service started...");
        }

        private string DecryptSenderSmsPassword()
        {
            var encryptedPassword = ConfigurationManager.AppSettings["PassSms"];
            if (encryptedPassword.Length < 32)
            {
                encryptedPassword = _stringCipher.Encrypt(encryptedPassword);

                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                configFile.AppSettings.Settings["PassSms"].Value = encryptedPassword;
                configFile.Save();
            }
            
            return _stringCipher.Decrypt(encryptedPassword);
        }

        private void DoWork(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (!ConectionServer.ConnectToServer(_ipServer1, _serverPort1)) SendError("1");
                if (!ConectionServer.ConnectToServer(_ipServer2, _serverPort2)) SendError("2");

            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw new Exception(ex.Message);
            }
        }

        private void SendError(string serverNr)
        {
            var messageText = $"Serwer numer: {serverNr} nie odpowieada!!!";

            Logger.Info(serwersms.SerwerSMS.SendSms(_loginSms, _passSms, _phoneNumber, messageText));
            Logger.Info(messageText);
        }

        protected override void OnStop()
        {
            Logger.Info("Service stopped...");
        }
    }
}
