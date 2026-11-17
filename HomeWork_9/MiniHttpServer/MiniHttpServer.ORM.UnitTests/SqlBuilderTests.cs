using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniHttpServer.ORM;

namespace MiniHttpServer.ORM.UnitTests;

[TestClass]
public class SqlBuilderTests
{
    [TestMethod]
    public void EscapeIdentifier_ValidName_ReturnsBracketed()
        => Assert.AreEqual("[Users]", SqlBuilder.EscapeIdentifier("Users"));

    [DataTestMethod]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("1Users")]
    [DataRow("Users;DROP TABLE Users")]
    [DataRow("dbo.Users")]
    [DataRow("Users]")]
    public void EscapeIdentifier_InvalidName_Throws(string name)
        => Assert.ThrowsExactly<ArgumentException>(() => SqlBuilder.EscapeIdentifier(name));

    [TestMethod]
    public void BuildSelect_ValidTable_ReturnsSelect()
        => Assert.AreEqual("SELECT * FROM [Users]", SqlBuilder.BuildSelect("Users"));

    [TestMethod]
    public void BuildSelectById_ValidTable_ContainsWhereId()
        => Assert.AreEqual("SELECT * FROM [Users] WHERE [Id] = @Id", SqlBuilder.BuildSelectById("Users"));

    [TestMethod]
    public void BuildInsert_Columns_BuildsParameterizedInsert()
    {
        string sql = SqlBuilder.BuildInsert("Users", new[] { "Name", "Email", "Age" });
        Assert.AreEqual("INSERT INTO [Users] ([Name], [Email], [Age]) VALUES (@Name, @Email, @Age)", sql);
    }

    [TestMethod]
    public void BuildInsert_EmptyColumns_Throws()
        => Assert.ThrowsExactly<ArgumentException>(() => SqlBuilder.BuildInsert("Users", Array.Empty<string>()));

    [TestMethod]
    public void BuildUpdate_Columns_BuildsParameterizedUpdate()
    {
        string sql = SqlBuilder.BuildUpdate("Users", new[] { "Name", "Age" });
        Assert.AreEqual("UPDATE [Users] SET [Name] = @Name, [Age] = @Age WHERE [Id] = @Id", sql);
    }

    [TestMethod]
    public void BuildUpdate_IdInColumns_Throws()
        => Assert.ThrowsExactly<ArgumentException>(() => SqlBuilder.BuildUpdate("Users", new[] { "Id", "Name" }));

    [TestMethod]
    public void BuildUpdate_ValidColumns_DoesNotChangePrimaryKey()
    {
        string sql = SqlBuilder.BuildUpdate("Users", new[] { "Name" });
        Assert.IsFalse(sql.Contains("[Id] = @Name"));
        StringAssert.Contains(sql, "WHERE [Id] = @Id");
    }

    [TestMethod]
    public void BuildDelete_ValidTable_BuildsParameterizedDelete()
        => Assert.AreEqual("DELETE FROM [Users] WHERE [Id] = @Id", SqlBuilder.BuildDelete("Users"));

    [TestMethod]
    public void BuildSelect_InvalidTable_Throws()
        => Assert.ThrowsExactly<ArgumentException>(() => SqlBuilder.BuildSelect("bad;name"));
}