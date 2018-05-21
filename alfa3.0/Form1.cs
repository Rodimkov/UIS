using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Collections;
using System.Security.Cryptography;
using System.IO;
using System.Diagnostics;
using Emgu.CV;
using Emgu.CV.UI;
using Emgu.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;


namespace alfa3._0
{
    public partial class Form1 : Form
    {

        int count;
        Hashtable ht = new Hashtable();
        VideoCapture capture;

        public Form1()
        {
            InitializeComponent();
            Run();                                                                      // че это?)
            Thread thread = new Thread(predictor);
            thread.Start();
            using (FileStream fsKeyFile = File.Create("communication.txt"))
            //File.Create("communication.txt");
            if (System.IO.File.Exists("key.txt"))
            {
                using (FileStream fstream = File.OpenRead(@"key.txt"))
                {
                    int i;
                    // преобразуем строку в байты
                    byte[] array = new byte[fstream.Length];
                    // считываем данные
                    fstream.Read(array, 0, array.Length);
                    // декодируем байты в строку
                    string textFromFile = System.Text.Encoding.Default.GetString(array);


                    string tex;
                    while (textFromFile != "")
                    {
                        i = textFromFile.IndexOf("\r\n");
                        tex = textFromFile.Substring(0, i);
                        textFromFile = textFromFile.Substring(i + 2, textFromFile.Length - i - 2);
                        i = tex.IndexOf(": ");
                        string text1 = tex.Substring(0, i);
                        string text2 = tex.Substring(i + 2, tex.Length - i - 2);
                        ht.Add(text1, text2);
                        count++;
                    }
                }
            }
        }

        public class CryptFile
        {
            internal static void DecryptFile<T>(string srcFile, string keyFile, string outFile) where T : SymmetricAlgorithm, new()
            {
                using (T cryptAlgorithm = new T())
                {
                    using (FileStream fsKeyFile = File.OpenRead(keyFile))
                    using (BinaryReader brFile = new BinaryReader(fsKeyFile))
                    {
                        cryptAlgorithm.Key = brFile.ReadBytes(cryptAlgorithm.KeySize >> 3);
                        cryptAlgorithm.IV = brFile.ReadBytes(cryptAlgorithm.BlockSize >> 3);
                    }

                    using (FileStream fsFileOut = File.Create(outFile))
                    using (FileStream fsFileIn = File.OpenRead(srcFile))
                    using (ICryptoTransform crypto = cryptAlgorithm.CreateDecryptor())
                    using (CryptoStream csEncrypt = new CryptoStream(fsFileIn, crypto, CryptoStreamMode.Read))
                        csEncrypt.CopyTo(fsFileOut);
                }

            }

            internal static void EncryptFile<T>(string srcFile, string keyFile, string outFile) where T : SymmetricAlgorithm, new()
            {
                using (T cryptAlgorithm = new T())
                {
                    using (FileStream fsKeyFile = File.Create(keyFile))
                    using (BinaryWriter bwFile = new BinaryWriter(fsKeyFile))
                    {
                        bwFile.Write(cryptAlgorithm.Key);
                        bwFile.Write(cryptAlgorithm.IV);
                    }

                    using (FileStream fsFileOut = File.Create(outFile))
                    using (FileStream fsFileIn = File.OpenRead(srcFile))
                    using (ICryptoTransform crypto = cryptAlgorithm.CreateEncryptor())
                    using (CryptoStream csEncrypt = new CryptoStream(fsFileOut, crypto, CryptoStreamMode.Write))
                        fsFileIn.CopyTo(csEncrypt);
                }
            }
        }


        // защифровать файл
        private void button1_Click(object sender, EventArgs e)
        {
            cam();
            if (check())
            {
                OpenFileDialog dialog = new OpenFileDialog();
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    int i;
                    string s, temp;
                    string str = dialog.FileName;
                    int len = str.Length;
                    i = str.IndexOf('.');
                    s = str.Substring(i, len - i);
                    temp = "temp" + s;
                    string key = count.ToString();
                    count++;


                    CryptFile.EncryptFile<TripleDESCryptoServiceProvider>(@dialog.FileName, key, temp);
                    File.Delete(dialog.FileName);
                    File.Move(temp, str);

                    ht.Add(str, key);
                }
            }
            else
                errormessage();

        }

        // открыть  файл
        private void button2_Click(object sender, EventArgs e)
        {
            cam();
            if(check())
                {

                string key = "";
                int i;
                OpenFileDialog dialog = new OpenFileDialog();
                if (dialog.ShowDialog() == DialogResult.OK)
                {

                    ICollection keys = ht.Keys;
                    foreach (string sу in keys)
                    {
                        if (sу == dialog.FileName)
                            key += ht[sу];
                    }
                    if (key == "")
                    {
                        emprtyerror();
                        return;
                    }

                    string s, temp;
                    string str = dialog.FileName;
                    int len = str.Length;
                    i = str.IndexOf('.');
                    s = str.Substring(i, len - i);
                    temp = "temp" + s;

                    CryptFile.DecryptFile<TripleDESCryptoServiceProvider>(dialog.FileName, key, temp);
                    File.Delete(dialog.FileName);
                    File.Move(temp, str);

                    System.Diagnostics.ProcessStartInfo psi =
                    new System.Diagnostics.ProcessStartInfo(str);

                    System.Diagnostics.Process rfp = new System.Diagnostics.Process();
                    rfp = System.Diagnostics.Process.Start(psi);

                    rfp.WaitForExit();//ожидание завершения процесса

                    CryptFile.EncryptFile<TripleDESCryptoServiceProvider>(@str, key, temp);
                    File.Delete(dialog.FileName);
                    File.Move(temp, str);
                }
                else
                    errormessage();
            }
        }

        // дещифровать файл
        private void button3_Click(object sender, EventArgs e)
        {
            cam();
            if (check())
            {
                string key = "";
                int i;
                OpenFileDialog dialog = new OpenFileDialog();
                if (dialog.ShowDialog() == DialogResult.OK)
                {

                    ICollection keys = ht.Keys;
                    foreach (string sу in keys)
                    {
                        if (sу == dialog.FileName)
                            key += ht[sу];
                    }
                    if (key == "")
                    {
                        emprtyerror();
                        return;
                    }

                    string s, temp;
                    string str = dialog.FileName;
                    int len = str.Length;
                    i = str.IndexOf('.');
                    s = str.Substring(i, len - i);
                    temp = "temp" + s;

                    CryptFile.DecryptFile<TripleDESCryptoServiceProvider>(dialog.FileName, key, temp);
                    File.Delete(dialog.FileName);
                    File.Move(temp, str);
                }
            }
            else
                errormessage();
        }

        bool check()
        {
            while (true)
            {
                try
                {
                    File.WriteAllText("communication.txt", "start");
                    break;
                }
                catch (Exception Ex)
                {
                    //Application.Exit();
                }
            }


            string s = "";
            double d = 0 ;

            s = predict.StandardOutput.ReadLine();

            s = s.Substring(1, s.Length - 2);

            d = Convert.ToDouble(s);


            if (d <= 1.6)
                return true;
            return true;
        }

        void errormessage()
        {
            MessageBox.Show(
                "отказано в доступе",
                "Сообщение",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.DefaultDesktopOnly);
        }

        void emprtyerror()
        {
            MessageBox.Show(
                "ключ не найден (возможно вы еще не добавили этот файл в наше приложение)",
                "Сообщение",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1,
                MessageBoxOptions.DefaultDesktopOnly);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            string text = "";
            ICollection keys = ht.Keys;
            foreach (string s in keys)
                text += s + ": " + ht[s] + "\r\n";

            // запись в файл
            using (FileStream fstream = new FileStream(@"key.txt", FileMode.OpenOrCreate))
            {
                // преобразуем строку в байты
                byte[] array = System.Text.Encoding.Default.GetBytes(text);
                // запись массива байтов в файл
                fstream.Write(array, 0, array.Length);
            }

            File.Delete("communication.txt");
            predict.Close(); // исключение
        }

        private void Run()
        {
            try
            {
                capture = new VideoCapture();
            }
            catch (Exception Ex)
            {
                return;
            }
            Application.Idle += ProcessFrame;
        }

        private void ProcessFrame(object sender, EventArgs e)
        {
            imageBox1.Image = capture.QuerySmallFrame();
        }


        Process predict;
        void predictor()
        {
            string path = "testcam.exe";
            //Process predictor;
            predict = new Process();
            predict.StartInfo.UseShellExecute = false;
            predict.StartInfo.CreateNoWindow = true;
            predict.StartInfo.RedirectStandardError = true;
            predict.StartInfo.RedirectStandardInput = true;
            predict.StartInfo.RedirectStandardOutput = true;
            predict.StartInfo.FileName = path;

            predict.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            predict.Start();                                                      //завершиться сам?
        }




        void cam()
        {
            var img = capture.QueryFrame();
            img.Save("photo/you.jpg");
        }
    }
}
