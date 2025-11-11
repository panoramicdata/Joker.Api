using Joker.Api.Models;

namespace Joker.Api.Test;

/// <summary>
/// Tests for DNS record models
/// </summary>
public class DnsRecordTests
{
	[Fact]
	public void CreateTxtRecord_WithAllParameters_CreatesCorrectRecord()
	{
		// Arrange
		const string label = "_acme-challenge";
		const string value = "verification-token";
		const int ttl = 300;

		// Act
		var record = DnsRecord.CreateTxtRecord(label, value, ttl);

		// Assert
		Assert.Equal("TXT", record.Type);
		Assert.Equal(label, record.Label);
		Assert.Equal(value, record.Value);
		Assert.Equal(ttl, record.Ttl);
		Assert.Null(record.Priority);
	}

	[Fact]
	public void CreateTxtRecord_WithoutTtl_CreatesRecordWithNullTtl()
	{
		// Arrange
		const string label = "test";
		const string value = "value";

		// Act
		var record = DnsRecord.CreateTxtRecord(label, value);

		// Assert
		Assert.Equal("TXT", record.Type);
		Assert.Equal(label, record.Label);
		Assert.Equal(value, record.Value);
		Assert.Null(record.Ttl);
	}

	[Fact]
	public void CreateARecord_WithAllParameters_CreatesCorrectRecord()
	{
		// Arrange
		const string label = "www";
		const string ipAddress = "192.168.1.1";
		const int ttl = 3600;

		// Act
		var record = DnsRecord.CreateARecord(label, ipAddress, ttl);

		// Assert
		Assert.Equal("A", record.Type);
		Assert.Equal(label, record.Label);
		Assert.Equal(ipAddress, record.Value);
		Assert.Equal(ttl, record.Ttl);
		Assert.Null(record.Priority);
	}

	[Fact]
	public void CreateARecord_WithoutTtl_CreatesRecordWithNullTtl()
	{
		// Arrange
		const string label = "www";
		const string ipAddress = "10.0.0.1";

		// Act
		var record = DnsRecord.CreateARecord(label, ipAddress, null);

		// Assert
		Assert.Equal("A", record.Type);
		Assert.Null(record.Ttl);
	}

	[Fact]
	public void CreateCnameRecord_WithAllParameters_CreatesCorrectRecord()
	{
		// Arrange
		const string label = "blog";
		const string target = "blog.provider.com";
		const int ttl = 7200;

		// Act
		var record = DnsRecord.CreateCnameRecord(label, target, ttl);

		// Assert
		Assert.Equal("CNAME", record.Type);
		Assert.Equal(label, record.Label);
		Assert.Equal(target, record.Value);
		Assert.Equal(ttl, record.Ttl);
		Assert.Null(record.Priority);
	}

	[Fact]
	public void CreateCnameRecord_WithoutTtl_CreatesRecordWithNullTtl()
	{
		// Arrange
		const string label = "mail";
		const string target = "mail.google.com";

		// Act
		var record = DnsRecord.CreateCnameRecord(label, target);

		// Assert
		Assert.Equal("CNAME", record.Type);
		Assert.Null(record.Ttl);
	}

	[Fact]
	public void ToZoneFormat_WithoutTtlAndPriority_ReturnsCorrectFormat()
	{
		// Arrange
		var record = new DnsRecord
		{
			Type = "A",
			Label = "www",
			Value = "192.168.1.1"
		};

		// Act
		var result = record.ToZoneFormat();

		// Assert
		Assert.Equal("A:www:192.168.1.1", result);
	}

	[Fact]
	public void ToZoneFormat_WithTtl_IncludesTtl()
	{
		// Arrange
		var record = new DnsRecord
		{
			Type = "A",
			Label = "www",
			Value = "192.168.1.1",
			Ttl = 3600
		};

		// Act
		var result = record.ToZoneFormat();

		// Assert
		Assert.Equal("A:www:192.168.1.1:3600", result);
	}

	[Fact]
	public void ToZoneFormat_WithPriority_IncludesPriority()
	{
		// Arrange
		var record = new DnsRecord
		{
			Type = "MX",
			Label = "@",
			Value = "mail.example.com",
			Priority = 10
		};

		// Act
		var result = record.ToZoneFormat();

		// Assert
		Assert.Equal("MX:@:10:mail.example.com", result);
	}

	[Fact]
	public void ToZoneFormat_WithPriorityAndTtl_IncludesBoth()
	{
		// Arrange
		var record = new DnsRecord
		{
			Type = "MX",
			Label = "@",
			Value = "mail.example.com",
			Priority = 10,
			Ttl = 7200
		};

		// Act
		var result = record.ToZoneFormat();

		// Assert
		Assert.Equal("MX:@:10:mail.example.com:7200", result);
	}

	[Fact]
	public void DnsRecord_SetProperties_WorksCorrectly()
	{
		// Arrange & Act
		var record = new DnsRecord
		{
			Type = "TXT",
			Label = "test",
			Value = "value",
			Ttl = 300,
			Priority = 5
		};

		// Assert
		Assert.Equal("TXT", record.Type);
		Assert.Equal("test", record.Label);
		Assert.Equal("value", record.Value);
		Assert.Equal(300, record.Ttl);
		Assert.Equal(5, record.Priority);
	}
}
