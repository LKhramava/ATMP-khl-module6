using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Configuration;
using System.Threading;

namespace WebDriverNUnit
{
	public class TestsForMailRu
	{
		private IWebDriver driver;
		private string baseUrl;

		private By userNameBy = By.Name("username");
		private By inputLoginSubmit = By.CssSelector(".login-row button[type='submit']");
		private By inputPassword = By.Name("password");

		private By sideBarContent = By.Id("sideBarContent");

		private By newEmailBy = By.XPath("//a[contains(@href,'compose')]");
		private By newEmailWindowBy = By.CssSelector(".compose-app__compose");
		private By newEmailWindowCloseBy = By.XPath("//button[@data-promo-id='extend']/following-sibling::button");

		private By to = By.XPath("//div[contains(@data-type,'to')]//input");
		private By subject = By.XPath("//input[contains(@name,'Subject')]");
		private By body = By.XPath("(//div[@contenteditable='true']//div)[1]");

		private By saveDraft = By.XPath("//button[@data-test-id='save']");

		private By draft = By.XPath("//div[@id='sideBarContent']//a[contains(@href,'drafts')]");

		private By sent = By.XPath("//div[@id='sideBarContent']//a[contains(@href,'sent')]");

		private By lettersBy = By.XPath("//div[contains(@class, 'letter-list')]//a[contains(@class, 'js-letter-list-item')]");
		private By letterCorrespondentBy = By.XPath("//span[contains(@class, 'll-crpt')]");
		private By letterSubjectBy = By.XPath("//span[contains(@class, 'llc__subject')]//span");
		private By letterSnippetBy = By.XPath("//span[contains(@class, 'llc__snippet')]//span");

		private By composeAppPopup = By.XPath("//div[contains(@class, 'compose-app_popup')]");
		private By composeAppPopupSend = By.XPath("//div[contains(@class, 'compose-app_popup')]//button[@data-test-id='send']");
		private By sentMessageClose = By.CssSelector(".layer__controls");

		private string configValue = ConfigurationManager.AppSettings.Get("browser");

		[SetUp]
		public void TestInit()
		{
			if ("ff".Equals(this.configValue))
			{
				var service = FirefoxDriverService.CreateDefaultService();
				this.driver = new FirefoxDriver(service);
			}
			else
			{
				ChromeOptions options = new ChromeOptions();
				options.AddArgument("disable-infobars");
				this.driver = new ChromeDriver(options);

			}
			this.baseUrl = "https://www.mail.ru/";
			this.driver.Navigate().GoToUrl(this.baseUrl);
			this.driver.Manage().Window.Maximize();

			//this.driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
		}

		[Test]
		[TestCase("lizakhramova", "070461040485")]
		public void TestLogin(string login, string password)
		{
			Login(login, password);
			//	Assert, that the login is successful.
			Assert.True(IsElementVisible(sideBarContent));
		}

		[Test]
		[TestCase("lizakhramova", "070461040485", "lizakhramova@mail.ru", "TestAT", "Hello! My name is Liza! How are you? See you later. Bye.")]
		public void SaveDraftEmail(string login, string password, string letterEmail, string letterSubject, string letterBody)
		{
			Login(login, password);
			//	Assert, that the login is successful.
			Assert.True(IsElementVisible(sideBarContent));

			letterSubject = letterSubject + DateTime.Now.TimeOfDay.Ticks.ToString();
			SaveDraftEmail(letterEmail, letterSubject, letterBody);

			//	Verify, that the mail presents in ‘Drafts’ folder
			//	Verify the draft content (addressee, subject and body – should be the same as in 3).
			IsElementVisible(sideBarContent);
			this.driver.FindElement(draft).Click();

			//var draftSubjectByStr = "//span[contains(@class, 'll-sj__normal') and contains(text(), '" + letterSubject + "')]";
			//var draftSubjectBy = By.XPath(draftSubjectByStr);
			//IsElementVisible(draftSubjectBy);
			Thread.Sleep(1000);

			var letterInDraft = FindLetterInList(lettersBy, letterEmail, letterSubject, letterBody);
			Assert.NotNull(letterInDraft);
		}

		[Test]
		[TestCase("lizakhramova", "070461040485", "lizakhramova@mail.ru", "TestAT", "Hello! My name is Liza! How are you? See you later. Bye.")]
		public void SendDraftEmail(string login, string password, string letterEmail, string letterSubject, string letterBody)
		{
			Login(login, password);
			//	Assert, that the login is successful.
			IsElementVisible(sideBarContent);

			letterSubject = letterSubject + DateTime.Now.TimeOfDay.Ticks.ToString();
			SaveDraftEmail(letterEmail, letterSubject, letterBody);

			var sendDraftEmailResult = SendDraftEmail(letterEmail, letterSubject, letterBody);

			Logout();

			Assert.IsTrue(sendDraftEmailResult);
		}

		private void Login(string login, string password)
		{
			this.driver.FindElement(By.ClassName("ph-login")).Click();
			this.driver.SwitchTo().Frame(this.driver.FindElement(By.ClassName("ag-popup__frame__layout__iframe")));
			IsElementVisible(userNameBy);
			this.driver.FindElement(userNameBy).SendKeys(login);
			this.driver.FindElement(inputLoginSubmit).Click();

			IsElementVisible(inputPassword);
			this.driver.FindElement(inputPassword).SendKeys(password);
			this.driver.FindElement(inputLoginSubmit).Click();

			var show = By.XPath("//a[@data-show-pixel]//div[@data-click-counter]");
			if (IsElementVisible(show))
				this.driver.FindElement(show).Click();
		}

		private void Logout()
		{
			var accoutContainer = By.XPath("//div[contains(@class, 'ph-project__account')]");
			this.driver.FindElement(accoutContainer).Click();
			var exit = By.XPath("//div[contains(@class, 'ph-accounts')]//div[contains(@class, 'ph-item')]//div[contains(@class, 'ph-icon')]");
			IsElementVisible(exit);
			this.driver.FindElement(exit).Click();
		}

		private void SaveDraftEmail(string letterEmail, string letterSubject, string letterBody)
		{
			IsElementVisible(newEmailBy);
			this.driver.FindElement(newEmailBy).Click();

			IsElementVisible(newEmailWindowBy);

			this.driver.FindElement(to).SendKeys(letterEmail);
			this.driver.FindElement(subject).SendKeys(letterSubject);
			this.driver.FindElement(body).SendKeys(letterBody);

			//	Save the mail as a draft.
			this.driver.FindElement(saveDraft).Click();

			var newEmailWindow = this.driver.FindElement(newEmailWindowBy);
			var newEmailWindowClose = newEmailWindow.FindElement(newEmailWindowCloseBy);
			newEmailWindowClose.Click();
		}

		private bool SendDraftEmail(string letterEmail, string letterSubject, string letterBody)
		{
			//	Verify, that the mail presents in ‘Drafts’ folder
			//	Verify the draft content (addressee, subject and body – should be the same as in 3).
			IsElementVisible(sideBarContent);
			this.driver.FindElement(draft).Click();
			Thread.Sleep(1000);

			var letterInDraft = FindLetterInList(lettersBy, letterEmail, letterSubject, letterBody);
			if (letterInDraft != null)
			{
				letterInDraft.Click();
				IsElementVisible(composeAppPopup);
				//	Send the mail
				this.driver.FindElement(composeAppPopupSend).Click();

				if (IsElementVisible(sentMessageClose))
					this.driver.FindElement(sentMessageClose).Click();

				var checkLetterInDraftResult = CheckLetterInDraft(letterEmail, letterSubject, letterBody);
				var checkLetterInSentResult = CheckLetterInSent(letterEmail, letterSubject, letterBody);
				return checkLetterInDraftResult == null && checkLetterInSentResult!=null;
			}
			return false;
		}

		private IWebElement? CheckLetterInDraft(string letterEmail, string letterSubject, string letterBody)
		{
			//	Verify, that the mail disappeared from ‘Drafts’ folder
			IsElementVisible(sideBarContent);
			this.driver.FindElement(draft).Click();
			Thread.Sleep(1000);

			var notExistLetterInDraft = FindLetterInList(lettersBy, letterEmail, letterSubject, letterBody);
			return notExistLetterInDraft;
		}

		private IWebElement? CheckLetterInSent(string letterEmail, string letterSubject, string letterBody)
		{
			//	Verify, that the mail is in ‘Sent’ folder.
			IsElementVisible(sideBarContent);
			this.driver.FindElement(sent).Click();
			Thread.Sleep(100);

			var letterInSent = FindLetterInList(lettersBy, letterEmail, letterSubject, letterBody);
			return letterInSent;
		}


		private IWebElement? FindLetterInList(By letters, string letterEmail, string letterSubject, string letterBody)
		{
			var lettersList = this.driver.FindElements(letters);
			foreach (var letter in lettersList)
			{
				var email = letter.FindElement(letterCorrespondentBy).GetAttribute("title");
				var subject = letter.FindElement(letterSubjectBy).Text;
				var data = letter.FindElement(letterSnippetBy).Text;

				if (string.Equals(email, letterEmail, StringComparison.OrdinalIgnoreCase) &&
					subject.Contains(letterSubject, StringComparison.OrdinalIgnoreCase) &&
					data.Contains(letterBody, StringComparison.OrdinalIgnoreCase))
				{
					return letter;
				}
			}
			return null;
		}

		public bool IsElementVisible(By element, int timeoutSecs = 10)
		{
			return new WebDriverWait(this.driver, TimeSpan.FromSeconds(timeoutSecs)).Until(ExpectedConditions.ElementIsVisible(element)) != null;
		}

		[TearDown]
		public void TestClean()
		{
			this.driver.Close();
			this.driver.Quit();
		}

	}
}