using System.Threading.Tasks;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1149
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		protected override void OnTearDown()
		{
			using (ISession session = OpenSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
				session.Delete("from Address");
				session.Delete("from Company");
				session.Delete("from AddressO2O");
				session.Delete("from CompanyO2O");
				session.Flush();
				transaction.Commit();
			}
		}

		[Test]
		public void StatelessSessionLoadsOneToOneRelatedObject_PropertyRef()
		{
			// Create and save company and address
			var companyId = SaveCompanyAndAddress();

			using (var stateless = Sfi.OpenStatelessSession())
			{
				var loadedCompany = stateless.Get<Company>(companyId);

				Assert.That(loadedCompany, Is.Not.Null);
				Assert.That(loadedCompany.Name, Is.Not.Null);
				Assert.That(loadedCompany.Address, Is.Not.Null);
				Assert.That(loadedCompany.Address.AddressLine1, Is.Not.Null);
			}
		}

		[Test]
		public void StatelessSessionLoadsOneToOneRelatedObject_WithoutPropertyRef()
		{
			var companyId = SaveCompanyAndAddressO2O();

			using (var stateless = Sfi.OpenStatelessSession())
			{
				var loadedCompany = stateless.Get<CompanyO2O>(companyId);

				Assert.That(loadedCompany, Is.Not.Null);
				Assert.That(loadedCompany.Name, Is.Not.Null);
				Assert.That(loadedCompany.Address, Is.Not.Null);
				Assert.That(loadedCompany.Address.AddressLine1, Is.Not.Null);
			}
		}

		[Test]
		public async Task StatelessSessionLoadsOneToOneRelatedObject_WithProperyRef_Async()
		{
			// Create and save company and address
			var companyId = SaveCompanyAndAddress();

			using (var stateless = Sfi.OpenStatelessSession())
			{
				var loadedCompany = await stateless.GetAsync<Company>(companyId);

				Assert.That(loadedCompany, Is.Not.Null);
				Assert.That(loadedCompany.Name, Is.Not.Null);
				Assert.That(loadedCompany.Address, Is.Not.Null);
				Assert.That(loadedCompany.Address.AddressLine1, Is.Not.Null);
			}
		}

		[Test]
		public async Task StatelessSessionLoadsOneToOneRelatedObject_WithoutPropertyRef_Async()
		{
			var companyId = SaveCompanyAndAddressO2O();

			using (var stateless = Sfi.OpenStatelessSession())
			{
				var loadedCompany = await stateless.GetAsync<CompanyO2O>(companyId);

				Assert.That(loadedCompany, Is.Not.Null);
				Assert.That(loadedCompany.Name, Is.Not.Null);
				Assert.That(loadedCompany.Address, Is.Not.Null);
				Assert.That(loadedCompany.Address.AddressLine1, Is.Not.Null);
			}
		}

		private int SaveCompanyAndAddress()
		{
			var companyId = 0;

			using (ISession session = OpenSession())
			{
				using (ITransaction tx = session.BeginTransaction())
				{
					var company = new Company { Name = "Test Company" };

					company.Address = new Address(company) { AddressLine1 = "Company Address" };

					companyId = (int) session.Save(company);

					tx.Commit();
				}

				return companyId;
			}
		}

		private int SaveCompanyAndAddressO2O()
		{
			var addressId = 0;

			using (ISession session = OpenSession())
			{
				using (ITransaction tx = session.BeginTransaction())
				{
					var company = new CompanyO2O { Name = "Test Company" };
					var address = new AddressO2O {AddressLine1 = "Company Address"};

					address.SetCompany(company);

					// Have to save the address to get the company to be saved as well
					// Saving company doesn't save the address.
					addressId = (int) session.Save(address);

					tx.Commit();
				}
			}

			return addressId;
		}
	}
}
