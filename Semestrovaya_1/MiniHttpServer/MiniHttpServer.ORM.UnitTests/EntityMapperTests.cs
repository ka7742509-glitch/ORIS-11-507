using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniHttpServer.ORM;

namespace MiniHttpServer.ORM.UnitTests;

[TestClass]
public class EntityMapperTests
{
    [TestMethod]
    public void Map_MatchingColumns_SetsProperties()
    {
        var row = new Dictionary<string, object?> { ["Id"] = 1, ["Name"] = "Alex", ["Age"] = 20 };
        var user = EntityMapper.Map<TestUser>(row);
        Assert.AreEqual(1, user.Id);
        Assert.AreEqual("Alex", user.Name);
        Assert.AreEqual(20, user.Age);
    }

    [TestMethod]
    public void Map_DifferentCaseColumns_SetsProperties()
    {
        var row = new Dictionary<string, object?> { ["name"] = "Maria" };
        Assert.AreEqual("Maria", EntityMapper.Map<TestUser>(row).Name);
    }

    [TestMethod]
    public void Map_MissingColumn_LeavesDefault()
    {
        var row = new Dictionary<string, object?> { ["Name"] = "Alex" };
        Assert.AreEqual(0, EntityMapper.Map<TestUser>(row).Age);
    }

    [TestMethod]
    public void Map_DbNullReferenceType_SetsNull()
    {
        var row = new Dictionary<string, object?> { ["Name"] = DBNull.Value };
        Assert.IsNull(EntityMapper.Map<TestUser>(row).Name);
    }

    [TestMethod]
    public void Map_DbNullNullableInt_SetsNull()
    {
        var row = new Dictionary<string, object?> { ["Nickname"] = DBNull.Value };
        Assert.IsNull(EntityMapper.Map<TestUser>(row).Nickname);
    }

    [TestMethod]
    public void ConvertValue_IntToDecimal_Converts()
        => Assert.AreEqual(2500m, EntityMapper.ConvertValue(2500, typeof(decimal)));

    [TestMethod]
    public void ConvertValue_StringToInt_Converts()
        => Assert.AreEqual(42, EntityMapper.ConvertValue("42", typeof(int)));

    [TestMethod]
    public void ConvertValue_IntToEnum_Converts()
        => Assert.AreEqual(TestStatus.Completed, EntityMapper.ConvertValue(1, typeof(TestStatus)));

    [TestMethod]
    public void ConvertValue_DbNullNonNullableInt_ReturnsDefault()
        => Assert.AreEqual(0, EntityMapper.ConvertValue(DBNull.Value, typeof(int)));

    [TestMethod]
    public void GetInsertProperties_TypeWithId_ExcludesId()
    {
        var names = EntityMapper.GetInsertProperties(typeof(TestUser)).Select(p => p.Name).ToArray();
        CollectionAssert.DoesNotContain(names, "Id");
        CollectionAssert.Contains(names, "Name");
    }

    [TestMethod]
    public void GetUpdateProperties_TypeWithId_ExcludesId()
    {
        var names = EntityMapper.GetUpdateProperties(typeof(TestProduct)).Select(p => p.Name).ToArray();
        CollectionAssert.DoesNotContain(names, "Id");
    }

    [TestMethod]
    public void Map_ProductModel_Works()
    {
        var row = new Dictionary<string, object?> { ["Id"] = 5, ["Name"] = "Mouse", ["Price"] = 1200.50m };
        var product = EntityMapper.Map<TestProduct>(row);
        Assert.AreEqual(5, product.Id);
        Assert.AreEqual(1200.50m, product.Price);
    }

    [TestMethod]
    public void Map_NullRow_Throws()
        => Assert.ThrowsExactly<ArgumentNullException>(() => EntityMapper.Map<TestUser>(null!));

    [TestMethod]
    public void ToParameterValue_Null_ReturnsDbNull()
        => Assert.AreEqual(DBNull.Value, EntityMapper.ToParameterValue(null));
}