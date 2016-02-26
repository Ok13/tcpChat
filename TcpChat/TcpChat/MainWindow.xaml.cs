using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Security.Cryptography;
using System.IO;
namespace TcpChat
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string ipS = null;
        public static string portS = null;
        Rijndael myRijndael = Rijndael.Create();
        public MainWindow()
        {
            InitializeComponent();
            myIP.Content ="myIP: "+Dns.GetHostByName(Dns.GetHostName()).AddressList[0].ToString();
            Thread myThread = new Thread(new ParameterizedThreadStart(ServerStart));
            myThread.Start("192.168.0.1"); 
         

        }
        static void ipset(string ip)
        {
            ipS = ip;
        }
       
       
        public  void ServerStart(object ip)
        {
            ipS = (string)ip;
         //IPAddress ipAddr = IPAddress.Parse("127.0.0.1");
            
            IPAddress ipAddr = Dns.GetHostByName(Dns.GetHostName()).AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 8800);
           
            // Создаем сокет Tcp/Ip
            Socket sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Назначаем сокет локальной конечной точке и слушаем входящие сокеты
            try
            {
                sListener.Bind(ipEndPoint);
                sListener.Listen(10);
                
                // Начинаем слушать соединения
                while (true)
                {

                    

                    // Программа приостанавливается, ожидая входящее соединение
                    Socket handler = sListener.Accept();
                    string data = null;
                    byte[] bytes = new byte[1024];                    
                    int bytesRec = handler.Receive(bytes);
                    Array.Resize(ref bytes, bytesRec);
                    byte[] msg;
                    try
                    {
                        data = Cryptography.Crypto.Decrypt(bytes, myRijndael.Key, myRijndael.IV);

                        this.Dispatcher.BeginInvoke(new Action(delegate()
                        {
                            DialogText.Text = DialogText.Text + ipAddr.ToString() + "\n" + data + "\n";
                        }));
                        msg = Encoding.UTF8.GetBytes("OK");
                    }
                    catch
                    {
                        MessageBox.Show("Ошибка ключа");
                        msg = Encoding.UTF8.GetBytes("NO");
                    }

                 
                            
                   
                    handler.Send(msg);

                    if (data.IndexOf("<TheEnd>") > -1)
                    {
                        MessageBox.Show("Сервер завершил соединение с клиентом.");
                        break;
                    }

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
          
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string Ip=null;
            Ip = ipText.Text;
            string message = null;
            byte[] msg = new byte[1024];
            message = MessageText.Text;
            if ((message == "") | (Ip == ""))
            {
                MessageBox.Show("Вы не ввели текст либо IP");
            }
            else
            {
               
                try
                {
                    DialogText.Text = DialogText.Text + Ip + "\n" + message + "\n";

                    
                    msg = Cryptography.Crypto.Encrypt(message, myRijndael.Key, myRijndael.IV);
              
                    ClientLib.Client.SendMessage(8800, Ip, msg);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            byte[] Key = Encoding.UTF8.GetBytes(PasswordBox.Password);
            Array.Resize(ref Key, 16);
            myRijndael.Key = Key;
            myRijndael.IV = Encoding.UTF8.GetBytes("1234567890123456");

        }
    }
}
