using System;
using System.Collections.Generic;
using System.Globalization;
using AwesomeAssertions;

namespace Joker.Api.Test;

/// <summary>
/// Unit tests for <see cref="DmapiParameters"/>
/// </summary>
public class DmapiParametersTests
{
	[Fact]
	public void Set_String_AddsValueAndReturnsSelf()
	{
		// Arrange
		var parameters = new DmapiParameters();

		// Act
		var returned = parameters.Set("name", "value");

		// Assert
		returned.Should().BeSameAs(parameters);
		parameters.Values["name"].Should().Be("value");
	}

	[Fact]
	public void Set_Int_FormatsWithInvariantCulture()
	{
		// Arrange
		var parameters = new DmapiParameters();

		// Act
		parameters.Set("period", 7);

		// Assert
		parameters.Values["period"].Should().Be("7");
	}

	[Fact]
	public void SetIfPresent_StringWithValue_AddsValue()
	{
		// Arrange
		var parameters = new DmapiParameters();

		// Act
		parameters.SetIfPresent("name", "value");

		// Assert
		parameters.Values["name"].Should().Be("value");
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("   ")]
	public void SetIfPresent_StringWithBlankValue_SkipsIt(string? value)
	{
		// Arrange
		var parameters = new DmapiParameters();

		// Act
		parameters.SetIfPresent("name", value);

		// Assert
		parameters.Values.Should().NotContainKey("name");
	}

	[Fact]
	public void SetIfPresent_IntWithValue_AddsValue()
	{
		// Arrange
		var parameters = new DmapiParameters();

		// Act
		parameters.SetIfPresent("period", 14);

		// Assert
		parameters.Values["period"].Should().Be("14");
	}

	[Fact]
	public void SetIfPresent_NullInt_SkipsIt()
	{
		// Arrange
		var parameters = new DmapiParameters();

		// Act
		parameters.SetIfPresent("period", (int?)null);

		// Assert
		parameters.Values.Should().NotContainKey("period");
	}

	[Fact]
	public void SetIfEnabled_True_AddsFlagValue()
	{
		// Arrange
		var parameters = new DmapiParameters();

		// Act
		parameters.SetIfEnabled("showstatus", true);

		// Assert
		parameters.Values["showstatus"].Should().Be("1");
	}

	[Fact]
	public void SetIfEnabled_False_SkipsIt()
	{
		// Arrange
		var parameters = new DmapiParameters();

		// Act
		parameters.SetIfEnabled("showstatus", false);

		// Assert
		parameters.Values.Should().NotContainKey("showstatus");
	}

	[Fact]
	public void Values_InitialisesEmpty()
	{
		// Arrange
		var parameters = new DmapiParameters();

		// Act & Assert
		parameters.Values.Should().NotBeNull().And.BeEmpty();
	}
}
