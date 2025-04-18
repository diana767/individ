using System;
using System.IO;
using ndividualnie;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PasswordManagerTests
{
    [TestClass]
    public class PasswordEntryTests
    {
        private string testLogPath = "password_entry_test.log";

        [TestMethod]
        public void TestPropertyStorage()
        {
            using (var writer = new StreamWriter(testLogPath))
            {
                writer.WriteLine("=== Начало теста PasswordEntryTest1 ===");
                writer.WriteLine("Проверка хранения и возврата данных пароля");

                // Тест 1
                var testEntry = new Form1.PasswordEntry
                {
                    Service = "Google",
                    Username = "user@gmail.com",
                    Password = "secure123"
                };

                LogTestResult(writer,
                    testEntry.Service == "Google" &&
                    testEntry.Username == "user@gmail.com" &&
                    testEntry.Password == "secure123",
                    "Тест 1: Хранение и возврат значений свойств",
                    "Все свойства должны корректно сохраняться и возвращаться");

                // Тест 2
                testEntry.Service = "Yandex";
                testEntry.Username = "newuser@yandex.ru";
                testEntry.Password = "newpassword456";

                LogTestResult(writer,
                    testEntry.Service == "Yandex" &&
                    testEntry.Username == "newuser@yandex.ru" &&
                    testEntry.Password == "newpassword456",
                    "Тест 2: Изменение значений свойств",
                    "Значения свойств должны корректно изменяться");

                writer.WriteLine("=== Окончание теста PasswordEntryTest1 ===");
            }
        }

        private void LogTestResult(StreamWriter writer, bool condition, string testName, string description)
        {
            if (condition)
            {
                writer.WriteLine($"[PASS] {testName} - {description}");
            }
            else
            {
                writer.WriteLine($"[FAIL] {testName} - {description}");
            }
        }
    }
}