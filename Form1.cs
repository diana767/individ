using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace ndividualnie
{
    public partial class Form1 : Form
    {
        public class PasswordEntry
        {
            public string Service { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
        }

        private List<PasswordEntry> passwords = new List<PasswordEntry>();
        private ListView listView;
        private string dataFile = "passwords.dat";
        private string encryptionKey = "MySecureKey123!"; // В реальном приложении храните это безопасно

        public Form1()
        {
            InitializeForm();
            LoadPasswords();
        }

        private void InitializeForm()
        {
            this.Text = "Менеджер паролей";
            this.Size = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Кнопки
            var btnPanel = new Panel { Dock = DockStyle.Top, Height = 40 };

            var btnAdd = new Button { Text = "Добавить", Size = new Size(80, 30), Location = new Point(10, 5) };
            btnAdd.Click += (s, e) => AddPassword();

            var btnEdit = new Button { Text = "Изменить", Size = new Size(80, 30), Location = new Point(100, 5) };
            btnEdit.Click += (s, e) => EditPassword();

            var btnDelete = new Button { Text = "Удалить", Size = new Size(80, 30), Location = new Point(190, 5) };
            btnDelete.Click += (s, e) => DeletePassword();

            var btnCopy = new Button { Text = "Копировать", Size = new Size(80, 30), Location = new Point(280, 5) };
            btnCopy.Click += (s, e) => CopyPassword();

            btnPanel.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDelete, btnCopy });

            // Список паролей
            listView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                Columns = {
                    { "Сервис", 200 },
                    { "Логин", 150 },
                    { "Пароль", 150 }
                }
            };

            this.Controls.AddRange(new Control[] { btnPanel, listView });
        }

        private void AddPassword()
        {
            using (var form = new PasswordForm())
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    passwords.Add(new PasswordEntry
                    {
                        Service = form.Service,
                        Username = form.Username,
                        Password = Encrypt(form.Password)
                    });
                    SavePasswords();
                    UpdateList();
                }
            }
        }

        private void EditPassword()
        {
            if (listView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Выберите пароль для редактирования");
                return;
            }

            var selected = passwords[listView.SelectedIndices[0]];
            using (var form = new PasswordForm(selected.Service, selected.Username, Decrypt(selected.Password)))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    selected.Service = form.Service;
                    selected.Username = form.Username;
                    selected.Password = Encrypt(form.Password);
                    SavePasswords();
                    UpdateList();
                }
            }
        }

        private void DeletePassword()
        {
            if (listView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Выберите пароль для удаления");
                return;
            }

            if (MessageBox.Show("Удалить выбранный пароль?", "Подтверждение",
                MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                passwords.RemoveAt(listView.SelectedIndices[0]);
                SavePasswords();
                UpdateList();
            }
        }

        private void CopyPassword()
        {
            if (listView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Выберите пароль для копирования");
                return;
            }

            var selected = passwords[listView.SelectedIndices[0]];
            Clipboard.SetText(Decrypt(selected.Password));
            MessageBox.Show("Пароль скопирован в буфер обмена");
        }

        private void UpdateList()
        {
            listView.Items.Clear();
            foreach (var entry in passwords)
            {
                var item = new ListViewItem(entry.Service);
                item.SubItems.Add(entry.Username);
                item.SubItems.Add("••••••••");
                listView.Items.Add(item);
            }
        }

        private void SavePasswords()
        {
            try
            {
                using (var writer = new StreamWriter(dataFile))
                {
                    foreach (var entry in passwords)
                    {
                        writer.WriteLine($"{entry.Service}|{entry.Username}|{entry.Password}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void LoadPasswords()
        {
            if (!File.Exists(dataFile)) return;

            try
            {
                passwords.Clear();
                using (var reader = new StreamReader(dataFile))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split('|');
                        if (parts.Length == 3)
                        {
                            passwords.Add(new PasswordEntry
                            {
                                Service = parts[0],
                                Username = parts[1],
                                Password = parts[2]
                            });
                        }
                    }
                }
                UpdateList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private string Encrypt(string text)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(encryptionKey.PadRight(32).Substring(0, 32));
                aes.IV = new byte[16]; // В реальном приложении используйте уникальный IV

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(text);
                        }
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
        }

        private string Decrypt(string cipherText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(encryptionKey.PadRight(32).Substring(0, 32));
                aes.IV = new byte[16];

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
    }

    public class PasswordForm : Form
    {
        public string Service { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }

        private TextBox txtService, txtUsername, txtPassword;
        private Button btnGenerate;

        public PasswordForm(string service = "", string username = "", string password = "")
        {
            this.Text = string.IsNullOrEmpty(service) ? "Добавить пароль" : "Изменить пароль";
            this.Size = new Size(350, 230);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;

            // Элементы формы
            var lblService = new Label { Text = "Сервис:", Location = new Point(10, 10) };
            txtService = new TextBox { Text = service, Location = new Point(10, 30), Width = 300 };

            var lblUsername = new Label { Text = "Логин:", Location = new Point(10, 60) };
            txtUsername = new TextBox { Text = username, Location = new Point(10, 80), Width = 300 };

            var lblPassword = new Label { Text = "Пароль:", Location = new Point(10, 110) };
            txtPassword = new TextBox { Text = password, Location = new Point(10, 130), Width = 200 };

            btnGenerate = new Button { Text = "Сгенерировать", Size = new Size(90, 23), Location = new Point(220, 130) };
            btnGenerate.Click += (s, e) => GeneratePassword();

            var btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, Location = new Point(150, 160) };
            var btnCancel = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel, Location = new Point(230, 160) };

            btnOk.Click += (s, e) =>
            {
                Service = txtService.Text;
                Username = txtUsername.Text;
                Password = txtPassword.Text;
            };

            this.Controls.AddRange(new Control[] {
                lblService, txtService,
                lblUsername, txtUsername,
                lblPassword, txtPassword, btnGenerate,
                btnOk, btnCancel
            });

            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
        }

        private void GeneratePassword()
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
            var rng = new RNGCryptoServiceProvider();
            var bytes = new byte[12];
            rng.GetBytes(bytes);

            var chars = new char[12];
            for (int i = 0; i < 12; i++)
            {
                chars[i] = validChars[bytes[i] % validChars.Length];
            }

            txtPassword.Text = new string(chars);
        }
    }
}