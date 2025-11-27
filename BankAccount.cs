using System;

namespace ConsoleApplication
{
    public class BankAccount
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; }
        public decimal Balance { get; set; }
        public string AccountHolder { get; set; }

        public BankAccount(string accountNumber, string accountHolder, decimal initialBalance = 0)
        {
            AccountNumber = accountNumber;
            AccountHolder = accountHolder;
            Balance = initialBalance;
        }

        public BankAccount() { }
    }
}
