﻿using NUnit.Framework;
using RegressionTests.Infrastructure;
using RegressionTests.Shared;
using hMailServer;

namespace RegressionTests.IMAP
{
   [TestFixture]
   public class Examine : TestFixtureBase
   {
      [Test]
      [Description("Assert that it's not possible to change flags while in READONLY-mode")]
      public void TestChangeFlags()
      {
         Account oAccount = SingletonProvider<TestSetup>.Instance.AddAccount(_domain, "examine@test.com", "test");

         CustomAssert.IsTrue(SMTPClientSimulator.StaticSend("test@test.com", oAccount.Address, "Test", "test"));
         POP3Simulator.AssertMessageCount(oAccount.Address, "test", 1);

         var simulator = new IMAPSimulator();
         simulator.ConnectAndLogon(oAccount.Address, "test");
         simulator.ExamineFolder("Inbox");
         CustomAssert.IsFalse(simulator.SetFlagOnMessage(1, true, @"\Deleted"));
      }

      [Test]
      [Description(
         "Assert that the \\RECENT flag isn't automatically changed when accessing a folder in READONLY-mode")]
      public void TestChangeRecentFlag()
      {
         Account oAccount = SingletonProvider<TestSetup>.Instance.AddAccount(_domain, "examine@test.com", "test");

         CustomAssert.IsTrue(SMTPClientSimulator.StaticSend("test@test.com", oAccount.Address, "Test", "test"));
         POP3Simulator.AssertMessageCount(oAccount.Address, "test", 1);

         var simulator = new IMAPSimulator();
         simulator.ConnectAndLogon(oAccount.Address, "test");
         string result = simulator.ExamineFolder("Inbox");
         CustomAssert.IsTrue(result.Contains("* 1 RECENT"), result);
         simulator.Close();
         simulator.Disconnect();

         simulator = new IMAPSimulator();
         simulator.ConnectAndLogon(oAccount.Address, "test");
         CustomAssert.IsTrue(simulator.SelectFolder("Inbox", out result));
         CustomAssert.IsTrue(result.Contains("* 1 RECENT"), result);
         simulator.Close();
         simulator.Disconnect();

         simulator = new IMAPSimulator();
         simulator.ConnectAndLogon(oAccount.Address, "test");
         result = simulator.ExamineFolder("Inbox");
         CustomAssert.IsTrue(result.Contains("* 0 RECENT"), result);
         simulator.Close();
         simulator.Disconnect();
      }

      [Test]
      [Description("Assert that the \\SEEN flag isn't automatically changed when accessing a message in READONLY-mode"
         )]
      public void TestChangeSeenFlag()
      {
         Account oAccount = SingletonProvider<TestSetup>.Instance.AddAccount(_domain, "examine@test.com", "test");

         CustomAssert.IsTrue(SMTPClientSimulator.StaticSend("test@test.com", oAccount.Address, "Test", "test"));
         POP3Simulator.AssertMessageCount(oAccount.Address, "test", 1);

         var simulator = new IMAPSimulator();
         simulator.ConnectAndLogon(oAccount.Address, "test");
         simulator.ExamineFolder("Inbox");
         string flags = simulator.GetFlags(1);
         string body = simulator.Fetch("1 RFC822");
         string flagsAfter = simulator.GetFlags(1);
         simulator.Close();
         simulator.Disconnect();

         CustomAssert.AreEqual(flags, flagsAfter);

         var secondSimulator = new IMAPSimulator();
         secondSimulator.ConnectAndLogon(oAccount.Address, "test");
         secondSimulator.SelectFolder("Inbox");
         string secondFlags = secondSimulator.GetFlags(1);
         string secondBody = secondSimulator.Fetch("1 RFC822");
         string secondFlagsAfter = secondSimulator.GetFlags(1);
         secondSimulator.Close();
         secondSimulator.Disconnect();

         CustomAssert.AreNotEqual(secondFlags, secondFlagsAfter);
      }

      [Test]
      public void TestExamine()
      {
         Account oAccount = SingletonProvider<TestSetup>.Instance.AddAccount(_domain, "examine@test.com", "test");

         var oSimulator = new IMAPSimulator();

         string sWelcomeMessage = oSimulator.Connect();
         oSimulator.Logon(oAccount.Address, "test");
         CustomAssert.IsTrue(oSimulator.CreateFolder("TestFolder"));
         string result = oSimulator.ExamineFolder("TestFolder");

         CustomAssert.IsTrue(result.Contains("[PERMANENTFLAGS ()]"), result);
         CustomAssert.IsTrue(result.Contains("[READ-ONLY]"), result);
      }

      [Test]
      [Description("Assert that it's not possible to EXPUNGE while in READONLY-mode")]
      public void TestExpunge()
      {
         Account oAccount = SingletonProvider<TestSetup>.Instance.AddAccount(_domain, "examine@test.com", "test");

         CustomAssert.IsTrue(SMTPClientSimulator.StaticSend("test@test.com", oAccount.Address, "Test", "test"));
         POP3Simulator.AssertMessageCount(oAccount.Address, "test", 1);

         var simulator = new IMAPSimulator();
         simulator.ConnectAndLogon(oAccount.Address, "test");
         simulator.SelectFolder("Inbox");
         CustomAssert.IsTrue(simulator.SetFlagOnMessage(1, true, @"\Deleted"));

         var secondSimulator = new IMAPSimulator();
         secondSimulator.ConnectAndLogon(oAccount.Address, "test");
         string result = secondSimulator.ExamineFolder("INBOX");
         CustomAssert.IsTrue(result.Contains("1 EXISTS"), result);
         CustomAssert.IsFalse(secondSimulator.Expunge());

         simulator.SelectFolder("INBOX");
         CustomAssert.IsTrue(simulator.Expunge());

         simulator.Close();
         secondSimulator.Close();
      }
   }
}