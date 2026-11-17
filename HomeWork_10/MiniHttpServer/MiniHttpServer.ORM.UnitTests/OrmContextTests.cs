using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniHttpServer.ORM;

namespace MiniHttpServer.ORM.UnitTests;

[TestClass]
public class OrmContextTests
{
    private const string FakeConnection = "Server=localhost;Database=OrisDb;Integrated Security=True";

    [DataTestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void Ctor_EmptyConnectionString_Throws(string value)
        => Assert.ThrowsExactly<ArgumentException>(() => new OrmContext(value));

    [TestMethod]
    public void Ctor_NullConnectionString_Throws()
        => Assert.ThrowsExactly<ArgumentNullException>(() => new OrmContext(null!));

    [TestMethod]
    public void Ctor_ValidConnectionString_CreatesContext()
        => Assert.IsNotNull(new OrmContext(FakeConnection));

    [TestMethod]
    public async Task GetAllAsync_InvalidTableName_ThrowsBeforeConnecting()
    {
        var orm = new OrmContext(FakeConnection);
        await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await orm.GetAllAsync<TestUser>("bad;name"));
    }

    [TestMethod]
    public async Task InsertAsync_NullEntity_Throws()
    {
        var orm = new OrmContext(FakeConnection);
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await orm.InsertAsync<TestUser>("Users", null!));
    }

    [TestMethod]
    public async Task UpdateAsync_NullEntity_Throws()
    {
        var orm = new OrmContext(FakeConnection);
        await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () => await orm.UpdateAsync<TestUser>("Users", 1, null!));
    }

    [TestMethod]
    public async Task DeleteAsync_InvalidTableName_ThrowsBeforeConnecting()
    {
        var orm = new OrmContext(FakeConnection);
        await Assert.ThrowsExactlyAsync<ArgumentException>(async () => await orm.DeleteAsync("dbo.Users", 1));
    }
}